using MediatR;
using Application.Manufacturing;
using Persistence;

namespace Application.WorkEfforts
{
    public class UpdateProductionRun
    {
        public class Command : IRequest<Result<UpdateProductionRunResponse>>
        {
            public UpdateProductionRunDto UpdateProductionRunDto { get; set; }
        }

        public class Handler : IRequestHandler<Command, Result<UpdateProductionRunResponse>>
        {
            private readonly IProductionRunService _productionRunService;
            private readonly DataContext _context;

            public Handler(IProductionRunService productionRunService, DataContext context)
            {
                _productionRunService = productionRunService;
                _context = context;
            }

            public async Task<Result<UpdateProductionRunResponse>> Handle(Command request, CancellationToken cancellationToken)
            {
                await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    // Call the service method to update production run

                    var response = await _productionRunService.UpdateProductionRun(
                        request.UpdateProductionRunDto.ProductionRunId,
                        request.UpdateProductionRunDto.Quantity,
                        request.UpdateProductionRunDto.EstimatedStartDate,
                        request.UpdateProductionRunDto.WorkEffortName,
                        request.UpdateProductionRunDto.Description,
                        request.UpdateProductionRunDto.FacilityId);
                        
                    
                    await _context.SaveChangesAsync(cancellationToken);

                    await transaction.CommitAsync(cancellationToken);

                    return Result<UpdateProductionRunResponse>.Success(response);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(cancellationToken);

                    // Handle exceptions and return failure response
                    return Result<UpdateProductionRunResponse>.Failure($"Error updating production run: {ex.Message}");
                }
            }
        }
    }
}

/*
Step-by-Step Configuration for Ofbiz Manufacturing Routing and Tasks:
Define the Routing:

Table: WORK_EFFORT
Key Fields:
WORK_EFFORT_ID: Unique identifier for the routing (e.g., ROUTING_ASPIRIN).
WORK_EFFORT_TYPE_ID: Set to ROUTING to indicate it's a routing.
CURRENT_STATUS_ID: Set to ROU_ACTIVE to indicate active status.
WORK_EFFORT_NAME: Name of the routing (e.g., Aspirin Manufacturing Process).
DESCRIPTION: Detailed description of the routing process.
Reference Static Tables for Routing:

WORK_EFFORT_TYPE:
WORK_EFFORT_TYPE_ID: ROUTING
DESCRIPTION: Routing
WORK_EFFORT_PURPOSE_TYPE:
WORK_EFFORT_PURPOSE_TYPE_ID: ROU_MANUFACTURING
DESCRIPTION: Manufacturing
Define the Routing Tasks:

Table: WORK_EFFORT
Define each task, linking them to the main routing and specifying relevant attributes such as WORK_EFFORT_TYPE_ID (ROU_TASK), WORK_EFFORT_PURPOSE_TYPE_ID (ROU_MANUFACTURING), DESCRIPTION, and FIXED_ASSET_ID.
Reference Static Tables for Routing Tasks:

WORK_EFFORT_TYPE:
WORK_EFFORT_TYPE_ID: ROU_TASK
DESCRIPTION: Routing Task
WORK_EFFORT_PURPOSE_TYPE:
WORK_EFFORT_PURPOSE_TYPE_ID: ROU_MANUFACTURING
DESCRIPTION: Manufacturing
Define the Work Effort Associations:

Table: WORK_EFFORT_ASSOC
Key Fields:
WORK_EFFORT_ID_FROM: Identifier for the parent work effort (e.g., ROUTING_ASPIRIN).
WORK_EFFORT_ID_TO: Identifier for the associated work effort (tasks).
WORK_EFFORT_ASSOC_TYPE_ID: Set to ROUTING_COMPONENT to indicate the association type.
SEQUENCE_NUM: Sequence number to indicate the order of tasks.
Reference Static Tables for Work Effort Associations:
*/



// logic implemented in CreateProductionRun function:
// 1. via GetProductRouting method we get the routingOutMap object
// that the Routing of the main finished product which is a WorkEffort
// and a list of WorkEffortAssocs that are the routing tasks.
// the above records are the 'workeffort config records' that are used to create the production run.

// 2. via GetManufacturingComponents method we get the list of manufacturing components

// 3. we create a new WorkEffort entity with the details from the CreateProductionRunDto
// that represents the dynamic record in WorkEffort table for the production run.

// 4. we create a new WorkEffortGoodStandard entity for the main
// finished product that is being produced in the production run.
// using method CreateWorkEffortGoodStandard

// 5. We loop through the WorkEffortAssocs - configurations of the routing tasks
// 5.1 retrieve the workeffort config record for this task
// 5.2 get estimated time to complete the task
// 5.4 calculate the end date of the task using CalendarService
// 5.5 create a new WorkEffort entity - dynamic - for the task
// 5.6 create a new WorkEffortAssoc entity for the task; Dynamic record in WorkEffortAssoc table
// 5.7 via CloneWorkEffortPartyAssignments method we clone the party assignments from the workeffort config record
// 5.8 via CloneWorkEffortCostCalcs method we clone
// the cost calculations from the workeffort config record
// into a dynamic record in WorkEffortCostCalc table

//Controlling the issuance of materials with certain tasks or with the first task
// in productAssocs table we have the RoutingWorkEffortID
// if the RoutingWorkEffortID is not null, we issue the materials with the mentioned task
// but if it is null, we issue the materials with the first task in the routing tasks list


