#nullable enable
using Application.Manufacturing;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.OData.Query;
using Persistence;

namespace Application.Projects;

public class ProjectsList
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
            _context = context;
            _mapper = mapper;
        }

        public async Task<IQueryable<WorkEffortRecord>> Handle(Query request, CancellationToken cancellationToken)
        {
            var query = from we in _context.WorkEfforts
                join p in _context.Parties on we.PartyId equals p.PartyId into partyGroup
                from p in partyGroup.DefaultIfEmpty()
                join si in _context.StatusItems on we.CurrentStatusId equals si.StatusId into statusGroup
                from si in statusGroup.DefaultIfEmpty()
                where we.WorkEffortTypeId == "PROJECT"
                select new WorkEffortRecord
                {
                    WorkEffortId = we.WorkEffortId,
                    ProjectNum = we.ProjectNum,
                    ProjectName = we.ProjectName,
                    CurrentStatusId = we.CurrentStatusId,
                    EstimatedStartDate = we.EstimatedStartDate,
                    EstimatedCompletionDate = we.EstimatedCompletionDate,
                };

            return query;
        }
    }
}