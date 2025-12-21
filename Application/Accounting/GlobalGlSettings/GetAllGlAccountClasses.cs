using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Accounting.GlobalGlSettings;

public class GetAllGlAccountClasses
{
    public class GlAccountClassesEnvelope
    {
        public List<GlAccountClassDto> GlAccountClasses { get; set; } = new();
        public int TotalCount { get; set; }
    }

    public class GlAccountClassDto
    {
        public string GlAccountClassId { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class Query : IRequest<Result<GlAccountClassesEnvelope>>
    {
        public GlAccountClassParams? Params { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<GlAccountClassesEnvelope>>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public Handler(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<GlAccountClassesEnvelope>> Handle(Query request, CancellationToken ct)
        {
            var query = _context.GlAccountClasses
                .Select(x => new GlAccountClassDto
                {
                    GlAccountClassId = x.GlAccountClassId,
                    Description = x.DescriptionArabic
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
                .Take(request.Params?.PageSize ?? 20)
                .ToListAsync(ct);

            var envelope = new GlAccountClassesEnvelope
            {
                GlAccountClasses = items,
                TotalCount = total
            };

            return Result<GlAccountClassesEnvelope>.Success(envelope);
        }
    }
}

public class GlAccountClassParams
{
    public int Skip { get; set; } = 0;
    public int PageSize { get; set; } = 20;
    public string? SearchTerm { get; set; }
}