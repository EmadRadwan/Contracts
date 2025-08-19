using Application.Facilities;
using Application.Order.Orders;
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;
using System.Linq;
using Application.Catalog.ProductStores; // <-- Add this


namespace Application.Catalog.Products;

public interface IProductService
{
    Task<decimal> GetOrderItemTotalCost(OrderItemDto2 orderItem);
    Task<string> GetProductTypeId(string productId);
    Task<bool> CheckIsPhysicalProduct(OrderItemDto2 orderItem);
    Task<decimal> CalculateServicePrice(string productId, string vehicleId);

    Task<CommonItemDto> CalculatePromoProductDiscount(string productPromoId, int quantity, decimal unitPrice,
        string productId);

    Task<string> GetVariantVirtualId(Domain.Product variantProduct);
    Task<Domain.Product> GetParentProduct(string productId);
}

public class ProductService : IProductService
{
    private readonly DataContext _context;
    private readonly IFacilityService _facilityService;
    private readonly ILogger _logger;

    public ProductService(DataContext context, IFacilityService facilityService, ILogger<ProductService> logger,
        IProductStoreService productStoreService)
    {
        _context = context;
        _facilityService = facilityService;
        _logger = logger;
    }


    public async Task<decimal> GetOrderItemTotalCost(OrderItemDto2 orderItem)
    {
        // get inventory item detail
        var inventoryItemDetail = await _facilityService.GetInventoryItemDetail(orderItem);

        // get inventory item
        var inventoryItem = await _facilityService.GetInventoryItem(inventoryItemDetail.InventoryItemId);

        var partyAccountingPreference = await _context.PartyAcctgPreferences.SingleOrDefaultAsync(x =>
            x.PartyId == inventoryItem.OwnerPartyId);

        // calculate product cost based on cogs method -- hard coded to average cost for now
        // todo: consider other cogs methods
        // cogs SIMPLE_AVG_COST depends on having defined record in ProductAverageCost for each product,
        // if not defined, it will use unit cost from inventoryItem

        var productAverageCost = await _context.ProductAverageCosts.SingleOrDefaultAsync(x =>
            x.ProductId == orderItem.ProductId);

        var productCost = productAverageCost != null ? productAverageCost.AverageCost : inventoryItem.UnitCost;
        var totalAmount = productCost * orderItem.Quantity;

        return totalAmount ?? 0;
    }

    public async Task<bool> CheckIsPhysicalProduct(OrderItemDto2 orderItem)
    {
        // get isPhysical from ProductType table using ProductId from orderItem and linking Products and ProductType tables
        var isPhysical = await _context.Products
            .Where(x => x.ProductId == orderItem.ProductId)
            .Select(a => a.ProductType.IsPhysical)
            .SingleOrDefaultAsync();

        return isPhysical == "Y";
    }

    public async Task<decimal> CalculateServicePrice(string productId, string vehicleId)
    {
        try
        {
            // get vehicle
            var vehicle = await _context.Vehicles.FirstOrDefaultAsync(x => x.VehicleId == vehicleId);

            if (vehicle == null)
            {
                _logger.LogError("Vehicle not found with ID: {VehicleId}", vehicleId);
                return 0; // or handle the error in an appropriate way
            }

            // get the hourly rate for the service
            var serviceRate = await _context.ServiceRates.FirstOrDefaultAsync(x => x.MakeId == vehicle.MakeId
                && x.ModelId == vehicle.ModelId);

            if (serviceRate == null)
            {
                _logger.LogError("Service rate not found for Make ID: {MakeId}, Model ID: {ModelId}",
                    vehicle.MakeId, vehicle.ModelId);
                return 0; // or handle the error in an appropriate way
            }

            // get standard rate in minutes for the service / make / model
            var serviceSpecification = await _context.ServiceSpecifications.FirstOrDefaultAsync(x =>
                x.ProductId == productId && x.ModelId == vehicle.ModelId && x.MakeId == vehicle.MakeId);

            if (serviceSpecification == null)
            {
                _logger.LogError("Service specification not found for Product ID: {ProductId}, " +
                                 "Make ID: {MakeId}, Model ID: {ModelId}", productId,
                    vehicle.MakeId, vehicle.ModelId);
                return 0; // or handle the error in an appropriate way
            }

            // calculate service price by multiplying the hourly rate by the standard time in minutes
            var hourlyRate = serviceRate.Rate;
            var standardTimeInMinutes = serviceSpecification.StandardTimeInMinutes;
            var servicePrice = hourlyRate * standardTimeInMinutes / 60;
            return servicePrice ?? 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while calculating service price.");
            return 0; // or handle the error in an appropriate way
        }
    }

