using Application.Catalog.Products.Services.Cost;
using Application.Manufacturing;
using Domain;
using MediatR;
using Persistence;
using Serilog;

// Update CostComponentCalc
public class UpdateCostComponentCalc
{
    public class Command : IRequest<Result<string>>
    {
        public CostComponentCalcDto CostComponentCalcDto { get; set; }
    }

    // REFACTOR: Implement handler to process UpdateCostComponentCalc command, delegating to ICostService
    public class Handler : IRequestHandler<Command, Result<string>>
    {
        private readonly DataContext _context;
        private readonly ICostService _costService;
        private readonly ILogger _loggerForTransaction;

        // REFACTOR: Inject ICostService and DataContext, initialize Serilog logger
        public Handler(ICostService costService, DataContext context)
        {
            _context = context;
            _costService = costService;
            _loggerForTransaction = Log.ForContext("Transaction", "update cost component calc");
        }

        public async Task<Result<string>> Handle(Command request, CancellationToken cancellationToken)
        {
            // REFACTOR: Use transaction to ensure atomicity, consistent with CreateCostComponentCalc
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                _loggerForTransaction.Information("UpdateCostComponentCalc.cs starting");

                // REFACTOR: Map DTO to CostComponentCalc entity, aligning with EditCostComponentCalc.tsx form
                var costComponentCalc = new CostComponentCalc
                {
                    CostComponentCalcId = request.CostComponentCalcDto.CostComponentCalcId,
                    CurrencyUomId = request.CostComponentCalcDto.CurrencyUomId,
                    Description = request.CostComponentCalcDto.Description,
                    FixedCost = request.CostComponentCalcDto.FixedCost,
                    VariableCost = request.CostComponentCalcDto.VariableCost,
                    PerMilliSecond = (int?)request.CostComponentCalcDto.PerMilliSecond
                };

                // REFACTOR: Call service to update CostComponentCalc
                var costComponentCalcId = await _costService.UpdateCostComponentCalc(costComponentCalc);

                // REFACTOR: Save changes to persist entity, ensuring transaction integrity
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                _loggerForTransaction.Information("UpdateCostComponentCalc.cs end - Success");
                return Result<string>.Success(costComponentCalcId);
            }
            catch (Exception ex)
            {
                // REFACTOR: Roll back transaction and log error, mirroring CreateCostComponentCalc
                await transaction.RollbackAsync(cancellationToken);
                _loggerForTransaction.Information("UpdateCostComponentCalc.cs error: {Error}", ex.Message);
                return Result<string>.Failure($"Error updating cost component calc: {ex.Message}");
            }
        }
    }
}