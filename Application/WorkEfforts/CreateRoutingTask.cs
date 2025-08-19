using Domain;
using MediatR;
using Persistence;
using Serilog;

namespace Application.WorkEfforts;

public class CreateRoutingTaskDto
{
    public string WorkEffortTypeId { get; set; }
    public string WorkEffortName { get; set; }
    public string? WorkEffortPurposeTypeId { get; set; }
    public string? FixedAssetId { get; set; }
    public decimal? EstimatedSetupMillis { get; set; }
    public decimal? EstimatedMilliSeconds { get; set; }
    public string? EstimateCalcMethod { get; set; }
    public decimal? ReservPersons { get; set; }
    public string CurrentStatusId { get; set; }
}

public class CreateRoutingTask
{
    public class Command : IRequest<Result<string>>
    {
        public CreateRoutingTaskDto CreateRoutingTaskDto { get; set; }
    }

    // REFACTOR: Implement handler to process CreateRoutingTask command, delegating to IWorkEffortService
    public class Handler : IRequestHandler<Command, Result<string>>
    {
        private readonly IWorkEffortService _workEfforts;
        private readonly DataContext _context;
        private readonly Serilog.ILogger _loggerForTransaction;

        // REFACTOR: Inject IWorkEffortService and DataContext, initialize Serilog logger
        public Handler(IWorkEffortService workEfforts, DataContext context)
        {
            _workEfforts = workEfforts;
            _context = context;
            _loggerForTransaction = Log.ForContext("Transaction", "create routing task");
        }

        public async Task<Result<string>> Handle(Command request, CancellationToken cancellationToken)
        {
            // REFACTOR: Use transaction to ensure atomicity, consistent with CreateRouting
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                _loggerForTransaction.Information("CreateRoutingTask.cs starting");

                // REFACTOR: Map DTO to WorkEffort entity, aligning with EditRoutingTask.tsx form
                var workEffort = new WorkEffort
                {
                    WorkEffortTypeId = request.CreateRoutingTaskDto.WorkEffortTypeId,
                    WorkEffortName = request.CreateRoutingTaskDto.WorkEffortName,
                    WorkEffortPurposeTypeId = request.CreateRoutingTaskDto.WorkEffortPurposeTypeId,
                    FixedAssetId = request.CreateRoutingTaskDto.FixedAssetId,
                    EstimatedSetupMillis = (double?)request.CreateRoutingTaskDto.EstimatedSetupMillis,
                    EstimatedMilliSeconds = (double?)request.CreateRoutingTaskDto.EstimatedMilliSeconds,
                    EstimateCalcMethod = request.CreateRoutingTaskDto.EstimateCalcMethod,
                    ReservPersons = request.CreateRoutingTaskDto.ReservPersons,
                    CurrentStatusId = request.CreateRoutingTaskDto.CurrentStatusId
                };

                // REFACTOR: Call IWorkEffortService to create WorkEffort and WorkEffortStatus
                var workEffortId = await _workEfforts.CreateWorkEffort(workEffort);

                // REFACTOR: Save changes to persist entities, ensuring transaction integrity
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                _loggerForTransaction.Information("CreateRoutingTask.cs end - Success");
                return Result<string>.Success(workEffortId);
            }
            catch (Exception ex)
            {
                // REFACTOR: Roll back transaction and log error, mirroring CreateRouting
                await transaction.RollbackAsync(cancellationToken);
                _loggerForTransaction.Information("CreateRoutingTask.cs error: {Error}", ex.Message);
                return Result<string>.Failure($"Error creating routing task: {ex.Message}");
            }
        }
    }
}
