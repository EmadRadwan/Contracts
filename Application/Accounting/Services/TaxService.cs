using Application.Accounting.Services.Models;
using Application.Catalog.Products;
using Application.Catalog.ProductStores;
using Application.Order.Orders;
using Application.Order.Quotes;
using Application.Shipments.Taxes;
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;
using Serilog.Core;

namespace Application.Accounting.Services;

public interface ITaxService
{
    Task<OrderAdjustmentDto2[]> CalculateTaxAdjustments(List<OrderItemDto2> orderItems);
    Task<QuoteAdjustmentDto2[]> CalculateTaxAdjustmentsForQuote(List<QuoteItemDto2> quoteItems);

    Task<OrderAdjustmentDto2[]> CalculateTaxAdjustmentsForSalesOrder(
        OrderItemsAndAdjustmentsDto orderItemsAndAdjustments);

    Task<List<TaxAuthorityRateProductDto>> GetTaxAuthorityRateProductsQuery(PartyTaxAuthInfo partyTaxAuth);
    Task<Func<TaxAuthorityRateProduct, bool>> SetProductCategoryCond(Domain.Product product);

    Task<ProductPrice> GetProductPrice(Domain.Product product, ProductStore productStore,
        string taxAuthGeoId, string taxAuthPartyId);

    Task HandlePartyTaxExempt(OrderAdjustment adjValue, HashSet<string> billToPartyIdSet, string taxAuthGeoId,
        string taxAuthPartyId, decimal taxAmount, DateTime nowTimestamp);

    Task<List<OrderAdjustment>> GetTaxAdjustments(
        Domain.Product product,
        ProductStore productStore,
        string payToPartyId,
        string billToPartyId,
        HashSet<TaxAuthority> taxAuthoritySet,
        decimal itemPrice,
        decimal itemQuantity,
        decimal itemAmount,
        decimal? shippingAmount,
        decimal? orderPromotionsAmount,
        decimal? weight);

    Task<ServiceResult<TaxCalcResult>> RateProductTaxCalc(
        string productStoreId,
        string billToPartyId,
        string productId,
        decimal? quantity,
        decimal basePrice,
        decimal? shippingPrice);
}

public class TaxService : ITaxService
{
    private readonly DataContext _context;
    private readonly IProductStoreService _productStoreService;
    private readonly IProductService _productService;
    private readonly ILogger<TaxService> _logger;


    public TaxService(DataContext context, IProductStoreService productStoreService, IProductService productService,
        ILogger<TaxService> logger)
    {
        _context = context;
        _productStoreService = productStoreService;
        _productService = productService;
        _logger = logger;
    }


    public async Task<OrderAdjustmentDto2[]> CalculateTaxAdjustments(List<OrderItemDto2> orderItems)
    {
        var stamp = DateTime.UtcNow;

        var orderAdjustments = new List<OrderAdjustmentDto2>();


        // get orderId from first OrderItem in OrderItemsAndAdjustmentsDto
        var orderId = orderItems.First().OrderId;

        var partyTaxAuth = await GetTaxAuthInfoForProductStore();

        // get tax authority override GL Account from TaxAuthorityGlAccount based on partyTaxAuth and productSore.paytopartyid
        var taxAuthorityOverrideGlAccount = await GetTaxAuthorityOverrideGlAccount()!;

        var taxAuthorityRateProducts = await GetTaxAuthorityRateProducts(partyTaxAuth);

        // loop forEach through taxAuthorityRateProducts
        foreach (var taxRate in taxAuthorityRateProducts)
            //check if tax type is SALES_TAX
            if (taxRate.TaxAuthorityRateTypeId == "SALES_TAX")
            {
                // get tax rate percentage
                var taxRatePercentage = taxRate.TaxPercentage;

                var noTaxWillBeApplied = false;


                // loop forEach through OrderItemsAndAdjustmentsDto.OrderItems 
                // to calculate tax amount based on orderItem quantity and subtotal
                foreach (var orderItem in orderItems)
                {
                    // get orderItem tax amount
                    var orderItemTaxAmount = orderItem.SubTotal * taxRatePercentage / 100;

                    // check if orderItemTaxAmount is 0 or less than 0
                    if (orderItemTaxAmount <= 0)
                        // set noTaxWillBeApplied to true
                        noTaxWillBeApplied = true;

                    // if taxAdjustment is null, create new OrderItemAdjustment
                    if (!noTaxWillBeApplied)
                    {
                        // create new OrderAdjustment
                        var newOrderAdjustment = new OrderAdjustmentDto2
                        {
                            OrderAdjustmentId = Guid.NewGuid().ToString(),
                            CorrespondingProductId = orderItem.ProductId,
                            CorrespondingProductName = orderItem.ProductName,
                            OrderAdjustmentTypeId = "SALES_TAX",
                            OrderAdjustmentTypeDescription = "Sales Tax",
                            OrderId = orderId,
                            OrderItemSeqId = orderItem.OrderItemSeqId,
                            TaxAuthGeoId = taxRate.TaxAuthGeoId,
                            TaxAuthPartyId = taxRate.TaxAuthPartyId,
                            Amount = orderItemTaxAmount,
                            SourcePercentage = taxRatePercentage,
                            Description = taxRate.Description,
                            IsManual = "N",
                            IsAdjustmentDeleted = false,
                            OverrideGlAccountId = taxAuthorityOverrideGlAccount == null
                                ? null
                                : taxAuthorityOverrideGlAccount.GlAccountId,
                            CreatedDate = stamp,
                            LastModifiedDate = stamp
                        };

                        // add newOrderAdjustment to orderAdjustments
                        orderAdjustments.Add(newOrderAdjustment);
                    }
                }
            }

        return orderAdjustments.ToArray();
    }

