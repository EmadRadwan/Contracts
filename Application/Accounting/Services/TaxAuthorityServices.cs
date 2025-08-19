using Application.Catalog.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Domain;
using Persistence;


namespace Application.Accounting.Services
{
    /// <summary>
    /// Provides services for calculating sales tax and VAT based on tax authority rules in Apache OFBiz.
    /// Handles tax calculations for individual products and entire orders, including exemptions and VAT-inclusive pricing.
    /// </summary>
    public class TaxAuthorityServices
    {
        // Defines the module name for logging, used to identify the source of log messages in OFBiz logs.
        private static readonly string MODULE = nameof(TaxAuthorityServices);

        // Specifies the resource name for localized UI labels, used for error messages in the accounting module.
        private static readonly string RESOURCE = "AccountingUiLabels";

        // Represents a zero value for financial calculations, ensuring consistent initialization.
        private static readonly decimal ZERO_BASE = 0m;

        // Represents a unit value (1) for calculations, used as a default quantity or multiplier.
        private static readonly decimal ONE_BASE = 1m;

        // Defines the scale for percentage calculations (100.000), used to convert tax rates from percentages.
        private static readonly decimal PERCENT_SCALE = 100.000m;

        // Specifies the number of decimal places for final tax amounts displayed to users (e.g., 2 decimals).
        private static readonly int TAX_FINAL_SCALE = 2; // Should be configurable via IConfiguration

        // Specifies the number of decimal places for intermediate tax calculations (e.g., 4 decimals).
        private static readonly int TAX_SCALE = 4; // Should be configurable via IConfiguration

        // Defines the rounding mode for tax calculations, using HALF_UP to match OFBiz’s financial precision.
        private static readonly RoundingMode TAX_ROUNDING = RoundingMode.HALF_UP;

        // Provides access to the Entity Framework DbContext for database operations, replacing OFBiz’s Delegator.
        private readonly DataContext _dbContext;

        // Enables logging of errors, warnings, and informational messages, replacing OFBiz’s Debug utility.
        private readonly ILogger<TaxAuthorityServices> _logger;
        private IProductService _productService;

        /// <summary>
        /// Initializes a new instance of TaxAuthorityServices with dependencies for database, logging, and configuration.
        /// </summary>
        /// <param name="dbContext">The EF Core DbContext for database access.</param>
        /// <param name="logger">The logger for recording errors and information.</param>
        public TaxAuthorityServices(DataContext dbContext, ILogger<TaxAuthorityServices> logger,
            IProductService productService)
        {
            // Validates and assigns the DbContext, ensuring database access is available.
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            // Validates and assigns the logger, ensuring logging is available.
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
        }

