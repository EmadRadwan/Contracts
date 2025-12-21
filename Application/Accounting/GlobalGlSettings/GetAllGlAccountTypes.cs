using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Accounting.GlobalGlSettings;

public class GetAllGlAccountTypes
{
    public class GlAccountTypesEnvelope
    {
        public List<GlAccountTypeDto> GlAccountTypes { get; set; } = new();
        public int TotalCount { get; set; }
    }

    public class GlAccountTypeDto
    {
        public string GlAccountTypeId { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class Query : IRequest<Result<GlAccountTypesEnvelope>>
    {
        public GlAccountTypeParams? Params { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<GlAccountTypesEnvelope>>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public Handler(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<GlAccountTypesEnvelope>> Handle(Query request, CancellationToken ct)
        {
            var query = _context.GlAccountTypes
                .Select(x => new GlAccountTypeDto
                {
                    GlAccountTypeId = x.GlAccountTypeId,
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

            var envelope = new GlAccountTypesEnvelope
            {
                GlAccountTypes = items,
                TotalCount = total
            };

            return Result<GlAccountTypesEnvelope>.Success(envelope);
        }
    }
}

public class GlAccountTypeParams
{
    public int Skip { get; set; } = 0;
    public int PageSize { get; set; } = 20;
    public string? SearchTerm { get; set; }
}