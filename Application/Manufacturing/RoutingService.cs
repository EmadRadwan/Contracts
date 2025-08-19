using API.Middleware;
using Application.Common;
using Application.Core;
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Serilog;
using Persistence;

namespace Application.Manufacturing;

public interface IRoutingService
{
    Task<RoutingResult> GetProductRouting(string productId, string workEffortId = null,
        DateTime? applicableDate = null, string ignoreDefaultRouting = null);

    Task<ManufacturingComponentsResult> GetManufacturingComponents(string productId,
        decimal? quantity = null, decimal? amount = null, string? fromDateStr = null, bool? excludeWIPs = null);
}

public class RoutingService : IRoutingService
{
    private readonly ICommonService _commonService;
    private readonly DataContext _context;
    private readonly IUtilityService _utilityService;
    private readonly Microsoft.Extensions.Logging.ILogger _logger;
    private readonly Serilog.ILogger loggerForTransaction;

    public RoutingService(DataContext context, ILogger<RoutingService> logger,
        ICommonService commonService, IUtilityService utilityService)
    {
        _context = context;
        _logger = logger;
        _commonService = commonService;
        _utilityService = utilityService;
        loggerForTransaction = Log.ForContext("Transaction", "create production run");
    }

    public async Task<RoutingResult> GetProductRouting(string productId, string workEffortId = null,
        DateTime? applicableDate = null, string ignoreDefaultRouting = null)
    {
        var result = new RoutingResult();
        DateTime filterDate = applicableDate ?? DateTime.UtcNow;

        try
        {
            loggerForTransaction.Information(
                "Starting to fetch routing for ProductId: {ProductId}, WorkEffortId: {WorkEffortId}, ApplicableDate: {ApplicableDate}",
                productId, workEffortId, filterDate);

            WorkEffortGoodStandard routingGS = null;

            if (!string.IsNullOrEmpty(workEffortId))
            {
                try
                {
                    loggerForTransaction.Information(
                        "Searching for routing with WorkEffortId: {WorkEffortId} for ProductId: {ProductId}",
                        workEffortId, productId);
                    routingGS = await _context.WorkEffortGoodStandards
                        .Where(w => w.ProductId == productId && w.WorkEffortId == workEffortId &&
                                    w.WorkEffortGoodStdTypeId == "ROU_PROD_TEMPLATE")
                        .Where(w => w.FromDate <= filterDate && (w.ThruDate == null || w.ThruDate >= filterDate))
                        .FirstOrDefaultAsync();
                }
                catch (Exception ex)
                {
                    loggerForTransaction.Error(ex,
                        "Error fetching routing for WorkEffortId: {WorkEffortId}, ProductId: {ProductId}", workEffortId,
                        productId);
                    throw new ServiceException("Failed to fetch specific routing details.", ex);
                }

                if (routingGS == null)
                {
                    loggerForTransaction.Warning(
                        "No routing found - looking for virtual product - for ProductId: {ProductId} and WorkEffortId: {WorkEffortId}. Searching for virtual product routing.",
                        productId, workEffortId);
                    try
                    {
                        var virtualProductAssoc = await _context.ProductAssocs
                            .Where(p => p.ProductIdTo == productId && p.ProductAssocTypeId == "PRODUCT_VARIANT")
                            .Where(p => p.FromDate <= filterDate && (p.ThruDate == null || p.ThruDate >= filterDate))
                            .FirstOrDefaultAsync();

                        if (virtualProductAssoc != null)
                        {
                            loggerForTransaction.Information(
                                "Found virtual product for ProductId: {ProductId}. Fetching routing for ProductIdTo: {ProductIdTo}",
                                productId, virtualProductAssoc.ProductIdTo);
                            routingGS = await _context.WorkEffortGoodStandards
                                .Where(w => w.ProductId == virtualProductAssoc.ProductIdTo &&
                                            w.WorkEffortGoodStdTypeId == "ROU_PROD_TEMPLATE")
                                .Where(w => w.FromDate <= filterDate &&
                                            (w.ThruDate == null || w.ThruDate >= filterDate))
                                .FirstOrDefaultAsync();
                        }
                    }
                    catch (Exception ex)
                    {
                        loggerForTransaction.Error(ex,
                            "Error fetching routing for virtual product for ProductId: {ProductId}", productId);
                        throw new ServiceException("Failed to fetch routing for virtual product.", ex);
                    }
                }
            }
            else
            {
                loggerForTransaction.Information(
                    "No WorkEffortId provided. Searching for default routing for ProductId: {ProductId}", productId);
                try
                {
                    routingGS = await _context.WorkEffortGoodStandards
                        .Where(w => w.ProductId == productId && w.WorkEffortGoodStdTypeId == "ROU_PROD_TEMPLATE")
                        .Where(w => w.FromDate <= filterDate && (w.ThruDate == null || w.ThruDate >= filterDate))
                        .FirstOrDefaultAsync();
                }
                catch (Exception ex)
                {
                    loggerForTransaction.Error(ex, "Error fetching default routing for ProductId: {ProductId}",
                        productId);
                    throw new ServiceException("Failed to fetch default routing.", ex);
                }

                if (routingGS == null)
                {
                    loggerForTransaction.Warning(
                        "No routing found for ProductId: {ProductId}. Searching for virtual product routing.",
                        productId);
                    try
                    {
                        var virtualProductAssoc = await _context.ProductAssocs
                            .Where(p => p.ProductIdTo == productId && p.ProductAssocTypeId == "PRODUCT_VARIANT")
                            .Where(p => p.FromDate <= filterDate && (p.ThruDate == null || p.ThruDate >= filterDate))
                            .FirstOrDefaultAsync();

                        if (virtualProductAssoc != null)
                        {
                            loggerForTransaction.Information(
                                "Found virtual product for ProductId: {ProductId}. Fetching routing for ProductIdTo: {ProductIdTo}",
                                productId, virtualProductAssoc.ProductIdTo);
                            routingGS = await _context.WorkEffortGoodStandards
                                .Where(w => w.ProductId == virtualProductAssoc.ProductIdTo &&
                                            w.WorkEffortGoodStdTypeId == "ROU_PROD_TEMPLATE")
                                .Where(w => w.FromDate <= filterDate &&
                                            (w.ThruDate == null || w.ThruDate >= filterDate))
                                .FirstOrDefaultAsync();
                        }
                    }
                    catch (Exception ex)
                    {
                        loggerForTransaction.Error(ex,
                            "Error fetching routing for virtual product for ProductId: {ProductId}", productId);
                        throw new ServiceException("Failed to fetch routing for virtual product.", ex);
                    }
                }
            }

            if (routingGS != null)
            {
                try
                {
                    loggerForTransaction.Information(
                        "Routing found for WorkEffortId: {WorkEffortId}. Fetching WorkEffort details.",
                        routingGS.WorkEffortId);
                    result.Routing = await _context.WorkEfforts.FindAsync(routingGS.WorkEffortId);
                }
                catch (Exception ex)
                {
                    loggerForTransaction.Error(ex, "Error fetching WorkEffort details for WorkEffortId: {WorkEffortId}",
                        routingGS.WorkEffortId);
                    throw new ServiceException("Failed to fetch WorkEffort details.", ex);
                }
            }
            else if (string.IsNullOrEmpty(ignoreDefaultRouting) || ignoreDefaultRouting == "N")
            {
                loggerForTransaction.Information("No specific routing found. Fetching default routing.");
                try
                {
                    result.Routing = await _context.WorkEfforts.FindAsync("DEFAULT_ROUTING");
                }
                catch (Exception ex)
                {
                    loggerForTransaction.Error(ex, "Error fetching default routing.");
                    throw new ServiceException("Failed to fetch default routing.", ex);
                }
            }

            if (result.Routing != null)
            {
                try
                {
                    loggerForTransaction.Information("Fetching associated tasks for RoutingId: {RoutingId}",
                        result.Routing.WorkEffortId);
                    result.Tasks = await _context.WorkEffortAssocs
                        .Where(w => w.WorkEffortIdFrom == result.Routing.WorkEffortId &&
                                    w.WorkEffortAssocTypeId == "ROUTING_COMPONENT")
                        .Where(w => w.FromDate <= filterDate && (w.ThruDate == null || w.ThruDate >= filterDate))
                        .OrderBy(w => w.SequenceNum)
                        .ToListAsync();

                    loggerForTransaction.Information("Found {TaskCount} tasks associated with RoutingId: {RoutingId}",
                        result.Tasks.Count, result.Routing.WorkEffortId);
                }
                catch (Exception ex)
                {
                    loggerForTransaction.Error(ex, "Error fetching tasks for RoutingId: {RoutingId}",
                        result.Routing.WorkEffortId);
                    throw new ServiceException("Failed to fetch tasks for routing.", ex);
                }
            }
            else
            {
                loggerForTransaction.Warning("No routing or tasks found for ProductId: {ProductId}", productId);
            }

            return result;
        }
        catch (ServiceException se)
        {
            loggerForTransaction.Error(se,
                "Service exception occurred while processing routing for ProductId: {ProductId}", productId);
            throw;
        }
        catch (Exception ex)
        {
            loggerForTransaction.Error(ex,
                "Unexpected error occurred while processing routing for ProductId: {ProductId}", productId);
            throw new ServiceException("An unexpected error occurred while processing routing.", ex);
        }
    }

