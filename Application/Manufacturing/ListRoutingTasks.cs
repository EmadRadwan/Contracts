using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.OData.Query;
using Persistence;

namespace Application.Manufacturing;

public class ListRoutingTasks
{
    public class Query : IRequest<IQueryable<WorkEffortRecord>>
    {
        public ODataQueryOptions<WorkEffortRecord> Options { get; set; }
    }

    public class Handler : IRequestHandler<Query, IQueryable<WorkEffortRecord>>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public Handler(DataContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task<IQueryable<WorkEffortRecord>> Handle(Query request, CancellationToken cancellationToken)
        {
            // REFACTOR: Changed inner join to left outer join using GroupJoin and SelectMany with DefaultIfEmpty
            // to include all WorkEfforts even if no matching WorkEffortPurposeType exists.
            // This ensures WorkEffort records are not excluded when purposeType is null, setting
            // WorkEffortPurposeTypeDescription to null for unmatched records.
            var query = from workEffort in _context.WorkEfforts
                        join purposeType in _context.WorkEffortPurposeTypes
                            on workEffort.WorkEffortPurposeTypeId equals purposeType.WorkEffortPurposeTypeId into purposeGroup
                        from purposeType in purposeGroup.DefaultIfEmpty()
                        where workEffort.WorkEffortTypeId == "ROU_TASK"
                        select new WorkEffortRecord
                        {
                            WorkEffortId = workEffort.WorkEffortId,
                            WorkEffortName = workEffort.WorkEffortName,
                            Description = workEffort.Description,
                            QuantityToProduce = workEffort.QuantityToProduce,
                            CurrentStatusId = workEffort.CurrentStatusId,
                            WorkEffortPurposeTypeId = workEffort.WorkEffortPurposeTypeId,
                            WorkEffortPurposeTypeDescription = purposeType != null ? purposeType.Description : null,
                            FixedAssetId = workEffort.FixedAssetId,
                            EstimatedSetupMillis = workEffort.EstimatedSetupMillis,
                            EstimatedMilliSeconds = workEffort.EstimatedMilliSeconds
                        };

            return query;
        }
    }
}