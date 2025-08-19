using API.Middleware;
using Application.Accounting.Services;
using Application.Catalog.Products.Services.Inventory;
using Application.Common;
using Application.Core;
using Application.Manufacturing;
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Persistence;
using Serilog;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using InvalidOperationException = System.InvalidOperationException;

namespace Application.Catalog.Products.Services.Cost;

public interface ICostService
{
    Task<decimal> GetProductCost(string productId, string currencyUomId, string costComponentTypePrefix);

    Task UpdateProductAverageCostOnReceiveInventory(string facilityId, decimal? quantityAccepted, string productId,
        string inventoryItemId);

    Task<decimal> GetProductAverageCost(InventoryItem inventoryItem);

    Task<List<BOMSimulationDto>> SimulateBomCost(string productId, decimal quantityToProduce, string currencyUomId);
    Task<decimal> CalculateProductCosts(string productId, string currencyUomId, string costComponentTypePrefix);

    Task<TaskTimeResult> GetEstimatedTaskTime(string workEffortId, string? productId = null, string? routingId = null,
        decimal? quantity = 1);

    Task CreateCostComponent(CostComponent costComponent);
    Task<string> CreateCostComponentCalc(CostComponentCalc costComponentCalc);
    Task<string> UpdateCostComponentCalc(CostComponentCalc costComponentCalc);
    Task<string> CreateWorkEffortCostCalc(WorkEffortCostCalc workEffortCostCalc);
    Task<string> UpdateWorkEffortCostCalc(WorkEffortCostCalc workEffortCostCalc);
}

public class CostService : ICostService
{
    private readonly IAcctgMiscService _acctgMiscService;
    private readonly ICommonService _commonService;
    private readonly DataContext _context;
    private readonly IUtilityService _utilityService;
    private readonly ILogger _logger;
    private readonly IRoutingService _routingService;
    private readonly IServiceProvider _serviceProvider;
    private readonly Lazy<IGeneralLedgerService> _generalLedgerService;


    public CostService(DataContext context, IAcctgMiscService acctgMiscService, ILogger<CostService> logger,
        ICommonService commonService, IUtilityService utilityService, IRoutingService routingService,
        IServiceProvider serviceProvider, Lazy<IGeneralLedgerService> generalLedgerService)
    {
        _context = context;
        _logger = logger;
        _commonService = commonService;
        _acctgMiscService = acctgMiscService;
        _utilityService = utilityService;
        _routingService = routingService;
        _serviceProvider = serviceProvider;
        _generalLedgerService = generalLedgerService;
    }


