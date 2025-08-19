using API.Middleware;
using Application.Catalog.Products.Services.Cost;
using Application.Catalog.ProductStores;
using Application.Core;
using Application.Facilities;
using Application.Facilities.FacilityInventories;
using Application.Facilities.InventoryTransfer;
using Application.Manufacturing;
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Catalog.Products.Services.Inventory;

public interface IInventoryHelperService
{
    Task<Result<ProductInventoryTotals>> GetInventoryAvailableByFacility(string facilityId, string productId);

    Task<MktgPackagesAvailableResponse> GetMktgPackagesAvailable(string productId, string facilityId = null,
        string statusId = null);
}

public class InventoryHelperService : IInventoryHelperService
{
    private readonly DataContext _context;
    private readonly ILogger _logger;
    private readonly IProductHelperService _productHelperService;
    private readonly IInventoryService _inventoryService;

    public InventoryHelperService(DataContext context,
        ILogger<InventoryHelperService> logger,
        IProductHelperService productHelperService, IInventoryService inventoryService)
    {
        _context = context;
        _logger = logger;
        _productHelperService = productHelperService;
        _inventoryService = inventoryService;
    }

    public async Task<Result<ProductInventoryTotals>> GetInventoryAvailableByFacility(string facilityId,
        string productId)
    {
        var inventoryItems = await _context.InventoryItems
            .Where(ii => ii.FacilityId == facilityId && ii.ProductId == productId)
            .ToListAsync();

        decimal quantityOnHandTotal = 0;
        decimal availableToPromiseTotal = 0;
        decimal accountingQuantityTotal = 0;

        foreach (var item in inventoryItems)
        {
            if (string.IsNullOrEmpty(item.StatusId) ||
                item.StatusId == "INV_AVAILABLE" ||
                item.StatusId == "INV_NS_RETURNED" ||
                item.InventoryItemTypeId == "SERIALIZED_INV_ITEM")
            {
                quantityOnHandTotal += item.QuantityOnHandTotal ?? 0;
                availableToPromiseTotal += item.AvailableToPromiseTotal ?? 0;
                accountingQuantityTotal += item.AccountingQuantityTotal ?? 0;
            }
        }

        return Result<ProductInventoryTotals>.Success(new ProductInventoryTotals
        {
            QuantityOnHandTotal = quantityOnHandTotal,
            AvailableToPromiseTotal = availableToPromiseTotal,
            AccountingQuantityTotal = accountingQuantityTotal
        });
    }

    public async Task<MktgPackagesAvailableResponse> GetMktgPackagesAvailable(string productId,
        string facilityId = null, string statusId = null)
    {
        // Initialize the response
        var response = new MktgPackagesAvailableResponse
        {
            AvailableToPromiseTotal = 0,
            QuantityOnHandTotal = 0
        };

        // Get the product
        var product = await _context.Products.FirstOrDefaultAsync(p => p.ProductId == productId);
        if (product == null)
        {
            throw new Exception("Product not found.");
        }

        // Check if the product is a marketing package
        bool isMarketingPackage = await IsMarketingPackage(product.ProductTypeId);

        if (isMarketingPackage)
        {
            // If it's a marketing package, get the available quantity from associated components
            var associatedProductsResponse = await _productHelperService.ProdFindAssociatedByType(
                productId: productId,
                productIdTo: null,
                type: "MARKETING_PKG_COMPONENT",
                checkViewAllow: false,
                bidirectional: false,
                sortDescending: false);

            // Check if there are any associated products
            if (associatedProductsResponse != null && associatedProductsResponse.AssocProducts.Any())
            {
                // Get inventory availability for the associated products
                var inventoryResult = await GetProductInventoryAvailableFromAssocProducts(
                    associatedProductsResponse.AssocProducts,
                    facilityId,
                    statusId);

                // Set the totals in the response
                response.AvailableToPromiseTotal = inventoryResult.AvailableToPromiseTotal;
                response.QuantityOnHandTotal = inventoryResult.QuantityOnHandTotal;
            }
        }
        else
        {
            // If not a marketing package, get available inventory from facilities
            var productFacilities = await _context.ProductFacilities
                .Where(pf => pf.ProductId == productId)
                .ToListAsync();

            foreach (var facility in productFacilities)
            {
                if (facility.LastInventoryCount.HasValue)
                {
                    response.QuantityOnHandTotal += facility.LastInventoryCount.Value;
                }
            }
        }

        return response;
    }

