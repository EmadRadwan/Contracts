using Application.Manufacturing;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

public class GetWorkEffort
{
    // REFACTOR: Query class to encapsulate workEffortId
    public class Query : IRequest<Result<WorkEffortDto>>
    {
        public string WorkEffortId { get; set; } = null!;
    }


    // REFACTOR: Handler to process the query
    // Purpose: Retrieves work effort from database and maps to DTO
    public class Handler : IRequestHandler<Query, Result<WorkEffortDto>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<WorkEffortDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var workEffort = await _context.WorkEfforts
                .AsNoTracking()
                .FirstOrDefaultAsync(we => we.WorkEffortId == request.WorkEffortId, cancellationToken);

            if (workEffort == null)
                return Result<WorkEffortDto>.Failure($"Work effort with ID {request.WorkEffortId} not found.");

            // REFACTOR: Map entity to DTO
            // Purpose: Ensures only required fields are returned to the client
            var dto = new WorkEffortDto
            {
                WorkEffortId = workEffort.WorkEffortId,
                WorkEffortName = workEffort.WorkEffortName,
                WorkEffortTypeId = workEffort.WorkEffortTypeId,
                WorkEffortPurposeTypeId = workEffort.WorkEffortPurposeTypeId,
                FixedAssetId = workEffort.FixedAssetId,
                EstimatedSetupMillis = workEffort.EstimatedSetupMillis,
                EstimatedMilliSeconds = workEffort.EstimatedMilliSeconds,
                EstimateCalcMethod = workEffort.EstimateCalcMethod,
                CurrentStatusId = workEffort.CurrentStatusId
            };

            return Result<WorkEffortDto>.Success(dto);
        }
    }
}