        /// <summary>
        /// Calculates tax for a single product for display purposes, including VAT if configured.
        /// Returns tax total, percentage, and price with tax for use in UI or checkout displays.
        /// </summary>
        /// <param name="productStoreId">The ID of the product store, used to determine tax settings.</param>
        /// <param name="billToPartyId">The ID of the billing party, used for tax exemptions.</param>
        /// <param name="productId">The ID of the product being taxed.</param>
        /// <param name="quantity">The quantity of the product (defaults to 1 if null).</param>
        /// <param name="basePrice">The base price of the product before tax.</param>
        /// <param name="shippingPrice">The shipping cost, if applicable, to include in tax calculations.</param>
        /// <returns>A TaxServiceResult containing taxTotal, taxPercentage, and priceWithTax, or an error message.</returns>
        public async Task<TaxServiceResult> RateProductTaxCalcForDisplay(
            string productStoreId, string billToPartyId, string productId,
            decimal? quantity, decimal basePrice, decimal? shippingPrice)
        {
            try
            {
                // Sets default quantity to 1 if not provided, ensuring calculations proceed with a valid quantity.
                if (quantity == null)
                {
                    quantity = ONE_BASE;
                }

                // Calculates the total amount by multiplying base price by quantity, representing the taxable subtotal.
                decimal amount = basePrice * quantity.Value;
                // Initializes the total tax amount to zero, to accumulate tax adjustments.
                decimal taxTotal = ZERO_BASE;
                // Initializes the cumulative tax percentage to zero, to sum up tax rates applied.
                decimal taxPercentage = ZERO_BASE;
                // Initializes price with tax as the base price, to be adjusted with tax and shipping.
                decimal priceWithTax = basePrice;
                // Adds shipping price to priceWithTax if provided, as shipping may be taxable in some jurisdictions.
                if (shippingPrice != null)
                {
                    priceWithTax = priceWithTax + shippingPrice.Value;
                }

                // Retrieves the product entity by ID using EF Core, checking if the product exists for tax purposes.
                Product product = await _dbContext.Products
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.ProductId == productId);
                // Retrieves the product store entity by ID, which contains tax configuration settings (e.g., VAT settings).
                ProductStore productStore = await _dbContext.ProductStores
                    .AsNoTracking()
                    .FirstOrDefaultAsync(ps => ps.ProductStoreId == productStoreId);
                // Validates that the product store exists, as it’s critical for determining tax rules.
                if (productStore == null)
                {
                    // Logs an error if the store is not found, indicating a configuration issue.
                    _logger.LogError($"Could not find ProductStore with ID [{productStoreId}] for tax calculation");
                    // Throws an exception to halt processing, as tax calculation cannot proceed without store settings.
                    throw new ArgumentException(
                        $"Could not find ProductStore with ID [{productStoreId}] for tax calculation");
                }

                // Checks if the store is configured to show prices inclusive of VAT, a common setting in European markets.
                if (productStore.ShowPricesWithVatTax == "Y")
                {
                    // Initializes a set to store applicable tax authorities, ensuring no duplicates.
                    HashSet<TaxAuthority> taxAuthoritySet = new HashSet<TaxAuthority>();
                    // Checks if the store specifies a single tax authority party ID for VAT.
                    if (string.IsNullOrEmpty(productStore.VatTaxAuthPartyId))
                    {
                        // Queries all tax authorities for the store’s VAT geo ID (e.g., country or region).
                        var taxAuthorityRawList = await _dbContext.TaxAuthorities
                            .AsNoTracking()
                            .Where(ta => ta.TaxAuthGeoId == productStore.VatTaxAuthGeoId)
                            .ToListAsync();
                        // Adds all found tax authorities to the set, covering multiple jurisdictions if applicable.
                        taxAuthoritySet.UnionWith(taxAuthorityRawList);
                    }
                    else
                    {
                        // Queries a specific tax authority by geo ID and party ID, for stores with a single tax authority.
                        TaxAuthority taxAuthority = await _dbContext.TaxAuthorities
                            .AsNoTracking()
                            .FirstOrDefaultAsync(ta => ta.TaxAuthGeoId == productStore.VatTaxAuthGeoId &&
                                                       ta.TaxAuthPartyId == productStore.VatTaxAuthPartyId);
                        // Adds the tax authority to the set if found, ensuring it’s included in calculations.
                        if (taxAuthority != null)
                        {
                            taxAuthoritySet.Add(taxAuthority);
                        }
                    }

                    // Validates that at least one tax authority was found, as tax calculation requires authority rules.
                    if (taxAuthoritySet.Count == 0)
                    {
                        // Logs an error indicating a configuration issue with the store’s tax settings.
                        _logger.LogError(
                            $"Could not find any Tax Authorities for store with ID [{productStoreId}] for tax calculation");
                        // Throws an exception to halt processing, as tax calculation cannot proceed without authorities.
                        throw new ArgumentException(
                            $"Could not find any Tax Authorities for store with ID [{productStoreId}] for tax calculation; the store settings may need to be corrected.");
                    }

                    // Calls the helper method to compute tax adjustments based on product, store, and tax authorities.
                    // Adjustments represent individual tax applications (e.g., sales tax, VAT) for the product.
                    List<OrderAdjustment> taxAdustmentList = await GetTaxAdjustments(
                        product, productStore, null, billToPartyId, taxAuthoritySet,
                        basePrice, quantity.Value, amount, shippingPrice, ZERO_BASE);
                    // Checks if no tax adjustments were found, which may occur for non-taxable products or misconfigured rules.
                    if (taxAdustmentList.Count == 0)
                    {
                        // Logs a warning, as this is a common scenario for certain products but may indicate a setup issue.
                        _logger.LogWarning(
                            $"Could not find any Tax Authority Rate Rules for store with ID [{productStoreId}], productId [{productId}], basePrice [{basePrice}], amount [{amount}] for tax calculation; the store settings may need to be corrected.");
                    }

                    // Iterates through tax adjustments to aggregate tax amounts and percentages.
                    foreach (OrderAdjustment taxAdjustment in taxAdustmentList)
                    {
                        // Processes only sales tax adjustments, ignoring other types (e.g., VAT or exemptions).
                        if (taxAdjustment.OrderAdjustmentTypeId == "VAT_TAX")
                        {
                            // Adds the tax rate percentage to the cumulative tax percentage, tracking the total rate applied.
                            taxPercentage = (decimal)(taxPercentage + taxAdjustment.SourcePercentage);
                            // Retrieves the tax amount for this adjustment, representing the tax liability.
                            decimal adjAmount = (decimal)taxAdjustment.Amount;
                            // Adds the adjustment amount to the total tax, accumulating all taxes for the product.
                            taxTotal = taxTotal + adjAmount;
                            // Calculates the per-unit tax by dividing the adjustment amount by quantity, then rounds it.
                            // Adds this to priceWithTax to reflect the tax-inclusive price per unit.
                            priceWithTax = priceWithTax + (adjAmount / quantity.Value).Round(TAX_SCALE, TAX_ROUNDING);
                            // Logs an informational message detailing the tax added for the product, aiding debugging.
                            _logger.LogInformation(
                                $"For productId [{productId}] added [{(adjAmount / quantity.Value).Round(TAX_SCALE, TAX_ROUNDING)}] of tax to price for geoId [{taxAdjustment.TaxAuthGeoId}], new price is [{priceWithTax}]");
                        }
                    }
                }

                // Rounds the total tax amount to the final scale (e.g., 2 decimals) for display purposes.
                taxTotal = taxTotal.Round(TAX_FINAL_SCALE, TAX_ROUNDING);
                // Rounds the price with tax to the final scale, ensuring consistency in UI presentation.
                priceWithTax = priceWithTax.Round(TAX_FINAL_SCALE, TAX_ROUNDING);

                // Creates a success result with the calculated tax values, formatted as an anonymous object.
                var resultData = new
                {
                    taxTotal,
                    taxPercentage,
                    priceWithTax
                };
                // Returns a success result, indicating the tax calculation was completed successfully.
                return TaxServiceResult.CreateSuccess(resultData);
            }
            catch (Exception e)
            {
                // Logs the exception with details, ensuring errors are traceable for debugging.
                _logger.LogError(e, $"Data error getting tax settings: {e.Message}");
                // Returns an error result with a localized message, mimicking OFBiz’s error handling.
                return TaxServiceResult.Error(e.Message);
            }
        }

