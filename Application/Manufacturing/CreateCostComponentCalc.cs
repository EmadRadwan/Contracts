using Application.Catalog.Products.Services.Cost;
using Application.Manufacturing;
using Domain;
using MediatR;
using Persistence;
using Serilog;


// Create CostComponentCalc
public class CreateCostComponentCalc
{
    public class Command : IRequest<Result<string>>
    {
        public CostComponentCalcDto CostComponentCalcDto { get; set; }
    }

    // REFACTOR: Implement handler to process CreateCostComponentCalc command, delegating to ICostComponentCalcService
    public class Handler : IRequestHandler<Command, Result<string>>
    {
        private readonly DataContext _context;
        private readonly ICostService _costService;
        private readonly ILogger _loggerForTransaction;

        // REFACTOR: Inject ICostComponentCalcService and DataContext, initialize Serilog logger
        public Handler(ICostService costService, DataContext context)
        {
            _context = context;
            _costService = costService;
            _loggerForTransaction = Log.ForContext("Transaction", "create cost component calc");
        }

        public async Task<Result<string>> Handle(Command request, CancellationToken cancellationToken)
        {
            // REFACTOR: Use transaction to ensure atomicity, consistent with CreateRouting
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                _loggerForTransaction.Information("CreateCostComponentCalc.cs starting");

                // REFACTOR: Map DTO to CostComponentCalc entity, aligning with EditCostComponentCalc.tsx form
                var costComponentCalc = new CostComponentCalc
                {
                    CostComponentCalcId = request.CostComponentCalcDto.CostComponentCalcId,
                    CostGlAccountTypeId = request.CostComponentCalcDto.CostGlAccountTypeId,
                    OffsettingGlAccountTypeId = request.CostComponentCalcDto.OffsettingGlAccountTypeId,
                    CurrencyUomId = request.CostComponentCalcDto.CurrencyUomId,
                    CostCustomMethodId = request.CostComponentCalcDto.CostCustomMethodId,
                    Description = request.CostComponentCalcDto.Description,
                    FixedCost = request.CostComponentCalcDto.FixedCost,
                    VariableCost = request.CostComponentCalcDto.VariableCost,
                    PerMilliSecond = (int?)request.CostComponentCalcDto.PerMilliSecond
                };

                // REFACTOR: Call service to create CostComponentCalc
                var costComponentCalcId = await _costService.CreateCostComponentCalc(costComponentCalc);

                // REFACTOR: Save changes to persist entity, ensuring transaction integrity
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                _loggerForTransaction.Information("CreateCostComponentCalc.cs end - Success");
                return Result<string>.Success(costComponentCalcId);
            }
            catch (Exception ex)
            {
                // REFACTOR: Roll back transaction and log error, mirroring error handling style
                await transaction.RollbackAsync(cancellationToken);
                _loggerForTransaction.Information("CreateCostComponentCalc.cs error: {Error}", ex.Message);
                return Result<string>.Failure($"Error creating cost component calc: {ex.Message}");
            }
        }
    }
}