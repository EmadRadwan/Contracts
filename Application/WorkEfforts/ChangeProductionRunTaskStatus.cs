using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Manufacturing;
using Application.WorkEfforts;
using Persistence;

namespace Application.WorkEfforts
{
    public class ChangeProductionRunTaskStatus
    {
        public class Command : IRequest<Result<ChangeProductionRunTaskStatusResult>>
        {
            public ChangeProductionRunTaskStatusDto ChangeProductionRunTaskStatusDto { get; set; }
        }

        public class Handler : IRequestHandler<Command, Result<ChangeProductionRunTaskStatusResult>>
        {
            private readonly IProductionRunService _productionRunService;
            private readonly DataContext _context;

            public Handler(IProductionRunService productionRunService, DataContext context)
            {
                _productionRunService = productionRunService;
                _context = context;
            }

            public async Task<Result<ChangeProductionRunTaskStatusResult>> Handle(Command request, CancellationToken cancellationToken)
            {
                await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    // Call the service method to change production run status

                    var response = await _productionRunService.ChangeProductionRunTaskStatus(
                        request.ChangeProductionRunTaskStatusDto.ProductionRunId,
                        request.ChangeProductionRunTaskStatusDto.TaskId,
                        request.ChangeProductionRunTaskStatusDto.StatusId,
                        request.ChangeProductionRunTaskStatusDto.IssueAllComponents
                    ); 
                    
                    await _context.SaveChangesAsync(cancellationToken);

                    await transaction.CommitAsync(cancellationToken);
                    
                    // Fetch the updated production run if needed
                    if (response.MainProductionRunStartDate == null && response.MainProductionRunStatus != null)
                    {
                        var updatedProductionRun = await _context.WorkEfforts.FindAsync(request.ChangeProductionRunTaskStatusDto.ProductionRunId);

                        response.MainProductionRunStartDate = updatedProductionRun.ActualStartDate;
                        response.MainProductionRunStatus = updatedProductionRun.CurrentStatusId switch
                        {
                            "PRUN_CREATED" => "Created",
                            "PRUN_SCHEDULED" => "Scheduled",
                            "PRUN_DOC_PRINTED" => "Confirmed",
                            "PRUN_RUNNING" => "Running",
                            "PRUN_COMPLETED" => "Completed",
                            "PRUN_CLOSED" => "Closed",
                            _ => "Unknown"
                        };
                    }

                    return Result<ChangeProductionRunTaskStatusResult>.Success(response);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(cancellationToken);

                    // Handle exceptions and return failure response
                    return Result<ChangeProductionRunTaskStatusResult>.Failure($"Error changing production run status: {ex.Message}");
                }
            }
        }
    }
}

// Logic for 'Start' production run task status

// ChangeProductionRunTaskStatus starts
// by getting the main production run

// then it proceeds to get GetProductionRunRoutingTasks

// it then gets the production run task
// that is to be started

// it then checks if all tasks before 
// this task have been completed or running

// it then checks if all tasks other
// than this task are completed

/*For tasks moving to PRUN_RUNNING:
Ensures prerequisite tasks are complete.
    Updates the task and main production run status to PRUN_RUNNING.
    Sets the actualStartDate when transitioning to PRUN_RUNNING.
    For tasks moving to PRUN_COMPLETED:
Issues all components if required.
    Updates the task and main production run status to PRUN_COMPLETED.
    Calculates and stores the actual costs.*/