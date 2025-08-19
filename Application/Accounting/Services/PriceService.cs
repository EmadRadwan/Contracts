using System.Text;
using Application.Accounting.Services.Models;
using Application.Catalog.Products;
using Application.Common;
using Application.Core;
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Accounting.Services;

public interface IPriceService
{
    Task<ProductPriceResult> CalculateProductPrice(
        Product product, // required
        string? prodCatalogId = null,
        string? webSiteId = null,
        string? partyId = null,
        string? productStoreId = null,
        string? productStoreGroupId = null,
        string? agreementId = null,
        decimal? quantity = null,
        decimal? amount = null,
        string? currencyUomId = null,
        string? currencyUomIdTo = null,
        string? productPricePurposeId = null, // defaults to "PURCHASE"
        string? termUomId = null,
        UserLogin? userLogin = null,
        UserLogin? autoUserLogin = null,
        string? checkIncludeVat = null,       // can be "Y" or "N"
        string? findAllQuantityPrices = null, // can be "Y" or "N"
        string? surveyResponseId = null,
        string? optimizeForLargeRuleSet = null // can be "Y" or "N"
    );
}

public class PriceService : IPriceService
{
    private readonly DataContext _context;
    private readonly ITaxService _taxService;
    private readonly ICommonService _commonService;
    private readonly IProductService _productService;
    private readonly ILogger<PriceService> _logger;

    public PriceService(DataContext context, ICommonService commonService,
        ILogger<PriceService> logger, IProductService productService, ITaxService taxService)
    {
        _context = context;
        _productService = productService;
        _commonService = commonService;
        _taxService = taxService;
        _logger = logger;
    }

