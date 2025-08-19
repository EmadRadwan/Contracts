using Microsoft.Extensions.Logging;
using Persistence;
using Serilog;

namespace Application.Manufacturing;

public interface IProductionRunHelperService
{
    Task<CreateProductionRunsResult> CreateProductionRunsForProductBom(
        string productId,
        DateTime startDate,
        decimal? quantity,
        string facilityId,
        string workEffortName,
        string description,
        string routingId);
}

public class ProductionRunHelperService : IProductionRunHelperService
{
    private readonly IBOMTreeService _bomTreeService;
    private readonly DataContext _context;
    private readonly Microsoft.Extensions.Logging.ILogger _logger;
    private readonly Serilog.ILogger loggerForTransaction;


    public ProductionRunHelperService(DataContext context, ILogger<ProductionRunHelperService> logger, IBOMTreeService bomTreeService)
    {
        _context = context;
        _logger = logger;
        _bomTreeService = bomTreeService;
        
        loggerForTransaction = Log.ForContext("Transaction", "create production run for BOM");
    }


    public async Task<CreateProductionRunsResult> CreateProductionRunsForProductBom(
        string productId,
        DateTime startDate,
        decimal? quantity,
        string facilityId,
        string workEffortName,
        string description,
        string routingId)
    {
        var result = new CreateProductionRunsResult();
        quantity ??= 1;
        string workEffortId = null;

        try
        {
            loggerForTransaction.Information("Before Initializing BOM tree for product {ProductId}", productId);

            await _bomTreeService.Initialize(productId, "MANUF_COMPONENT", startDate, BOMTree.EXPLOSION, (decimal)quantity);

            loggerForTransaction.Information("Initialized BOM tree for product {ProductId}", productId);

            /*_bomTree.SetRootQuantity(quantity.Value);
            _bomTree.SetRootAmount(0);*/


            await _bomTreeService.CreateManufacturingOrders(
                facilityId,
                startDate,
                workEffortName,
                description,
                routingId 
            );
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error creating Bill of Materials tree: {ex.Message}", ex);
        }

        /*if (string.IsNullOrEmpty(workEffortId))
            throw new InvalidOperationException(
                $"Production run is not required for product ID {productId} starting on {startDate}");
                */

        result.ProductionRunId = workEffortId;
        return result;
    }
}