    public async Task<QuoteAdjustmentDto2[]> CalculateTaxAdjustmentsForQuote(List<QuoteItemDto2> quoteItems)
    {
        var stamp = DateTime.UtcNow;

        var quoteAdjustments = new List<QuoteAdjustmentDto2>();


        // get quoteId from first QuoteItem in QuoteItemsAndAdjustmentsDto
        var quoteId = quoteItems.First().QuoteId;

        var partyTaxAuth = await GetTaxAuthInfoForProductStore();

        // get tax authority override GL Account from TaxAuthorityGlAccount based on partyTaxAuth and productSore.paytopartyid
        var taxAuthorityOverrideGlAccount = await GetTaxAuthorityOverrideGlAccount()!;

        var taxAuthorityRateProducts = await GetTaxAuthorityRateProducts(partyTaxAuth);

        // loop forEach through taxAuthorityRateProducts
        foreach (var taxRate in taxAuthorityRateProducts)
            //check if tax type is SALES_TAX
            if (taxRate.TaxAuthorityRateTypeId == "SALES_TAX")
            {
                // get tax rate percentage
                var taxRatePercentage = taxRate.TaxPercentage;

                var noTaxWillBeApplied = false;


                // loop forEach through QuoteItemsAndAdjustmentsDto.QuoteItems 
                // to calculate tax amount based on quoteItem quantity and subtotal
                foreach (var quoteItem in quoteItems)
                {
                    // get quoteItem tax amount
                    var quoteItemTaxAmount = quoteItem.SubTotal * taxRatePercentage / 100;

                    // check if quoteItemTaxAmount is 0 or less than 0
                    if (quoteItemTaxAmount <= 0)
                        // set noTaxWillBeApplied to true
                        noTaxWillBeApplied = true;

                    // if taxAdjustment is null, create new QuoteItemAdjustment
                    if (!noTaxWillBeApplied)
                    {
                        // create new QuoteAdjustment
                        var newQuoteAdjustment = new QuoteAdjustmentDto2
                        {
                            QuoteAdjustmentId = Guid.NewGuid().ToString(),
                            CorrespondingProductId = quoteItem.ProductId,
                            CorrespondingProductName = quoteItem.ProductName,
                            QuoteAdjustmentTypeId = "SALES_TAX",
                            QuoteAdjustmentTypeDescription = "Sales Tax",
                            QuoteId = quoteId,
                            QuoteItemSeqId = quoteItem.QuoteItemSeqId,
                            TaxAuthGeoId = taxRate.TaxAuthGeoId,
                            TaxAuthPartyId = taxRate.TaxAuthPartyId,
                            Amount = quoteItemTaxAmount,
                            SourcePercentage = taxRatePercentage,
                            Description = taxRate.Description,
                            IsManual = "N",
                            IsAdjustmentDeleted = false,
                            OverrideGlAccountId = taxAuthorityOverrideGlAccount == null
                                ? null
                                : taxAuthorityOverrideGlAccount.GlAccountId,
                            CreatedDate = stamp,
                            LastModifiedDate = stamp
                        };

                        // add newQuoteAdjustment to quoteAdjustments
                        quoteAdjustments.Add(newQuoteAdjustment);
                    }
                }
            }

        return quoteAdjustments.ToArray();
    }

    public async Task<OrderAdjustmentDto2[]> CalculateTaxAdjustmentsForSalesOrder(
        OrderItemsAndAdjustmentsDto orderItemsAndAdjustments)
    {
        var stamp = DateTime.UtcNow;


        // get orderId from first OrderItem in OrderItemsAndAdjustmentsDto
        var orderId = orderItemsAndAdjustments.OrderItems.First().OrderId;

        var partyTaxAuth = await GetTaxAuthInfoForProductStore();

        // get tax authority override GL Account from TaxAuthorityGlAccount based on partyTaxAuth and productSore.paytopartyid
        var taxAuthorityOverrideGlAccount = await GetTaxAuthorityOverrideGlAccount()!;

        var taxAuthorityRateProducts = await GetTaxAuthorityRateProducts(partyTaxAuth);

        // loop forEach through taxAuthorityRateProducts
        foreach (var taxRate in taxAuthorityRateProducts)
            //check if tax type is SALES_TAX
            if (taxRate.TaxAuthorityRateTypeId == "SALES_TAX")
            {
                // get tax rate percentage
                var taxRatePercentage = taxRate.TaxPercentage;

                var noTaxWillBeApplied = false;


                // loop forEach through OrderItemsAndAdjustmentsDto.OrderItems 
                // to calculate tax amount based on orderItem quantity and subtotal
                foreach (var orderItem in orderItemsAndAdjustments.OrderItems)
                {
                    // get orderItem tax amount
                    var orderItemTaxAmount = orderItem.SubTotal * taxRatePercentage / 100;

                    // check if orderItemTaxAmount is 0 or less than 0
                    if (orderItemTaxAmount <= 0)
                        // set noTaxWillBeApplied to true
                        noTaxWillBeApplied = true;

                    // if taxAdjustment is null, create new OrderItemAdjustment
                    if (!noTaxWillBeApplied)
                    {
                        // create new OrderAdjustment
                        var newOrderAdjustment = new OrderAdjustmentDto2
                        {
                            OrderAdjustmentId = Guid.NewGuid().ToString(),
                            CorrespondingProductId = orderItem.ProductId,
                            CorrespondingProductName = orderItem.ProductName,
                            OrderAdjustmentTypeId = "SALES_TAX",
                            OrderAdjustmentTypeDescription = "Sales Tax",
                            OrderId = orderId,
                            OrderItemSeqId = orderItem.OrderItemSeqId,
                            TaxAuthGeoId = taxRate.TaxAuthGeoId,
                            TaxAuthPartyId = taxRate.TaxAuthPartyId,
                            Amount = orderItemTaxAmount,
                            SourcePercentage = taxRatePercentage,
                            Description = taxRate.Description,
                            IsManual = "N",
                            IsAdjustmentDeleted = false,
                            OverrideGlAccountId = taxAuthorityOverrideGlAccount == null
                                ? null
                                : taxAuthorityOverrideGlAccount.GlAccountId,
                            CreatedDate = stamp,
                            LastModifiedDate = stamp
                        };

                        // add newOrderAdjustment to OrderItemsAndAdjustmentsDto.OrderAdjustments
                        orderItemsAndAdjustments.OrderAdjustments.Add(newOrderAdjustment);
                    }
                }
            }


        var orderAdjustments = orderItemsAndAdjustments.OrderAdjustments.ToArray();
        return orderAdjustments;
    }


