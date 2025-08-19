using Domain;
using MediatR;
using Persistence;
using Serilog;

namespace Application.WorkEfforts;

public class CreateRoutingDto
{
    public string WorkEffortTypeId { get; set; }
    public string WorkEffortName { get; set; }
    public string Description { get; set; }
    public decimal? QuantityToProduce { get; set; }
    public string CurrentStatusId { get; set; }
}

public class CreateRouting
{
    public class Command : IRequest<Result<string>>
    {
        public CreateRoutingDto CreateRoutingDto { get; set; }
    }

    // REFACTOR: Implement handler to process CreateWorkEffort command, delegating to IWorkEfforts
    public class Handler : IRequestHandler<Command, Result<string>>
    {
        private readonly IWorkEffortService _workEfforts;
        private readonly DataContext _context;
        private readonly Serilog.ILogger _loggerForTransaction;

        // REFACTOR: Inject IWorkEfforts and YourDbContext, initialize Serilog logger for transaction
        public Handler(IWorkEffortService workEfforts, DataContext context)
        {
            _workEfforts = workEfforts;
            _context = context;
            _loggerForTransaction = Log.ForContext("Transaction", "create work effort");
        }

        public async Task<Result<string>> Handle(Command request, CancellationToken cancellationToken)
        {
            // REFACTOR: Use transaction to ensure atomicity, consistent with CreateProductionRunsForProductBom
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                _loggerForTransaction.Information("CreateWorkEffort.cs starting");

                // REFACTOR: Map DTO to WorkEffort entity, aligning with EditRouting.tsx form
                var workEffort = new WorkEffort
                {
                    WorkEffortTypeId = request.CreateRoutingDto.WorkEffortTypeId,
                    WorkEffortName = request.CreateRoutingDto.WorkEffortName,
                    Description = request.CreateRoutingDto.Description,
                    QuantityToProduce = request.CreateRoutingDto.QuantityToProduce,
                    CurrentStatusId = request.CreateRoutingDto.CurrentStatusId
                };

                // REFACTOR: Call IWorkEfforts service to create WorkEffort and WorkEffortStatus
                var workEffortId = await _workEfforts.CreateWorkEffort(workEffort);

                // REFACTOR: Save changes to persist entities, ensuring transaction integrity
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                _loggerForTransaction.Information("CreateWorkEffort.cs end - Success");
                return Result<string>.Success(workEffortId);
            }
            catch (Exception ex)
            {
                // REFACTOR: Roll back transaction and log error, mirroring error handling style
                await transaction.RollbackAsync(cancellationToken);
                _loggerForTransaction.Information("CreateWorkEffort.cs error: {Error}", ex.Message);
                return Result<string>.Failure($"Error creating work effort: {ex.Message}");
            }
        }
    }
}