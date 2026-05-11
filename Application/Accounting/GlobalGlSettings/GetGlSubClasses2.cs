using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Accounting.GlobalGlSettings;

public class GetGlSubClasses2
{
    public class GlSubClasses2Envelope
    {
        public List<GlSubClass2Dto> GlSubClasses2 { get; set; } = new();
        public int TotalCount { get; set; }
    }

    public class GlSubClass2Dto
    {
        public string GlSubClass2Id { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class Query : IRequest<Result<GlSubClasses2Envelope>>
    {
        public GlSubClass2Params? Params { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<GlSubClasses2Envelope>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<GlSubClasses2Envelope>> Handle(Query request, CancellationToken ct)
        {
            var query = _context.GlSubClasses2
                .Select(x => new GlSubClass2Dto
                {
                    GlSubClass2Id = x.GlSubClass2Id,
                    Description = x.DescriptionArabic ?? x.Description ?? x.GlSubClass2Id
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

            var envelope = new GlSubClasses2Envelope
            {
                GlSubClasses2 = items,
                TotalCount = total
            };

            return Result<GlSubClasses2Envelope>.Success(envelope);
        }
    }
}

public class GlSubClass2Params
{
    public int Skip { get; set; } = 0;
    public int PageSize { get; set; } = 100;
    public string? SearchTerm { get; set; }
}
