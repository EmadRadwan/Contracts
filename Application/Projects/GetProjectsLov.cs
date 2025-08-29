using Application.Interfaces;
using Application.Parties.Parties;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Projects;

public class GetProjectsLov
{
    public class ProjectsEnvelop
    {
        public List<ProjectDto>? Projects { get; set; }
        public int ProjectCount { get; set; }
    }

    public class ProjectDto
    {
        public string WorkEffortId { get; set; }
        public string ProjectName { get; set; }
    }

    public class Query : IRequest<Result<ProjectsEnvelop>>
    {
        public PartyLovParams? Params { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<ProjectsEnvelop>>
    {
        private readonly DataContext _context;
        private readonly ILogger<Handler> _logger;
        private readonly IMapper _mapper;
        private readonly IUserAccessor _userAccessor;

        public Handler(DataContext context, IMapper mapper, IUserAccessor userAccessor, ILogger<Handler> logger)
        {
            _mapper = mapper;
            _context = context;
            _userAccessor = userAccessor;
            _logger = logger;
        }

        public async Task<Result<ProjectsEnvelop>> Handle(Query request, CancellationToken cancellationToken)
        {
            // REFACTOR: Optimized query to filter projects and apply search term efficiently
            var query = _context.WorkEfforts
                .Where(x => x.WorkEffortTypeId == "PROJECT")
                .AsQueryable();

            if (!string.IsNullOrEmpty(request.Params?.SearchTerm))
            {
                var lowerCaseSearchTerm = request.Params.SearchTerm.Trim().ToLower();
                query = query.Where(p => p.ProjectName.ToLower().Contains(lowerCaseSearchTerm));
            }

            var projects = await query
                .OrderBy(x => x.ProjectName)
                .Skip(request.Params?.Skip ?? 0)
                .Take(request.Params?.PageSize ?? 10)
                .Select(x => new ProjectDto
                {
                    WorkEffortId = x.WorkEffortId,
                    ProjectName = x.ProjectName
                })
                .ToListAsync(cancellationToken);

            var projectEnvelop = new ProjectsEnvelop
            {
                Projects = projects,
                ProjectCount = await query.CountAsync(cancellationToken)
            };

            return Result<ProjectsEnvelop>.Success(projectEnvelop);
        }
    }
}