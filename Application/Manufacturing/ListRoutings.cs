using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.OData.Query;
using Persistence;

namespace Application.Manufacturing;

public class ListRoutings
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
            var query = _context.WorkEfforts
                .Where(z => z.WorkEffortTypeId == "ROUTING")
                .Select(x => new WorkEffortRecord
                    {
                        WorkEffortId = x.WorkEffortId,
                        WorkEffortName = x.WorkEffortName,
                        Description = x.Description,
                        QuantityToProduce = x.QuantityToProduce,
                        CurrentStatusId = x.CurrentStatusId
                    }
                );
            
            return query;
        }
    }
}