    public async Task<ProductInventoryAvailableResponse> GetProductInventoryAvailableFromAssocProducts(
        List<ProductAssoc> assocProducts,
        string facilityId = null,
        string statusId = null)
    {
        var response = new ProductInventoryAvailableResponse
        {
            AvailableToPromiseTotal = 0,
            QuantityOnHandTotal = 0
        };

        if (assocProducts != null && assocProducts.Any())
        {
            decimal? minQuantityOnHandTotal = null;
            decimal? minAvailableToPromiseTotal = null;

            foreach (var productAssoc in assocProducts)
            {
                string productIdTo = productAssoc.ProductIdTo;
                decimal assocQuantity = productAssoc.Quantity ?? 1; // Default to 1.0 if no quantity is available

                // Log warning if no quantity is specified for associated product
                if (productAssoc.Quantity == null)
                {
                    _logger?.LogWarning(
                        $"ProductAssoc from [{productAssoc.ProductId}] to [{productIdTo}] has no quantity, assuming 1.0");
                }

                // Initialize resultOutput to store inventory data
                InventoryTotals resultOutput = null;

                // Determine the inventory available for the associated product
                if (!string.IsNullOrEmpty(facilityId))
                {
                    // Fetch inventory available by facility
                    var facilityResult = await GetInventoryAvailableByFacility(facilityId, productIdTo);
                    if (facilityResult.IsSuccess)
                    {
                        // Convert from ProductInventoryTotals to InventoryTotals
                        resultOutput = new InventoryTotals
                        {
                            QuantityOnHandTotal = facilityResult.Value.QuantityOnHandTotal,
                            AvailableToPromiseTotal = facilityResult.Value.AvailableToPromiseTotal,
                            AccountingQuantityTotal = facilityResult.Value.AccountingQuantityTotal
                        };
                    }
                    else
                    {
                        _logger?.LogError(
                            $"Failed to get inventory available by facility for productId {productIdTo} in facility {facilityId}");
                        continue;
                    }
                }
                else
                {
                    // Fetch product inventory available without specific facility
                    resultOutput =
                        await _inventoryService.GetProductInventoryAvailable(facilityId, productIdTo, statusId);
                }

                if (resultOutput != null)
                {
                    // Calculate QOH and ATP based on associated product's quantity
                    decimal currentQuantityOnHandTotal = (decimal)resultOutput.QuantityOnHandTotal;
                    decimal currentAvailableToPromiseTotal = (decimal)resultOutput.AvailableToPromiseTotal;

                    // Adjust quantities by dividing by assocQuantity
                    decimal tmpQuantityOnHandTotal = currentQuantityOnHandTotal / assocQuantity;
                    decimal tmpAvailableToPromiseTotal = currentAvailableToPromiseTotal / assocQuantity;

                    // Update the minimum QOH and ATP totals if current values are smaller
                    if (minQuantityOnHandTotal == null || tmpQuantityOnHandTotal < minQuantityOnHandTotal)
                    {
                        minQuantityOnHandTotal = tmpQuantityOnHandTotal;
                    }

                    if (minAvailableToPromiseTotal == null || tmpAvailableToPromiseTotal < minAvailableToPromiseTotal)
                    {
                        minAvailableToPromiseTotal = tmpAvailableToPromiseTotal;
                    }

                    _logger?.LogInformation(
                        $"productIdTo = {productIdTo}, assocQuantity = {assocQuantity}, current QOH = {currentQuantityOnHandTotal}, " +
                        $"current ATP = {currentAvailableToPromiseTotal}, min QOH = {minQuantityOnHandTotal}, min ATP = {minAvailableToPromiseTotal}");
                }
            }

            // Set the final QOH and ATP as the minimum values encountered
            response.QuantityOnHandTotal = minQuantityOnHandTotal ?? 0;
            response.AvailableToPromiseTotal = minAvailableToPromiseTotal ?? 0;
        }

        return response;
    }