    public async Task<CommonItemDto> CalculatePromoProductDiscount(string productPromoId, int quantity,
        decimal unitListPrice, string productId)
    {
        var result = new CommonItemDto();
        // get promo
        var promo = await _context.ProductPromos
            .Where(x => x.ProductPromoId == productPromoId)
            .SingleOrDefaultAsync();

        var productPromoCond = await _context.ProductPromoConds
            .Where(x => x.ProductPromoId == productPromoId)
            .SingleOrDefaultAsync();

        var promoAction = await _context.ProductPromoActions
            .Where(x => x.ProductPromoId == productPromoId)
            .SingleOrDefaultAsync();

        var targetProduct = promoAction.ProductId ?? productId;

        // check if promoAction has a productId


        var targetProductListPrice = productId == promoAction.ProductId
            ? unitListPrice
            : _context.ProductPrices
                .Where(prc =>
                    prc.ProductId == promoAction.ProductId && prc.ProductPriceTypeId == "LIST_PRICE")
                .OrderByDescending(prc => prc.FromDate)
                .Select(prc => prc.Price)
                .FirstOrDefault() ?? 0;


        var targetProductDefaultPrice = productId == promoAction.ProductId
            ? unitListPrice
            : _context.ProductPrices
                .Where(prc =>
                    prc.ProductId == promoAction.ProductId && prc.ProductPriceTypeId == "DEFAULT_PRICE")
                .OrderByDescending(prc => prc.FromDate)
                .Select(prc => prc.Price)
                .FirstOrDefault() ?? 0;


        // check if productPromoCond is not null
        if (productPromoCond != null && promoAction != null)
            // check if productPromoCond.InputParamEnumId is PPIP_QUANTITY
            switch (productPromoCond.InputParamEnumId)
            {
                case "PPIP_PRODUCT_QUANT":
                    switch (productPromoCond.OperatorEnumId)
                    {
                        case "PPC_GTE":
                            if (quantity >= int.Parse(productPromoCond.CondValue!))
                            {
                                // get promoAction amount
                                var promoActionAmount = promoAction.Amount;
                                var promoActionQuantity = promoAction.Quantity;

                                // calculate promoAction amount based on orderItem.UnitListPrice
                                // and multiply by -1 to make it negative value and render the value
                                // in two decimal places
                                var promoActionAmountBasedOnOrderItemUnitDefaultPrice =
                                    targetProductDefaultPrice * promoActionAmount * promoActionQuantity
                                    / 100 * -1;
                                if (promoActionAmountBasedOnOrderItemUnitDefaultPrice != null)
                                {
                                    promoActionAmountBasedOnOrderItemUnitDefaultPrice =
                                        Math.Round((decimal)promoActionAmountBasedOnOrderItemUnitDefaultPrice, 2);

                                    result.ProductId = targetProduct;
                                    // get product name from Products
                                    result.ProductName = await _context.Products
                                        .Where(x => x.ProductId == targetProduct)
                                        .Select(x => x.ProductName)
                                        .FirstOrDefaultAsync();
                                    result.PromoAmount = promoActionAmountBasedOnOrderItemUnitDefaultPrice;
                                    result.Quantity = promoAction.Quantity;
                                    result.PromoText = promo?.PromoText ?? string.Empty;
                                    result.PromoActionAmount = promoActionAmount;
                                    result.DefaultPrice = targetProductDefaultPrice;
                                    result.ListPrice = targetProductListPrice;
                                    result.ProductPromoId = productPromoId;
                                    result.ProductPromoRuleId = promoAction.ProductPromoRuleId;
                                    result.ProductPromoActionSeqId = promoAction.ProductPromoActionSeqId;
                                }


                                result.ResultMessage = "Success";
                            }
                            else
                            {
                                result.ResultMessage = "Quantity does not match";
                            }

                            break;
                        case "PPC_LT":
                            // Code to execute if the conditions are met
                            break;
                        // Add additional cases for other possible values of OperatorEnumId
                    }

                    break;
                case "PPIP_SOME_OTHER_VALUE":
                    // Code to execute if the conditions are met
                    break;
                // Add additional cases for other possible values of InputParamEnumId
            }

        return result;
    }

    public async Task<string> GetProductTypeId(string productId)
    {
        var productTypeId = await _context.Products
            .Where(x => x.ProductId == productId)
            .Select(x => x.ProductTypeId)
            .SingleOrDefaultAsync();

        return productTypeId;
    }

    public async Task<string> GetVariantVirtualId(Domain.Product variantProduct)
    {
        try
        {
            // Call the function to retrieve variant virtual associations for the given product.
            // This method is expected to return a list of associations that link a variant to its virtual product.
            List<ProductAssoc> productAssocs = await GetVariantVirtualAssocs(variantProduct);

            // If the list of associations is null, it indicates that no virtual associations were found.
            // In such a case, return null as there is no virtual product id available.
            if (productAssocs == null)
            {
                return null;
            }

            // Retrieve the first association from the list.
            // This simulates the behavior of EntityUtil.getFirst() in Ofbiz, returning the first element.
            ProductAssoc productAssoc = productAssocs.FirstOrDefault();

            // If an association is found, return its productId.
            // The productId here represents the virtual product id corresponding to the variant.
            if (productAssoc != null)
            {
                return productAssoc.ProductId;
            }

            // If no association is found in the list, return null to indicate that there is no virtual product id.
            return null;
        }
        catch (Exception ex)
        {
            // Log the exception with detailed error information for debugging purposes.
            _logger.LogError(ex, "Error retrieving variant virtual id for the given product.");
            // Rethrow the exception to propagate the error to the caller.
            throw;
        }
    }

