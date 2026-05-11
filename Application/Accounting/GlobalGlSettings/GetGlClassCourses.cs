using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Accounting.GlobalGlSettings;

public class GetGlClassCourses
{
    public class GlClassCoursesEnvelope
    {
        public List<GlClassCourseDto> GlClassCourses { get; set; } = new();
        public int TotalCount { get; set; }
    }

    public class GlClassCourseDto
    {
        public string GlClassCourseId { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class Query : IRequest<Result<GlClassCoursesEnvelope>>
    {
        public GlClassCourseParams? Params { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<GlClassCoursesEnvelope>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<GlClassCoursesEnvelope>> Handle(Query request, CancellationToken ct)
        {
            var query = _context.GlClassCourses
                .Select(x => new GlClassCourseDto
                {
                    GlClassCourseId = x.GlClassCourseId,
                    Description = x.DescriptionArabic ?? x.Description ?? x.GlClassCourseId
                })
                .OrderBy(x => x.Description)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Params?.SearchTerm))
            {
                var term = request.Params.SearchTerm.Trim().ToLower();
                query = query.Where(x => x.Description.ToLower().Contains(term));
            }

            var total = await query.CountAsync(ct);

            var items = await query
                .Skip(request.Params?.Skip ?? 0)
                .Take(request.Params?.PageSize ?? 100)
                .ToListAsync(ct);

            var envelope = new GlClassCoursesEnvelope
            {
                GlClassCourses = items,
                TotalCount = total
            };

            return Result<GlClassCoursesEnvelope>.Success(envelope);
        }
    }
}

public class GlClassCourseParams
{
    public int Skip { get; set; } = 0;
    public int PageSize { get; set; } = 100;
    public string? SearchTerm { get; set; }
}
