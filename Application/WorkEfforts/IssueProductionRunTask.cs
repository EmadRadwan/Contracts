using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Core;
using Application.Manufacturing;
using Application.WorkEfforts;
using Persistence;

namespace Application.WorkEfforts
{
    public class IssueProductionRunTask
    {
        public class Command : IRequest<Results<IssueProductionRunTaskResult>>
        {
            public IssueProductionRunTaskParams IssueProductionRunTaskParams { get; set; }
        }

        public class Handler : IRequestHandler<Command, Results<IssueProductionRunTaskResult>>
        {
            private readonly IProductionRunService _productionRunService;
            private readonly DataContext _context;

            public Handler(IProductionRunService productionRunService, DataContext context)
            {
                _productionRunService = productionRunService;
                _context = context;
            }

            public async Task<Results<IssueProductionRunTaskResult>> Handle(Command request, CancellationToken cancellationToken)
            {
                await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    // Call the service method to change production run status

                    var result = await _productionRunService.IssueProductionRunTask(
                        request.IssueProductionRunTaskParams.WorkEffortId,
                        request.IssueProductionRunTaskParams.ReserveOrderEnumId,
                        request.IssueProductionRunTaskParams.FailIfItemsAreNotAvailable,
                        request.IssueProductionRunTaskParams.FailIfItemsAreNotOnHand
                    );
                    
                    
                    // Ensures early exit with meaningful error if issuance fails
                    if (!result.IsSuccess)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return Results<IssueProductionRunTaskResult>.Failure(result.ErrorMessage, result.ErrorCode);
                    }
                    
                    await _context.SaveChangesAsync(cancellationToken);

                    await transaction.CommitAsync(cancellationToken);
                    

                    return Results<IssueProductionRunTaskResult>.Success(result.Value);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(cancellationToken);

                    return Results<IssueProductionRunTaskResult>.Failure(
                        ex.Message ?? "An unexpected error occurred while issuing production run task.");
                }
            }
        }
    }
}

// IssueProductionRunTask
// 1 - select from WorkEffortGoodStandards
// where StatusId == "WEGS_CREATED" and WorkEffortGoodStdTypeId == "PRUNT_PROD_NEEDED"
// with valid dates

// 2 - loop in the results components
//  check if there're issuances in WorkEffortInventoryAssigns
// for the curret  componet
// determine the quantity to issue by comparing
// total issuance with estimaed quantity

// 3 - call IssueProductionRunTaskComponent


// IssueProductionRunTaskComponent
// 1 - select the workEffort
// 2 - select the productioRun
// 3 - check if fromDate is null 