    public async Task<bool> IsMarketingPackage(string productTypeId)
    {
        var parentTypeId = productTypeId;

        // Traverse the product type hierarchy to check if 'MARKETING_PKG' is a parent
        while (!string.IsNullOrEmpty(parentTypeId))
        {
            var productType = await _context.ProductTypes
                .Where(pt => pt.ProductTypeId == parentTypeId)
                .Select(pt => pt.ParentTypeId)
                .FirstOrDefaultAsync();

            if (productType == null)
            {
                break;
            }

            if (productType == "MARKETING_PKG")
            {
                return true;
            }

            parentTypeId = productType;
        }

        return false;
    }

    public async Task<string> IsStoreInventoryAvailable(string productStoreId, string productId, decimal quantity)
    {
        var productStore = await _context.ProductStores.FirstOrDefaultAsync(ps => ps.ProductStoreId == productStoreId);
        var product = await _context.Products.FirstOrDefaultAsync(p => p.ProductId == productId);

        if (productStore == null || product == null)
        {
            throw new Exception("ProductStore or Product not found");
        }

        // If the product is a SERVICE or DIGITAL_GOOD, inventory check is not required
        if (product.ProductTypeId == "SERVICE" || product.ProductTypeId == "DIGITAL_GOOD")
        {
            _logger?.LogInformation(
                $"Product {product.ProductId} is of type {product.ProductTypeId}, skipping inventory check.");
            return "Y";
        }

        // If the ProductStore is set to not check inventory, skip the inventory check
        if (productStore.CheckInventory == "N")
        {
            _logger?.LogInformation(
                $"ProductStore {productStore.ProductStoreId} is set to NOT check inventory, skipping inventory check.");
            return "Y";
        }

        // Check if ProductStore has oneInventoryFacility
        if (productStore.OneInventoryFacility == "Y")
        {
            if (string.IsNullOrEmpty(productStore.InventoryFacilityId))
            {
                throw new Exception("Inventory Facility ID is not set in ProductStore.");
            }

            // Determine if the product is a marketing package
            bool isMarketingPkg = await IsMarketingPackage(product.ProductTypeId);

            // Call appropriate service based on whether it's a marketing package or a regular product
            decimal availableToPromiseTotal;
            if (isMarketingPkg)
            {
                var mktgPackagesAvailableResponse =
                    await GetMktgPackagesAvailable(productId, productStore.InventoryFacilityId);
                availableToPromiseTotal = mktgPackagesAvailableResponse.AvailableToPromiseTotal;
            }
            else
            {
                var productInventoryTotals =
                    await GetInventoryAvailableByFacility(productId, productStore.InventoryFacilityId);
                availableToPromiseTotal = productInventoryTotals.Value.AvailableToPromiseTotal;
            }

            // Check if available inventory meets or exceeds the requested quantity
            if (availableToPromiseTotal >= quantity)
            {
                _logger?.LogInformation(
                    $"Sufficient inventory available in facility {productStore.InventoryFacilityId} for product {productId}, available quantity: {availableToPromiseTotal}");
                return "Y";
            }
            else
            {
                _logger?.LogInformation(
                    $"Insufficient inventory in facility {productStore.InventoryFacilityId} for product {productId}, available quantity: {availableToPromiseTotal}");
                return "N";
            }
        }
        else
        {
            // Handle multiple facilities
            var productStoreFacilities = await _context.ProductStoreFacilities
                .Where(psf => psf.ProductStoreId == productStore.ProductStoreId)
                .OrderBy(psf => psf.SequenceNum)
                .ToListAsync();

            foreach (var productStoreFacility in productStoreFacilities)
            {
                bool isMarketingPkg = await IsMarketingPackage(product.ProductTypeId);

                decimal availableToPromiseTotal;
                if (isMarketingPkg)
                {
                    var mktgPackagesAvailableResponse =
                        await GetMktgPackagesAvailable(productId, productStore.InventoryFacilityId);
                    availableToPromiseTotal = mktgPackagesAvailableResponse.AvailableToPromiseTotal;
                }
                else
                {
                    var productInventoryTotals =
                        await GetInventoryAvailableByFacility(productId, productStore.InventoryFacilityId);
                    availableToPromiseTotal = productInventoryTotals.Value.AvailableToPromiseTotal;
                }

                if (availableToPromiseTotal >= quantity)
                {
                    _logger?.LogInformation(
                        $"Sufficient inventory available in facility {productStoreFacility.FacilityId} for product {productId}, available quantity: {availableToPromiseTotal}");
                    return "Y";
                }
            }

            _logger?.LogInformation($"Insufficient inventory for product {productId} across all facilities.");
            return "N";
        }
    }

