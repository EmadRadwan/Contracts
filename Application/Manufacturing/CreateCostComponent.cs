using Application.Catalog.Products.Services.Cost;
using Application.Manufacturing;
using Domain;
using MediatR;
using Persistence;
using Serilog;


public class CreateCostComponent
{
    public class Command : IRequest<Result<string>>
    {
        public CostComponentDto CostComponentDto { get; set; }
    }

    // REFACTOR: Implement handler to process CreateCostComponent command, delegating to ICostService
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
            _loggerForTransaction = Log.ForContext("Transaction", "create cost component");
        }

        public async Task<Result<string>> Handle(Command request, CancellationToken cancellationToken)
        {
            // REFACTOR: Use transaction to ensure atomicity, consistent with CreateCostComponentCalc
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                _loggerForTransaction.Information("CreateCostComponent.cs starting");

                // REFACTOR: Map DTO to CostComponent entity, aligning with EditCostComponent.tsx form
                var costComponent = new CostComponent
                {
                    CostComponentId = Guid.NewGuid().ToString(),
                    ProductId = request.CostComponentDto.ProductId,
                    CostComponentTypeId = request.CostComponentDto.CostComponentTypeId,
                    CostUomId = request.CostComponentDto.CurrencyUomId,
                    Cost = request.CostComponentDto.Cost,
                    FromDate = request.CostComponentDto.FromDate
                };

                // REFACTOR: Call service to create CostComponent
                await _costService.CreateCostComponent(costComponent);

                // REFACTOR: Save changes to persist entity, ensuring transaction integrity
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                _loggerForTransaction.Information("CreateCostComponent.cs end - Success");
                return Result<string>.Success(null);
            }
            catch (Exception ex)
            {
                // REFACTOR: Roll back transaction and log error, mirroring error handling style
                await transaction.RollbackAsync(cancellationToken);
                _loggerForTransaction.Information("CreateCostComponent.cs error: {Error}", ex.Message);
                return Result<string>.Failure($"Error creating cost component: {ex.Message}");
            }
        }
    }
}