    public async Task<ManufacturingComponentsResult> GetManufacturingComponents(string productId,
        decimal? quantity = null,
        decimal? amount = null, string? fromDateStr = null, bool? excludeWIPs = null)
    {
        var result = new ManufacturingComponentsResult();
        quantity ??= 1;
        amount ??= 0;
        DateTime fromDate;

        try
        {
            loggerForTransaction.Information(
                "Starting to fetch manufacturing components for ProductId: {ProductId} with quantity {Quantity} and amount {Amount}",
                productId, quantity, amount);

            // Parse fromDate or set to current date
            if (!string.IsNullOrEmpty(fromDateStr) && DateTime.TryParse(fromDateStr, out DateTime parsedDate))
            {
                fromDate = parsedDate;
                loggerForTransaction.Information("Using provided FromDate: {FromDate}", fromDate);
            }
            else
            {
                fromDate = DateTime.UtcNow;
                loggerForTransaction.Information("No FromDate provided. Using current date: {FromDate}", fromDate);
            }

            excludeWIPs ??= true;
            loggerForTransaction.Information("Exclude WIPs: {ExcludeWIPs}", excludeWIPs);

            // Load BOM components
            try
            {
                loggerForTransaction.Information(
                    "Fetching BOM components for ProductId: {ProductId} with FromDate: {FromDate}", productId,
                    fromDate);
                var components = await _context.ProductAssocs
                    .Where(c => c.ProductId == productId &&
                                c.ProductAssocTypeId == "MANUF_COMPONENT" &&
                                c.FromDate <= fromDate &&
                                (c.ThruDate == null || c.ThruDate >= fromDate))
                    .ToListAsync();

                if (!components.Any())
                {
                    loggerForTransaction.Warning("No BOM components found for ProductId: {ProductId}", productId);
                }
                else
                {
                    loggerForTransaction.Information("Found {ComponentCount} BOM components for ProductId: {ProductId}",
                        components.Count, productId);
                    result.Components = components;
                }
            }
            catch (Exception ex)
            {
                loggerForTransaction.Error(ex, "Error fetching BOM components for ProductId: {ProductId}", productId);
                throw new ServiceException("Failed to fetch BOM components.", ex);
            }

            // Attempt to get the product's routing information
            try
            {
                loggerForTransaction.Information("Fetching routing information for ProductId: {ProductId}", productId);
                RoutingResult? routingResult = await GetProductRouting(productId);

                if (routingResult?.Routing != null)
                {
                    loggerForTransaction.Information(
                        "Routing found for ProductId: {ProductId} with WorkEffortId: {WorkEffortId}", productId,
                        routingResult.Routing.WorkEffortId);
                    result.WorkEffortId = routingResult?.Routing?.WorkEffortId;
                    result.RoutingTasks = routingResult?.Tasks;
                }
                else
                {
                    loggerForTransaction.Warning("No routing found for ProductId: {ProductId}", productId);
                }
            }
            catch (Exception ex)
            {
                loggerForTransaction.Error(ex, "Error fetching routing information for ProductId: {ProductId}",
                    productId);
                throw new ServiceException("Failed to fetch routing information.", ex);
            }
            
            if (result.Components == null || !result.Components.Any())
            {
                loggerForTransaction.Warning("No components found for ProductId: {ProductId}", productId);
                return result;
            }

            // Create the component map and multiply the quantity by the finished product quantity
            try
            {
                loggerForTransaction.Information(
                    "Creating ComponentsMap and multiplying each component's quantity by the finished product quantity: {Quantity}",
                    quantity);

                var componentsMap = new List<ComponentMap>();

                foreach (var component in result?.Components)
                {
                    try
                    {
                        var componentMap = new ComponentMap
                        {
                            ProductId = component.ProductIdTo,
                            Quantity = (decimal)component.Quantity * (quantity ?? 1)
                        };

                        // Log each component being processed (optional)
                        loggerForTransaction.Debug(
                            "Processed component: ProductIdTo = {ProductIdTo}, Original Quantity = {OriginalQuantity}, Multiplied Quantity = {MultipliedQuantity}",
                            component.ProductIdTo, component.Quantity, componentMap.Quantity);

                        componentsMap.Add(componentMap);
                    }
                    catch (Exception ex)
                    {
                        loggerForTransaction.Error(
                            ex,
                            "Error processing component: ProductIdTo = {ProductIdTo}, Original Quantity = {OriginalQuantity}",
                            component.ProductIdTo, component.Quantity);
                        throw new ServiceException(
                            $"Failed to process component with ProductIdTo: {component.ProductIdTo}", ex);
                    }
                }

                result.ComponentsMap = componentsMap;
            }
            catch (Exception ex)
            {
                loggerForTransaction.Error(ex, "Error creating component map for ProductId: {ProductId}", productId);
                throw new ServiceException("Failed to create component map.", ex);
            }


            loggerForTransaction.Information("Completed fetching manufacturing components for ProductId: {ProductId}");
            return result;
        }
        catch (ServiceException se)
        {
            loggerForTransaction.Error(se,
                "Service exception occurred while fetching manufacturing components for ProductId: {ProductId}",
                productId);
            throw;
        }
        catch (Exception ex)
        {
            loggerForTransaction.Error(ex,
                "Unexpected error occurred while fetching manufacturing components for ProductId: {ProductId}",
                productId);
            throw new ServiceException("An unexpected error occurred while fetching manufacturing components.", ex);
        }
    }
}