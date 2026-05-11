using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Accounting.GlobalGlSettings;

public class GetGlReports
{
    public class GlReportsEnvelope
    {
        public List<GlReportDto> GlReports { get; set; } = new();
        public int TotalCount { get; set; }
    }

    public class GlReportDto
    {
        public string GlReportId { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class Query : IRequest<Result<GlReportsEnvelope>>
    {
        public GlReportParams? Params { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<GlReportsEnvelope>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<GlReportsEnvelope>> Handle(Query request, CancellationToken ct)
        {
            var query = _context.GlReports
                .Select(x => new GlReportDto
                {
                    GlReportId = x.GlReportId,
                    Description = x.DescriptionArabic ?? x.Description ?? x.GlReportId
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

            var envelope = new GlReportsEnvelope
            {
                GlReports = items,
                TotalCount = total
            };

            return Result<GlReportsEnvelope>.Success(envelope);
        }
    }
}

public class GlReportParams
{
    public int Skip { get; set; } = 0;
    public int PageSize { get; set; } = 100;
    public string? SearchTerm { get; set; }
}