    private async Task<TaxAuthorityGlAccount>? GetTaxAuthorityOverrideGlAccount()
    {
        var partyTaxAuth = await GetTaxAuthInfoForProductStore();

        var productStore = await _productStoreService.GetProductStoreForLoggedInUser();
        // get tax authority override GL Account from TaxAuthorityGlAccount based on partyTaxAuth and productSore.paytopartyid
        var taxAuthorityGlAccount = await _context.TaxAuthorityGlAccounts
            .Where(x => x.TaxAuthGeoId == partyTaxAuth.TaxAuthGeoId
                        && x.TaxAuthPartyId == partyTaxAuth.TaxAuthPartyId
                        && x.OrganizationPartyId == productStore.PayToPartyId)
            .FirstOrDefaultAsync();

        // if taxAuthorityGlAccount is not null, return taxAuthorityGlAccount.GlAccountId
        if (taxAuthorityGlAccount != null) return taxAuthorityGlAccount;

        /*  // if taxAuthorityGlAccount is null, get tax authority default GL Account from glAccountOrganization as this is the default chart for the company where
         // link table GlAccount where is 'SALES TAX COLLECTED'
         var glAccount = await (from glo in _context.GlAccountOrganizations 
                                join gla in _context.GlAccounts on glo.GlAccountId equals gla.GlAccountId
                                where glo.OrganizationPartyId == productStore.PayToPartyId && gla.AccountName == "SALES_TAX_COLLECTED"
                                select glo.GlAccountId).FirstOrDefaultAsync();
 
         if (glAccount != null) return glAccount;
 
         // if glAccount is null, get tax authority default GL Account from glAccount 
         glAccount = await (from gla in _context.GlAccounts
                            where gla.AccountName == "SALES_TAX_COLLECTED"
                            select gla.GlAccountId).FirstOrDefaultAsync(); */

        return null!;
    }

    private async Task<List<TaxAuthorityRateProduct>> GetTaxAuthorityRateProducts(PartyTaxAuthInfo partyTaxAuth)
    {
        // get tax authority rate products from TaxAuthorityRateProduct based partyTaxAuth
        var taxAuthorityRateProducts = await _context.TaxAuthorityRateProducts
            .Where(x => x.TaxAuthGeoId == partyTaxAuth.TaxAuthGeoId
                        && x.TaxAuthPartyId == partyTaxAuth.TaxAuthPartyId
            )
            .ToListAsync();
        return taxAuthorityRateProducts;
    }

    public async Task<List<TaxAuthorityRateProductDto>> GetTaxAuthorityRateProductsQuery(PartyTaxAuthInfo partyTaxAuth)
    {
        var taxAuthorityRateProducts = await (from taxProduct in _context.TaxAuthorityRateProducts
                join taxRateType in _context.TaxAuthorityRateTypes
                    on taxProduct.TaxAuthorityRateTypeId equals taxRateType.TaxAuthorityRateTypeId
                where taxProduct.TaxAuthGeoId == partyTaxAuth.TaxAuthGeoId
                      && taxProduct.TaxAuthPartyId == partyTaxAuth.TaxAuthPartyId
                select new TaxAuthorityRateProductDto
                {
                    TaxAuthorityRateSeqId = taxProduct.TaxAuthorityRateSeqId,
                    TaxAuthGeoId = taxProduct.TaxAuthGeoId,
                    TaxAuthPartyId = taxProduct.TaxAuthPartyId,
                    TaxAuthorityRateTypeId = taxProduct.TaxAuthorityRateTypeId,
                    ProductStoreId = taxProduct.ProductStoreId,
                    ProductCategoryId = taxProduct.ProductCategoryId,
                    TitleTransferEnumId = taxProduct.TitleTransferEnumId,
                    MinItemPrice = taxProduct.MinItemPrice ?? 0M, // Safely handle nullable decimal
                    MinPurchase = taxProduct.MinPurchase ?? 0M, // Safely handle nullable decimal
                    TaxShipping = taxProduct.TaxShipping,
                    TaxPercentage = taxProduct.TaxPercentage ?? 0M, // Safely handle nullable decimal
                    TaxPromotions = taxProduct.TaxPromotions,
                    FromDate = taxProduct.FromDate ?? DateTime.MinValue, // Handle nullable DateTime
                    ThruDate = taxProduct.ThruDate,
                    Description = taxProduct.Description,
                    IsTaxInShippingPrice = taxProduct.IsTaxInShippingPrice,
                    TaxAuthorityRateTypeDescription =
                        taxRateType.Description // Get description from the TaxAuthorityRateType
                })
            .ToListAsync();

        return taxAuthorityRateProducts;
    }


    private async Task<PartyTaxAuthInfo> GetTaxAuthInfoForProductStore()
    {
        var productStore = await _productStoreService.GetProductStoreForLoggedInUser();

        // get party tax auths from PartyTaxAuthInfo based gioId from productStore's VAT_TAX_AUTH_GEO_ID and taxAuthPartyId
        // from productStore's VAT_TAX_AUTH_PARTY_ID and thruDate is null and partyId equals pay_to_party from productStore
        var partyTaxAuth = await _context.PartyTaxAuthInfos
            .Where(x => x.TaxAuthGeoId == productStore.VatTaxAuthGeoId
                        && x.TaxAuthPartyId == productStore.VatTaxAuthPartyId
                        && x.ThruDate == null && x.PartyId == productStore.PayToPartyId)
            .FirstOrDefaultAsync();
        return partyTaxAuth;
    }