    public async Task<string> IsStoreInventoryAvailableOrNotRequired(string productStoreId, string productId,
        decimal quantity)
    {
        // Simulate fetching product store and product data (in place of the original Minilang logic)
        var productStore = await _context.ProductStores.FirstOrDefaultAsync(ps => ps.ProductStoreId == productStoreId);
        var product = await _context.Products.FirstOrDefaultAsync(p => p.ProductId == productId);

        if (productStore == null || product == null)
        {
            throw new Exception("ProductStore or Product not found.");
        }

        // Check if inventory is required
        bool requireInventory = IsStoreInventoryRequiredInline(productStore, product);

        if (!requireInventory)
        {
            return "Y"; // Inventory not required
        }

        // Call the actual service to check inventory availability
        var available = await IsStoreInventoryAvailable(productStoreId, productId, quantity);

        return available == "Y" ? "Y" : "N"; // Return "Y" if available, otherwise "N"
    }

    public bool IsStoreInventoryRequiredInline(ProductStore productStore, Product product)
    {
        // First check if the product has a 'requireInventory' setting
        var requireInventory = product.RequireInventory;

        // If the product's 'requireInventory' is null or empty, check the productStore's setting
        if (string.IsNullOrEmpty(requireInventory))
        {
            requireInventory = productStore.RequireInventory;
        }

        // If both the product and productStore's 'requireInventory' are empty, default to "Y"
        if (string.IsNullOrEmpty(requireInventory))
        {
            requireInventory = "Y";
        }

        // Return true if 'requireInventory' is "Y", otherwise false
        return requireInventory == "Y";
    }

    /// <summary>
    /// Checks if a given product type has a specific parent type.
    /// </summary>
    /// <param name="productTypeId">The product type ID to check.</param>
    /// <param name="parentTypeId">The parent type ID to verify against.</param>
    /// <returns>True if the product type has the specified parent type; otherwise, false.</returns>
    private async Task<bool> HasParentType(string productTypeId, string parentTypeId)
    {
        // Assuming ProductType has a ParentTypeId field
        var productType = await _context.ProductTypes.FirstOrDefaultAsync(pt => pt.ProductTypeId == productTypeId);
        if (productType != null)
        {
            return productType.ParentTypeId == parentTypeId;
        }

        return false;
    }