    public async Task<ProductPriceResult> CalculateProductPrice(
        Product product, // required
        string? prodCatalogId = null,
        string? webSiteId = null,
        string? partyId = null,
        string? productStoreId = null,
        string? productStoreGroupId = null,
        string? agreementId = null,
        decimal? quantity = null,
        decimal? amount = null,
        string? currencyUomId = null,
        string? currencyUomIdTo = null,
        string? productPricePurposeId = null, // defaults to "PURCHASE"
        string? termUomId = null,
        UserLogin? userLogin = null,
        UserLogin? autoUserLogin = null,
        string? checkIncludeVat = null,       // can be "Y" or "N"
        string? findAllQuantityPrices = null, // can be "Y" or "N"
        string? surveyResponseId = null,
        string? optimizeForLargeRuleSet = null // can be "Y" or "N"
    )
    {
        // Create the result DTO that will hold all output pricing information.
        var result = new ProductPriceResult();
        try
        {
            // -----------------------------------------------------------------
            // 1. INITIALIZATION & CONTEXT EXTRACTION
            // -----------------------------------------------------------------
            // Get the current timestamp to filter date-effective pricing records.
            DateTime nowTimestamp = DateTime.UtcNow;
            // Get the productId from the product entity.
            string productId = product.ProductId;
            // Log: The productId is the identifier for the product whose price is being calculated.

            // Convert the flag "findAllQuantityPrices" to a boolean.
            bool findAllQuantityPricesBool = (findAllQuantityPrices == "Y");
            // Convert the flag "optimizeForLargeRuleSet" to a boolean.
            bool optimizeForLargeRuleSetBool = (optimizeForLargeRuleSet == "Y");

            // -----------------------------------------------------------------
            // 2. RETRIEVING PRODUCT STORE AND DETERMINING STORE GROUP
            // -----------------------------------------------------------------
            ProductStore productStore = null;
            try
            {
                // Retrieve the ProductStore entity for the given productStoreId using EF LINQ.
                productStore =
                    await _context.ProductStores.FirstOrDefaultAsync(ps => ps.ProductStoreId == productStoreId);
                // Business note: The product store contains default settings (such as currency) needed for price calculation.
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error getting product store info from the database while calculating price: " + ex.ToString());
                throw;
            }

            // If productStoreGroupId is not provided, determine it from the ProductStore or ProductStoreGroupMember.
            if (string.IsNullOrEmpty(productStoreGroupId))
            {
                if (productStore != null)
                {
                    try
                    {
                        // If the ProductStore has a primaryStoreGroupId, use it.
                        if (!string.IsNullOrEmpty(productStore.PrimaryStoreGroupId))
                        {
                            productStoreGroupId = productStore.PrimaryStoreGroupId;
                        }
                        else
                        {
                            // Otherwise, query the ProductStoreGroupMember records by productStoreId, ordered by sequenceNum descending.
                            var productStoreGroupMemberList = await _context.ProductStoreGroupMembers
                                .Where(psgm => psgm.ProductStoreId == productStoreId)
                                .OrderByDescending(psgm => psgm.SequenceNum)
                                .ToListAsync();
                            // Filter the list by checking if the record is effective at nowTimestamp.
                            productStoreGroupMemberList = productStoreGroupMemberList
                                .Where(psgm =>
                                    psgm.FromDate <= nowTimestamp &&
                                    (psgm.ThruDate == null || psgm.ThruDate >= nowTimestamp))
                                .ToList();
                            if (productStoreGroupMemberList.Count > 0)
                            {
                                // Use the first valid record.
                                var productStoreGroupMember = productStoreGroupMemberList.First();
                                productStoreGroupId = productStoreGroupMember.ProductStoreGroupId;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex,
                            "Error getting product store group info from the database while calculating price: " +
                            ex.ToString());
                        throw;
                    }
                }

                // If still not set, default to "_NA_" to indicate not applicable.
                if (string.IsNullOrEmpty(productStoreGroupId))
                {
                    productStoreGroupId = "_NA_";
                }
            }

            // -----------------------------------------------------------------
            // 3. DETERMINING THE CURRENCY
            // -----------------------------------------------------------------
            // If currencyUomId is not provided, try to obtain it from the product store; otherwise, default to "USD".
            string currencyDefaultUomId = currencyUomId;
            if (string.IsNullOrEmpty(currencyDefaultUomId))
            {
                if (productStore != null && !string.IsNullOrEmpty(productStore.DefaultCurrencyUomId))
                {
                    currencyDefaultUomId = productStore.DefaultCurrencyUomId;
                }
                else
                {
                    currencyDefaultUomId = "USD";
                }
            }
            // Business note: Currency is a critical factor in price calculations.

            // -----------------------------------------------------------------
            // 4. SETTING DEFAULT PRICE PURPOSE
            // -----------------------------------------------------------------
            // If productPricePurposeId is not provided, default to "PURCHASE".
            if (string.IsNullOrEmpty(productPricePurposeId))
            {
                productPricePurposeId = "PURCHASE";
            }
            // termUomId remains as provided (used for recurring or term-based pricing)

            // -----------------------------------------------------------------
            // 5. HANDLING PRODUCT VARIANTS & VIRTUAL PRODUCTS
            // -----------------------------------------------------------------
            // If the product is a variant (flag "Y"), retrieve its virtual product ID.
            string virtualProductId = null;
            if (product.IsVariant == "Y")
            {
                try
                {
                    // Retrieve virtual product id via the ProductWorker.
                    virtualProductId = await _productService.GetVariantVirtualId(product);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Error getting virtual product id from the database while calculating price: " + ex.ToString());
                    throw;
                }
            }

            // If a virtual product exists, retrieve its pricing information.
            List<ProductPrice> virtualProductPrices = null;
            if (!string.IsNullOrEmpty(virtualProductId))
            {
                try
                {
                    virtualProductPrices = await _context.ProductPrices
                        .Where(pp => pp.ProductId == virtualProductId &&
                                     pp.CurrencyUomId == currencyDefaultUomId &&
                                     pp.ProductStoreGroupId == productStoreGroupId &&
                                     pp.FromDate <= nowTimestamp &&
                                     (pp.ThruDate == null || pp.ThruDate >= nowTimestamp))
                        .OrderByDescending(pp => pp.FromDate)
                        .ToListAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "An error occurred while getting the virtual product prices: " + ex.ToString());
                    // Continue with virtualProductPrices as null if an error occurs.
                }
            }

            // -----------------------------------------------------------------
            // 6. RESOLVING THE PARTY ID
            // -----------------------------------------------------------------
            // If partyId is not provided, attempt to obtain it from userLogin or autoUserLogin.
            if (string.IsNullOrEmpty(partyId))
            {
                if (userLogin != null)
                {
                    partyId = userLogin.PartyId;
                }
                else if (autoUserLogin != null)
                {
                    partyId = autoUserLogin.PartyId;
                }
            }

            // Ensure quantity has a value; default to 1 if not provided.
            if (!quantity.HasValue)
            {
                quantity = 1;
            }

            // -----------------------------------------------------------------
            // 7. BUILDING THE PRODUCT PRICE QUERY
            // -----------------------------------------------------------------
            // Construct a LINQ query to retrieve ProductPrice records matching the criteria.
            IQueryable<ProductPrice> productPriceQuery = _context.ProductPrices
                .Where(pp => pp.ProductId == productId &&
                             pp.CurrencyUomId == currencyDefaultUomId &&
                             pp.ProductStoreGroupId == productStoreGroupId &&
                             pp.FromDate <= nowTimestamp &&
                             (pp.ThruDate == null || pp.ThruDate >= nowTimestamp));

            // Add condition for productPricePurposeId: if "PURCHASE", allow records where the field is null.
            if (productPricePurposeId == "PURCHASE")
            {
                productPriceQuery = productPriceQuery.Where(pp =>
                    pp.ProductPricePurposeId == productPricePurposeId || pp.ProductPricePurposeId == null);
            }
            else
            {
                productPriceQuery = productPriceQuery.Where(pp => pp.ProductPricePurposeId == productPricePurposeId);
            }

            // If termUomId is provided, add it to the filter.
            if (!string.IsNullOrEmpty(termUomId))
            {
                productPriceQuery = productPriceQuery.Where(pp => pp.TermUomId == termUomId);
            }

            // Order the results by FromDate descending.
            List<ProductPrice> productPrices = new List<ProductPrice>();
            try
            {
                productPrices = await productPriceQuery.OrderByDescending(pp => pp.FromDate).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting the product prices: " + ex.ToString());
            }

            // -----------------------------------------------------------------
            // 8. RETRIEVING SPECIFIC PRICE TYPES (INLINE "getPriceValueForType" LOGIC)
            // -----------------------------------------------------------------
            // For each price type, attempt to get the value from the productPrices list;
            // if not found, try the virtualProductPrices.
            ProductPrice listPriceValue = productPrices.FirstOrDefault(pp => pp.ProductPriceTypeId == "LIST_PRICE")
                                          ?? (virtualProductPrices != null
                                              ? virtualProductPrices.FirstOrDefault(pp =>
                                                  pp.ProductPriceTypeId == "LIST_PRICE")
                                              : null);
            ProductPrice defaultPriceValue =
                productPrices.FirstOrDefault(pp => pp.ProductPriceTypeId == "DEFAULT_PRICE")
                ?? (virtualProductPrices != null
                    ? virtualProductPrices.FirstOrDefault(pp => pp.ProductPriceTypeId == "DEFAULT_PRICE")
                    : null);
            ProductPrice competitivePriceValue =
                productPrices.FirstOrDefault(pp => pp.ProductPriceTypeId == "COMPETITIVE_PRICE")
                ?? (virtualProductPrices != null
                    ? virtualProductPrices.FirstOrDefault(pp => pp.ProductPriceTypeId == "COMPETITIVE_PRICE")
                    : null);
            ProductPrice averageCostValue = productPrices.FirstOrDefault(pp => pp.ProductPriceTypeId == "AVERAGE_COST")
                                            ?? (virtualProductPrices != null
                                                ? virtualProductPrices.FirstOrDefault(pp =>
                                                    pp.ProductPriceTypeId == "AVERAGE_COST")
                                                : null);
            ProductPrice promoPriceValue = productPrices.FirstOrDefault(pp => pp.ProductPriceTypeId == "PROMO_PRICE")
                                           ?? (virtualProductPrices != null
                                               ? virtualProductPrices.FirstOrDefault(pp =>
                                                   pp.ProductPriceTypeId == "PROMO_PRICE")
                                               : null);
            ProductPrice minimumPriceValue =
                productPrices.FirstOrDefault(pp => pp.ProductPriceTypeId == "MINIMUM_PRICE")
                ?? (virtualProductPrices != null
                    ? virtualProductPrices.FirstOrDefault(pp => pp.ProductPriceTypeId == "MINIMUM_PRICE")
                    : null);
            ProductPrice maximumPriceValue =
                productPrices.FirstOrDefault(pp => pp.ProductPriceTypeId == "MAXIMUM_PRICE")
                ?? (virtualProductPrices != null
                    ? virtualProductPrices.FirstOrDefault(pp => pp.ProductPriceTypeId == "MAXIMUM_PRICE")
                    : null);
            ProductPrice wholesalePriceValue =
                productPrices.FirstOrDefault(pp => pp.ProductPriceTypeId == "WHOLESALE_PRICE")
                ?? (virtualProductPrices != null
                    ? virtualProductPrices.FirstOrDefault(pp => pp.ProductPriceTypeId == "WHOLESALE_PRICE")
                    : null);
            ProductPrice specialPromoPriceValue =
                productPrices.FirstOrDefault(pp => pp.ProductPriceTypeId == "SPECIAL_PROMO_PRICE")
                ?? (virtualProductPrices != null
                    ? virtualProductPrices.FirstOrDefault(pp => pp.ProductPriceTypeId == "SPECIAL_PROMO_PRICE")
                    : null);
            // Business note: These inline selections mimic the helper getPriceValueForType to extract each required price.

            // -----------------------------------------------------------------
            // 9. AGREEMENT OVERRIDE: OVERRIDING DEFAULT PRICE IF AGREEMENT EXISTS
            // -----------------------------------------------------------------
            if (!string.IsNullOrEmpty(agreementId))
            {
                try
                {
                    // Retrieve an AgreementItemAndProductAppl record if one exists.
                    var agreementPriceValue = await (from agi in _context.AgreementItems
                        join agpa in _context.AgreementProductAppls
                            on new { agi.AgreementId, agi.AgreementItemSeqId }
                            equals new { agpa.AgreementId, agpa.AgreementItemSeqId }
                        join ag in _context.Agreements
                            on agi.AgreementId equals ag.AgreementId
                        where agi.AgreementId == agreementId &&
                              agpa.ProductId == productId &&
                              agi.CurrencyUomId == currencyDefaultUomId
                        select new
                        {
                            Price = agpa.Price
                        }).FirstOrDefaultAsync();

                    if (agreementPriceValue != null && agreementPriceValue.Price.HasValue)
                    {
                        defaultPriceValue.Price = agreementPriceValue.Price;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Error getting agreement info from the database while calculating price: " + ex.ToString());
                    throw;
                }
            }


            // -----------------------------------------------------------------
            // 10. HANDLING VIRTUAL PRODUCTS: FALLBACK FOR VARIANTS
            // -----------------------------------------------------------------
            if (product.IsVirtual == "Y")
            {
                if (defaultPriceValue == null)
                {
                    try
                    {
                        // Query for variant associations where the product is associated with its variants.
                        var variantAssocList = await _context.ProductAssocs
                            .Where(pa =>
                                pa.ProductId == product.ProductId && pa.ProductAssocTypeId == "PRODUCT_VARIANT")
                            .OrderByDescending(pa => pa.FromDate)
                            .ToListAsync();
                        decimal? minDefaultPrice = null;
                        List<ProductPrice> variantProductPrices = null;
                        // Iterate through each variant association.
                        foreach (var variantAssoc in variantAssocList)
                        {
                            string curVariantProductId = variantAssoc.ProductIdTo;
                            // Retrieve the pricing list for the variant product.
                            var curVariantPriceList = await _context.ProductPrices
                                .Where(pp => pp.ProductId == curVariantProductId &&
                                             pp.FromDate <= nowTimestamp &&
                                             (pp.ThruDate == null || pp.ThruDate >= nowTimestamp))
                                .OrderByDescending(pp => pp.FromDate)
                                .ToListAsync();
                            // Filter for DEFAULT_PRICE records.
                            var tempDefaultPrice =
                                curVariantPriceList.FirstOrDefault(pp => pp.ProductPriceTypeId == "DEFAULT_PRICE");
                            if (tempDefaultPrice != null)
                            {
                                decimal curDefaultPrice = tempDefaultPrice.Price.Value;
                                if (!minDefaultPrice.HasValue || curDefaultPrice < minDefaultPrice)
                                {
                                    // Check if the variant product is not discontinued for sale.
                                    var curVariantProduct =
                                        await _context.Products.FirstOrDefaultAsync(p =>
                                            p.ProductId == curVariantProductId);
                                    if (curVariantProduct != null)
                                    {
                                        DateTime? salesDiscontinuationDate = curVariantProduct.SalesDiscontinuationDate;
                                        if (!salesDiscontinuationDate.HasValue ||
                                            salesDiscontinuationDate.Value > nowTimestamp)
                                        {
                                            minDefaultPrice = curDefaultPrice;
                                            variantProductPrices = curVariantPriceList;
                                        }
                                    }
                                }
                            }
                        }

                        if (variantProductPrices != null)
                        {
                            // For each price type, if not already set, try to retrieve it from the variant prices.
                            if (listPriceValue == null)
                                listPriceValue =
                                    variantProductPrices.FirstOrDefault(pp => pp.ProductPriceTypeId == "LIST_PRICE");
                            if (competitivePriceValue == null)
                                competitivePriceValue =
                                    variantProductPrices.FirstOrDefault(pp =>
                                        pp.ProductPriceTypeId == "COMPETITIVE_PRICE");
                            if (averageCostValue == null)
                                averageCostValue =
                                    variantProductPrices.FirstOrDefault(pp => pp.ProductPriceTypeId == "AVERAGE_COST");
                            if (promoPriceValue == null)
                                promoPriceValue =
                                    variantProductPrices.FirstOrDefault(pp => pp.ProductPriceTypeId == "PROMO_PRICE");
                            if (minimumPriceValue == null)
                                minimumPriceValue =
                                    variantProductPrices.FirstOrDefault(pp => pp.ProductPriceTypeId == "MINIMUM_PRICE");
                            if (maximumPriceValue == null)
                                maximumPriceValue =
                                    variantProductPrices.FirstOrDefault(pp => pp.ProductPriceTypeId == "MAXIMUM_PRICE");
                            if (wholesalePriceValue == null)
                                wholesalePriceValue =
                                    variantProductPrices.FirstOrDefault(
                                        pp => pp.ProductPriceTypeId == "WHOLESALE_PRICE");
                            if (specialPromoPriceValue == null)
                                specialPromoPriceValue =
                                    variantProductPrices.FirstOrDefault(pp =>
                                        pp.ProductPriceTypeId == "SPECIAL_PROMO_PRICE");
                            defaultPriceValue =
                                variantProductPrices.FirstOrDefault(pp => pp.ProductPriceTypeId == "DEFAULT_PRICE");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex,
                            "An error occurred while getting variant prices from the database while calculating price: " +
                            ex.ToString());
                        // Continue even if variant pricing is not found.
                    }
                }
            }

            // -----------------------------------------------------------------
            // 11. SETTING PROMO AND WHOLESALE PRICES
            // -----------------------------------------------------------------
            decimal promoPrice = (promoPriceValue != null && promoPriceValue.Price.HasValue)
                ? promoPriceValue.Price.Value
                : 0;
            decimal wholesalePrice = (wholesalePriceValue != null && wholesalePriceValue.Price.HasValue)
                ? wholesalePriceValue.Price.Value
                : 0;

            // Initialize flags and accumulators.
            bool validPriceFound = false;
            decimal defaultPrice = 0;
            List<OrderItemPriceInfo> orderItemPriceInfos = new List<OrderItemPriceInfo>();

            // -----------------------------------------------------------------
            // 12. CUSTOM PRICE CALCULATION LOGIC
            // -----------------------------------------------------------------
            /*if (defaultPriceValue != null)
            {
                // Check if a custom price calculation service is defined on the default price record.
                if (defaultPriceValue.EntityName == "ProductPrice" &&
                    !string.IsNullOrEmpty(defaultPriceValue.CustomPriceCalcService))
                {
                    CustomMethod customMethod = null;
                    try
                    {
                        // Retrieve the related CustomMethod (assumed available as a navigation property).
                        customMethod = defaultPriceValue.CustomMethod;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex,
                            "An error occurred while retrieving the custom price calc service: " + ex.ToString());
                    }

                    if (customMethod != null && !string.IsNullOrEmpty(customMethod.CustomMethodName))
                    {
                        // Build the input for the custom price calculation.
                        var inMap = new CustomPriceCalcInput
                        {
                            UserLogin = userLogin,
                            Product = product,
                            InitialPrice = defaultPriceValue.Price,
                            CurrencyUomId = currencyDefaultUomId,
                            Quantity = quantity,
                            Amount = amount,
                            SurveyResponseId = surveyResponseId,
                            // Include customAttributes if available
                            CustomAttributes = customAttributes
                        };

                        try
                        {
                            // Call the custom service (simulate a synchronous call).
                            var outMap = RunSync(customMethod.CustomMethodName, inMap);
                            // If the service call is successful, use the returned price and order item price infos.
                            if (outMap != null && outMap.IsSuccess)
                            {
                                decimal? calculatedDefaultPrice = outMap.Price;
                                orderItemPriceInfos = outMap.OrderItemPriceInfos;
                                if (calculatedDefaultPrice.HasValue)
                                {
                                    defaultPrice = calculatedDefaultPrice.Value;
                                    validPriceFound = true;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "An error occurred while running the custom price calc service [" +
                                                 customMethod.CustomMethodName + "]: " + ex.ToString());
                        }
                    }
                }

                // If no custom calculation succeeded, use the default price from the record.
                if (!validPriceFound && defaultPriceValue.Price.HasValue)
                {
                    defaultPrice = defaultPriceValue.Price.Value;
                    validPriceFound = true;
                }
            }
            */

            // -----------------------------------------------------------------
            // 13. DETERMINING THE FINAL PRICE WHEN NO LIST PRICE EXISTS
            // -----------------------------------------------------------------
            // Get listPrice from the listPriceValue if available.
            decimal? listPrice = listPriceValue != null ? listPriceValue.Price : (decimal?)null;
            if (!listPrice.HasValue)
            {
                // If no list price, then adjust the default price within allowed maximum/minimum bounds.
                decimal? maxSellPrice = maximumPriceValue != null ? maximumPriceValue.Price : (decimal?)null;
                if (maxSellPrice.HasValue && defaultPrice > maxSellPrice.Value)
                {
                    defaultPrice = maxSellPrice.Value;
                }

                decimal? minSellPrice = minimumPriceValue != null ? minimumPriceValue.Price : (decimal?)null;
                if (minSellPrice.HasValue && defaultPrice < minSellPrice.Value)
                {
                    defaultPrice = minSellPrice.Value;
                    validPriceFound = true;
                }

                // Set the result fields based on the computed default price.
                result.BasePrice = defaultPrice;
                result.Price = defaultPrice;
                result.DefaultPrice = defaultPrice;
                result.CompetitivePrice = competitivePriceValue != null ? competitivePriceValue.Price : (decimal?)null;
                result.AverageCost = averageCostValue != null ? averageCostValue.Price : (decimal?)null;
                result.PromoPrice = promoPriceValue != null ? promoPriceValue.Price : (decimal?)null;
                result.SpecialPromoPrice =
                    specialPromoPriceValue != null ? specialPromoPriceValue.Price : (decimal?)null;
                result.ValidPriceFound = validPriceFound;
                result.IsSale = false; // Business rule: No sale is indicated here.
                result.OrderItemPriceInfos = orderItemPriceInfos;

                // Call addGeneralResults to further adjust the result if necessary.
                var errorResult = await AddGeneralResults(competitivePriceValue, specialPromoPriceValue, productStore,
                    checkIncludeVat, currencyDefaultUomId, productId, (decimal)quantity, partyId);
                if (errorResult != null)
                {
                    return errorResult;
                }
            }
            else
            {
                // -----------------------------------------------------------------
                // 14. HANDLING QUANTITY-BASED PRICING RULES
                // -----------------------------------------------------------------
                try
                {
                    // Retrieve all product price rules applicable to the product or its virtual equivalent.
                    List<ProductPriceRule> allProductPriceRules = await _context.ProductPriceRules
                        .Where(rule => rule.ProductPriceRuleId == productId)
                        .ToListAsync();
                    // Filter the rules based on effective date.
                    allProductPriceRules = allProductPriceRules
                        .Where(rule =>
                            rule.FromDate <= nowTimestamp && (rule.ThruDate == null || rule.ThruDate >= nowTimestamp))
                        .ToList();

                    List<ProductPriceRule> quantityProductPriceRules = null;
                    List<ProductPriceRule> nonQuantityProductPriceRules = null;

                    if (findAllQuantityPricesBool)
                    {
                        quantityProductPriceRules = new List<ProductPriceRule>();
                        nonQuantityProductPriceRules = new List<ProductPriceRule>();
                        // Iterate over each pricing rule to split those with a quantity condition.
                        foreach (var productPriceRule in allProductPriceRules)
                        {
                            // Retrieve the pricing conditions for the rule.
                            var productPriceCondList = await _context.ProductPriceConds
                                .Where(cond => cond.ProductPriceRuleId == productPriceRule.ProductPriceRuleId)
                                .ToListAsync();
                            bool foundQuantityInputParam = false;
                            bool allExceptQuantTrue = true;
                            foreach (var productPriceCond in productPriceCondList)
                            {
                                if (productPriceCond.InputParamEnumId == "PRIP_QUANTITY")
                                {
                                    foundQuantityInputParam = true;
                                }
                                else
                                {
                                    // Evaluate the condition (simulate inline CheckPriceCondition).
                                    var result1 = await CheckPriceCondition(productPriceCond, productId,
                                        virtualProductId,
                                        prodCatalogId, productStoreGroupId, webSiteId, partyId, quantity.Value,
                                        (decimal)listPrice, currencyDefaultUomId, nowTimestamp);
                                    if (!result1)
                                    {
                                        allExceptQuantTrue = false;
                                    }
                                }
                            }

                            if (foundQuantityInputParam && allExceptQuantTrue)
                            {
                                quantityProductPriceRules.Add(productPriceRule);
                            }
                            else
                            {
                                nonQuantityProductPriceRules.Add(productPriceRule);
                            }
                        }
                    }

                    if (findAllQuantityPricesBool)
                    {
                        // Process each quantity-specific pricing rule.
                        List<ProductPriceResult> allQuantityPrices = new List<ProductPriceResult>();
                        foreach (var quantityProductPriceRule in quantityProductPriceRules)
                        {
                            // Combine the quantity rule with non-quantity rules.
                            List<ProductPriceRule> ruleListToUse = new List<ProductPriceRule>();
                            ruleListToUse.Add(quantityProductPriceRule);
                            ruleListToUse.AddRange(nonQuantityProductPriceRules);

                            // Calculate pricing based on the combined rules.
                            var quantCalcResults = await CalcPriceResultFromRules(ruleListToUse, (decimal)listPrice,
                                defaultPrice,
                                promoPrice, wholesalePrice, maximumPriceValue, minimumPriceValue, validPriceFound,
                                averageCostValue, productId, virtualProductId, prodCatalogId, productStoreGroupId,
                                webSiteId, partyId, null, currencyDefaultUomId, nowTimestamp);

                            var quantErrorResult = await AddGeneralResults(competitivePriceValue,
                                specialPromoPriceValue, productStore, checkIncludeVat, currencyDefaultUomId, productId,
                                (decimal)quantity, partyId);

                            if (quantErrorResult != null)
                            {
                                return quantErrorResult;
                            }

                            // Map the PriceCalcResult to a ProductPriceResult.
                            ProductPriceResult quantCalcResultsMapped = new ProductPriceResult
                            {
                                BasePrice = quantCalcResults.BasePrice,
                                Price = quantCalcResults.Price,
                                ListPrice = quantCalcResults.ListPrice,
                                DefaultPrice = quantCalcResults.DefaultPrice,
                                AverageCost = quantCalcResults.AverageCost,
                                IsSale = quantCalcResults.IsSale,
                                ValidPriceFound = quantCalcResults.ValidPriceFound,
                                OrderItemPriceInfos = quantCalcResults.OrderItemPriceInfos
                            };

                            allQuantityPrices.Add(quantCalcResultsMapped);
                        }

                        result.AllQuantityPrices = allQuantityPrices;

                        // For the main price, use a quantity of 1.
                        var calcResults = await CalcPriceResultFromRules(allProductPriceRules, (decimal)listPrice,
                            defaultPrice,
                            promoPrice, wholesalePrice, maximumPriceValue, minimumPriceValue, validPriceFound,
                            averageCostValue, productId, virtualProductId, prodCatalogId, productStoreGroupId,
                            webSiteId, partyId, 1, currencyDefaultUomId, nowTimestamp);

                        // Map the returned PriceCalcResult to a ProductPriceResult.
                        ProductPriceResult calcResultsMapped = new ProductPriceResult
                        {
                            BasePrice = calcResults.BasePrice,
                            Price = calcResults.Price,
                            ListPrice = calcResults.ListPrice,
                            DefaultPrice = calcResults.DefaultPrice,
                            AverageCost = calcResults.AverageCost,
                            IsSale = calcResults.IsSale,
                            ValidPriceFound = calcResults.ValidPriceFound,
                            OrderItemPriceInfos = calcResults.OrderItemPriceInfos
                        };

                        // Inline merge: assign properties from calcResultsMapped to the final result.
                        result.BasePrice = calcResultsMapped.BasePrice;
                        result.Price = calcResultsMapped.Price;
                        result.ListPrice = calcResultsMapped.ListPrice;
                        result.DefaultPrice = calcResultsMapped.DefaultPrice;
                        result.AverageCost = calcResultsMapped.AverageCost;
                        result.IsSale = calcResultsMapped.IsSale;
                        result.ValidPriceFound = calcResultsMapped.ValidPriceFound;

                        if (calcResultsMapped.OrderItemPriceInfos != null &&
                            calcResultsMapped.OrderItemPriceInfos.Any())
                        {
                            orderItemPriceInfos.AddRange(calcResultsMapped.OrderItemPriceInfos);
                        }

                        result.OrderItemPriceInfos = orderItemPriceInfos;

                        var errorResult = await AddGeneralResults(
                            competitivePriceValue, specialPromoPriceValue, productStore, checkIncludeVat,
                            currencyDefaultUomId, productId, (decimal)quantity, partyId);
                        if (errorResult != null)
                        {
                            return errorResult;
                        }
                    }
                    else
                    {
                        // If quantity-specific pricing is not required, process all rules together.
                        var calcResults = await CalcPriceResultFromRules(allProductPriceRules, (decimal)listPrice,
                            defaultPrice,
                            promoPrice, wholesalePrice, maximumPriceValue, minimumPriceValue, validPriceFound,
                            averageCostValue, productId, virtualProductId, prodCatalogId, productStoreGroupId,
                            webSiteId, partyId, quantity, currencyDefaultUomId, nowTimestamp);

                        result.BasePrice = calcResults.BasePrice;
                        result.Price = calcResults.Price;
                        result.ListPrice = calcResults.ListPrice;
                        result.DefaultPrice = calcResults.DefaultPrice;
                        result.AverageCost = calcResults.AverageCost;
                        result.IsSale = calcResults.IsSale;
                        result.ValidPriceFound = calcResults.ValidPriceFound;


                        result.OrderItemPriceInfos = orderItemPriceInfos;

                        var errorResult = await AddGeneralResults(competitivePriceValue, specialPromoPriceValue,
                            productStore, checkIncludeVat, currencyDefaultUomId, productId, (decimal)quantity, partyId);
                        if (errorResult != null)
                        {
                            return errorResult;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Error getting rules from the database while calculating price: " + ex.ToString());
                    throw;
                }
            }

            // -----------------------------------------------------------------
            // 15. CURRENCY CONVERSION (IF REQUIRED)
            // -----------------------------------------------------------------
            try
            {
                // Simulate checking a system property to determine if currency conversion is enabled.
                bool convertProductPriceCurrency = true; // Assume conversion is enabled.
                if (convertProductPriceCurrency && !string.IsNullOrEmpty(currencyDefaultUomId) &&
                    !string.IsNullOrEmpty(currencyUomIdTo) && currencyDefaultUomId != currencyUomIdTo)
                {
                    // Create a temporary DTO to hold converted prices.
                    var convertPriceResult = new ProductPriceResult();

                    // Convert BasePrice
                    if (result.BasePrice != 0)
                    {
                        try
                        {
                            var convertedValue = await _commonService.ConvertUom(
                                currencyDefaultUomId,
                                currencyUomIdTo,
                                DateTime.UtcNow,
                                result.BasePrice,
                                null
                            );
                            if (convertedValue != null)
                            {
                                convertPriceResult.BasePrice = (decimal)convertedValue;
                            }
                            else
                            {
                                _logger.LogWarning("Unable to convert basePrice for product " + productId);
                                convertPriceResult.BasePrice = result.BasePrice;
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error converting basePrice: " + ex.ToString());
                            convertPriceResult.BasePrice = result.BasePrice;
                        }
                    }
                    else
                    {
                        convertPriceResult.BasePrice = result.BasePrice;
                    }

                    // Convert Price
                    if (result.Price != 0)
                    {
                        try
                        {
                            var convertedValue = await _commonService.ConvertUom(
                                currencyDefaultUomId,
                                currencyUomIdTo,
                                DateTime.UtcNow,
                                result.Price,
                                null
                            );
                            if (convertedValue != null)
                            {
                                convertPriceResult.Price = (decimal)convertedValue;
                            }
                            else
                            {
                                _logger.LogWarning("Unable to convert price for product " + productId);
                                convertPriceResult.Price = result.Price;
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error converting price: " + ex.ToString());
                            convertPriceResult.Price = result.Price;
                        }
                    }
                    else
                    {
                        convertPriceResult.Price = result.Price;
                    }

                    // Convert DefaultPrice
                    if (result.DefaultPrice.HasValue && result.DefaultPrice.Value != 0)
                    {
                        try
                        {
                            var convertedValue = await _commonService.ConvertUom(
                                currencyDefaultUomId,
                                currencyUomIdTo,
                                DateTime.UtcNow,
                                result.DefaultPrice.Value,
                                null
                            );
                            if (convertedValue != null)
                            {
                                convertPriceResult.DefaultPrice = convertedValue;
                            }
                            else
                            {
                                _logger.LogWarning("Unable to convert defaultPrice for product " + productId);
                                convertPriceResult.DefaultPrice = result.DefaultPrice;
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error converting defaultPrice: " + ex.ToString());
                            convertPriceResult.DefaultPrice = result.DefaultPrice;
                        }
                    }
                    else
                    {
                        convertPriceResult.DefaultPrice = result.DefaultPrice;
                    }

                    // Convert CompetitivePrice
                    if (result.CompetitivePrice.HasValue && result.CompetitivePrice.Value != 0)
                    {
                        try
                        {
                            var convertedValue = await _commonService.ConvertUom(
                                currencyDefaultUomId,
                                currencyUomIdTo,
                                DateTime.UtcNow,
                                result.CompetitivePrice.Value,
                                null);

                            if (convertedValue != null)
                            {
                                convertPriceResult.CompetitivePrice = convertedValue;
                            }
                            else
                            {
                                _logger.LogWarning("Unable to convert competitivePrice for product " + productId);
                                convertPriceResult.CompetitivePrice = result.CompetitivePrice;
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error converting competitivePrice: " + ex.ToString());
                            convertPriceResult.CompetitivePrice = result.CompetitivePrice;
                        }
                    }
                    else
                    {
                        convertPriceResult.CompetitivePrice = result.CompetitivePrice;
                    }

                    // Convert AverageCost
                    if (result.AverageCost.HasValue && result.AverageCost.Value != 0)
                    {
                        try
                        {
                            var convertedValue = await _commonService.ConvertUom(
                                currencyDefaultUomId,
                                currencyUomIdTo,
                                DateTime.UtcNow,
                                result.AverageCost.Value,
                                null
                            );
                            if (convertedValue != null)
                            {
                                convertPriceResult.AverageCost = convertedValue;
                            }
                            else
                            {
                                _logger.LogWarning("Unable to convert averageCost for product " + productId);
                                convertPriceResult.AverageCost = result.AverageCost;
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error converting averageCost: " + ex.ToString());
                            convertPriceResult.AverageCost = result.AverageCost;
                        }
                    }
                    else
                    {
                        convertPriceResult.AverageCost = result.AverageCost;
                    }

                    // Convert PromoPrice
                    if (result.PromoPrice.HasValue && result.PromoPrice.Value != 0)
                    {
                        try
                        {
                            var convertedValue = await _commonService.ConvertUom(
                                currencyDefaultUomId,
                                currencyUomIdTo,
                                DateTime.UtcNow,
                                result.PromoPrice.Value,
                                null
                            );
                            if (convertedValue != null)
                            {
                                convertPriceResult.PromoPrice = convertedValue;
                            }
                            else
                            {
                                _logger.LogWarning("Unable to convert promoPrice for product " + productId);
                                convertPriceResult.PromoPrice = result.PromoPrice;
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error converting promoPrice: " + ex.ToString());
                            convertPriceResult.PromoPrice = result.PromoPrice;
                        }
                    }
                    else
                    {
                        convertPriceResult.PromoPrice = result.PromoPrice;
                    }

                    // Convert SpecialPromoPrice
                    if (result.SpecialPromoPrice.HasValue && result.SpecialPromoPrice.Value != 0)
                    {
                        try
                        {
                            var convertedValue = await _commonService.ConvertUom(
                                currencyDefaultUomId,
                                currencyUomIdTo,
                                DateTime.UtcNow,
                                result.SpecialPromoPrice.Value,
                                null
                            );
                            if (convertedValue != null)
                            {
                                convertPriceResult.SpecialPromoPrice = convertedValue;
                            }
                            else
                            {
                                _logger.LogWarning("Unable to convert specialPromoPrice for product " + productId);
                                convertPriceResult.SpecialPromoPrice = result.SpecialPromoPrice;
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error converting specialPromoPrice: " + ex.ToString());
                            convertPriceResult.SpecialPromoPrice = result.SpecialPromoPrice;
                        }
                    }
                    else
                    {
                        convertPriceResult.SpecialPromoPrice = result.SpecialPromoPrice;
                    }

                    // Set the target currency.
                    convertPriceResult.CurrencyUsed = currencyUomIdTo;
                    // Use the converted result as the final result.
                    result = convertPriceResult;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during currency conversion: " + ex.ToString());
            }

            // -----------------------------------------------------------------
            // 16. FINAL RETURN
            // -----------------------------------------------------------------
            // Return the fully computed and possibly converted ProductPriceResult.
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in calculateProductPrice: " + ex.ToString());
            throw;
        }
    }

    // This method builds and returns a ProductPriceResult DTO with general pricing adjustments.
    // It sets competitive and special promo prices, defines the currency used, and if VAT inclusion is requested,
    // it calls the tax calculation function (CalcTaxForDisplay) to adjust the base price and related price fields.
    // In case of a tax calculation error, it returns an error result using ReturnError.
    public async Task<ProductPriceResult> AddGeneralResults(
        ProductPrice competitivePriceValue, // Competitive price record (if available).
        ProductPrice specialPromoPriceValue, // Special promotional price record (if available).
        ProductStore productStore, // Product store containing VAT display settings.
        string checkIncludeVat, // Flag ("Y" or "N") indicating if VAT should be included.
        string currencyUomId, // Currency unit of measure identifier.
        string productId, // The product identifier.
        decimal quantity, // Quantity used in price calculation.
        string partyId) // Billing party id (if provided).
    {
        try
        {
            // Define shared constants as per the original Java code.
            // ONE_BASE represents the multiplicative identity.
            decimal ONE_BASE = 1m;
            // PERCENT_SCALE is used to convert a percentage value (e.g., 15% becomes 15/100).
            decimal PERCENT_SCALE = 100m;
            // taxCalcScale defines the precision when calculating the tax percentage.
            int taxCalcScale = 2;
            // taxFinalScale defines the final rounding precision for the price fields.
            int taxFinalScale = 2;
            // taxRounding specifies the rounding mode (MidpointRounding.AwayFromZero simulates "HalfUp").
            MidpointRounding taxRounding = MidpointRounding.AwayFromZero;

            // Create a new ProductPriceResult instance to hold the output pricing details.
            var result = new ProductPriceResult();

            // Update the result DTO with competitive and special promo prices.
            // If the respective price record exists, its price is used; otherwise, the field remains null.
            result.CompetitivePrice = competitivePriceValue != null ? competitivePriceValue.Price : null;
            result.SpecialPromoPrice = specialPromoPriceValue != null ? specialPromoPriceValue.Price : null;
            // Set the currency used for the price.
            result.CurrencyUsed = currencyUomId;

            // Business rule: If VAT should be included (checkIncludeVat equals "Y") and
            // the product store is set to show prices with VAT tax, then adjust prices accordingly.
            if (checkIncludeVat == "Y" && productStore != null && productStore.ShowPricesWithVatTax == "Y")
            {
                try
                {
                    // Call the tax calculation service using RateProductTaxCalc.
                    // Shipping price is null here since it's not used in this context.
                    var taxServiceResult = await _taxService.RateProductTaxCalc(
                        productStore.ProductStoreId,
                        partyId, // BillToPartyId
                        productId,
                        quantity,
                        result.Price,
                        null);

                    // Check if the tax calculation encountered an error.
                    if (taxServiceResult.IsError)
                    {
                        return new ProductPriceResult
                        {
                            ErrorMessage = "Product price cannot calculate VAT tax"
                        };
                    }

                    // Tax calculation succeeded: extract the tax result.
                    var taxCalcResult = taxServiceResult.Data;

                    // Update the main price in the result with the price including VAT.
                    result.Price = taxCalcResult.PriceWithTax;

                    // Retrieve the tax percentage from the tax calculation result.
                    decimal taxPercentage = taxCalcResult.TaxPercentage;

                    // Calculate the tax multiplier.
                    // For example, if taxPercentage is 15, then taxMultiplier = 1 + (15 / 100) = 1.15.
                    decimal taxMultiplier = ONE_BASE + (taxPercentage / PERCENT_SCALE);

                    // Adjust each price field if a value exists.
                    if (result.ListPrice.HasValue)
                    {
                        result.ListPrice = Math.Round(result.ListPrice.Value * taxMultiplier, taxFinalScale,
                            taxRounding);
                    }

                    if (result.DefaultPrice.HasValue)
                    {
                        result.DefaultPrice = Math.Round(result.DefaultPrice.Value * taxMultiplier, taxFinalScale,
                            taxRounding);
                    }

                    if (result.AverageCost.HasValue)
                    {
                        result.AverageCost = Math.Round(result.AverageCost.Value * taxMultiplier, taxFinalScale,
                            taxRounding);
                    }

                    if (result.PromoPrice.HasValue)
                    {
                        result.PromoPrice = Math.Round(result.PromoPrice.Value * taxMultiplier, taxFinalScale,
                            taxRounding);
                    }

                    if (result.CompetitivePrice.HasValue)
                    {
                        result.CompetitivePrice = Math.Round(result.CompetitivePrice.Value * taxMultiplier,
                            taxFinalScale, taxRounding);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Error calculating VAT tax (with RateProductTaxCalc service): " + ex.ToString());
                    return new ProductPriceResult
                    {
                        ErrorMessage = "Product price cannot calculate VAT tax"
                    };
                }
            }

            // Return the fully built and adjusted ProductPriceResult.
            return result;
        }
        catch (Exception ex)
        {
            // Log any unexpected errors encountered in AddGeneralResults.
            _logger.LogError(ex, "Error in AddGeneralResults: " + ex.ToString());
            throw;
        }
    }


    /// <summary>
    /// Asynchronously checks whether a given product price condition is satisfied based on various parameters.
    /// </summary>
    /// <param name="productPriceCond">The price condition object containing input and operator details.</param>
    /// <param name="productId">The product ID.</param>
    /// <param name="virtualProductId">The virtual product ID (if any).</param>
    /// <param name="prodCatalogId">The product catalog ID.</param>
    /// <param name="productStoreGroupId">The product store group ID.</param>
    /// <param name="webSiteId">The website ID.</param>
    /// <param name="partyId">The party ID.</param>
    /// <param name="quantity">The quantity (can be null).</param>
    /// <param name="listPrice">The list price.</param>
    /// <param name="currencyUomId">The currency unit of measure ID.</param>
    /// <param name="nowTimestamp">The current timestamp for effective date filtering.</param>
    /// <returns>True if the condition is satisfied; otherwise, false.</returns>
    public async Task<bool> CheckPriceCondition(
        ProductPriceCond productPriceCond,
        string productId,
        string virtualProductId,
        string prodCatalogId,
        string productStoreGroupId,
        string webSiteId,
        string partyId,
        decimal? quantity,
        decimal listPrice,
        string currencyUomId,
        DateTime nowTimestamp)
    {
        int compare = 0;
        string inputParamEnumId = productPriceCond.InputParamEnumId;
        string condValue = productPriceCond.CondValue;

        if (inputParamEnumId == "PRIP_PRODUCT_ID")
        {
            // Check if productId or virtualProductId is equal to condValue.
            var idList = new List<string> { productId, virtualProductId };
            compare = idList.Contains(condValue) ? 0 : 1;
        }
        else if (inputParamEnumId == "PRIP_PROD_CAT_ID")
        {
            // Query ProductCategoryMember for productId with the specified productCategoryId.
            var pcmList = await _context.ProductCategoryMembers
                .Where(pcm => pcm.ProductId == productId &&
                              pcm.ProductCategoryId == condValue &&
                              pcm.FromDate <= nowTimestamp &&
                              (pcm.ThruDate == null || pcm.ThruDate >= nowTimestamp))
                .ToListAsync();

            compare = pcmList.Any() ? 0 : 1;

            // If not found and a virtualProductId exists, try that as well.
            if (compare == 1 && !string.IsNullOrEmpty(virtualProductId))
            {
                var virtualPcmList = await _context.ProductCategoryMembers
                    .Where(pcm => pcm.ProductId == virtualProductId &&
                                  pcm.ProductCategoryId == condValue &&
                                  pcm.FromDate <= nowTimestamp &&
                                  (pcm.ThruDate == null || pcm.ThruDate >= nowTimestamp))
                    .ToListAsync();
                if (virtualPcmList.Any())
                {
                    compare = 0;
                }
            }
        }
        else if (inputParamEnumId == "PRIP_PROD_FEAT_ID")
        {
            // Query for ProductFeatureAppl records for productId and the specified productFeatureId.
            var featureAppls = await _context.ProductFeatureAppls
                .Where(pfa => pfa.ProductId == productId &&
                              pfa.ProductFeatureId == condValue &&
                              pfa.FromDate <= nowTimestamp &&
                              (pfa.ThruDate == null || pfa.ThruDate >= nowTimestamp))
                .ToListAsync();
            compare = featureAppls.Any() ? 0 : 1;
        }
        else if (inputParamEnumId == "PRIP_PROD_CLG_ID")
        {
            if (!string.IsNullOrEmpty(prodCatalogId))
            {
                compare = string.Compare(prodCatalogId, condValue, StringComparison.Ordinal);
            }
            else
            {
                compare = 1;
            }
        }
        else if (inputParamEnumId == "PRIP_PROD_SGRP_ID")
        {
            if (!string.IsNullOrEmpty(productStoreGroupId))
            {
                compare = string.Compare(productStoreGroupId, condValue, StringComparison.Ordinal);
            }
            else
            {
                compare = 1;
            }
        }
        else if (inputParamEnumId == "PRIP_WEBSITE_ID")
        {
            if (!string.IsNullOrEmpty(webSiteId))
            {
                compare = string.Compare(webSiteId, condValue, StringComparison.Ordinal);
            }
            else
            {
                compare = 1;
            }
        }
        else if (inputParamEnumId == "PRIP_QUANTITY")
        {
            // If no quantity is provided, assume the condition passes.
            if (!quantity.HasValue)
            {
                return true;
            }
            else
            {
                compare = decimal.Compare(quantity.Value, decimal.Parse(condValue));
            }
        }
        else if (inputParamEnumId == "PRIP_PARTY_ID")
        {
            if (!string.IsNullOrEmpty(partyId))
            {
                compare = string.Compare(partyId, condValue, StringComparison.Ordinal);
            }
            else
            {
                compare = 1;
            }
        }
        else if (inputParamEnumId == "PRIP_PARTY_GRP_MEM")
        {
            if (string.IsNullOrEmpty(partyId))
            {
                compare = 1;
            }
            else
            {
                string groupPartyId = condValue;
                if (partyId == groupPartyId)
                {
                    compare = 0;
                }
                else
                {
                    // Query PartyRelationship for GROUP_ROLLUP relationships.
                    var prList = await _context.PartyRelationships
                        .Where(pr => pr.PartyIdFrom == groupPartyId &&
                                     pr.PartyIdTo == partyId &&
                                     pr.PartyRelationshipTypeId == "GROUP_ROLLUP" &&
                                     pr.FromDate <= nowTimestamp &&
                                     (pr.ThruDate == null || pr.ThruDate >= nowTimestamp))
                        .ToListAsync();
                    if (prList.Any())
                    {
                        compare = 0;
                    }
                    else
                    {
                        // Fallback to a hierarchical check (assumed to be implemented elsewhere).
                        compare = await CheckConditionPartyHierarchy(nowTimestamp, groupPartyId, partyId);
                    }
                }
            }
        }
        else if (inputParamEnumId == "PRIP_PARTY_CLASS")
        {
            if (string.IsNullOrEmpty(partyId))
            {
                compare = 1;
            }
            else
            {
                string partyClassificationGroupId = condValue;
                var pcList = await _context.PartyClassifications
                    .Where(pc => pc.PartyId == partyId &&
                                 pc.PartyClassificationGroupId == partyClassificationGroupId &&
                                 pc.FromDate <= nowTimestamp &&
                                 (pc.ThruDate == null || pc.ThruDate >= nowTimestamp))
                    .ToListAsync();
                compare = pcList.Any() ? 0 : 1;
            }
        }
        else if (inputParamEnumId == "PRIP_ROLE_TYPE")
        {
            if (!string.IsNullOrEmpty(partyId))
            {
                var partyRole = await _context.PartyRoles
                    .Where(pr => pr.PartyId == partyId &&
                                 pr.RoleTypeId == condValue)
                    .FirstOrDefaultAsync();
                compare = (partyRole != null) ? 0 : 1;
            }
            else
            {
                compare = 1;
            }
        }
        else if (inputParamEnumId == "PRIP_LIST_PRICE")
        {
            compare = decimal.Compare(listPrice, decimal.Parse(condValue));
        }
        else if (inputParamEnumId == "PRIP_CURRENCY_UOMID")
        {
            compare = string.Compare(currencyUomId, condValue, StringComparison.Ordinal);
        }
        else
        {
            _logger.LogWarning("An unsupported productPriceCond input parameter (lhs) was used: " + inputParamEnumId +
                               ", returning false, i.e. check failed");
            return false;
        }

        // Now, evaluate the condition based on the operatorEnumId.
        string operatorEnumId = productPriceCond.OperatorEnumId;
        if (operatorEnumId == "PRC_EQ")
        {
            if (compare == 0) return true;
        }
        else if (operatorEnumId == "PRC_NEQ")
        {
            if (compare != 0) return true;
        }
        else if (operatorEnumId == "PRC_LT")
        {
            if (compare < 0) return true;
        }
        else if (operatorEnumId == "PRC_LTE")
        {
            if (compare <= 0) return true;
        }
        else if (operatorEnumId == "PRC_GT")
        {
            if (compare > 0) return true;
        }
        else if (operatorEnumId == "PRC_GTE")
        {
            if (compare >= 0) return true;
        }
        else
        {
            _logger.LogWarning("An unsupported productPriceCond condition was used: " + operatorEnumId +
                               ", returning false, i.e. check failed");
            return false;
        }

        return false;
    }

    /// <summary>
    /// Asynchronously checks whether a given party (partyId) is part of a hierarchical group that matches groupPartyId.
    /// It recursively retrieves PartyRelationship records (of type "GROUP_ROLLUP") effective at nowTimestamp.
    /// If any relationship's PartyIdFrom equals the groupPartyId, or if a recursive check returns 0, the method returns 0 (match found).
    /// Otherwise, it returns 1.
    /// </summary>
    /// <param name="nowTimestamp">The current effective timestamp used for date filtering.</param>
    /// <param name="groupPartyId">The group party ID to match.</param>
    /// <param name="partyId">The party ID to check within the hierarchy.</param>
    /// <returns>0 if a match is found; otherwise, 1.</returns>
    public async Task<int> CheckConditionPartyHierarchy(DateTime nowTimestamp, string groupPartyId, string partyId)
    {
        try
        {
            // Retrieve all PartyRelationship records where:
            // - partyIdTo equals the provided partyId.
            // - The relationship type is "GROUP_ROLLUP".
            // - The record is effective (FromDate <= nowTimestamp and (ThruDate is null or ThruDate >= nowTimestamp)).
            var partyRelationshipList = await _context.PartyRelationships
                .Where(pr => pr.PartyIdTo == partyId &&
                             pr.PartyRelationshipTypeId == "GROUP_ROLLUP" &&
                             pr.FromDate <= nowTimestamp &&
                             (pr.ThruDate == null || pr.ThruDate >= nowTimestamp))
                .ToListAsync();

            // Iterate over each relationship.
            foreach (var partyRelationship in partyRelationshipList)
            {
                string partyIdFrom = partyRelationship.PartyIdFrom;
                // If this related party ID matches the groupPartyId, the condition is satisfied.
                if (partyIdFrom == groupPartyId)
                {
                    return 0;
                }

                // Recursively check the hierarchy: if any parent relationship satisfies the condition, return 0.
                int recursiveResult = await CheckConditionPartyHierarchy(nowTimestamp, groupPartyId, partyIdFrom);
                if (recursiveResult == 0)
                {
                    return 0;
                }
            }

            // If no matching relationship is found, return 1.
            return 1;
        }
        catch (Exception ex)
        {
            // Log the error and propagate the exception.
            _logger.LogError(ex, "Error checking condition party hierarchy");
            throw;
        }
    }

    /// <summary>
    /// Asynchronously calculates the final price result based on a list of price rules and a variety of inputs.
    /// This method loops over each price rule, evaluates all its conditions, and if they are met, applies the defined actions
    /// to adjust the price. It also builds a list of order item price info records (for audit/display purposes).
    /// Finally, it enforces minimum and maximum boundaries on the final price.
    /// </summary>
    /// <param name="productPriceRules">List of price rules to evaluate.</param>
    /// <param name="listPrice">The list price for the product.</param>
    /// <param name="defaultPrice">The default price for the product.</param>
    /// <param name="promoPrice">The promotional price, if any.</param>
    /// <param name="wholesalePrice">The wholesale price, if applicable.</param>
    /// <param name="maximumPriceValue">A record containing the maximum allowed sale price.</param>
    /// <param name="minimumPriceValue">A record containing the minimum allowed sale price.</param>
    /// <param name="validPriceFound">Initial flag indicating if a valid price has been determined.</param>
    /// <param name="averageCostValue">A record containing the average cost; if not available, listPrice is used.</param>
    /// <param name="productId">The product ID.</param>
    /// <param name="virtualProductId">The virtual product ID, if applicable.</param>
    /// <param name="prodCatalogId">The product catalog ID.</param>
    /// <param name="productStoreGroupId">The product store group ID.</param>
    /// <param name="webSiteId">The website ID.</param>
    /// <param name="partyId">The party (customer) ID.</param>
    /// <param name="quantity">The quantity for the price calculation (can be null).</param>
    /// <param name="currencyUomId">The currency unit of measure ID.</param>
    /// <param name="nowTimestamp">The current timestamp for effective date filtering.</param>
    /// <param name="locale">The locale (for any message lookups, if needed).</param>
    /// <returns>A PriceCalcResult object containing the calculated price, related price info, and flags.</returns>
    public async Task<PriceCalcResult> CalcPriceResultFromRules(
        List<ProductPriceRule> productPriceRules,
        decimal listPrice,
        decimal defaultPrice,
        decimal promoPrice,
        decimal wholesalePrice,
        ProductPrice maximumPriceValue,
        ProductPrice minimumPriceValue,
        bool validPriceFound,
        ProductPrice averageCostValue,
        string productId,
        string virtualProductId,
        string prodCatalogId,
        string productStoreGroupId,
        string webSiteId,
        string partyId,
        decimal? quantity,
        string currencyUomId,
        DateTime nowTimestamp)
    {
        // Initialize the result DTO.
        var calcResult = new PriceCalcResult();
        // List to accumulate order item price info records.
        var orderItemPriceInfos = new List<OrderItemPriceInfo>();
        bool isSale = false;

        // Counters for logging/diagnostics.
        int totalConds = 0;
        int totalActions = 0;
        int totalRules = 0;

        // Determine the base average cost: if available from averageCostValue, use it; otherwise, use listPrice.
        decimal averageCost = (decimal)((averageCostValue != null && averageCostValue.Price != 0)
            ? averageCostValue.Price
            : listPrice);
        // Calculate the margin (listPrice minus averageCost).
        decimal margin = listPrice - averageCost;

        // Start with the list price as the running price.
        decimal price = listPrice;

        // Loop over each price rule.
        foreach (var productPriceRule in productPriceRules)
        {
            // Get the rule ID.
            string productPriceRuleId = productPriceRule.ProductPriceRuleId;

            // Check fromDate and thruDate for the rule; if the rule hasn't started yet or is expired, skip it.
            if (productPriceRule.FromDate != null && productPriceRule.FromDate > nowTimestamp)
                continue;
            if (productPriceRule.ThruDate != null && productPriceRule.ThruDate < nowTimestamp)
                continue;

            // Initialize condition evaluation.
            bool allTrue = true;
            var condsDescription = new StringBuilder();

            // Retrieve all conditions for this rule.
            var productPriceConds = await _context.ProductPriceConds
                .Where(ppc => ppc.ProductPriceRuleId == productPriceRuleId)
                .ToListAsync();

            // Loop over each condition.
            foreach (var productPriceCond in productPriceConds)
            {
                totalConds++;

                // Evaluate the condition using a helper method (which returns true/false).
                bool conditionMet = await CheckPriceCondition(productPriceCond, productId, virtualProductId,
                    prodCatalogId, productStoreGroupId, webSiteId, partyId, quantity, listPrice, currencyUomId,
                    nowTimestamp);
                if (!conditionMet)
                {
                    allTrue = false;
                    break;
                }

                // Build a description string for this condition.
                condsDescription.Append("[");
                // Assume navigation property InputParamEnum gives access to enumCode.
                condsDescription.Append(productPriceCond.InputParamEnum.EnumCode);
                // Append operator description from related OperatorEnum.
                condsDescription.Append(productPriceCond.OperatorEnum.Description);
                // Append the condition value.
                condsDescription.Append(productPriceCond.CondValue);
                condsDescription.Append("] ");
            }

            // Append some base price info to the description.
            condsDescription.Append("[list:" + listPrice + ";avgCost:" + averageCost + ";margin:" + margin + "] ");

            bool foundFlatOverride = false;

            // If all conditions are met for this rule, perform the actions.
            if (allTrue)
            {
                // Check if the rule indicates a sale.
                if (productPriceRule.IsSale == "Y")
                {
                    isSale = true;
                }

                // Retrieve all actions for this rule.
                var productPriceActions = await _context.ProductPriceActions
                    .Where(p => p.ProductPriceRuleId == productPriceRuleId)
                    .ToListAsync();

                // Loop over each action.
                foreach (var productPriceAction in productPriceActions)
                {
                    totalActions++;
                    decimal modifyAmount = 0m;

                    // Evaluate the action type.
                    string actionType = productPriceAction.ProductPriceActionTypeId;
                    if (actionType == "PRICE_POD")
                    {
                        if (productPriceAction.Amount != null)
                        {
                            // PRICE_POD: Set modifyAmount = defaultPrice * (action.Amount/100); and set price = defaultPrice.
                            modifyAmount = (decimal)(defaultPrice * (productPriceAction.Amount / 100m));
                            price = defaultPrice;
                        }
                    }
                    else if (actionType == "PRICE_POL")
                    {
                        if (productPriceAction.Amount != null)
                        {
                            modifyAmount = (decimal)(listPrice * (productPriceAction.Amount / 100m));
                        }
                    }
                    else if (actionType == "PRICE_POAC")
                    {
                        if (productPriceAction.Amount != null)
                        {
                            modifyAmount = (decimal)(averageCost * (productPriceAction.Amount / 100m));
                        }
                    }
                    else if (actionType == "PRICE_POM")
                    {
                        if (productPriceAction.Amount != null)
                        {
                            modifyAmount = (decimal)(margin * (productPriceAction.Amount / 100m));
                        }
                    }
                    else if (actionType == "PRICE_POWHS")
                    {
                        if (productPriceAction.Amount != null && wholesalePrice != 0m)
                        {
                            modifyAmount = (decimal)(wholesalePrice * (productPriceAction.Amount / 100m));
                        }
                    }
                    else if (actionType == "PRICE_FOL")
                    {
                        if (productPriceAction.Amount != null)
                        {
                            modifyAmount = (decimal)productPriceAction.Amount;
                        }
                    }
                    else if (actionType == "PRICE_FLAT")
                    {
                        // Flat override: use the provided amount as the final price.
                        foundFlatOverride = true;
                        if (productPriceAction.Amount != null)
                        {
                            price = (decimal)productPriceAction.Amount;
                        }
                        else
                        {
                            _logger.LogInformation(
                                "ProductPriceAction had null amount, using default price: " + defaultPrice +
                                " for product with id " + productId, "module");
                            price = defaultPrice;
                            isSale = false; // reverse sale flag as rule not applied
                        }
                    }
                    else if (actionType == "PRICE_PFLAT")
                    {
                        foundFlatOverride = true;
                        price = promoPrice;
                        if (productPriceAction.Amount != null)
                        {
                            price += (decimal)productPriceAction.Amount;
                        }

                        if (price == 0m)
                        {
                            if (defaultPrice != 0m)
                            {
                                _logger.LogInformation(
                                    "PromoPrice and ProductPriceAction had null amount, using default price: " +
                                    defaultPrice + " for product with id " + productId, "module");
                                price = defaultPrice;
                            }
                            else if (listPrice != 0m)
                            {
                                _logger.LogInformation(
                                    "PromoPrice and ProductPriceAction had null amount and no default price was available, using list price: " +
                                    listPrice + " for product with id " + productId, "module");
                                price = listPrice;
                            }
                            else
                            {
                                _logger.LogError(
                                    "PromoPrice and ProductPriceAction had null amount and no default or list price was available, so price is set to zero for product with id " +
                                    productId, "module");
                                price = 0m;
                            }

                            isSale = false;
                        }
                    }
                    else if (actionType == "PRICE_WFLAT")
                    {
                        // Same as PRICE_PFLAT but using wholesale price.
                        foundFlatOverride = true;
                        price = wholesalePrice;
                        if (productPriceAction.Amount != null)
                        {
                            price += (decimal)productPriceAction.Amount;
                        }

                        if (price == 0m)
                        {
                            if (defaultPrice != 0m)
                            {
                                _logger.LogInformation(
                                    "WholesalePrice and ProductPriceAction had null amount, using default price: " +
                                    defaultPrice + " for product with id " + productId, "module");
                                price = defaultPrice;
                            }
                            else if (listPrice != 0m)
                            {
                                _logger.LogInformation(
                                    "WholesalePrice and ProductPriceAction had null amount and no default price was available, using list price: " +
                                    listPrice + " for product with id " + productId, "module");
                                price = listPrice;
                            }
                            else
                            {
                                _logger.LogError(
                                    "WholesalePrice and ProductPriceAction had null amount and no default or list price was available, so price is set to zero for product with id " +
                                    productId, "module");
                                price = 0m;
                            }

                            isSale = false;
                        }
                    }

                    // Build a description for the price info.
                    var priceInfoDescription = new StringBuilder();
                    priceInfoDescription.Append(condsDescription.ToString());
                    // For demonstration, we append a literal text along with the action type.
                    priceInfoDescription.Append("[ProductPriceConditionType:");
                    priceInfoDescription.Append(productPriceAction.ProductPriceActionTypeId);
                    priceInfoDescription.Append("]");

                    // Create a new OrderItemPriceInfo record.
                    var orderItemPriceInfo = new OrderItemPriceInfo
                    {
                        ProductPriceRuleId = productPriceAction.ProductPriceRuleId,
                        ProductPriceActionSeqId = productPriceAction.ProductPriceActionSeqId,
                        ModifyAmount = modifyAmount,
                        RateCode = productPriceAction.RateCode,
                        // Ensure description does not exceed 250 characters.
                        Description = priceInfoDescription.Length > 250
                            ? priceInfoDescription.ToString().Substring(0, 250)
                            : priceInfoDescription.ToString()
                    };
                    orderItemPriceInfos.Add(orderItemPriceInfo);

                    // If a flat override was applied, break out of the actions loop.
                    if (foundFlatOverride)
                    {
                        break;
                    }
                    else
                    {
                        // Otherwise, add the modifyAmount to the running price.
                        price += modifyAmount;
                    }
                }
            }

            totalRules++;
            if (foundFlatOverride)
            {
                // If a flat override was found in any rule, break out of the rules loop.
                break;
            }
        } // end loop over productPriceRules

        // If no actions were applied at all, then default to the default price.
        if (totalActions == 0)
        {
            price = defaultPrice;
            // validPriceFound remains as originally set.
        }
        else
        {
            // At least one action was applied; mark the price as valid.
            validPriceFound = true;
        }

        // Enforce maximum and minimum sale price limits.
        decimal? maxSellPrice = maximumPriceValue != null ? maximumPriceValue.Price : (decimal?)null;
        if (maxSellPrice.HasValue && price > maxSellPrice.Value)
        {
            price = maxSellPrice.Value;
        }

        decimal? minSellPrice = minimumPriceValue != null ? minimumPriceValue.Price : (decimal?)null;
        if (minSellPrice.HasValue && price < minSellPrice.Value)
        {
            price = minSellPrice.Value;
            validPriceFound = true;
        }


        // Build and return the final result.
        var resultData = new PriceCalcResult
        {
            BasePrice = price,
            Price = price,
            ListPrice = listPrice,
            DefaultPrice = defaultPrice,
            AverageCost = averageCost,
            OrderItemPriceInfos = orderItemPriceInfos,
            IsSale = isSale,
            ValidPriceFound = validPriceFound
        };

        return resultData;
    }
}