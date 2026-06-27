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
            // Build ancestor chain: the selected GL account + all its parents up the tree.
            // A project whose GlAccountId or OperatingExpenseGlAccountId appears anywhere in
            // this chain is considered the owner of the selected account.
            var ancestorIds = new HashSet<string>();

            if (!string.IsNullOrEmpty(request.GlAccountId))
            {
                ancestorIds.Add(request.GlAccountId);

                var allGlAccounts = await _context.GlAccounts
                    .AsNoTracking()
                    .Select(g => new { g.GlAccountId, g.ParentGlAccountId })
                    .ToListAsync(cancellationToken);

                var currentId = request.GlAccountId;
                while (true)
                {
                    var current = allGlAccounts.FirstOrDefault(g => g.GlAccountId == currentId);
                    if (current?.ParentGlAccountId == null) break;
                    ancestorIds.Add(current.ParentGlAccountId);
                    currentId = current.ParentGlAccountId;
                }
            }

            var query = _context.WorkEfforts.AsQueryable();

            if (ancestorIds.Count > 0)
            {
                query = query.Where(x =>
                    ancestorIds.Contains(x.GlAccountId) ||
                    ancestorIds.Contains(x.OperatingExpenseGlAccountId));
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