        /// <summary>
        /// Calculates taxes for an entire order, including multiple items, shipping, and promotions.
        /// Returns order-level and item-level tax adjustments for accounting and checkout processes.
        /// </summary>
        /// <param name="productStoreId">The ID of the product store, used for tax settings.</param>
        /// <param name="facilityId">The ID of the facility, used for face-to-face sale address lookup.</param>
        /// <param name="payToPartyId">The ID of the party receiving payment, used for tax accounting.</param>
        /// <param name="billToPartyId">The ID of the billing party, used for tax exemptions.</param>
        /// <param name="itemProductList">List of products in the order.</param>
        /// <param name="itemAmountList">List of total amounts for each item.</param>
        /// <param name="itemPriceList">List of unit prices for each item.</param>
        /// <param name="itemQuantityList">List of quantities for each item.</param>
        /// <param name="itemShippingList">List of shipping amounts for each item.</param>
        /// <param name="orderShippingAmount">Total shipping amount for the order.</param>
        /// <param name="orderPromotionsAmount">Total promotions amount for the order.</param>
        /// <param name="shippingAddress">The shipping address, used to determine tax jurisdictions.</param>
        /// <returns>A TaxServiceResult containing orderAdjustments and itemAdjustments, or an error message.</returns>
        public async Task<TaxServiceResult> RateProductTaxCalc(
            string productStoreId, string facilityId, string payToPartyId, string billToPartyId,
            List<Product> itemProductList, List<decimal> itemAmountList, List<decimal> itemPriceList,
            List<decimal> itemQuantityList, List<decimal> itemShippingList, decimal? orderShippingAmount,
            decimal? orderPromotionsAmount, PostalAddress shippingAddress)
        {
            try
            {
                // Initializes the product store variable, to be populated if a store ID is provided.
                ProductStore productStore = null;
                // Initializes the facility variable, to be populated if a facility ID is provided.
                Facility facility = null;
                // Attempts to retrieve the product store by ID if provided, as it contains tax settings.
                if (!string.IsNullOrEmpty(productStoreId))
                {
                    // Queries the ProductStore entity using EF Core, retrieving store configuration.
                    productStore = await _dbContext.ProductStores
                        .AsNoTracking()
                        .FirstOrDefaultAsync(ps => ps.ProductStoreId == productStoreId);
                }

                // Attempts to retrieve the facility by ID if provided, used for face-to-face sale scenarios.
                if (!string.IsNullOrEmpty(facilityId))
                {
                    // Queries the Facility entity using EF Core, retrieving facility details.
                    facility = await _dbContext.Facilities
                        .AsNoTracking()
                        .FirstOrDefaultAsync(f => f.FacilityId == facilityId);
                }

                // Validates that either a product store or pay-to party ID is provided, as one is required for tax calculations.
                if (productStore == null && payToPartyId == null)
                {
                    // Logs an error indicating missing configuration critical for tax processing.
                    _logger.LogError("Could not find payToPartyId or ProductStore for tax calculation");
                    // Throws an exception to halt processing, as tax calculation cannot proceed.
                    throw new ArgumentException("Could not find payToPartyId or ProductStore for tax calculation");
                }

                // Checks if a shipping address is missing but a facility is provided, indicating a face-to-face sale.
                if (shippingAddress == null && facility != null)
                {
                    // Queries the facility’s contact mechanism for shipping or primary location purposes.
                    ContactMech facilityContactMech = await _dbContext.ContactMeches
                        .AsNoTracking()
                        .Join(_dbContext.FacilityContactMechPurposes,
                            cm => cm.ContactMechId,
                            fcmp => fcmp.ContactMechId,
                            (cm, fcmp) => new { ContactMech = cm, Purpose = fcmp })
                        .Where(x => x.Purpose.FacilityId == facilityId &&
                                    (x.Purpose.ContactMechPurposeTypeId == "SHIP_ORIG_LOCATION" ||
                                     x.Purpose.ContactMechPurposeTypeId == "PRIMARY_LOCATION"))
                        .Select(x => x.ContactMech)
                        .FirstOrDefaultAsync();
                    // If a contact mechanism is found, retrieves the associated postal address.
                    if (facilityContactMech != null)
                    {
                        // Queries the PostalAddress entity linked to the contact mechanism.
                        shippingAddress = await _dbContext.PostalAddresses
                            .AsNoTracking()
                            .FirstOrDefaultAsync(pa => pa.ContactMechId == facilityContactMech.ContactMechId);
                    }
                }


                // Initializes a set to store tax authorities applicable to the shipping address’s jurisdictions.
                HashSet<TaxAuthority> taxAuthoritySet = new HashSet<TaxAuthority>();
                // Retrieves tax authorities based on the shipping address’s geo IDs (country, state, etc.).
                await GetTaxAuthorities(_dbContext, shippingAddress, taxAuthoritySet);

                // Initializes a list to store order-level tax adjustments, such as shipping or promotion taxes.
                List<OrderAdjustment> orderAdjustments = new List<OrderAdjustment>();
                // Initializes a list of lists to store per-item tax adjustments, maintaining one list per item.
                List<List<OrderAdjustment>> itemAdjustments = new List<List<OrderAdjustment>>();
                // Initializes the total price of the order, used to calculate proportional weights for shipping taxes.
                decimal totalPrice = ZERO_BASE;
                // Initializes a dictionary to track each product’s contribution to the total price.
                Dictionary<Product, decimal> productWeight = new Dictionary<Product, decimal>();
                // Iterates through the items in the order to calculate taxes for each.
                for (int i = 0; i < itemProductList.Count; i++)
                {
                    // Retrieves the current product from the item list.
                    Product product = itemProductList[i];
                    // Retrieves the total amount for the item (price * quantity).
                    decimal itemAmount = itemAmountList[i];
                    // Retrieves the unit price for the item.
                    decimal itemPrice = itemPriceList[i];
                    // Retrieves the quantity for the item, or null if not provided.
                    decimal? itemQuantity = itemQuantityList != null ? itemQuantityList[i] : null;
                    // Retrieves the shipping amount for the item, or null if not provided.
                    decimal? shippingAmount = itemShippingList != null ? itemShippingList[i] : null;

                    if (itemAmount <= 0)
                    {
                        _logger.LogDebug("Skipping tax calculation for product {ProductId} with amount {ItemAmount}", product.ProductId, itemAmount);
                        itemAdjustments.Add(new List<OrderAdjustment>());
                        continue;
                    }
                    
                    // Adds the item’s amount to the total order price, used for proportional weight calculations.
                    totalPrice = totalPrice + itemAmount;

                    // Computes tax adjustments for the item, considering product, price, and tax authorities.
                    List<OrderAdjustment> taxList = await GetTaxAdjustments(
                        product, productStore, payToPartyId, billToPartyId, taxAuthoritySet,
                        itemPrice, itemQuantity, itemAmount, shippingAmount, ZERO_BASE);

                    // Adds the item’s tax adjustments to the itemAdjustments list, maintaining a per-item structure.
                    itemAdjustments.Add(taxList);

                    // Retrieves the current total price for the product, defaulting to zero if not yet tracked.
                    decimal currentTotalPrice = productWeight.ContainsKey(product) ? productWeight[product] : ZERO_BASE;
                    // Adds the item’s amount to the product’s total price, tracking its contribution to the order.
                    currentTotalPrice = currentTotalPrice + itemAmount;
                    // Updates the product’s total price in the dictionary.
                    productWeight[product] = currentTotalPrice;
                }

                // Converts the total prices of products into percentage weights for distributing order-level taxes.
                foreach (Product prod in productWeight.Keys.ToList())
                {
                    // Retrieves the product’s total price contribution.
                    decimal value = productWeight[prod];
                    // Ensures the total price is positive to avoid division by zero.
                    if (totalPrice > ZERO_BASE)
                    {
                        // Calculates the product’s weight as a percentage of the total order price, rounded to 100 decimals.
                        decimal weight = (value / totalPrice).Round(2, TAX_ROUNDING);
                        // Updates the product’s weight in the dictionary, used for apportioning shipping taxes.
                        productWeight[prod] = weight;
                    }
                }

                // Checks if there’s a non-zero order shipping amount, which may be taxable.
                if (orderShippingAmount != null && orderShippingAmount > ZERO_BASE)
                {
                    // Iterates through products to apportion shipping taxes based on their weights.
                    foreach (Product prod in productWeight.Keys)
                    {
                        // Computes tax adjustments for the shipping amount, weighted by the product’s contribution.
                        List<OrderAdjustment> taxList = await GetTaxAdjustments(
                            prod, productStore, payToPartyId, billToPartyId, taxAuthoritySet,
                            ZERO_BASE, ZERO_BASE, ZERO_BASE, orderShippingAmount, 0, productWeight[prod]);
                        // Adds the shipping tax adjustments to the order-level adjustments list.
                        orderAdjustments.AddRange(taxList);
                    }
                }

                // Checks if there’s a non-zero promotions amount, which may be taxable or exempt.
                if (orderPromotionsAmount != null && orderPromotionsAmount != ZERO_BASE)
                {
                    // Computes tax adjustments for the promotions amount, applying relevant tax rules.
                    List<OrderAdjustment> taxList = await GetTaxAdjustments(
                        null, productStore, payToPartyId, billToPartyId, taxAuthoritySet,
                        ZERO_BASE, ZERO_BASE, ZERO_BASE, null, (decimal)orderPromotionsAmount);
                    // Adds the promotion tax adjustments to the order-level adjustments list.
                    orderAdjustments.AddRange(taxList);
                }

                // Creates a success result with the calculated order and item adjustments.
                var resultData = new
                {
                    orderAdjustments,
                    itemAdjustments
                };
                // Returns a success result, indicating the tax calculation was completed successfully.
                return TaxServiceResult.CreateSuccess(resultData);
            }
            catch (Exception e)
            {
                // Logs the exception with details, ensuring errors are traceable for debugging.
                _logger.LogError(e, $"Data error getting tax settings: {e.Message}");
                // Returns an error result with a localized message, mimicking OFBiz’s error handling.
                return TaxServiceResult.Error(e.Message);
            }
        }