    public async Task<decimal> GetProductCost(string productId, string currencyUomId, string costComponentTypePrefix)
    {
        decimal productCost = 0;

        try
        {
            // 1) Retrieve cost components (filtered by date, prefix, currency, productId)
            var costComponentsTemp = await _utilityService.FindLocalOrDatabaseListAsync<CostComponent>(
                query => query.Where(cc =>
                    cc.FromDate <= DateTime.Now && (cc.ThruDate == null || cc.ThruDate >= DateTime.Now)),
                productId);

            var costComponents = costComponentsTemp
                .Where(cc => cc.ProductId == productId
                             && cc.CostComponentTypeId.StartsWith(costComponentTypePrefix + "_")
                             && cc.CostUomId == currencyUomId)
                .ToList();

            // Sum up cost
            productCost = (decimal)costComponents.Sum(cc => cc.Cost);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching cost components for product {productId}: {ex.Message}");
            throw;
        }

        try
        {
            // 2) If cost is 0 => check if product is a variant, call getProductCost on the virtual parent
            if (productCost == 0)
            {
                // We check the ProductAssoc for type "PRODUCT_VARIANT", where productId is the variant, productIdTo is the virtual
                var assoc = await _context.ProductAssocs
                    .Where(pa => pa.ProductId == productId
                                 && pa.ProductAssocTypeId == "PRODUCT_VARIANT")
                    .OrderBy(pa => pa.FromDate) // or any ordering you prefer
                    .FirstOrDefaultAsync();

                if (assoc != null)
                {
                    // The parent is assoc.ProductIdTo
                    var virtualProductId = assoc.ProductIdTo;

                    // Recursively call the same method
                    decimal virtualCost =
                        await GetProductCost(virtualProductId, currencyUomId, costComponentTypePrefix);

                    // If the virtual product has a cost, we use it
                    productCost = virtualCost;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error checking variant costs for product {productId}: {ex.Message}");
            throw;
        }

        try
        {
            // 3) If cost is still zero => SupplierProduct (same currency)
            if (productCost == 0)
            {
                var priceCosts = await _context.SupplierProducts
                    .Where(sp => sp.ProductId == productId && sp.CurrencyUomId == currencyUomId)
                    .OrderBy(sp => sp.SupplierPrefOrderId).ThenBy(sp => sp.LastPrice)
                    .ToListAsync();

                // Filter by date
                priceCosts = priceCosts
                    .Where(sp => sp.AvailableFromDate <= DateTime.Now
                                 && (sp.AvailableThruDate == null || sp.AvailableThruDate >= DateTime.Now))
                    .OrderByDescending(sp => sp.AvailableFromDate)
                    .ToList();

                if (priceCosts.Any())
                {
                    var priceCost = priceCosts.First();
                    if (priceCost.LastPrice.HasValue) productCost = priceCost.LastPrice.Value;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"Error retrieving product cost from SupplierProduct (same currency) for product {productId}: {ex.Message}");
            throw;
        }

        try
        {
            // 4) If cost is still zero => SupplierProduct (different currency) => convert
            if (productCost == 0)
            {
                var priceCosts = _context.SupplierProducts
                    .Where(sp => sp.ProductId == productId)
                    .OrderBy(sp => sp.SupplierPrefOrderId).ThenBy(sp => sp.LastPrice)
                    .ToList();

                // Filter by date
                priceCosts = priceCosts
                    .Where(sp => sp.AvailableFromDate <= DateTime.Now
                                 && (sp.AvailableThruDate == null || sp.AvailableThruDate >= DateTime.Now))
                    .ToList();

                if (priceCosts.Any())
                {
                    var priceCost = priceCosts.First();
                    if (priceCost.LastPrice.HasValue)
                    {
                        var originalValue = priceCost.LastPrice.Value;
                        var uomId = priceCost.CurrencyUomId;
                        var uomIdTo = currencyUomId;

                        try
                        {
                            // Attempt to convert from uomId => uomIdTo
                            var convertedValue = await _commonService.ConvertUom(
                                uomId,
                                uomIdTo,
                                DateTime.Now,
                                originalValue,
                                null
                            );

                            if (convertedValue != 0)
                            {
                                productCost = (decimal)convertedValue;
                            }
                            else
                            {
                                Console.WriteLine(
                                    $"Currency conversion failed from {uomId} to {uomIdTo} for product {productId}");
                                productCost = 0;
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(
                                $"Error during currency conversion for product {productId}: {ex.Message}");
                            throw;
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"Error retrieving product cost from SupplierProduct (different currency) for product {productId}: {ex.Message}");
            throw;
        }

        // 5) If cost is still zero => optionally check ProductPrice 
        // (Commented out in your Ofbiz snippet, so we'll leave it out or only mention it)
        // if (productCost == 0) { ... }

        return productCost;
    }

    public async Task UpdateProductAverageCostOnReceiveInventory(string facilityId, decimal? quantityAccepted,
        string productId, string inventoryItemId)
    {
        var stamp = DateTime.UtcNow;
        decimal? averageCost = null;


        var inventoryItem = _context.ChangeTracker.Entries<InventoryItem>()
            .FirstOrDefault(e => e.Entity.InventoryItemId == inventoryItemId &&
                                 e.State == EntityState.Added && e.Entity.FacilityId == facilityId)!.Entity;


        // Retrieve organizationPartyId
        var organizationPartyId = inventoryItem?.OwnerPartyId;

        // Check and retrieve organizationPartyId from facility if not available
        if (string.IsNullOrEmpty(organizationPartyId))
        {
            var facility = await _context.Facilities
                .Where(f => f.FacilityId == facilityId)
                .FirstOrDefaultAsync();

            organizationPartyId = facility?.OwnerPartyId;
        }

        var productAverageCost = await _context.ProductAverageCosts
            .Where(pac => pac.ProductId == productId
                          && pac.FacilityId == facilityId
                          && pac.ProductAverageCostTypeId == "SIMPLE_AVG_COST"
                          && pac.OrganizationPartyId == organizationPartyId
                          && pac.FromDate <= DateTime.Now
                          && (pac.ThruDate == null || pac.ThruDate >= DateTime.Now))
            .OrderByDescending(pac => pac.FromDate) // Assuming you want the latest record if there are multiple
            .FirstOrDefaultAsync();

        if (productAverageCost == null)
        {
            averageCost = inventoryItem.UnitCost;
        }
        else
        {
            // Expire existing one and calculate average cost
            productAverageCost.ThruDate = stamp;
            productAverageCost.LastUpdatedStamp = stamp;

            // call GetProductInventoryAvailable to get the quantityOnHandTotal
            var inventoryTotals = await GetProductInventoryAvailable(facilityId, productId, "", inventoryItem);

            // Retrieve quantityOnHandTotal
            var quantityOnHandTotal = inventoryTotals.QuantityOnHandTotal;

            // get the old product quantity
            var oldProductQuantity = quantityOnHandTotal - quantityAccepted;

            /// <summary>
            /// Weighted cost is a calculated average cost that takes into account different quantities or values
            /// associated with individual components. It is often used in inventory management or financial
            /// calculations to find an average cost that reflects the proportional impact of each component.
            /// </summary>
            /// <remarks>
            /// The formula for calculating the weighted cost is:
            /// \[ \text{Weighted Cost} = \frac{\sum_{i=1}^{n} (\text{Component}_i \times \text{Weight}_i)}{\sum_{i=1}^{n} \text{Weight}_i \]
            /// 
            /// Where:
            /// - \( \text{Component}_i \) represents the value or cost of the i-th component.
            /// - \( \text{Weight}_i \) represents the weight assigned to the i-th component.
            /// - \( n \) is the total number of components.
            /// 
            /// In the context of inventory management:
            /// \[ \text{Weighted Average Cost} = \frac{\text{Cost of Existing Inventory} + \text{Cost of Newly Received Inventory}}{\text{Total Quantity on Hand}} \]
            /// 
            /// The "Weight" for the existing inventory is its quantity before the new items are received.
            /// The "Weight" for the newly received inventory is its quantity.
            /// 
            /// This calculation ensures that the average cost reflects the proportional contribution of each part
            /// of the inventory to the total cost. If you have different costs associated with different quantities
            /// of items, the weighted cost provides a more accurate representation of the average cost per unit,
            /// considering the quantities involved.
            /// </remarks>
            averageCost =
                (productAverageCost.AverageCost * oldProductQuantity + inventoryItem.UnitCost * quantityAccepted) /
                quantityOnHandTotal;
        }

        // Create new ProductAverageCost record
        var newProductAverageCost = new ProductAverageCost
        {
            ProductId = productId,
            FacilityId = facilityId,
            ProductAverageCostTypeId = "SIMPLE_AVG_COST",
            OrganizationPartyId = organizationPartyId,
            AverageCost = averageCost,
            FromDate = stamp,
            CreatedStamp = stamp,
            LastUpdatedStamp = stamp
        };
        _context.ProductAverageCosts.Add(newProductAverageCost);
    }

    public async Task<decimal> GetProductAverageCost(InventoryItem inventoryItem)
    {
        decimal unitCost;

        var partyAccountingPreference =
            await _acctgMiscService.GetPartyAccountingPreferences(inventoryItem.OwnerPartyId);

        if (partyAccountingPreference.CogsMethodId == "COGS_AVG_COST")
        {
            // TODO: handle productAverageCostTypeId for WEIGHTED_AVG_COST and MOVING_AVG_COST
            var productAverageCost = await _context.ProductAverageCosts
                .Where(pac => pac.ProductAverageCostTypeId == "SIMPLE_AVG_COST" &&
                              pac.OrganizationPartyId == inventoryItem.OwnerPartyId &&
                              pac.ProductId == inventoryItem.ProductId &&
                              pac.FacilityId == inventoryItem.FacilityId
                              && pac.FromDate <= DateTime.Now
                              && (pac.ThruDate == null || pac.ThruDate >= DateTime.Now))
                .OrderByDescending(pac => pac.FromDate) // Assuming you want the latest record if there are multiple
                .FirstOrDefaultAsync();

            if (productAverageCost != null)
                unitCost = (decimal)productAverageCost.AverageCost;
            else
                unitCost = (decimal)inventoryItem.UnitCost;
        }
        else
        {
            unitCost = (decimal)inventoryItem.UnitCost;
        }

        return unitCost;
    }

    private async Task<InventoryTotals> GetProductInventoryAvailable(string facilityId, string productId,
        string statusId = "", InventoryItem? inventoryItem = null)
    {
        var inventoryItemsQuery = _context.InventoryItems
            .Where(item => item.FacilityId == facilityId && item.ProductId == productId);

        if (!string.IsNullOrEmpty(statusId))
            inventoryItemsQuery = inventoryItemsQuery.Where(item => item.StatusId == statusId);
        else
            inventoryItemsQuery = inventoryItemsQuery
                .Where(item =>
                    item.StatusId == "INV_AVAILABLE" ||
                    item.StatusId == "INV_NS_RETURNED" ||
                    item.InventoryItemTypeId == "SERIALIZED_INV_ITEM" ||
                    item.InventoryItemTypeId == null);

        // Execute the query
        var inventoryItems = await inventoryItemsQuery.ToListAsync();

        // Add the inventoryItem to the list if it is not null
        if (inventoryItem != null)
            inventoryItems.Add(inventoryItem);

        // Summing up quantities based on conditions
        var availableToPromiseTotal = inventoryItems.Sum(item => item.AvailableToPromiseTotal);
        var quantityOnHandTotal = inventoryItems.Sum(item => item.QuantityOnHandTotal);
        var accountingQuantityTotal = inventoryItems.Sum(item => item.AccountingQuantityTotal);

        // Return the results
        return new InventoryTotals
        {
            AvailableToPromiseTotal = availableToPromiseTotal,
            QuantityOnHandTotal = quantityOnHandTotal,
            AccountingQuantityTotal = accountingQuantityTotal
        };
    }

    // A simple class to hold QOH data per product
    public class ProductQOH
    {
        public string ProductId { get; set; }
        public decimal QOH { get; set; }
    }

    public async Task<List<BOMSimulationDto>> SimulateBomCost(string productId, decimal quantityToProduce,
        string currencyUomId)
    {
        var result = new List<BOMSimulationDto>();
        var currentDate = DateTime.Now;

        // Fetch BOM components
        var bomComponents = await _context.ProductAssocs
            .Where(pa => pa.ProductId == productId &&
                         pa.ProductAssocTypeId == "MANUF_COMPONENT" &&
                         pa.FromDate <= currentDate &&
                         (pa.ThruDate >= currentDate || pa.ThruDate == null))
            .OrderBy(pa => pa.SequenceNum)
            .ToListAsync();

        // Fetch product details for the main product
        var mainProduct = await _context.Products
            .FirstOrDefaultAsync(p => p.ProductId == productId);
        var mainProductName = mainProduct?.ProductName ?? "Unknown";

        // Get list of all BOM product IDs (including the main product)
        var bomProductIds = bomComponents.Select(c => c.ProductIdTo).Append(productId).ToList();

        // Fetch QOH for each product involved in the BOM
        var qohData = await (from prd in _context.Products
            //join prdf in _context.ProductFacilities on prd.ProductId equals prdf.ProductId
            join inv in _context.InventoryItems on prd.ProductId equals inv.ProductId
            join uom in _context.Uoms on prd.QuantityUomId equals uom.UomId
            join fac in _context.Facilities on inv.FacilityId equals fac.FacilityId
            where bomProductIds.Contains(prd.ProductId)
            group inv by new
            {
                prd.ProductId,
                prd.ProductName,
                fac.FacilityName,
                //prdf.MinimumStock,
                //prdf.ReorderQuantity
            }
            into grp
            select new
            {
                grp.Key.ProductId,
                grp.Key.ProductName,
                QOH = grp.Sum(g => g.QuantityOnHandTotal)
            }).ToListAsync();

        var qohList = qohData.Select(x => new ProductQOH
        {
            ProductId = x.ProductId,
            QOH = (decimal)x.QOH
        }).ToList();

        var mainProdUom = await _context.Uoms
            .FirstOrDefaultAsync(u => u.UomId == mainProduct!.QuantityUomId);

        var mainProductQOH = qohList.FirstOrDefault(q => q.ProductId == productId)?.QOH ?? 0;

        // Add main product entry
        result.Add(new BOMSimulationDto
        {
            ProductLevel = 0,
            ProductId = productId,
            ProductName = mainProductName,
            Quantity = quantityToProduce,
            UomId = mainProdUom?.UomId ?? "Unknown",
            UomDescription = mainProdUom?.Description ?? "Unknown",
            Cost = 0, // Will sum component costs
            QOH = mainProductQOH
        });

        foreach (var component in bomComponents)
        {
            // Check if the component is a sub-assembly (has its own BOM)
            bool isSubAssembly = await _context.ProductAssocs
                .AnyAsync(pa => pa.ProductId == component.ProductIdTo &&
                                pa.ProductAssocTypeId == "MANUF_COMPONENT" &&
                                pa.FromDate <= currentDate &&
                                (pa.ThruDate >= currentDate || pa.ThruDate == null));

            // Calculate the required quantity
            var componentQuantity = component.Quantity * quantityToProduce;

            // Fetch product details for the component
            var componentProduct = await _context.Products
                .FirstOrDefaultAsync(p => p.ProductId == component.ProductIdTo);
            var componentProductName = componentProduct?.ProductName ?? "Unknown";
            var uom = await _context.Uoms
                .FirstOrDefaultAsync(u => u.UomId == componentProduct.QuantityUomId);

            // Look up QOH for this component
            var componentQOH = qohList.FirstOrDefault(q => q.ProductId == component.ProductIdTo)?.QOH ?? 0;

            // Prepare DTO
            var dto = new BOMSimulationDto
            {
                ProductLevel = isSubAssembly ? 1 : 1, // Same level, but flagged
                ProductId = component.ProductIdTo,
                ProductName = componentProductName,
                UomId = uom?.UomId ?? "Unknown",
                UomDescription = uom?.Description ?? "Unknown",
                Quantity = (decimal)componentQuantity,
                Cost = 0, // Default, updated below if not sub-assembly
                QOH = componentQOH,
                IsTemplateLink = isSubAssembly, // New flag for frontend
                Instruction = component.Instruction // Include for "For a batch of 20 tablets"
            };

            if (!isSubAssembly)
            {
                // Only calculate cost for direct components
                decimal? componentCost = await GetProductCost(component.ProductIdTo, currencyUomId, "EST_STD_MAT");
                dto.Cost = componentCost ?? 0;
                result[0].Cost += (componentCost ?? 0) * componentQuantity;
            }

            // Add component to result (template link included)
            result.Add(dto);
        }

        return result;
    }

    public async Task<decimal> CalculateProductCosts(string productId, string currencyUomId,
        string costComponentTypePrefix)
    {
        decimal totalProductsCost = 0m;
        decimal totalTaskCost = 0m;
        decimal totalOtherTaskCost = 0m; // Initialize variable for other task costs
        decimal totalCost = 0m;

        try
        {
            // Cancel existing costs for the product
            await CancelCostComponents(productId, $"{costComponentTypePrefix}_ROUTE_COST", currencyUomId);
            await CancelCostComponents(productId, $"{costComponentTypePrefix}_MAT_COST", currencyUomId);

            // Fetch manufacturing components
            var manufacturingComponentsResult = await _routingService.GetManufacturingComponents(productId);
            var components = manufacturingComponentsResult.ComponentsMap;

            if (components != null && components.Any())
            {
                foreach (var component in components)
                {
                    var productCost = await GetProductCost(component.ProductId, currencyUomId, "EST_STD_MAT");
                    totalProductsCost += component.Quantity * productCost;
                }
            }
            else
            {
                var productCost = await GetProductCost(productId, currencyUomId, "EST_STD_MAT");
                totalProductsCost += productCost;
            }

            // Retrieve product routing tasks ordered by priority
            var tasks = await _routingService.GetProductRouting(productId);

            var taskCostResult = new TaskCostResult();
            var accumulatedCosts =
                new Dictionary<string, decimal>(); // Dictionary to accumulate task costs by CostComponentTypeId


            if (tasks != null && tasks.Tasks != null && tasks.Tasks.Any())
            {
                foreach (var task in tasks.Tasks)
                {
                    taskCostResult = await GetTaskCost(task.WorkEffortIdTo, currencyUomId, productId,
                        task.WorkEffortIdFrom);
                    totalTaskCost += taskCostResult.TaskCost;

                    // Handle any additional costs associated with the task if available
                    if (taskCostResult.CostsByType != null)
                    {
                        foreach (var costByType in taskCostResult.CostsByType)
                        {
                            // Accumulate cost by CostComponentTypeId, accounting for duplicate keys
                            if (accumulatedCosts.ContainsKey(costByType.CostComponentTypeId))
                            {
                                accumulatedCosts[costByType.CostComponentTypeId] +=
                                    costByType.TotalCostComponentCost ?? 0;
                            }
                            else
                            {
                                accumulatedCosts[costByType.CostComponentTypeId] =
                                    costByType.TotalCostComponentCost ?? 0;
                            }
                        }
                    }
                }
            }

            // Calculate totalOtherTaskCost from accumulated costs
            totalOtherTaskCost = accumulatedCosts.Values.Sum();


            // Compute the total cost of the product by summing material costs, task costs, and other task costs
            totalCost = totalProductsCost + totalTaskCost + totalOtherTaskCost;

            if (totalTaskCost > 0)
            {
                await RecreateCostComponent(productId, currencyUomId, $"{costComponentTypePrefix}_ROUTE_COST",
                    totalTaskCost);
            }

            if (totalProductsCost > 0)
            {
                await RecreateCostComponent(productId, currencyUomId, $"{costComponentTypePrefix}_MAT_COST",
                    totalProductsCost);
            }

            // Handle additional cost types from accumulated task cost results
            foreach (var accumulatedCost in accumulatedCosts)
            {
                await RecreateCostComponent(productId, currencyUomId,
                    $"{costComponentTypePrefix}_{accumulatedCost.Key}", accumulatedCost.Value);
            }


            // Compute costs derived from CostComponentCalc records
            var productCostComponentCalcs = await _context.ProductCostComponentCalcs
                .Where(p => p.ProductId == productId
                            && p.FromDate <= DateTime.UtcNow && (p.ThruDate == null || p.ThruDate >= DateTime.UtcNow)
                )
                .OrderBy(p => p.SequenceNum)
                .ToListAsync();

            foreach (var productCostComponentCalc in productCostComponentCalcs)
            {
                var costComponentCalc =
                    await _context.CostComponentCalcs.FindAsync(productCostComponentCalc.CostComponentCalcId);
                var customMethod = await _context.CustomMethods.FindAsync(costComponentCalc.CostCustomMethodId);

                if (customMethod != null)
                {
                    var customMethodParameters = new CustomMethodParameters
                    {
                        ProductCostComponentCalc = productCostComponentCalc,
                        CostComponentCalc = costComponentCalc,
                        CurrencyUomId = currencyUomId,
                        CostComponentTypePrefix = costComponentTypePrefix,
                        BaseCost = totalCost
                    };

                    //var methodInfo = typeof(CustomMethodsService).GetMethod(customMethod.CustomMethodName);

                    // Inject CustomMethodsService through DI
                    var customMethodsService = _serviceProvider.GetRequiredService<CustomMethodsService>();

                    // Call the method directly without using reflection
                    decimal productCostAdjustment =
                        await customMethodsService.ProductCostPercentageFormula(customMethodParameters);

                    await RecreateCostComponent(productId, currencyUomId,
                        $"{costComponentTypePrefix}_{productCostComponentCalc.CostComponentTypeId}",
                        productCostAdjustment);

                    totalCost += productCostAdjustment;
                }
                else
                {
                    Console.WriteLine(
                        $"Unable to create cost component for cost component calc with id [{costComponentCalc.CostComponentCalcId}] because customMethod is not set");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error while computing costs from CostComponentCalc records: {ex.Message}");
            throw;
        }

        return totalCost;
    }

    private async Task CancelCostComponents(string productId, string costComponentTypeId, string costUomId)
    {
        var currentTimestamp = DateTime.UtcNow;

        // Find existing cost components based on the provided parameters
        var existingCosts = await _context.CostComponents
            .Where(c =>
                c.ProductId == productId &&
                c.CostUomId == costUomId &&
                c.CostComponentTypeId == costComponentTypeId &&
                (c.ThruDate == null || c.ThruDate >= currentTimestamp)
            )
            .ToListAsync();

        // Iterate through existing cost components
        foreach (var existingCost in existingCosts)
        {
            // Set the thruDate of the existing cost component to the current timestamp
            existingCost.ThruDate = currentTimestamp;
        }
    }


    public async Task<string> RecreateCostComponent(string productId, string costUomId, string costComponentTypeId,
        decimal? cost)
    {
        // Find existing CostComponents of the same type
        var existingCosts = await _context.CostComponents
            .Where(c => c.ProductId == productId && c.CostUomId == costUomId &&
                        c.CostComponentTypeId == costComponentTypeId &&
                        c.FromDate <= DateTime.UtcNow &&
                        (c.ThruDate == null || c.ThruDate >= DateTime.UtcNow))
            .ToListAsync();

        // Expire existing CostComponents
        foreach (var existingCost in existingCosts)
        {
            existingCost.ThruDate = DateTime.UtcNow;
        }

        // Create a new CostComponent
        var newCostComponent = new CostComponent
        {
            CostComponentId = Guid.NewGuid().ToString(), // Use a GUID for the ID
            ProductId = productId,
            CostUomId = costUomId,
            CostComponentTypeId = costComponentTypeId,
            Cost = cost,
            FromDate = DateTime.UtcNow,
            CreatedStamp = DateTime.UtcNow,
            LastUpdatedStamp = DateTime.UtcNow
        };

        await CreateCostComponent(newCostComponent);


        return newCostComponent.CostComponentId;
    }


    public async Task<TaskCostResult> GetTaskCost(string workEffortId, string currencyUomId, string productId,
        string routingId)
    {
        var result = new TaskCostResult();
        decimal taskCost = 0;

        try
        {
            // Fetch estimated task times
            var estimatedTaskTimeResult = await GetEstimatedTaskTime(workEffortId, productId, routingId);
            var totalEstimatedTaskTime = estimatedTaskTimeResult.EstimatedTaskTime;
            var setupTime = estimatedTaskTimeResult.SetupTime;
            var estimatedTaskTime = totalEstimatedTaskTime - setupTime;

            // Retrieve the task
            var task = await _context.WorkEfforts.FindAsync(workEffortId);
            if (task != null)
            {
                try
                {
                    // Retrieve associated fixed asset
                    var fixedAsset = await _context.FixedAssets.FindAsync(task.FixedAssetId);

                    if (fixedAsset != null)
                    {
                        // Fetch setup costs
                        var setupCosts = await _context.FixedAssetStdCosts
                            .Where(c => c.FixedAssetId == fixedAsset.FixedAssetId &&
                                        c.FixedAssetStdCostTypeId == "SETUP_COST" &&
                                        c.AmountUomId == currencyUomId &&
                                        (c.FromDate <= DateTime.UtcNow &&
                                         (c.ThruDate == null || c.ThruDate > DateTime.UtcNow)))
                            .ToListAsync();
                        var setupCost = setupCosts.FirstOrDefault();

                        // Fetch usage costs
                        var usageCosts = await _context.FixedAssetStdCosts
                            .Where(c => c.FixedAssetId == fixedAsset.FixedAssetId &&
                                        c.FixedAssetStdCostTypeId == "USAGE_COST" &&
                                        c.AmountUomId == currencyUomId &&
                                        (c.FromDate <= DateTime.UtcNow &&
                                         (c.ThruDate == null || c.ThruDate > DateTime.UtcNow)))
                            .ToListAsync();
                        var usageCost = usageCosts.FirstOrDefault();

                        // Calculate task cost
                        taskCost = (usageCost?.Amount ?? 0) * estimatedTaskTime + (setupCost?.Amount ?? 0) * setupTime;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error calculating costs for task {workEffortId}: {ex.Message}");
                    throw;
                }
            }

            // Convert time from milliseconds to hours
            taskCost /= 3600000;
            result.TaskCost = taskCost;

            try
            {
                // Compute costs derived from CostComponentCalc records associated with the task
                var weccs = await _context.WorkEffortCostCalcs
                    .Where(wecc => wecc.WorkEffortId == workEffortId &&
                                   (wecc.FromDate <= DateTime.UtcNow &&
                                    (wecc.ThruDate == null || wecc.ThruDate > DateTime.UtcNow)))
                    .ToListAsync();

                foreach (var wecc in weccs)
                {
                    var costComponentCalc = await _context.CostComponentCalcs.FindAsync(wecc.CostComponentCalcId);

                    try
                    {
                        var customMethod = await _context.CustomMethods.FindAsync(costComponentCalc.CostCustomMethodId);
                        if (customMethod == null)
                        {
                            if (costComponentCalc.PerMilliSecond != 0)
                            {
                                var totalCostComponentTime = totalEstimatedTaskTime / costComponentCalc.PerMilliSecond;
                                var totalCostComponentCost = totalCostComponentTime * costComponentCalc.VariableCost +
                                                             costComponentCalc.FixedCost;

                                var existingCost = result.CostsByType.FirstOrDefault(c =>
                                    c.CostComponentTypeId == wecc.CostComponentTypeId);
                                if (existingCost == null)
                                {
                                    result.CostsByType.Add(new CostByType
                                    {
                                        CostComponentTypeId = wecc.CostComponentTypeId,
                                        TotalCostComponentCost = (decimal)totalCostComponentCost
                                    });
                                }
                                else
                                {
                                    existingCost.TotalCostComponentCost += totalCostComponentCost;
                                }
                            }
                        }
                        else
                        {
                            // FIXME: formulas are still not supported for standard costs
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing CostComponentCalc for task {workEffortId}: {ex.Message}");
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving WorkEffortCostCalcs for task {workEffortId}: {ex.Message}");
                throw;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetTaskCost for workEffortId {workEffortId}: {ex.Message}");
            throw;
        }

        return result;
    }

    public async Task<TaskTimeResult> GetEstimatedTaskTime(string workEffortId, string? productId = null,
        string? routingId = null, decimal? quantity = 1)
    {
        decimal taskTime = 1; // Default task time
        decimal setupTime = 0; // Initialize setup time
        decimal totalTaskTime = 0;

        // Create a logger specifically for this transaction
        var loggerForTransaction = Log.ForContext("Transaction", "get estimated task time");

        try
        {
            loggerForTransaction.Information("Fetching task details for WorkEffortId: {WorkEffortId}", workEffortId);

            // Fetch the task details
            var task = await _context.WorkEfforts.FindAsync(workEffortId);
            if (task == null)
            {
                loggerForTransaction.Warning("Task with WorkEffortId {WorkEffortId} not found.", workEffortId);
                throw new ServiceException($"Task with WorkEffortId {workEffortId} not found.");
            }

            // Calculate task time based on estimated milliseconds
            if (task.EstimatedMilliSeconds != null)
            {
                taskTime = (decimal)task.EstimatedMilliSeconds.Value;
                loggerForTransaction.Information(
                    "Estimated task time for WorkEffortId {WorkEffortId} is {TaskTime} ms.", workEffortId, taskTime);
            }
            else
            {
                loggerForTransaction.Information(
                    "No estimated task time found for WorkEffortId {WorkEffortId}, using default task time.",
                    workEffortId);
            }

            // Retrieve setup time (if available)
            if (task.EstimatedSetupMillis != null)
            {
                setupTime = (decimal)task.EstimatedSetupMillis.Value;
                loggerForTransaction.Information("Setup time for WorkEffortId {WorkEffortId} is {SetupTime} ms.",
                    workEffortId, setupTime);
            }

            // Calculate total task time, including setup
            totalTaskTime = taskTime * (quantity ?? 1) + setupTime;
            loggerForTransaction.Information(
                "Total task time (including setup) for WorkEffortId {WorkEffortId} is {TotalTaskTime} ms.",
                workEffortId, totalTaskTime);

            // Check if there's a custom method associated with the task
            var customMethod =
                await _context.CustomMethods.FirstOrDefaultAsync(c => c.CustomMethodId == task.EstimateCalcMethod);
            if (customMethod != null && !string.IsNullOrEmpty(customMethod?.CustomMethodName))
            {
                loggerForTransaction.Information(
                    "Custom method {CustomMethodName} found for WorkEffortId {WorkEffortId}.",
                    customMethod.CustomMethodName, workEffortId);

                // Ensure required parameters for the custom method are available
                if (productId == null || routingId == null)
                {
                    loggerForTransaction.Error(
                        "Custom method requires productId and routingId to proceed for WorkEffortId {WorkEffortId}.",
                        workEffortId);
                    return new TaskTimeResult
                    {
                        EstimatedTaskTime = (long)totalTaskTime,
                        SetupTime = (long)setupTime,
                        TaskUnitTime = taskTime
                    };
                }

                // Attempt to invoke the custom method
                try
                {
                    loggerForTransaction.Information(
                        "Invoking custom method {CustomMethodName} for task {WorkEffortId}.",
                        customMethod.CustomMethodName, workEffortId);
                    var methodInfo = typeof(CustomMethodsService).GetMethod(customMethod.CustomMethodName);
                    if (methodInfo == null)
                    {
                        throw new ServiceException($"Custom method {customMethod.CustomMethodName} not found.");
                    }

                    var customMethodResult =
                        (decimal)methodInfo.Invoke(null, new object[] { task, productId, routingId, quantity });
                    totalTaskTime = customMethodResult; // Update total task time with custom method result
                    loggerForTransaction.Information(
                        "Custom method {CustomMethodName} returned a new task time of {TotalTaskTime} ms for WorkEffortId {WorkEffortId}.",
                        customMethod.CustomMethodName, totalTaskTime, workEffortId);
                }
                catch (Exception ex)
                {
                    loggerForTransaction.Error(ex,
                        "Problem calling the custom method {CustomMethodName} for WorkEffortId {WorkEffortId}.",
                        customMethod.CustomMethodName, workEffortId);
                    throw new ServiceException($"Failed to execute custom method {customMethod.CustomMethodName}.", ex);
                }
            }
            else
            {
                loggerForTransaction.Warning(
                    "No custom method associated with the task {WorkEffortId}. Proceeding with default calculation.",
                    workEffortId);
            }

            // Return the result
            return new TaskTimeResult
            {
                EstimatedTaskTime = (long)totalTaskTime,
                SetupTime = (long)setupTime,
                TaskUnitTime = taskTime
            };
        }
        catch (ServiceException se)
        {
            loggerForTransaction.Error(se,
                "Service exception occurred while fetching task time for WorkEffortId: {WorkEffortId}", workEffortId);
            throw;
        }
        catch (Exception ex)
        {
            loggerForTransaction.Error(ex,
                "Unexpected error occurred while fetching task time for WorkEffortId: {WorkEffortId}", workEffortId);
            throw new ServiceException("An unexpected error occurred while fetching task time.", ex);
        }
    }

    /// <summary>
    /// OFBiz service createCostComponent translated to C#.
    /// This method persists a fully formed CostComponent object to the database.
    /// </summary>
    public async Task CreateCostComponent(CostComponent costComponent)
    {
        try
        {
            // Add the CostComponent entity to the EF change tracker
            // Technical: EF will insert all non-primary-key fields automatically
            // Business: record cost details in financial ledger
            _context.CostComponents.Add(costComponent);

            // ----------------------
            // ECA: on commit event
            // ----------------------
            // Condition: only fire if WorkEffortId is provided
            // and costComponentTypeId is not 'ACTUAL_MAT_COST'
            if (!string.IsNullOrEmpty(costComponent.WorkEffortId)
                && costComponent.CostComponentTypeId != "ACT_MAT_COST")
            {
                // Action: invoke createAcctgTransForWorkEffortCost synchronously
                // Technical: call the accounting service to create GL entries
                // Business: ensure financial transactions are recorded for non-material costs
                await _generalLedgerService.Value.CreateAcctgTransForWorkEffortCost(
                    costComponent.WorkEffortId,
                    costComponent.CostComponentId
                );
            }
        }
        catch (Exception ex)
        {
            // Log any exception for diagnostics
            // Technical: capture stack trace and message
            // Business: flag failures so finance team can investigate
            _logger.LogError(ex, "Failed to create CostComponent for WorkEffort {WorkEffortId}",
                costComponent.WorkEffortId);
            // Swallow or rethrow based on error-handling policy
        }
    }

    public async Task<string> CreateCostComponentCalc(CostComponentCalc costComponentCalc)
    {
        // REFACTOR: Validate input to prevent null entity, ensuring robust error handling
        if (costComponentCalc == null)
        {
            throw new ArgumentNullException(nameof(costComponentCalc), "CostComponentCalc entity cannot be null");
        }

        try
        {
            // REFACTOR: Add entity to DbContext for persistence
            await _context.CostComponentCalcs.AddAsync(costComponentCalc);

            return costComponentCalc.CostComponentCalcId;
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    public async Task<string> UpdateCostComponentCalc(CostComponentCalc costComponentCalc)
    {
        // REFACTOR: Validate input to prevent null entity and missing ID
        if (costComponentCalc == null)
        {
            throw new ArgumentNullException(nameof(costComponentCalc), "CostComponentCalc entity cannot be null");
        }

        if (string.IsNullOrEmpty(costComponentCalc.CostComponentCalcId))
        {
            throw new ArgumentException("CostComponentCalcId is required",
                nameof(costComponentCalc.CostComponentCalcId));
        }

        try
        {
            var existingCostComponentCalc = await _context.CostComponentCalcs
                .FirstOrDefaultAsync(c => c.CostComponentCalcId == costComponentCalc.CostComponentCalcId);

            if (existingCostComponentCalc == null)
            {
                throw new InvalidOperationException(
                    $"CostComponentCalc with ID {costComponentCalc.CostComponentCalcId} not found");
            }

            // REFACTOR: Update fields from DTO, preserving existing values where not provided
            existingCostComponentCalc.CurrencyUomId = costComponentCalc.CurrencyUomId;
            existingCostComponentCalc.Description =
                costComponentCalc.Description ?? existingCostComponentCalc.Description;
            existingCostComponentCalc.FixedCost = costComponentCalc.FixedCost ?? existingCostComponentCalc.FixedCost;
            existingCostComponentCalc.VariableCost =
                costComponentCalc.VariableCost ?? existingCostComponentCalc.VariableCost;
            existingCostComponentCalc.PerMilliSecond =
                costComponentCalc.PerMilliSecond ?? existingCostComponentCalc.PerMilliSecond;

            // REFACTOR: Mark entity as modified (optional, as EF Core tracks changes)
            _context.CostComponentCalcs.Update(existingCostComponentCalc);

            return costComponentCalc.CostComponentCalcId;
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    public async Task<string> CreateWorkEffortCostCalc(WorkEffortCostCalc workEffortCostCalc)
    {
        // REFACTOR: Validate input to prevent null entity and required fields
        if (workEffortCostCalc == null)
        {
            throw new ArgumentNullException(nameof(workEffortCostCalc), "WorkEffortCostCalc entity cannot be null");
        }

        if (string.IsNullOrEmpty(workEffortCostCalc.WorkEffortId))
        {
            throw new ArgumentException("WorkEffortId is required", nameof(workEffortCostCalc.WorkEffortId));
        }

        if (string.IsNullOrEmpty(workEffortCostCalc.CostComponentCalcId))
        {
            throw new ArgumentException("CostComponentCalcId is required",
                nameof(workEffortCostCalc.CostComponentCalcId));
        }

        try
        {
            // REFACTOR: Add entity to DbContext for persistence
            await _context.WorkEffortCostCalcs.AddAsync(workEffortCostCalc);

            return workEffortCostCalc.WorkEffortId; // Return WorkEffortId as primary identifier
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    public async Task<string> UpdateWorkEffortCostCalc(WorkEffortCostCalc workEffortCostCalc)
    {
        // REFACTOR: Validate input to prevent null entity and required fields
        if (workEffortCostCalc == null)
        {
            throw new ArgumentNullException(nameof(workEffortCostCalc), "WorkEffortCostCalc entity cannot be null");
        }

        if (string.IsNullOrEmpty(workEffortCostCalc.WorkEffortId))
        {
            throw new ArgumentException("WorkEffortId is required", nameof(workEffortCostCalc.WorkEffortId));
        }

        if (string.IsNullOrEmpty(workEffortCostCalc.CostComponentCalcId))
        {
            throw new ArgumentException("CostComponentCalcId is required",
                nameof(workEffortCostCalc.CostComponentCalcId));
        }

        if (string.IsNullOrEmpty(workEffortCostCalc.CostComponentTypeId))
        {
            throw new ArgumentException("CostComponentTypeId is required",
                nameof(workEffortCostCalc.CostComponentTypeId));
        }

        if (workEffortCostCalc.FromDate == default)
        {
            throw new ArgumentException("FromDate is required", nameof(workEffortCostCalc.FromDate));
        }

        try
        {
            // REFACTOR: Find the existing record using composite key
            var existingRecord = await _context.WorkEffortCostCalcs
                .FirstOrDefaultAsync(x =>
                    x.WorkEffortId == workEffortCostCalc.WorkEffortId &&
                    x.CostComponentCalcId == workEffortCostCalc.CostComponentCalcId &&
                    x.CostComponentTypeId == workEffortCostCalc.CostComponentTypeId &&
                    x.FromDate == workEffortCostCalc.FromDate);

            if (existingRecord == null)
            {
                throw new InvalidOperationException("WorkEffortCostCalc record not found for the provided key.");
            }

            // REFACTOR: Update the existing record with new values
            // Purpose: Ensures only the provided fields are updated, maintaining data integrity
            existingRecord.CostComponentTypeId = workEffortCostCalc.CostComponentTypeId;
            existingRecord.CostComponentCalcId = workEffortCostCalc.CostComponentCalcId;
            existingRecord.FromDate = workEffortCostCalc.FromDate;
            existingRecord.ThruDate = workEffortCostCalc.ThruDate;

            // REFACTOR: Mark the entity as modified for persistence
            // Purpose: Informs EF Core to include the entity in the next SaveChanges call
            _context.WorkEffortCostCalcs.Update(existingRecord);

            return workEffortCostCalc.WorkEffortId; // Return WorkEffortId as primary identifier
        }
        catch (Exception ex)
        {
            throw;
        }
    }
    
    
}

// List of reviewed and enhanced methods
// CalculateProductCosts
// CancelCostComponents
// GetManufacturingComponents
// GetProductCost
// GetProductRouting
// GetTaskCost
// GetEstimatedTaskTime
// RecreateCostComponent