    // This function retrieves the virtual associations for a variant product.
    // It returns a list of ProductAssoc objects that represent the associations
    // where the product is linked to its virtual product, filtering by the effective date.
    public async Task<List<ProductAssoc>> GetVariantVirtualAssocs(Domain.Product variantProduct)
    {
        try
        {
            // Check if the provided variantProduct is not null and its "isVariant" flag equals "Y".
            // Business perspective: Only variant products are expected to have virtual associations.
            if (variantProduct != null && variantProduct.IsVariant == "Y")
            {
                // Get the current timestamp for date filtering.
                DateTime nowTimestamp = DateTime.UtcNow;

                // Query the database using EF LINQ to retrieve related associations.
                // This simulates the Ofbiz getRelated call:
                // It retrieves associations where:
                //   - The association's ProductId matches the variant product's ID.
                //   - The productAssocTypeId is "PRODUCT_VARIANT" (indicating a variant association).
                //   - The association is effective as of now (FromDate <= now and (ThruDate is null or ThruDate >= now)).
                List<ProductAssoc> productAssocs = await _context.ProductAssocs
                    .Where(pa => pa.ProductId == variantProduct.ProductId
                                 && pa.ProductAssocTypeId == "PRODUCT_VARIANT"
                                 && pa.FromDate <= nowTimestamp
                                 && (pa.ThruDate == null || pa.ThruDate >= nowTimestamp))
                    .ToListAsync();

                // Return the filtered list of associations.
                return productAssocs;
            }

            // If the product is null or not a variant, return null as no associations are applicable.
            return null;
        }
        catch (Exception ex)
        {
            // Log the exception details for debugging purposes.
            _logger.LogError(ex, "Error retrieving variant virtual associations for the given product.");
            // Rethrow the exception to propagate the error to the caller.
            throw;
        }
    }
    
    // This asynchronous method retrieves the parent product for a given product ID.
// It first attempts to find associated ProductAssoc records with the type "PRODUCT_VARIANT".
// If none are found, it falls back to "UNIQUE_ITEM" associations.
// It then returns the MainProduct from the first association found.
// If the productId is null or empty, a warning is logged.
public async Task<Domain.Product> GetParentProduct(string productId)
{
    // Initialize the parentProduct variable to null.
    Domain.Product parentProduct = null;
    
    // If the provided productId is null or empty, log a warning.
    if (string.IsNullOrEmpty(productId))
    {
        _logger.LogWarning("Bad product id");
    }
    
    try
    {
        // Get the current time for filtering by effective date.
        DateTime now = DateTime.UtcNow;
        
        // Query the ProductAssoc records where:
        // - productIdTo equals the given productId.
        // - productAssocTypeId equals "PRODUCT_VARIANT".
        // - The record is effective (FromDate <= now and (ThruDate is null or ThruDate >= now)).
        // Order the results by descending FromDate.
        var virtualProductAssocs = await _context.ProductAssocs
            .Where(pa => pa.ProductIdTo == productId &&
                         pa.ProductAssocTypeId == "PRODUCT_VARIANT" &&
                         pa.FromDate <= now &&
                         (pa.ThruDate == null || pa.ThruDate >= now))
            .OrderByDescending(pa => pa.FromDate)
            .ToListAsync();
        
        // If no PRODUCT_VARIANT associations are found, try to find UNIQUE_ITEM associations.
        if (!virtualProductAssocs.Any())
        {
            virtualProductAssocs = await _context.ProductAssocs
                .Where(pa => pa.ProductIdTo == productId &&
                             pa.ProductAssocTypeId == "UNIQUE_ITEM" &&
                             pa.FromDate <= now &&
                             (pa.ThruDate == null || pa.ThruDate >= now))
                .OrderByDescending(pa => pa.FromDate)
                .ToListAsync();
        }
        
        // If any associations were found, retrieve the first one and then its MainProduct.
        if (virtualProductAssocs.Any())
        {
            var productAssoc = virtualProductAssocs.First();
            // Assumes that the ProductAssoc entity has a navigation property "MainProduct".
            parentProduct = productAssoc.Product;
        }
    }
    catch (Exception e)
    {
        // Wrap and rethrow any exceptions with additional context.
        throw new Exception("Entity Engine error getting Parent Product (" + e.Message + ")");
    }
    
    // Return the found parent product, or null if none were found.
    return parentProduct;
}

}