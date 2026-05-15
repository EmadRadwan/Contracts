using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Accounting.GlobalGlSettings;

public class GetGlAccountCourseLabels
{
    public class GlAccountCourseLabelsEnvelope
    {
        public List<GlAccountCourseLabelDto> GlAccountCourseLabels { get; set; } = new();
        public int TotalCount { get; set; }
    }

    public class GlAccountCourseLabelDto
    {
        public string GlAccountCourseLabelId { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class Query : IRequest<Result<GlAccountCourseLabelsEnvelope>>
    {
        public GlAccountCourseLabelParams? Params { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<GlAccountCourseLabelsEnvelope>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<GlAccountCourseLabelsEnvelope>> Handle(Query request, CancellationToken ct)
        {
            var query = _context.GlAccountCourseLabels
                .Select(x => new GlAccountCourseLabelDto
                {
                    GlAccountCourseLabelId = x.GlAccountCourseLabelId,
                    Description = x.DescriptionArabic ?? x.Description ?? x.GlAccountCourseLabelId
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

            var envelope = new GlAccountCourseLabelsEnvelope
            {
                GlAccountCourseLabels = items,
                TotalCount = total
            };

            return Result<GlAccountCourseLabelsEnvelope>.Success(envelope);
        }
    }
}

public class GlAccountCourseLabelParams
{
    public int Skip { get; set; } = 0;
    public int PageSize { get; set; } = 100;
    public string? SearchTerm { get; set; }
}
