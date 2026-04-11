using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Projects;

public class ListWorkEffortsByGlAccount
{
    public class Query : IRequest<Result<List<WorkEffortDto>>>
    {
        public string GlAccountId { get; set; }
        public string WorkEffortTypeId { get; set; }
        public string WorkEffortParentId { get; set; }
    }

    public class WorkEffortDto
    {
        public string WorkEffortId { get; set; }
        public string WorkEffortName { get; set; }
        public string WorkEffortTypeId { get; set; }
        public string ProjectId { get; set; }
        public string SubProjectName { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<List<WorkEffortDto>>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<List<WorkEffortDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var query = _context.WorkEfforts.AsQueryable();

            if (!string.IsNullOrEmpty(request.GlAccountId))
            {
                query = query.Where(x => x.GlAccountId == request.GlAccountId);
            }

            if (!string.IsNullOrEmpty(request.WorkEffortTypeId))
            {
                query = query.Where(x => x.WorkEffortTypeId == request.WorkEffortTypeId);
            }

            if (!string.IsNullOrEmpty(request.WorkEffortParentId))
            {
                query = query.Where(x => x.ProjectId == request.WorkEffortParentId);
            }

            var result = await query
                .Select(x => new WorkEffortDto
                {
                    WorkEffortId = x.WorkEffortId,
                    WorkEffortName = x.WorkEffortName,
                    WorkEffortTypeId = x.WorkEffortTypeId,
                    ProjectId = x.ProjectId,
                    SubProjectName = x.SubProjectName
                })
                .ToListAsync(cancellationToken);

            return Result<List<WorkEffortDto>>.Success(result);
        }
    }
}
