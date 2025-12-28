using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Catalog.Products;

public class GetSimpleApartmentsLov
{
    public class ApartmentsEnvelope
    {
        public List<ApartmentLovDto> Apartments { get; set; } = new();
        public int ApartmentCount { get; set; }
    }

    public class ApartmentLovDto
    {
        public string ApartmentId { get; set; } = string.Empty; // ProductId
        public string ApartmentName { get; set; } = string.Empty; // ProductName
        public string ApartmentType { get; set; } = string.Empty; // ProductTypeId
        public string ProjectName { get; set; } = string.Empty; // from WorkEffort
        public string FloorNumber { get; set; } = string.Empty;
        public decimal ApartmentSpaceM2 { get; set; }
        public decimal? GardenSpaceM2 { get; set; }
        public decimal? GardenPricePerM2 { get; set; }
        public decimal ApartmentPricePerM2 { get; set; }
        public string ApartmentStatusId { get; set; } = string.Empty;
        public string ApartmentStatusDescription { get; set; }
    }

    public class Query : IRequest<Result<ApartmentsEnvelope>>
    {
        public ApartmentLovParams? Params { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<ApartmentsEnvelope>>
    {
        private readonly DataContext _context;
        private readonly ILogger<Handler> _logger;

        public Handler(DataContext context, ILogger<Handler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Result<ApartmentsEnvelope>> Handle(Query request, CancellationToken ct)
        {
            try
            {
                if (request?.Params == null)
                {
                    _logger.LogWarning("Invalid request: Params is null");
                    return Result<ApartmentsEnvelope>.Failure("Invalid request parameters.");
                }

                const string apartmentType = "APARTMENT";

                // -----------------------------------------------------------------
                // 1. Load Project names (unchanged)
                // -----------------------------------------------------------------
                var projectNameLookup = await _context.WorkEfforts
                    .Where(w => w.WorkEffortTypeId == "PROJECT")
                    .GroupBy(w => w.WorkEffortId) // WorkEffortId = ProjectId
                    .Select(g => new
                    {
                        ProjectId = g.Key,
                        ProjectName = g.OrderByDescending(w => w.WorkEffortId)
                            .Select(w => w.ProjectName)
                            .FirstOrDefault()
                    })
                    .ToDictionaryAsync(x => x.ProjectId, x => x.ProjectName ?? "", ct);

                // -----------------------------------------------------------------
                // 2. Load Status descriptions (new)
                // -----------------------------------------------------------------
                // REFACTOR: Join with StatusItems to fetch the human-readable description.
                //           Improves UX by showing the status text instead of just the Id.
                var statusLookup = await _context.StatusItems
                    .Where(s => s.StatusTypeId == "APARTMENT_STATUS") // adjust type if needed
                    .ToDictionaryAsync(s => s.StatusId, s => s.Description ?? s.StatusId, ct);

                // -----------------------------------------------------------------
                // 3. Base query (unchanged filtering)
                // -----------------------------------------------------------------
                var baseQuery = _context.Products
                    .Where(p => p.ProductTypeId == apartmentType);

                if (!string.IsNullOrWhiteSpace(request.Params.SearchTerm))
                {
                    var term = request.Params.SearchTerm.Trim();
                    baseQuery = baseQuery.Where(p => p.ProductName.Contains(term) ||
                                                     p.FloorNumber.Contains(term));
                }

                var total = await baseQuery.CountAsync(ct);

                // -----------------------------------------------------------------
                // 4. Materialise products
                // -----------------------------------------------------------------
                var products = await baseQuery
                    .OrderBy(p => p.ProductId) 
                    .Skip(request.Params.Skip)
                    .Take(request.Params.PageSize)
                    .ToListAsync(ct);
                
                products = products
                    .OrderBy(p => p.ProductId, Comparer<string>.Create(NaturalCompare))
                    .ToList();

                // -----------------------------------------------------------------
                // 5. Floor-number mapping (new)
                // -----------------------------------------------------------------
                // REFACTOR: Map raw floorNumber to Arabic description using a static lookup.
                //           Keeps the mapping in one place, avoids repeated switch/if-else,
                //           and guarantees consistent UI text.
                var floorMap = new Dictionary<string, string>
                {
                    { "0", "الطابق الأرضي" },
                    { "1", "الطابق الأول" },
                    { "2", "الطابق الثاني" },
                    { "3", "الطابق الثالث" },
                    { "4", "الطابق الرابع" },
                    { "5", "الطابق الخامس" },
                    { "6", "الطابق السادس" }
                };

                // -----------------------------------------------------------------
                // 6. Final projection
                // -----------------------------------------------------------------
                var apartments = products.Select(p => new ApartmentLovDto
                {
                    ApartmentId = p.ProductId,
                    ApartmentName = p.ProductName,
                    ApartmentType = p.ProductTypeId,
                    ProjectName = p.ProjectId != null && projectNameLookup.TryGetValue(p.ProjectId, out var projectName)
                        ? projectName
                        : "",
                    // REFACTOR: Use mapped description for floor number
                    FloorNumber = p.FloorNumber != null && floorMap.TryGetValue(p.FloorNumber, out var floorDesc)
                        ? floorDesc
                        : p.FloorNumber ?? "",
                    ApartmentSpaceM2 = (decimal)p.ApartmentSpaceM2,
                    GardenSpaceM2 = p.GardenSpaceM2,
                    GardenPricePerM2 = p.GardenPricePerM2,
                    ApartmentPricePerM2 = (decimal)p.ApartmentPricePerM2,
                    ApartmentStatusId = p.ApartmentStatusId,
                    ApartmentStatusDescription = p.ApartmentStatusId != null &&
                                                 statusLookup.TryGetValue(p.ApartmentStatusId, out var desc)
                        ? desc : p.ApartmentStatusId
                }).ToList();

                // -----------------------------------------------------------------
                // 7. Envelope & success
                // -----------------------------------------------------------------
                var envelope = new ApartmentsEnvelope
                {
                    Apartments = apartments,
                    ApartmentCount = total
                };

                return Result<ApartmentsEnvelope>.Success(envelope);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve apartments");
                return Result<ApartmentsEnvelope>.Failure("Failed to retrieve apartments.");
            }
        }
    }
    
    static int NaturalCompare(string? a, string? b)
    {
        if (a == null && b == null) return 0;
        if (a == null) return -1;
        if (b == null) return 1;

        var partsA = a.Split('-');
        var partsB = b.Split('-');

        int prefixCompare = string.Compare(partsA[0], partsB[0], StringComparison.OrdinalIgnoreCase);
        if (prefixCompare != 0) return prefixCompare;

        if (partsA.Length == 1 && partsB.Length == 1) return 0;
        if (partsA.Length == 1) return -1;
        if (partsB.Length == 1) return 1;

        if (int.TryParse(partsA[1], out int numA) && int.TryParse(partsB[1], out int numB))
        {
            return numA.CompareTo(numB);
        }

        return string.Compare(partsA[1], partsB[1], StringComparison.OrdinalIgnoreCase);
    }
}

public class ApartmentLovParams
{
    public int Skip { get; set; } = 0;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
}