    /// <summary>
    /// Retrieves a list of outstanding purchase orders for a given product.
    /// An outstanding purchase order is one that is not completed, cancelled, or rejected.
    /// </summary>
    /// <param name="productId">The product identifier.</param>
    /// <returns>List of outstanding purchase orders.</returns>
    public async Task<List<OrderHeader>> GetOutstandingPurchaseOrders(string productId)
    {
        try
        {
            var purchaseOrders = await _context.OrderHeaders
                .Include(oh => oh.OrderItems)
                .Where(oh => oh.OrderTypeId == "PURCHASE_ORDER"
                             && oh.StatusId != "ORDER_COMPLETED"
                             && oh.StatusId != "ORDER_CANCELLED"
                             && oh.StatusId != "ORDER_REJECTED"
                             && oh.OrderItems.Any(oi => oi.ProductId == productId
                                                        && oi.StatusId != "ITEM_COMPLETED"
                                                        && oi.StatusId != "ITEM_CANCELLED"
                                                        && oi.StatusId != "ITEM_REJECTED"))
                .OrderByDescending(oh => oh.OrderDate) // Assuming OrderDate is a proxy for EstimatedDeliveryDate
                .ThenBy(oh => oh.OrderId)
                .ToListAsync();

            return purchaseOrders;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                $"Unable to find outstanding purchase orders for product [{productId}] due to {ex.Message} - returning null");
            return null;
        }
    }

    /// <summary>
    /// Calculates the total outstanding purchased quantity for a given product.
    /// </summary>
    /// <param name="productId">The product identifier.</param>
    /// <returns>Total outstanding purchased quantity.</returns>
    public async Task<decimal> GetOutstandingPurchasedQuantity(string productId)
    {
        decimal qty = 0m;
        var purchaseOrders = await GetOutstandingPurchaseOrders(productId);
        if (purchaseOrders == null || !purchaseOrders.Any())
        {
            return qty;
        }

        foreach (var order in purchaseOrders)
        {
            foreach (var item in order.OrderItems.Where(oi => oi.ProductId == productId))
            {
                if (item.Quantity != 0m)
                {
                    decimal itemQuantity = (decimal)item.Quantity;
                    decimal cancelQuantity = item.CancelQuantity ?? 0m;
                    itemQuantity -= cancelQuantity;
                    if (itemQuantity >= 0m)
                    {
                        qty += itemQuantity;
                    }
                }
            }
        }

        return qty;
    }

    public async Task<List<FacilityInventoryRecord>> GetProductInventoryAndFacilitySummary(
        DateTime? checkTime,
        string facilityId,
        string productId,
        decimal? minimumStock,
        string statusId)
    {
        string module = "GetProductInventoryAndFacilitySummaryAsync";
        var records = new List<FacilityInventoryRecord>();

        try
        {
            // Validate required parameters
            if (string.IsNullOrEmpty(productId))
            {
                _logger.LogError($"{module}: ProductId is required.");
                return records;
            }

            // Retrieve the product with its type
            var product = await _context.Products
                .AsNoTracking()
                .Include(p => p.ProductType)
                .FirstOrDefaultAsync(p => p.ProductId == productId);

            if (product == null)
            {
                _logger.LogWarning($"{module}: Product with ID {productId} not found.");
                return records;
            }

            // Check if the product type has a parent type "MARKETING_PKG" using helper function
            bool isMarketingPkg = await HasParentType(product.ProductTypeId, "MARKETING_PKG");

            List<FacilityInventoryRecord> tempRecords;

            if (isMarketingPkg)
            {
                // Retrieve marketing package inventory using ProductAssoc (BILL_OF_MATERIALS)
                var componentProductIds = await _context.ProductAssocs
                    .AsNoTracking()
                    .Where(pa => pa.ProductId == productId && pa.ProductAssocTypeId == "BILL_OF_MATERIALS")
                    .Select(pa => pa.ProductIdTo)
                    .ToListAsync();

                // Query InventoryItems for components
                var inventoryQuery = _context.InventoryItems
                    .AsNoTracking()
                    .Include(inv => inv.Facility)
                    .Where(inv => componentProductIds.Contains(inv.ProductId));

                if (!string.IsNullOrEmpty(facilityId))
                {
                    inventoryQuery = inventoryQuery.Where(inv => inv.FacilityId == facilityId);
                }

                // Aggregate quantities
                tempRecords = await inventoryQuery
                    .GroupBy(inv => new { inv.FacilityId, inv.Facility.FacilityName })
                    .Select(grp => new FacilityInventoryRecord
                    {
                        FacilityId = grp.Key.FacilityId,
                        ProductId = productId,
                        ProductName = "Marketing Package", // Adjust as needed
                        QuantityOnHandTotal = grp.Sum(g => g.QuantityOnHand),
                        AvailableToPromiseTotal = grp.Sum(g => g.AvailableToPromise),
                        QuantityUomId = product.QuantityUomId
                    })
                    .ToListAsync();
            }
            else
            {
                // Retrieve inventory data for standard products
                var inventoryQuery = _context.InventoryItems
                    .AsNoTracking()
                    .Include(inv => inv.Facility)
                    .Where(inv => inv.ProductId == productId);

                if (!string.IsNullOrEmpty(facilityId))
                {
                    inventoryQuery = inventoryQuery.Where(inv => inv.FacilityId == facilityId);
                }

                // Aggregate quantities
                tempRecords = await inventoryQuery
                    .GroupBy(inv => new { inv.FacilityId, inv.Facility.FacilityName })
                    .Select(grp => new FacilityInventoryRecord
                    {
                        FacilityId = grp.Key.FacilityId,
                        ProductId = productId,
                        ProductName = product.ProductName,
                        QuantityOnHandTotal = grp.Sum(g => g.QuantityOnHand),
                        AvailableToPromiseTotal = grp.Sum(g => g.AvailableToPromise),
                        QuantityUomId = product.QuantityUomId
                    })
                    .ToListAsync();
            }

            // Assign prices
            var productPrices = await _context.ProductPrices
                .AsNoTracking()
                .Where(pp => pp.ProductId == productId)
                .OrderByDescending(pp => pp.FromDate)
                .ToListAsync();

            foreach (var record in tempRecords)
            {
                var defaultPrice = productPrices.FirstOrDefault(pp => pp.ProductPriceTypeId == "DEFAULT_PRICE");
                var wholesalePrice = productPrices.FirstOrDefault(pp => pp.ProductPriceTypeId == "WHOLESALE_PRICE");
                var listPrice = productPrices.FirstOrDefault(pp => pp.ProductPriceTypeId == "LIST_PRICE");

                // Assign prices based on productPriceTypeId
                if (defaultPrice != null)
                {
                    record.DefaultPrice = defaultPrice.Price;
                }
                else
                {
                    // Assign default if specific type not found
                    record.DefaultPrice = 0m; // or any default value
                }

                if (wholesalePrice != null)
                {
                    record.WholeSalePrice = wholesalePrice.Price;
                }
                else
                {
                    record.WholeSalePrice = 0m; // or any default value
                }

                if (listPrice != null)
                {
                    record.ListPrice = listPrice.Price;
                }
                else
                {
                    record.ListPrice = 0m; // or any default value
                }
            }

            // Calculate UsageQuantity if CheckTime is provided
            if (checkTime.HasValue)
            {
                decimal usageQuantity = 0m;

                // Calculate sales usage
                var salesUsage = await (from oi in _context.OrderItems
                    join oh in _context.OrderHeaders on oi.OrderId equals oh.OrderId
                    join itIss in _context.ItemIssuances on new { oi.OrderId, oi.OrderItemSeqId } equals new
                        { itIss.OrderId, itIss.OrderItemSeqId }
                    join invIt in _context.InventoryItems on itIss.InventoryItemId equals invIt.InventoryItemId
                    where invIt.ProductId == productId &&
                          (string.IsNullOrEmpty(facilityId) ? true : invIt.FacilityId == facilityId) &&
                          new List<string>
                              { "ORDER_COMPLETED", "ORDER_APPROVED", "ORDER_HELD" }.Contains(oh.StatusId) &&
                          oh.OrderTypeId == "SALES_ORDER" &&
                          oh.OrderDate >= checkTime.Value
                    select oi.Quantity).ToListAsync();

                usageQuantity += (decimal)salesUsage.Sum();

                // Calculate production usage
                var productionUsage = await (from wea in _context.WorkEffortInventoryAssigns
                    join we in _context.WorkEfforts on wea.WorkEffortId equals we.WorkEffortId
                    join ii in _context.InventoryItems on wea.InventoryItemId equals ii.InventoryItemId
                    where ii.ProductId == productId &&
                          (string.IsNullOrEmpty(facilityId) ? true : ii.FacilityId == facilityId) &&
                          we.WorkEffortTypeId == "PROD_ORDER_TASK" &&
                          we.ActualCompletionDate >= checkTime.Value
                    select wea.Quantity).ToListAsync();

                usageQuantity += (decimal)productionUsage.Sum();

                // Assign UsageQuantity to each record
                foreach (var record in tempRecords)
                {
                    record.UsageQuantity = usageQuantity;
                }
            }

            // Get QuantityOnOrder using helper function
            var quantityOnOrder = await GetOutstandingPurchasedQuantity(productId);

            foreach (var record in tempRecords)
            {
                record.QuantityOnOrder = quantityOnOrder;
                record.OffsetQOHQtyAvailable = record.QuantityOnHandTotal - (minimumStock ?? 0m);
                record.OffsetATPQtyAvailable = record.AvailableToPromiseTotal - (minimumStock ?? 0m);
            }

            records = tempRecords;

            return records;
        }
        catch (Exception ex)
        {
            // Log the exception with detailed information
            _logger.LogError(ex, $"{module}: An unexpected error occurred while retrieving inventory summary.");

            // Return an empty list to ensure the application remains stable
            return new List<FacilityInventoryRecord>();
        }
    }
}