    //  helper method which determines, based on the state of the product,
    // how the product category condition should be built for further filtering.
    // Returns a Func<TaxAuthorityRateProduct, bool> predicate that can be used to filter
    // records (e.g., in-memory) based on the product's associated category IDs.
    // If the product is null, it returns a predicate that matches records with a null ProductCategoryId.
    public async Task<Func<TaxAuthorityRateProduct, bool>> SetProductCategoryCond(Domain.Product product)
    {
        // If the product is null, return a predicate that matches records with a null ProductCategoryId.
        if (product == null)
        {
            // Without product information, assume no category filtering.
            return x => x.ProductCategoryId == null;
        }

        // For variant products, also consider the virtual product's categories.
        string virtualProductId = null;
        if (product.IsVariant == "Y")
        {
            // Retrieve the virtual product ID associated with this variant.
            virtualProductId = await _productService.GetVariantVirtualId(product);
        }

        // Create a set to hold all product category IDs associated with the product (and its virtual product if applicable).
        var productCategoryIdSet = new HashSet<string>();

        // Build a delegate to filter ProductCategoryMember records.
        // If a virtual product exists, include records for either the original or the virtual product.
        Func<ProductCategoryMember, bool> productIdCond;
        if (!string.IsNullOrEmpty(virtualProductId))
        {
            productIdCond = pcm => pcm.ProductId == product.ProductId || pcm.ProductId == virtualProductId;
        }
        else
        {
            productIdCond = pcm => pcm.ProductId == product.ProductId;
        }

        // Get the current timestamp for effective date filtering.
        DateTime now = DateTime.UtcNow;

        // Query the ProductCategoryMember table using EF LINQ:
        // - Filter by product IDs (original and/or virtual).
        // - Filter by effective date: fromDate <= now and (thruDate is null or thruDate >= now).
        // - Select only the ProductCategoryId field.
        var pcmList = _context.ProductCategoryMembers
            .Where(pcm => productIdCond(pcm) &&
                          pcm.FromDate <= now &&
                          (pcm.ThruDate == null || pcm.ThruDate >= now))
            .Select(pcm => pcm.ProductCategoryId)
            .ToList();

        // Add each retrieved ProductCategoryId to the set.
        foreach (var categoryId in pcmList)
        {
            productCategoryIdSet.Add(categoryId);
        }

        // If no categories are found, return a predicate that matches records with a null ProductCategoryId.
        if (productCategoryIdSet.Count == 0)
        {
            return x => x.ProductCategoryId == null;
        }

        // Otherwise, return a predicate that matches records where either the ProductCategoryId is null
        // or is contained in the set of product category IDs.
        return x => x.ProductCategoryId == null || productCategoryIdSet.Contains(x.ProductCategoryId);
    }

    // This method retrieves a ProductPrice record for a given product, based on tax authority information.
// If the product store has a non-empty PrimaryStoreGroupId, it includes that in the query; otherwise, it omits it.
// It also applies effective date filtering (records whose FromDate is before now and ThruDate is null or after now),
// orders the results by descending FromDate (to get the latest record), and returns the first match.
    public async Task<ProductPrice> GetProductPrice(Domain.Product product, ProductStore productStore,
        string taxAuthGeoId, string taxAuthPartyId)
    {
        // Get the current time for effective date filtering.
        DateTime now = DateTime.UtcNow;

        // Check if the product store is provided and has a non-empty PrimaryStoreGroupId.
        if (productStore != null && !string.IsNullOrEmpty(productStore.PrimaryStoreGroupId))
        {
            // Business intent: When a primary store group is defined, use it to narrow down the product price query.
            return await _context.ProductPrices
                .Where(pp =>
                        pp.ProductId == product.ProductId && // Match the product ID.
                        pp.TaxAuthPartyId == taxAuthPartyId && // Match the tax authority party ID.
                        pp.TaxAuthGeoId == taxAuthGeoId && // Match the tax authority geographic ID.
                        pp.ProductPricePurposeId == "PURCHASE" && // Only consider prices for purchase.
                        pp.ProductStoreGroupId == productStore.PrimaryStoreGroupId && // Filter by the store group.
                        pp.FromDate <= now && // The record must be effective now...
                        (pp.ThruDate == null || pp.ThruDate >= now) // ...and not expired.
                )
                .OrderByDescending(pp => pp.FromDate) // Order by the most recent fromDate.
                .FirstOrDefaultAsync(); // Return the first matching record (or null if none).
        }
        else
        {
            // Business intent: For purchase orders without a primary store group, query without that filter.
            return await _context.ProductPrices
                .Where(pp =>
                    pp.ProductId == product.ProductId &&
                    pp.TaxAuthPartyId == taxAuthPartyId &&
                    pp.TaxAuthGeoId == taxAuthGeoId &&
                    pp.ProductPricePurposeId == "PURCHASE" &&
                    pp.FromDate <= now &&
                    (pp.ThruDate == null || pp.ThruDate >= now)
                )
                .OrderByDescending(pp => pp.FromDate)
                .FirstOrDefaultAsync();
        }
    }