        /// <summary>
        /// Retrieves tax authorities applicable to a shipping address’s geographic regions.
        /// Populates a set with tax authorities based on country, state, county, and postal code.
        /// </summary>
        /// <param name="dbContext">The EF Core DbContext for database access.</param>
        /// <param name="shippingAddress">The shipping address containing geo IDs.</param>
        /// <param name="taxAuthoritySet">The set to populate with tax authorities.</param>
        private async Task GetTaxAuthorities(DbContext dbContext, PostalAddress shippingAddress,
            HashSet<TaxAuthority> taxAuthoritySet)
        {
            try
            {
                if (shippingAddress == null || string.IsNullOrEmpty(shippingAddress.CountryGeoId))
                {
                    _logger.LogWarning("shippingAddress or CountryGeoId was null, adding nothing to taxAuthoritySet");
                    return;
                }

                var taxAuthorityRawList = await _dbContext.TaxAuthorities
                    .AsNoTracking()
                    .Where(ta => ta.TaxAuthGeoId == shippingAddress.CountryGeoId)
                    .ToListAsync();

                taxAuthoritySet.UnionWith(taxAuthorityRawList);

                if (!taxAuthorityRawList.Any())
                {
                    _logger.LogWarning($"No tax authorities found for geo ID [{shippingAddress.CountryGeoId}]");
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Data error getting tax settings: {e.Message}");
                throw;
            }
        }

        /// <summary>
        /// Computes tax adjustments for a product or order component (e.g., item, shipping, promotion).
        /// Overload that defaults weight to null, simplifying calls for item-level calculations.
        /// </summary>
        /// <param name="product">The product being taxed, or null for order-level adjustments.</param>
        /// <param name="productStore">The product store containing tax settings.</param>
        /// <param name="payToPartyId">The ID of the party receiving payment.</param>
        /// <param name="billToPartyId">The ID of the billing party for exemptions.</param>
        /// <param name="taxAuthoritySet">The set of applicable tax authorities.</param>
        /// <param name="itemPrice">The unit price of the item.</param>
        /// <param name="itemQuantity">The quantity of the item.</param>
        /// <param name="itemAmount">The total amount for the item.</param>
        /// <param name="shippingAmount">The shipping amount, if applicable.</param>
        /// <param name="orderPromotionsAmount">The promotions amount, if applicable.</param>
        /// <returns>A list of tax adjustments (e.g., sales tax, VAT).</returns>
        private Task<List<OrderAdjustment>> GetTaxAdjustments(
            Product product, ProductStore productStore, string payToPartyId, string billToPartyId,
            HashSet<TaxAuthority> taxAuthoritySet, decimal itemPrice, decimal? itemQuantity,
            decimal itemAmount, decimal? shippingAmount, decimal orderPromotionsAmount)
        {
            // Calls the main GetTaxAdjustments method with a null weight, used for item-level tax calculations.
            return GetTaxAdjustments(product, productStore, payToPartyId, billToPartyId, taxAuthoritySet,
                itemPrice, itemQuantity, itemAmount, shippingAmount, orderPromotionsAmount, null);
        }

        /// <summary>
        /// Computes tax adjustments for a product or order component based on tax authority rules.
        /// Supports sales tax, VAT, exemptions, and VAT-inclusive price corrections.
        /// </summary>
        /// <param name="product">The product being taxed, or null for order-level taxes.</param>
        /// <param name="productStore">The product store with tax settings.</param>
        /// <param name="payToPartyId">The party receiving payment.</param>
        /// <param name="billToPartyId">The billing party for exemptions.</param>
        /// <param name="taxAuthoritySet">Applicable tax authorities.</param>
        /// <param name="itemPrice">Unit price of the item.</param>
        /// <param name="itemQuantity">Quantity of the item.</param>
        /// <param name="itemAmount">Total amount for the item.</param>
        /// <param name="shippingAmount">Shipping amount, if applicable.</param>
        /// <param name="orderPromotionsAmount">Promotions amount, if applicable.</param>
        /// <param name="weight">Proportional weight for order-level taxes.</param>
        /// <returns>List of tax adjustments (sales tax, VAT, corrections).</returns>
        private async Task<List<OrderAdjustment>> GetTaxAdjustments(
            Product product, ProductStore productStore, string payToPartyId, string billToPartyId,
            HashSet<TaxAuthority> taxAuthoritySet, decimal itemPrice, decimal? itemQuantity,
            decimal itemAmount, decimal? shippingAmount, decimal orderPromotionsAmount, decimal? weight)
        {
            try
            {
                // Initialize defaults
                // Technical: Sets timestamp, weight, and payToPartyId for tax calculations
                // Business Purpose: Ensures accurate tax application based on current time and store settings
                var now = DateTime.UtcNow;
                weight = weight ?? 1m;
                payToPartyId = payToPartyId ?? productStore?.PayToPartyId;
                var adjustments = new List<OrderAdjustment>();
                const decimal ZERO = 0m;
                const decimal PERCENT_SCALE = 100m;
                const int TAX_SCALE = 2;

                // Exit if no tax authorities
                // Technical: Skips processing if no jurisdictions apply
                // Business Purpose: Optimizes performance for non-taxable scenarios
                if (!taxAuthoritySet.Any()) return adjustments;

                // Get product categories
                // Technical: Queries ProductCategoryMembers for product’s categories
                // Business Purpose: Identifies tax categories for accurate tax rules
                var categoryIds = new HashSet<string>();
                if (product != null)
                {
                    var pcmList = await _dbContext.ProductCategoryMembers
                        .AsNoTracking()
                        .Where(pcm => pcm.ProductId == product.ProductId &&
                                      pcm.FromDate <= now &&
                                      (pcm.ThruDate == null || pcm.ThruDate > now))
                        .Select(pcm => pcm.ProductCategoryId)
                        .ToListAsync();
                    categoryIds.UnionWith(pcmList);

                    if (product.IsVariant == "Y")
                    {
                        var virtualProductId = await _productService.GetVariantVirtualId(product);
                        if (virtualProductId != null)
                        {
                            var virtualPcmList = await _dbContext.ProductCategoryMembers
                                .AsNoTracking()
                                .Where(pcm => pcm.ProductId == virtualProductId &&
                                              pcm.FromDate <= now &&
                                              (pcm.ThruDate == null || pcm.ThruDate > now))
                                .Select(pcm => pcm.ProductCategoryId)
                                .ToListAsync();
                            categoryIds.UnionWith(virtualPcmList);
                        }
                    }
                }

                // Query tax rates
                // Technical: Fetches active tax rates matching store, authorities, and categories
                // Business Purpose: Retrieves tax rules for compliance with regional tax laws
                var storeId = productStore?.ProductStoreId;
                var taxAuthPairs = taxAuthoritySet.Select(ta => new { ta.TaxAuthGeoId, ta.TaxAuthPartyId })
                    .Concat(new[] { new { TaxAuthGeoId = "_NA_", TaxAuthPartyId = "_NA_" } })
                    .ToList();
                var isShippingTax = product == null && shippingAmount != null;
                var isPromotionTax = product == null && orderPromotionsAmount != null;

                // Extract distinct TaxAuthGeoId and TaxAuthPartyId from taxAuthPairs
                var taxAuthGeoIds = taxAuthPairs.Select(ta => ta.TaxAuthGeoId).Distinct().ToList();
                var taxAuthPartyIds = taxAuthPairs.Select(ta => ta.TaxAuthPartyId).Distinct().ToList();

                var taxRates = await _dbContext.TaxAuthorityRateProducts
                    .AsNoTracking()
                    .Where(t => t.FromDate <= now && 
                                (t.ThruDate == null || t.ThruDate > now) &&
                                (storeId == null
                                    ? t.ProductStoreId == null
                                    : t.ProductStoreId == storeId || t.ProductStoreId == null) &&
                                taxAuthGeoIds.Contains(t.TaxAuthGeoId) &&
                                taxAuthPartyIds.Contains(t.TaxAuthPartyId) &&
                                (t.ProductCategoryId == null ||
                                 categoryIds.Contains(t.ProductCategoryId) ||
                                 (isShippingTax && (t.TaxShipping == null || t.TaxShipping == "Y")) ||
                                 (isPromotionTax && (t.TaxPromotions == null || t.TaxPromotions == "Y"))) &&
                                (t.MinItemPrice == null || t.MinItemPrice <= itemPrice) &&
                                (t.MinPurchase == null || t.MinPurchase <= itemAmount))
                    .OrderBy(t => t.MinItemPrice)
                    .ThenBy(t => t.MinPurchase)
                    .ThenBy(t => t.FromDate)
                    .ToListAsync();

                // Exit if no tax rates
                // Technical: Logs warning for missing tax rules
                // Business Purpose: Prevents tax application without valid configuration
                if (!taxRates.Any())
                {
                    _logger.LogWarning("No tax rates found for given conditions");
                    return adjustments;
                }

                // Process tax rates
                foreach (var rate in taxRates)
                {
                    // Calculate taxable amount
                    // Technical: Sums amounts subject to tax based on product, shipping, and promotions
                    // Business Purpose: Determines the base for tax calculation
                    var taxRate = (rate.TaxPercentage ?? ZERO) * weight.Value;
                    var taxable = ZERO;
                    if (product != null && (product.Taxable == null || product.Taxable == "T"))
                        taxable += itemAmount;
                    if (shippingAmount != null && (rate.TaxShipping == null || rate.TaxShipping == "Y"))
                        taxable += shippingAmount.Value;
                    if (orderPromotionsAmount != null && (rate.TaxPromotions == null || rate.TaxPromotions == "Y"))
                        taxable += orderPromotionsAmount;

                    // Skip if no taxable amount
                    if (taxable == ZERO) continue;

                    // Calculate tax
                    // Technical: Applies tax rate to taxable amount with proper rounding
                    // Business Purpose: Computes tax liability for the transaction
                    var taxAmount = (taxable * taxRate / PERCENT_SCALE).Round(TAX_SCALE, RoundingMode.HALF_UP);

                    // Get GL account
                    // Technical: Queries TaxAuthorityGlAccount for accounting
                    // Business Purpose: Ensures tax is recorded to correct ledger account
                    var glAccount = await _dbContext.TaxAuthorityGlAccounts
                        .AsNoTracking()
                        .FirstOrDefaultAsync(taga => taga.TaxAuthPartyId == rate.TaxAuthPartyId &&
                                                     taga.TaxAuthGeoId == rate.TaxAuthGeoId &&
                                                     taga.OrganizationPartyId == payToPartyId);
                    var glAccountId = glAccount?.GlAccountId;

                    // Get product price for VAT
                    // Technical: Retrieves price with tax info for VAT calculations
                    // Business Purpose: Supports VAT-inclusive pricing adjustments
                    ProductPrice productPrice = null;
                    if (product != null && rate.TaxAuthPartyId != null && rate.TaxAuthGeoId != null)
                    {
                        productPrice = await GetProductPrice(product, productStore, rate.TaxAuthGeoId,
                            rate.TaxAuthPartyId);
                        if (productPrice == null && product.IsVariant == "Y")
                        {
                            var virtualProduct = await _productService.GetParentProduct(product.ProductId);
                            if (virtualProduct != null)
                                productPrice = await GetProductPrice(virtualProduct, productStore, rate.TaxAuthGeoId,
                                    rate.TaxAuthPartyId);
                        }
                    }

                    // Create tax adjustment
                    // Technical: Builds OrderAdjustment for tax application
                    // Business Purpose: Records tax details for order processing
                    var taxAdj = new OrderAdjustment
                    {
                        OrderAdjustmentTypeId = "VAT_TAX",
                        Amount = taxAmount,
                        SourcePercentage = taxRate,
                        TaxAuthorityRateSeqId = rate.TaxAuthorityRateSeqId,
                        CorrespondingProductId = product?.ProductId,
                        PrimaryGeoId = rate.TaxAuthGeoId,
                        Comments = rate.Description,
                        TaxAuthPartyId = rate.TaxAuthPartyId,
                        TaxAuthGeoId = rate.TaxAuthGeoId,
                        OverrideGlAccountId = glAccountId
                    };
                    adjustments.Add(taxAdj);

                    // Handle VAT-inclusive pricing
                    // Technical: Adjusts tax for VAT-included prices and discounts
                    // Business Purpose: Ensures accurate VAT reporting for discounted items
                    if (productPrice != null && productPrice.TaxInPrice == "Y" && itemQuantity != 0)
                    {
                        taxAdj.OrderAdjustmentTypeId = "VAT_TAX";
                        var taxAmountIncludedInFullPrice = (itemPrice - (itemPrice / (1 + taxRate / PERCENT_SCALE))).Round(2, RoundingMode.HALF_UP) * itemQuantity.Value;
                        var netItemPrice = itemAmount / itemQuantity.Value;
                        var netTax = (netItemPrice - (netItemPrice / (1 + taxRate / PERCENT_SCALE))).Round(2, RoundingMode.HALF_UP) * itemQuantity.Value;
                        var discountedSalesTax = netTax - taxAmountIncludedInFullPrice;
                        taxAdj.AmountAlreadyIncluded = taxAmountIncludedInFullPrice;
                        taxAdj.Amount = 0;

                        if (discountedSalesTax < 0)
                        {
                            var negativeAdj = new OrderAdjustment
                            {
                                OrderAdjustmentTypeId = taxAdj.OrderAdjustmentTypeId,
                                SourcePercentage = taxRate,
                                TaxAuthorityRateSeqId = rate.TaxAuthorityRateSeqId,
                                PrimaryGeoId = rate.TaxAuthGeoId,
                                Comments = rate.Description,
                                TaxAuthPartyId = rate.TaxAuthPartyId,
                                OverrideGlAccountId = taxAdj?.OverrideGlAccountId,
                                TaxAuthGeoId = rate.TaxAuthGeoId,
                                AmountAlreadyIncluded = discountedSalesTax
                            };
                            adjustments.Add(negativeAdj);
                        }
                    }
                    else
                    {
                        taxAdj.Amount = taxAmount;
                    }
                    
                    // REFACTORED: Added VAT_PRICE_CORRECT adjustments to handle priceWithTax discrepancies, matching OFBiz
                    if (productPrice != null && itemQuantity != null && productPrice.PriceWithTax != null && productPrice.TaxInPrice != "Y")
                    {
                        var priceWithTax = productPrice.PriceWithTax;
                        var price = productPrice.Price;
                        var baseSubtotal = price * itemQuantity.Value;
                        var baseTaxAmount = (baseSubtotal * taxRate / PERCENT_SCALE);
                        var enteredTotalPriceWithTax = priceWithTax * itemQuantity.Value;
                        var calcedTotalPriceWithTax = baseSubtotal + baseTaxAmount;
                        if (enteredTotalPriceWithTax != calcedTotalPriceWithTax)
                        {
                            var correctionAmount = enteredTotalPriceWithTax - calcedTotalPriceWithTax;
                            var correctionAdj = new OrderAdjustment
                            {
                                TaxAuthorityRateSeqId = rate.TaxAuthorityRateSeqId,
                                Amount = correctionAmount,
                                OrderAdjustmentTypeId = "VAT_PRICE_CORRECT",
                                PrimaryGeoId = rate.TaxAuthGeoId,
                                Comments = rate.Description,
                                TaxAuthPartyId = rate.TaxAuthPartyId,
                                OverrideGlAccountId = glAccount?.GlAccountId,
                                TaxAuthGeoId = rate.TaxAuthGeoId
                            };
                            adjustments.Add(correctionAdj);
                        }
                    }

                    
                    // Handle exemptions
                    // Technical: Checks for tax exemptions based on billing party
                    // Business Purpose: Applies exemptions for eligible parties or groups
                    if (!string.IsNullOrEmpty(billToPartyId) && !string.IsNullOrEmpty(rate.TaxAuthGeoId))
                    {
                        var partyIds = new HashSet<string> { billToPartyId };
                        var relationships = await _dbContext.PartyRelationships
                            .AsNoTracking()
                            .Where(pr => pr.PartyIdTo == billToPartyId &&
                                         pr.PartyRelationshipTypeId == "GROUP_ROLLUP" &&
                                         pr.FromDate <= now &&
                                         (pr.ThruDate == null || pr.ThruDate > now))
                            .Select(pr => pr.PartyIdFrom)
                            .ToListAsync();
                        partyIds.UnionWith(relationships);
                        await HandlePartyTaxExempt(taxAdj, partyIds, rate.TaxAuthGeoId, rate.TaxAuthPartyId, taxAmount,
                            now);
                    }
                }

                // Return adjustments
                // Technical: Returns list of computed tax adjustments
                // Business Purpose: Provides tax details for order processing and reporting
                return adjustments;
            }
            catch (Exception ex)
            {
                // Log and return empty list
                // Technical: Logs errors for traceability
                // Business Purpose: Allows processing to continue without taxes
                _logger.LogError(ex, "Error computing tax adjustments");
                return new List<OrderAdjustment>();
            }
        }

        /// <summary>
        /// Retrieves the product price with tax information for a specific tax authority and store.
        /// Supports store group-specific pricing and falls back to general pricing for purchase orders.
        /// </summary>
        /// <param name="product">The product for which to retrieve the price.</param>
        /// <param name="productStore">The product store containing pricing settings.</param>
        /// <param name="taxAuthGeoId">The geo ID of the tax authority.</param>
        /// <param name="taxAuthPartyId">The party ID of the tax authority.</param>
        /// <returns>The product price with tax information, or null if not found.</returns>
        private async Task<ProductPrice> GetProductPrice(Product product, ProductStore productStore,
            string taxAuthGeoId, string taxAuthPartyId)
        {
            try
            {
                // Checks if the store has a primary store group ID, indicating group-specific pricing.
                if (productStore != null && !string.IsNullOrEmpty(productStore.PrimaryStoreGroupId))
                {
                    // Queries the ProductPrice entity for group-specific pricing, filtered by date.
                    return await _dbContext.ProductPrices
                        .AsNoTracking()
                        .Where(pp => pp.ProductId == product.ProductId &&
                                     pp.TaxAuthPartyId == taxAuthPartyId &&
                                     pp.TaxAuthGeoId == taxAuthGeoId &&
                                     pp.ProductPricePurposeId == "PURCHASE" &&
                                     pp.ProductStoreGroupId == productStore.PrimaryStoreGroupId &&
                                     pp.FromDate <= DateTime.UtcNow &&
                                     (pp.ThruDate == null || pp.ThruDate > DateTime.UtcNow))
                        .OrderByDescending(pp => pp.FromDate)
                        .FirstOrDefaultAsync();
                }
                else
                {
                    // Queries the ProductPrice entity for general pricing, used for purchase orders.
                    return await _dbContext.ProductPrices
                        .AsNoTracking()
                        .Where(pp => pp.ProductId == product.ProductId &&
                                     pp.TaxAuthPartyId == taxAuthPartyId &&
                                     pp.TaxAuthGeoId == taxAuthGeoId &&
                                     pp.ProductPricePurposeId == "PURCHASE" &&
                                     pp.FromDate <= DateTime.UtcNow &&
                                     (pp.ThruDate == null || pp.ThruDate > DateTime.UtcNow))
                        .OrderByDescending(pp => pp.FromDate)
                        .FirstOrDefaultAsync();
                }
            }
            catch (Exception e)
            {
                // Logs the exception, ensuring price lookup errors are traceable.
                _logger.LogError(e, $"Error retrieving product price: {e.Message}");
                // Returns null, allowing the caller to handle the absence of a price.
                return null;
            }
        }

        /// <summary>
        /// Determines the product category condition for tax rules, considering product variants.
        /// Returns a condition that matches the product’s categories or null for order-level taxes.
        /// </summary>
        /// <param name="product">The product to check, or null for order-level taxes.</param>
        /// <returns>A function representing the product category condition.</returns>
        private async Task<Func<TaxAuthorityRateProduct, bool>> SetProductCategoryCond(Product product)
        {
            try
            {
                // Checks if no product is provided, indicating an order-level tax (e.g., shipping, promotions).
                if (product == null)
                {
                    // Returns a condition matching null product categories, used for non-product taxes.
                    return t => t.ProductCategoryId == null;
                }

                // Initializes a variable to store the virtual product ID for variants.
                string virtualProductId = null;
                // Checks if the product is a variant, requiring the virtual product’s categories.
                if (product.IsVariant == "Y")
                {
                    // Retrieves the virtual product ID for the variant, linking to the parent product.
                    virtualProductId = await _productService.GetVariantVirtualId(product);
                }

                // Initializes a set to store product category IDs, ensuring no duplicates.
                HashSet<string> productCategoryIdSet = new HashSet<string>();
                // Defines a condition for product categories, including the product and its virtual parent if applicable.
                IQueryable<ProductCategoryMember> productIdQuery;
                if (virtualProductId != null)
                {
                    // Queries categories for both the product and its virtual parent, covering all relevant categories.
                    productIdQuery = _dbContext.ProductCategoryMembers
                        .AsNoTracking()
                        .Where(pcm => pcm.ProductId == product.ProductId || pcm.ProductId == virtualProductId);
                }
                else
                {
                    // Queries categories for the product only, as it’s not a variant.
                    productIdQuery = _dbContext.ProductCategoryMembers
                        .AsNoTracking()
                        .Where(pcm => pcm.ProductId == product.ProductId);
                }

                // Filters categories by date, ensuring only active memberships are included.
                var pcmList = await productIdQuery
                    .Where(pcm => pcm.FromDate <= DateTime.UtcNow &&
                                  (pcm.ThruDate == null || pcm.ThruDate > DateTime.UtcNow))
                    .Select(pcm => new { pcm.ProductCategoryId })
                    .ToListAsync();
                // Adds all category IDs to the set, representing the product’s tax categories.
                foreach (var pcm in pcmList)
                {
                    productCategoryIdSet.Add(pcm.ProductCategoryId);
                }

                // Checks if no categories were found, indicating the product has no specific tax categories.
                if (productCategoryIdSet.Count == 0)
                {
                    // Returns a condition matching null product categories, allowing general tax rules.
                    return t => t.ProductCategoryId == null;
                }

                // Returns a condition matching either null categories or the product’s categories, enabling flexible tax rules.
                return t => t.ProductCategoryId == null || productCategoryIdSet.Contains(t.ProductCategoryId);
            }
            catch (Exception e)
            {
                // Logs the exception, ensuring category lookup errors are traceable.
                _logger.LogError(e, $"Error setting product category condition: {e.Message}");
                // Returns a default condition to allow processing to continue, matching null categories.
                return t => t.ProductCategoryId == null;
            }
        }

        /// <summary>
        /// Checks if a party is tax-exempt in a jurisdiction and adjusts the tax amount accordingly.
        /// Supports group-based exemptions and inherited exemptions from parent jurisdictions.
        /// </summary>
        /// <param name="adjValue">The tax adjustment to modify with exemption details.</param>
        /// <param name="billToPartyIdSet">The set of party IDs to check for exemptions.</param>
        /// <param name="taxAuthGeoId">The geo ID of the tax authority.</param>
        /// <param name="taxAuthPartyId">The party ID of the tax authority.</param>
        /// <param name="taxAmount">The calculated tax amount before exemptions.</param>
        /// <param name="nowTimestamp">The current timestamp for date filtering.</param>
        private async Task HandlePartyTaxExempt(OrderAdjustment adjValue, HashSet<string> billToPartyIdSet,
            string taxAuthGeoId, string taxAuthPartyId, decimal taxAmount, DateTime nowTimestamp)
        {
            try
            {
                // Logs an informational message indicating an exemption check, aiding debugging.
                _logger.LogInformation($"Checking for tax exemption : {taxAuthGeoId} / {taxAuthPartyId}");
                // Queries the PartyTaxAuthInfo entity to check for exemptions, filtering by party, authority, and date.
                PartyTaxAuthInfo partyTaxInfo = await _dbContext.PartyTaxAuthInfos
                    .AsNoTracking()
                    .Where(pti => billToPartyIdSet.Contains(pti.PartyId) &&
                                  pti.TaxAuthGeoId == taxAuthGeoId &&
                                  pti.TaxAuthPartyId == taxAuthPartyId &&
                                  pti.FromDate <= nowTimestamp &&
                                  (pti.ThruDate == null || pti.ThruDate > nowTimestamp))
                    .OrderByDescending(pti => pti.FromDate)
                    .FirstOrDefaultAsync();

                // Initializes a flag to track if an exemption was found, controlling recursive checks.
                bool foundExemption = false;
                // Checks if party tax info was found, indicating potential exemption or tax ID details.
                if (partyTaxInfo != null)
                {
                    // Sets the customer reference ID to the party’s tax ID, used for tax reporting.
                    adjValue.CustomerReferenceId = partyTaxInfo.PartyTaxId;
                    // Checks if the party is explicitly exempt in this jurisdiction.
                    if (partyTaxInfo.IsExempt == "Y")
                    {
                        // Sets the adjustment amount to zero, indicating no tax is applied.
                        adjValue.Amount = ZERO_BASE;
                        // Sets the exempt amount to the original tax amount, tracking the exempted tax.
                        adjValue.ExemptAmount = taxAmount;
                        // Marks that an exemption was found, preventing further checks.
                        foundExemption = true;
                    }
                }

                // If no exemption was found, checks for inherited exemptions from parent jurisdictions.
                if (!foundExemption)
                {
                    // Queries the TaxAuthorityAssoc entity for a parent tax authority with inherited exemptions.
                    TaxAuthorityAssoc taxAuthorityAssoc = await _dbContext.TaxAuthorityAssocs
                        .AsNoTracking()
                        .Where(taa => taa.ToTaxAuthGeoId == taxAuthGeoId &&
                                      taa.ToTaxAuthPartyId == taxAuthPartyId &&
                                      taa.TaxAuthorityAssocTypeId == "EXEMPT_INHER" &&
                                      taa.FromDate <= nowTimestamp &&
                                      (taa.ThruDate == null || taa.ThruDate > nowTimestamp))
                        .OrderByDescending(taa => taa.FromDate)
                        .FirstOrDefaultAsync();
                    // If a parent tax authority is found, recursively checks for exemptions.
                    if (taxAuthorityAssoc != null)
                    {
                        // Recursively calls the method for the parent jurisdiction, propagating exemptions.
                        await HandlePartyTaxExempt(adjValue, billToPartyIdSet, taxAuthorityAssoc.TaxAuthGeoId,
                            taxAuthorityAssoc.TaxAuthPartyId, taxAmount, nowTimestamp);
                    }
                }
            }
            catch (Exception e)
            {
                // Logs the exception, ensuring exemption check errors are traceable.
                _logger.LogError(e, $"Error checking party tax exemption: {e.Message}");
                // Continues processing without modifying the adjustment, preserving the original tax.
            }
        }

        /// <summary>
        /// Retrieves the postal code geo ID for a postal address, used for precise tax jurisdiction mapping.
        /// Stub implementation, as OFBiz’s ContactMechWorker logic is not directly translatable.
        /// </summary>
        /// <param name="address">The postal address to process.</param>
        /// <returns>The postal code geo ID, or null if not implemented.</returns>
        private async Task<string> GetPostalAddressPostalCodeGeoId(PostalAddress address)
        {
            // Logs a warning indicating the method is not implemented, as it requires OFBiz-specific logic.
            _logger.LogWarning("GetPostalAddressPostalCodeGeoId is not implemented. Returning null.");
            // Returns null, as the geo ID mapping depends on external data or services.
            return null;
            // TODO: Implement logic to map postal codes to geo IDs using a geo service or database.
        }
    }


    /// <summary>
    /// Defines the rounding mode for financial calculations, matching OFBiz’s UtilNumber.
    /// </summary>
    public enum RoundingMode
    {
        HALF_UP
    }

    /// <summary>
    /// Provides extension methods for decimal rounding, matching OFBiz’s UtilNumber.
    /// </summary>
    public static class DecimalExtensions
    {
        // Rounds a decimal value to the specified number of decimals using the given mode.
        public static decimal Round(this decimal value, int decimals, RoundingMode mode)
        {
            // Applies HALF_UP rounding, equivalent to OFBiz’s rounding mode for financial calculations.
            return mode == RoundingMode.HALF_UP
                ? Math.Round(value, decimals, MidpointRounding.AwayFromZero)
                : Math.Round(value, decimals);
        }
    }
}