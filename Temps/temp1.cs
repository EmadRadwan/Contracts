using Domain;
using Persistence;
using Serilog;
using System;
using System.Threading.Tasks;

// Service interface
public interface ICostComponentCalcService
{
    // REFACTOR: Define method to create CostComponentCalc, returning the generated ID
    Task<string> CreateCostComponentCalc(CostComponentCalc costComponentCalc);
}

// Service implementation
public class CostComponentCalcService : ICostComponentCalcService
{
    private readonly DataContext _context;
    private readonly ILogger _logger;

    // REFACTOR: Inject DataContext and initialize Serilog logger for service operations
    public CostComponentCalcService(DataContext context)
    {
        _context = context;
        _logger = Log.ForContext<CostComponentCalcService>();
    }

    public async Task<string> CreateCostComponentCalc(CostComponentCalc costComponentCalc)
    {
        // REFACTOR: Validate input to prevent null entity, ensuring robust error handling
        if (costComponentCalc == null)
        {
            _logger.Error("CreateCostComponentCalc: Received null costComponentCalc entity");
            throw new ArgumentNullException(nameof(costComponentCalc), "CostComponentCalc entity cannot be null");
        }

        // REFACTOR: Ensure CostComponentCalcId is set, generating if not provided
        if (string.IsNullOrEmpty(costComponentCalc.CostComponentCalcId))
        {
            costComponentCalc.CostComponentCalcId = Guid.NewGuid().ToString();
            _logger.Information("CreateCostComponentCalc: Generated new CostComponentCalcId {Id}", costComponentCalc.CostComponentCalcId);
        }

        // REFACTOR: Validate required fields (CostGlAccountTypeId, CurrencyUomId) to match OFBiz constraints
        if (string.IsNullOrEmpty(costComponentCalc.CostGlAccountTypeId))
        {
            _logger.Error("CreateCostComponentCalc: CostGlAccountTypeId is required");
            throw new ArgumentException("CostGlAccountTypeId is required", nameof(costComponentCalc.CostGlAccountTypeId));
        }

        if (string.IsNullOrEmpty(costComponentCalc.CurrencyUomId))
        {
            _logger.Error("CreateCostComponentCalc: CurrencyUomId is required");
            throw new ArgumentException("CurrencyUomId is required", nameof(costComponentCalc.CurrencyUomId));
        }

        try
        {
            _logger.Information("CreateCostComponentCalc: Adding CostComponentCalc {Id}", costComponentCalc.CostComponentCalcId);

            // REFACTOR: Add entity to DbContext for persistence
            await _context.CostComponentCalcs.AddAsync(costComponentCalc);

            // REFACTOR: Save changes to generate the record, but defer transaction commit to handler
            await _context.SaveChangesAsync();

            _logger.Information("CreateCostComponentCalc: Successfully created CostComponentCalc {Id}", costComponentCalc.CostComponentCalcId);
            return costComponentCalc.CostComponentCalcId;
        }
        catch (Exception ex)
        {
            // REFACTOR: Log detailed error and rethrow to be handled by caller (MediatR handler)
            _logger.Error(ex, "CreateCostComponentCalc: Failed to create CostComponentCalc {Id}", costComponentCalc.CostComponentCalcId);
            throw;
        }
    }
}