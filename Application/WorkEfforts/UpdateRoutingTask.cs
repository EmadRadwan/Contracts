using MediatR;
using Persistence;
using Serilog;

namespace Application.WorkEfforts;

public class UpdateRoutingTaskDto
{
    public string WorkEffortId { get; set; }
    public string WorkEffortTypeId { get; set; }
    public string WorkEffortName { get; set; }
    public string? WorkEffortPurposeTypeId { get; set; }
    public string? FixedAssetId { get; set; }
    // REFACTOR: Change to double? to match WorkEffort entity
    public double? EstimatedSetupMillis { get; set; }
    public double? EstimatedMilliSeconds { get; set; }
    public string? EstimateCalcMethod { get; set; }
    public double? ReservPersons { get; set; }
    public string CurrentStatusId { get; set; }
}
public class UpdateRoutingTask
{
    public class Command : IRequest<Result<string>>
    {
        public UpdateRoutingTaskDto UpdateRoutingTaskDto { get; set; }
    }

    // REFACTOR: Implement handler to process UpdateRoutingTask command, using UpdateWorkEffort
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
            _loggerForTransaction = Log.ForContext("Transaction", "update routing task");
        }

        public async Task<Result<string>> Handle(Command request, CancellationToken cancellationToken)
        {
            // REFACTOR: Use transaction to ensure atomicity, consistent with UpdateRouting
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                _loggerForTransaction.Information("UpdateRoutingTask.cs starting for WorkEffortId: {WorkEffortId}", request.UpdateRoutingTaskDto.WorkEffortId);

                // REFACTOR: Construct dictionary from DTO, converting decimal? to double? for EstimatedSetupMillis and EstimatedMilliSeconds
                var parameters = new Dictionary<string, object>();
                if (request.UpdateRoutingTaskDto.WorkEffortName != null) parameters["WorkEffortName"] = request.UpdateRoutingTaskDto.WorkEffortName;
                if (request.UpdateRoutingTaskDto.WorkEffortTypeId != null) parameters["WorkEffortTypeId"] = request.UpdateRoutingTaskDto.WorkEffortTypeId;
                if (request.UpdateRoutingTaskDto.WorkEffortPurposeTypeId != null) parameters["WorkEffortPurposeTypeId"] = request.UpdateRoutingTaskDto.WorkEffortPurposeTypeId;
                if (request.UpdateRoutingTaskDto.FixedAssetId != null) parameters["FixedAssetId"] = request.UpdateRoutingTaskDto.FixedAssetId;
                // REFACTOR: Convert decimal? to double? to match WorkEffort entity
                if (request.UpdateRoutingTaskDto.EstimatedSetupMillis.HasValue) parameters["EstimatedSetupMillis"] = (double?)request.UpdateRoutingTaskDto.EstimatedSetupMillis;
                if (request.UpdateRoutingTaskDto.EstimatedMilliSeconds.HasValue) parameters["EstimatedMilliSeconds"] = (double?)request.UpdateRoutingTaskDto.EstimatedMilliSeconds;
                if (request.UpdateRoutingTaskDto.EstimateCalcMethod != null) parameters["EstimateCalcMethod"] = request.UpdateRoutingTaskDto.EstimateCalcMethod;
                if (request.UpdateRoutingTaskDto.ReservPersons.HasValue) parameters["ReservPersons"] = (double?)request.UpdateRoutingTaskDto.ReservPersons; // Assuming ReservPersons is double? in WorkEffort
                if (request.UpdateRoutingTaskDto.CurrentStatusId != null) parameters["CurrentStatusId"] = request.UpdateRoutingTaskDto.CurrentStatusId;

                // REFACTOR: Call UpdateWorkEffort with workEffortId and parameters
                var workEffort = await _workEfforts.UpdateWorkEffort(request.UpdateRoutingTaskDto.WorkEffortId, parameters);

                // REFACTOR: Save changes to persist entities, ensuring transaction integrity
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                _loggerForTransaction.Information("UpdateRoutingTask.cs end - Success for WorkEffortId: {WorkEffortId}", workEffort.WorkEffortId);
                return Result<string>.Success(workEffort.WorkEffortId);
            }
            catch (Exception ex)
            {
                // REFACTOR: Roll back transaction and log error, mirroring UpdateRouting style
                await transaction.RollbackAsync(cancellationToken);
                _loggerForTransaction.Error("UpdateRoutingTask.cs error: {Error}", ex.Message);
                return Result<string>.Failure($"Error updating routing task: {ex.Message}");
            }
        }
    }
}