    /// <summary>
    /// Asynchronously checks for tax exemption for the given adjustment value based on party tax information.
    /// It first queries for a matching PartyTaxAuthInfo record using the provided conditions. If found and marked exempt,
    /// it updates the adjustment accordingly. If no exemption is found, it recursively checks for a parent TaxAuthorityAssoc.
    /// </summary>
    /// <param name="adjValue">The OrderAdjustment object to update with tax exemption info.</param>
    /// <param name="billToPartyIdSet">A set of bill-to party IDs.</param>
    /// <param name="taxAuthGeoId">The tax authority geographic ID.</param>
    /// <param name="taxAuthPartyId">The tax authority party ID.</param>
    /// <param name="taxAmount">The tax amount (as a decimal).</param>
    /// <param name="nowTimestamp">The current timestamp for effective date filtering.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    public async Task HandlePartyTaxExempt(OrderAdjustment adjValue, HashSet<string> billToPartyIdSet,
        string taxAuthGeoId, string taxAuthPartyId, decimal taxAmount, DateTime nowTimestamp)
    {
        try
        {
            // Log the start of tax exemption check.
            _logger.LogInformation("Checking for tax exemption : " + taxAuthGeoId + " / " + taxAuthPartyId, "module");

            // Query the PartyTaxAuthInfo table for a record that matches:
            // - A partyId in the provided billToPartyIdSet.
            // - taxAuthGeoId equals the provided taxAuthGeoId.
            // - taxAuthPartyId equals the provided taxAuthPartyId.
            // - fromDate is less than or equal to now.
            // - (thruDate is null OR thruDate is greater than now).
            var partyTaxInfo = await _context.PartyTaxAuthInfos
                .Where(pti => billToPartyIdSet.Contains(pti.PartyId)
                              && pti.TaxAuthGeoId == taxAuthGeoId
                              && pti.TaxAuthPartyId == taxAuthPartyId
                              && pti.FromDate <= nowTimestamp
                              && (pti.ThruDate == null || pti.ThruDate > nowTimestamp))
                .OrderByDescending(pti => pti.FromDate)
                .FirstOrDefaultAsync();

            bool foundExemption = false;
            if (partyTaxInfo != null)
            {
                // Set the customer reference ID in the adjustment to the partyTaxId from the tax info.
                adjValue.CustomerReferenceId = partyTaxInfo.PartyTaxId;
                // If the tax info indicates exemption ("Y"), update the adjustment to reflect that no tax should be charged.
                if (partyTaxInfo.IsExempt == "Y")
                {
                    adjValue.Amount = 0m;
                    adjValue.ExemptAmount = taxAmount;
                    foundExemption = true;
                }
            }

            // If no exemption was found in the current jurisdiction, try to look up a parent TaxAuthorityAssoc.
            if (!foundExemption)
            {
                var taxAuthorityAssoc = await _context.TaxAuthorityAssocs
                    .Where(taa => taa.ToTaxAuthGeoId == taxAuthGeoId
                                  && taa.ToTaxAuthPartyId == taxAuthPartyId
                                  && taa.TaxAuthorityAssocTypeId == "EXEMPT_INHER"
                                  && taa.FromDate <= nowTimestamp
                                  && (taa.ThruDate == null || taa.ThruDate > nowTimestamp))
                    .OrderByDescending(taa => taa.FromDate)
                    .FirstOrDefaultAsync();

                // If a parent tax authority association is found, recursively check for exemption.
                if (taxAuthorityAssoc != null)
                {
                    await HandlePartyTaxExempt(adjValue, billToPartyIdSet, taxAuthorityAssoc.TaxAuthGeoId,
                        taxAuthorityAssoc.TaxAuthPartyId, taxAmount, nowTimestamp);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in HandlePartyTaxExemptAsync", "module");
            throw;
        }
    }

    /// <summary>
    /// Retrieves the tax adjustments applicable to a product, based on product, store, and tax authority settings.
    /// This method replicates the Java logic in getTaxAdjustments using EF and delegate predicates instead of Expression trees.
    /// </summary>
    /// <param name="product">The product entity (can be null).</param>
    /// <param name="productStore">The product store entity.</param>
    /// <param name="payToPartyId">The pay-to party ID; if null, it will be taken from the productStore's PayToPartyId.</param>
    /// <param name="billToPartyId">The bill-to party ID.</param>
    /// <param name="taxAuthoritySet">
    /// A set of TaxAuthority entities that define the applicable tax authorities.
    /// </param>
    /// <param name="itemPrice">The price per item.</param>
    /// <param name="itemQuantity">The quantity of items.</param>
    /// <param name="itemAmount">The total item amount (typically itemPrice * itemQuantity).</param>
    /// <param name="shippingAmount">The shipping amount (if any).</param>
    /// <param name="orderPromotionsAmount">The order promotions amount (if any).</param>
    /// <param name="weight">The weight to multiply the tax rate by (if null, defaults to 1).</param>
    /// <returns>
    /// A list of OrderAdjustment objects representing tax adjustments.
    /// In case of error, an empty list is returned.
    /// </returns>
    public async Task<List<OrderAdjustment>> GetTaxAdjustments(
        Domain.Product product,
        ProductStore productStore,
        string payToPartyId,
        string billToPartyId,
        HashSet<TaxAuthority> taxAuthoritySet,
        decimal itemPrice,
        decimal itemQuantity,
        decimal itemAmount,
        decimal? shippingAmount,
        decimal? orderPromotionsAmount,
        decimal? weight)
    {
        try
        {
            // Get the current timestamp for filtering by effective date.
            DateTime nowTimestamp = DateTime.UtcNow;
            List<OrderAdjustment> adjustments = new List<OrderAdjustment>();

            // Default weight to 1 if not provided.
            if (!weight.HasValue)
            {
                weight = 1m;
            }

            // If payToPartyId is null, attempt to retrieve it from productStore.
            if (string.IsNullOrEmpty(payToPartyId) && productStore != null)
            {
                payToPartyId = productStore.PayToPartyId;
            }

            // Build store condition: we require TaxAuthorityRateProduct records where either the ProductStoreId equals
            // the productStore's ID or is null.
            string storeId = productStore != null ? productStore.ProductStoreId : null;

            // Build the tax authority condition in the LINQ query.
            // (We’ll later check that either the record has _NA_ values or matches any TaxAuthority in the set.)

            // Retrieve the product category predicate from a helper.
            // We assume SetProductCategoryCond returns a Func<TaxAuthorityRateProduct, bool>.
            Func<TaxAuthorityRateProduct, bool> productCategoryPredicate = await SetProductCategoryCond(product);

            // If product is null and a shipping amount is provided, combine the predicate with a shipping condition.
            if (product == null && shippingAmount.HasValue)
            {
                var originalPredicate = productCategoryPredicate;
                productCategoryPredicate = x => originalPredicate(x) || (x.TaxShipping == null || x.TaxShipping == "Y");
            }

            // Similarly, if product is null and order promotions amount is provided, include promotions condition.
            if (product == null && orderPromotionsAmount.HasValue)
            {
                var originalPredicate = productCategoryPredicate;
                productCategoryPredicate = x =>
                    originalPredicate(x) || (x.TaxPromotions == null || x.TaxPromotions == "Y");
            }

            // Query the TaxAuthorityRateProduct table applying:
            // - Effective date filtering.
            // - Store condition: (ProductStoreId == storeId OR ProductStoreId == null).
            // - Tax authority condition: either the default (_NA_) or any record matching an authority in taxAuthoritySet.
            // - The product category predicate.
            // - Conditions for minItemPrice and minPurchase.
            decimal PERCENT_SCALE = 100m; // For percentage conversions.
            var lookupList = _context.TaxAuthorityRateProducts
                .Where(x =>
                    x.FromDate <= nowTimestamp &&
                    (x.ThruDate == null || x.ThruDate >= nowTimestamp) &&
                    // Store condition.
                    (((storeId != null && x.ProductStoreId == storeId) || x.ProductStoreId == null)) &&
                    // Tax authority condition.
                    (
                        (x.TaxAuthPartyId == "_NA_" && x.TaxAuthGeoId == "_NA_") ||
                        taxAuthoritySet.Any(ta =>
                            x.TaxAuthPartyId == ta.TaxAuthPartyId && x.TaxAuthGeoId == ta.TaxAuthGeoId)
                    ) &&
                    // Product category condition.
                    productCategoryPredicate(x) &&
                    // minItemPrice condition: either null or less than or equal to itemPrice.
                    (x.MinItemPrice == null || x.MinItemPrice <= itemPrice) &&
                    // minPurchase condition: either null or less than or equal to itemAmount.
                    (x.MinPurchase == null || x.MinPurchase <= itemAmount)
                )
                .OrderBy(x => x.MinItemPrice)
                .ThenBy(x => x.MinPurchase)
                .ThenBy(x => x.FromDate)
                .ToList();

            if (lookupList.Count == 0)
            {
                _logger.LogWarning("No TaxAuthorityRateProduct records found for the given conditions.");
                return adjustments;
            }

            // Process each TaxAuthorityRateProduct record.
            foreach (var taxAuthorityRateProduct in lookupList)
            {
                // Retrieve and adjust the tax rate.
                decimal taxRate = taxAuthorityRateProduct.TaxPercentage ?? 0m;
                taxRate *= weight.Value;

                // Determine the taxable amount.
                decimal taxable = 0m;
                if (product != null && (product.Taxable == null || product.Taxable == "Y"))
                {
                    taxable += itemAmount;
                }

                {
                    taxable += itemAmount;
                }

                if (shippingAmount.HasValue && (string.IsNullOrEmpty(taxAuthorityRateProduct.TaxShipping) ||
                                                taxAuthorityRateProduct.TaxShipping == "Y"))
                {
                    taxable += shippingAmount.Value;
                }

                if (orderPromotionsAmount.HasValue && (string.IsNullOrEmpty(taxAuthorityRateProduct.TaxPromotions) ||
                                                       taxAuthorityRateProduct.TaxPromotions == "Y"))
                {
                    taxable += orderPromotionsAmount.Value;
                }

                if (taxable == 0m)
                {
                    // Skip if nothing is taxable.
                    continue;
                }

                // Calculate tax amount: (taxable * taxRate) / 100.
                int salestaxCalcDecimals = 4;
                decimal taxAmount = Math.Round((taxable * taxRate) / PERCENT_SCALE, salestaxCalcDecimals,
                    MidpointRounding.AwayFromZero);

                string taxAuthGeoId = taxAuthorityRateProduct.TaxAuthGeoId;
                string taxAuthPartyId = taxAuthorityRateProduct.TaxAuthPartyId;

                // Retrieve GL account details for tax.
                var taxAuthorityGlAccount = _context.TaxAuthorityGlAccounts.FirstOrDefault(ta =>
                    ta.TaxAuthPartyId == taxAuthPartyId &&
                    ta.TaxAuthGeoId == taxAuthGeoId &&
                    ta.OrganizationPartyId == payToPartyId);
                string taxAuthGlAccountId = taxAuthorityGlAccount != null ? taxAuthorityGlAccount.GlAccountId : null;

                // Determine the ProductPrice record if available.
                ProductPrice productPrice = null;
                if (product != null && !string.IsNullOrEmpty(taxAuthPartyId) && !string.IsNullOrEmpty(taxAuthGeoId))
                {
                    productPrice =
                        await GetProductPrice(product, productStore, taxAuthGeoId, taxAuthPartyId);
                    if (productPrice == null)
                    {
                        var virtualProduct = await _productService.GetParentProduct(product.ProductId);
                        if (virtualProduct != null)
                        {
                            productPrice = await GetProductPrice(virtualProduct, productStore, taxAuthGeoId,
                                taxAuthPartyId);
                        }
                    }
                }

                // Create an OrderAdjustment object for this tax adjustment.
                OrderAdjustment taxAdjValue = new OrderAdjustment();
                taxAdjValue.OrderAdjustmentTypeId = "SALES_TAX";
                decimal discountedSalesTax = 0m;
                if (productPrice != null && productPrice.TaxInPrice == "Y" && itemQuantity != 0m)
                {
                    taxAdjValue.OrderAdjustmentTypeId = "VAT_TAX";
                    int calcDecimals = 2;
                    decimal taxAmountIncludedInFullPrice =
                        Math.Round(
                            itemPrice - Math.Round(itemPrice / (1 + (taxRate / PERCENT_SCALE)), calcDecimals,
                                MidpointRounding.AwayFromZero), calcDecimals, MidpointRounding.AwayFromZero) *
                        itemQuantity;
                    decimal netItemPrice = itemAmount / itemQuantity;
                    decimal netTax = (netItemPrice - Math.Round(netItemPrice / (1 + (taxRate / PERCENT_SCALE)),
                        calcDecimals, MidpointRounding.AwayFromZero)) * itemQuantity;
                    discountedSalesTax = netTax - taxAmountIncludedInFullPrice;
                    taxAdjValue.AmountAlreadyIncluded = taxAmountIncludedInFullPrice;
                    taxAdjValue.Amount = 0m;
                }
                else
                {
                    taxAdjValue.Amount = taxAmount;
                }

                taxAdjValue.SourcePercentage = taxRate;
                taxAdjValue.TaxAuthorityRateSeqId = taxAuthorityRateProduct.TaxAuthorityRateSeqId;
                taxAdjValue.PrimaryGeoId = taxAuthGeoId;
                taxAdjValue.Comments = taxAuthorityRateProduct.Description;
                if (!string.IsNullOrEmpty(taxAuthPartyId))
                {
                    taxAdjValue.TaxAuthPartyId = taxAuthPartyId;
                }

                if (!string.IsNullOrEmpty(taxAuthGlAccountId))
                {
                    taxAdjValue.OverrideGlAccountId = taxAuthGlAccountId;
                }

                if (!string.IsNullOrEmpty(taxAuthGeoId))
                {
                    taxAdjValue.TaxAuthGeoId = taxAuthGeoId;
                }

                // Check for party tax exemptions if billToPartyId and taxAuthGeoId are provided.
                if (!string.IsNullOrEmpty(billToPartyId) && !string.IsNullOrEmpty(taxAuthGeoId))
                {
                    HashSet<string> billToPartyIdSet = new HashSet<string> { billToPartyId };
                    var partyRelationshipList = _context.PartyRelationships
                        .Where(pr => pr.PartyIdTo == billToPartyId && pr.PartyRelationshipTypeId == "GROUP_ROLLUP" &&
                                     pr.FromDate <= nowTimestamp &&
                                     (pr.ThruDate == null || pr.ThruDate >= nowTimestamp))
                        .ToList();
                    foreach (var partyRelationship in partyRelationshipList)
                    {
                        billToPartyIdSet.Add(partyRelationship.PartyIdFrom);
                    }

                    await HandlePartyTaxExempt(taxAdjValue, billToPartyIdSet, taxAuthGeoId, taxAuthPartyId, taxAmount,
                        nowTimestamp);
                }
                else
                {
                    _logger.LogInformation(
                        "Tax calculation done without billToPartyId or taxAuthGeoId; no tax exemptions applied.");
                }

                if (discountedSalesTax < 0)
                {
                    var taxAdjValueNegative = new OrderAdjustment();
                    // Inline copy of all fields from taxAdjValue.
                    taxAdjValueNegative.OrderAdjustmentTypeId = taxAdjValue.OrderAdjustmentTypeId;
                    taxAdjValueNegative.Amount = taxAdjValue.Amount;
                    taxAdjValueNegative.AmountAlreadyIncluded = taxAdjValue.AmountAlreadyIncluded;
                    taxAdjValueNegative.SourcePercentage = taxAdjValue.SourcePercentage;
                    taxAdjValueNegative.TaxAuthorityRateSeqId = taxAdjValue.TaxAuthorityRateSeqId;
                    taxAdjValueNegative.PrimaryGeoId = taxAdjValue.PrimaryGeoId;
                    taxAdjValueNegative.Comments = taxAdjValue.Comments;
                    taxAdjValueNegative.TaxAuthPartyId = taxAdjValue.TaxAuthPartyId;
                    taxAdjValueNegative.OverrideGlAccountId = taxAdjValue.OverrideGlAccountId;
                    taxAdjValueNegative.TaxAuthGeoId = taxAdjValue.TaxAuthGeoId;
                    // Now set the negative fields.
                    taxAdjValueNegative.AmountAlreadyIncluded = discountedSalesTax;
                    adjustments.Add(taxAdjValueNegative);
                }

                adjustments.Add(taxAdjValue);

                // If a ProductPrice record exists and meets conditions, calculate VAT price correction.
                if (productPrice != null && itemQuantity != 0m &&
                    productPrice.PriceWithTax.HasValue && productPrice.TaxInPrice != "Y")
                {
                    decimal priceWithTaxValue = productPrice.PriceWithTax.Value;
                    decimal priceValue = productPrice.Price.HasValue ? productPrice.Price.Value : 0m;
                    decimal baseSubtotal = priceValue * itemQuantity;
                    decimal baseTaxAmount = Math.Round((baseSubtotal * taxRate) / PERCENT_SCALE, salestaxCalcDecimals,
                        MidpointRounding.AwayFromZero);

                    decimal enteredTotalPriceWithTax = priceWithTaxValue * itemQuantity;
                    decimal calcedTotalPriceWithTax = baseSubtotal + baseTaxAmount;
                    if (enteredTotalPriceWithTax != calcedTotalPriceWithTax)
                    {
                        decimal correctionAmount = enteredTotalPriceWithTax - calcedTotalPriceWithTax;
                        OrderAdjustment correctionAdjValue = new OrderAdjustment();
                        correctionAdjValue.TaxAuthorityRateSeqId = taxAuthorityRateProduct.TaxAuthorityRateSeqId;
                        correctionAdjValue.Amount = correctionAmount;
                        correctionAdjValue.OrderAdjustmentTypeId = "VAT_PRICE_CORRECT";
                        correctionAdjValue.PrimaryGeoId = taxAuthGeoId;
                        correctionAdjValue.Comments = taxAuthorityRateProduct.Description;
                        if (!string.IsNullOrEmpty(taxAuthPartyId))
                        {
                            correctionAdjValue.TaxAuthPartyId = taxAuthPartyId;
                        }

                        if (!string.IsNullOrEmpty(taxAuthGlAccountId))
                        {
                            correctionAdjValue.OverrideGlAccountId = taxAuthGlAccountId;
                        }

                        if (!string.IsNullOrEmpty(taxAuthGeoId))
                        {
                            correctionAdjValue.TaxAuthGeoId = taxAuthGeoId;
                        }

                        adjustments.Add(correctionAdjValue);
                    }
                }
            }

            return adjustments;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Problems looking up tax rates: " + e.ToString());
            return new List<OrderAdjustment>();
        }
    }

    /// <summary>
    /// Calculates tax for display based on product and store settings. This method replicates the logic of the Ofbiz
    /// service "rateProductTaxCalcForDisplay". It returns a ServiceResult containing a TaxCalcResult on success,
    /// or an error message on failure.
    /// </summary>
    /// <param name="productStoreId">Identifier for the product store.</param>
    /// <param name="billToPartyId">Billing party identifier (may be null or empty).</param>
    /// <param name="productId">Identifier for the product.</param>
    /// <param name="quantity">Quantity of the product (if null, defaults to 1).</param>
    /// <param name="basePrice">Base price of the product.</param>
    /// <param name="shippingPrice">Shipping price (if any) to be added to the base price.</param>
    /// <param name="locale">Locale used for error messages.</param>
    /// <returns>A ServiceResult containing the tax calculation result or an error message.</returns>
    public async Task<ServiceResult<TaxCalcResult>> RateProductTaxCalc(
        string productStoreId,
        string billToPartyId,
        string productId,
        decimal? quantity,
        decimal basePrice,
        decimal? shippingPrice)
    {
        try
        {
            // Define shared constants (mirroring the original Java code).
            decimal ONE_BASE = 1m; // Multiplicative identity.
            decimal ZERO_BASE = 0m; // Zero value.
            int salestaxCalcDecimals = 4; // Precision for intermediate tax calculations.
            int salestaxFinalDecimals = 2; // Final rounding precision.
            MidpointRounding salestaxRounding = MidpointRounding.AwayFromZero; // Rounding mode ("HalfUp").

            // If quantity is null, default to ONE_BASE (i.e., 1).
            if (!quantity.HasValue)
            {
                quantity = ONE_BASE;
            }

            // Calculate the total amount (basePrice * quantity).
            decimal amount = basePrice * quantity.Value;

            // Initialize tax-related variables.
            decimal taxTotal = ZERO_BASE; // Total accumulated tax.
            decimal taxPercentage = ZERO_BASE; // Cumulative tax percentage.
            decimal priceWithTax = basePrice; // Start with base price.
            // If shippingPrice is provided, add it to the price-with-tax.
            if (shippingPrice.HasValue)
            {
                priceWithTax += shippingPrice.Value;
            }

            // Retrieve the Product entity using EF (simulating the Ofbiz query).
            var product = _context.Products.FirstOrDefault(p => p.ProductId == productId);

            // Retrieve the ProductStore entity for the given productStoreId.
            var productStore = _context.ProductStores.FirstOrDefault(ps => ps.ProductStoreId == productStoreId);

            // If no ProductStore is found, return an error result.
            if (productStore == null)
            {
                return ServiceHelper.ReturnError<TaxCalcResult>("Could not find ProductStore with ID [" +
                                                                productStoreId + "] for tax calculation");
            }

            // Check if the ProductStore is configured to show prices with VAT tax.
            if (productStore.ShowPricesWithVatTax == "Y")
            {
                // Create a list to hold applicable TaxAuthority records.
                List<TaxAuthority> taxAuthorityList = new List<TaxAuthority>();

                // If no specific VAT tax authority party is set, retrieve all TaxAuthorities matching the store's VAT tax geographic ID.
                if (string.IsNullOrEmpty(productStore.VatTaxAuthPartyId))
                {
                    var taxAuthorityRawList = _context.TaxAuthorities
                        .Where(ta => ta.TaxAuthGeoId == productStore.VatTaxAuthGeoId)
                        .ToList();
                    taxAuthorityList.AddRange(taxAuthorityRawList);
                }
                else
                {
                    // If a specific VAT tax authority party is provided, retrieve that single TaxAuthority.
                    var taxAuthority = _context.TaxAuthorities.FirstOrDefault(ta =>
                        ta.TaxAuthGeoId == productStore.VatTaxAuthGeoId &&
                        ta.TaxAuthPartyId == productStore.VatTaxAuthPartyId);
                    if (taxAuthority != null)
                    {
                        taxAuthorityList.Add(taxAuthority);
                    }
                }

                // If no TaxAuthority records were found, return an error result indicating misconfiguration.
                if (taxAuthorityList.Count == 0)
                {
                    return ServiceHelper.ReturnError<TaxCalcResult>(
                        "Could not find any Tax Authorities for store with ID [" +
                        productStoreId + "] for tax calculation; the store settings may need to be corrected.");
                }

                // Convert the list to a HashSet.
                HashSet<TaxAuthority> taxAuthoritySet = new HashSet<TaxAuthority>(taxAuthorityList);

                // Retrieve tax adjustments by calling the helper GetTaxAdjustments.
                // The parameters include the product entity, productStore, a null value for payToPartyId,
                // billToPartyId, the set of TaxAuthority records, basePrice, quantity, total amount,
                // shippingPrice, ZERO_BASE (for order promotions amount), and null for weight.
                List<OrderAdjustment> taxAdjustmentList = await GetTaxAdjustments(
                    product,
                    productStore,
                    null,
                    billToPartyId,
                    taxAuthoritySet,
                    basePrice,
                    quantity.Value,
                    amount,
                    shippingPrice,
                    ZERO_BASE,
                    null // Weight: pass null to default to 1m inside the method
                );

                // If no tax adjustments are found, log a warning but continue.
                if (taxAdjustmentList.Count == 0)
                {
                    _logger.LogWarning("Could not find any Tax Authorities Rate Rules for store with ID [" +
                                       productStoreId
                                       + "], productId [" + productId + "], basePrice [" + basePrice + "], amount [" +
                                       amount
                                       + "], for tax calculation; the store settings may need to be corrected.");
                }

                // Process each tax adjustment.
                foreach (var taxAdjustment in taxAdjustmentList)
                {
                    // Only process adjustments with type "SALES_TAX".
                    if (taxAdjustment.OrderAdjustmentTypeId == "SALES_TAX")
                    {
                        // Add the adjustment's source percentage to the cumulative tax percentage.
                        taxPercentage += (decimal)taxAdjustment.SourcePercentage;
                        // Retrieve the adjustment amount.
                        decimal adjAmount = (decimal)taxAdjustment.Amount;
                        // Accumulate the tax total.
                        taxTotal += adjAmount;
                        // Calculate per-unit tax by dividing the adjustment amount by quantity, rounding as specified.
                        decimal taxPerUnit = Math.Round(adjAmount / quantity.Value, salestaxCalcDecimals,
                            salestaxRounding);
                        // Increase the price-with-tax by the per-unit tax.
                        priceWithTax += taxPerUnit;
                        // Log the tax adjustment details.
                        _logger.LogInformation("For productId [" + productId + "] added [" + taxPerUnit +
                                               "] of tax to price for geoId [" + taxAdjustment.TaxAuthGeoId +
                                               "], new price is [" + priceWithTax + "]");
                    }
                }
            }

            // Round the total tax and final price-with-tax to the defined final decimal precision.
            taxTotal = Math.Round(taxTotal, salestaxFinalDecimals, salestaxRounding);
            priceWithTax = Math.Round(priceWithTax, salestaxFinalDecimals, salestaxRounding);

            // Build the tax calculation result DTO.
            TaxCalcResult resultData = new TaxCalcResult
            {
                TaxTotal = taxTotal,
                TaxPercentage = taxPercentage,
                PriceWithTax = priceWithTax
            };

            // Return a successful service result containing the tax calculation result.
            return new ServiceResult<TaxCalcResult>
            {
                IsError = false,
                Data = resultData,
                ErrorMessage = null
            };
        }
        catch (Exception e)
        {
            // Log any errors that occur during the tax calculation process.
            _logger.LogError(e, "Data error getting tax settings: " + e.ToString());
            // Return an error result with a literal error message.
            return ServiceHelper.ReturnError<TaxCalcResult>("Accounting tax setting error: " + e.ToString());
        }
    }
}