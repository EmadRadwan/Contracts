using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Accounting.GlobalGlSettings;

public class GetGlSubClasses
{
    public class GlSubClassesEnvelope
    {
        public List<GlSubClassDto> GlSubClasses { get; set; } = new();
        public int TotalCount { get; set; }
    }

    public class GlSubClassDto
    {
        public string GlSubClassId { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class Query : IRequest<Result<GlSubClassesEnvelope>>
    {
        public GlSubClassParams? Params { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<GlSubClassesEnvelope>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<GlSubClassesEnvelope>> Handle(Query request, CancellationToken ct)
        {
            var query = _context.GlSubClasses
                .Select(x => new GlSubClassDto
                {
                    GlSubClassId = x.GlSubClassId,
                    Description = x.DescriptionArabic ?? x.Description ?? x.GlSubClassId
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

            var envelope = new GlSubClassesEnvelope
            {
                GlSubClasses = items,
                TotalCount = total
            };

            return Result<GlSubClassesEnvelope>.Success(envelope);
        }
    }
}

public class GlSubClassParams
{
    public int Skip { get; set; } = 0;
    public int PageSize { get; set; } = 100;
    public string? SearchTerm { get; set; }
}
