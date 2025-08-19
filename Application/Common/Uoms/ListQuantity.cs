using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Uoms;

public class ListQuantity
{
    public class Query : IRequest<Result<List<QuantityDto>>>
    {
        public string Language { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<List<QuantityDto>>>
    {
        private readonly DataContext _context;

        private static readonly List<string> AllowedUoms = new List<string> { "WT_kg", "LEN_m", "OTH_ea", "OT_set" };

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<List<QuantityDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            // REFACTOR: Updated query to filter by English UOM descriptions in AllowedUoms list.
            //   Purpose: Ensures only UOMs with English descriptions ("Kilogram", "Meter", "Piece", "Set") are returned.
            //   Improvement: Simplifies filtering by using English descriptions directly, assuming u.Description contains English names.
            var query = _context.Uoms
                .Where(u => AllowedUoms.Contains(u.UomId))
                .Select(u => new QuantityDto
                {
                    QuantityUomId = u.UomId,
                    Description = request.Language == "ar" && u.DescriptionArabic != null ? u.DescriptionArabic : request.Language == "tr" ? u.DescriptionTurkish : u.Description
                })
                .OrderBy(x => x.Description)
                .AsQueryable();

            var uoms = await query
                .ToListAsync();

            return Result<List<QuantityDto>>.Success(uoms);
        }
    }
}