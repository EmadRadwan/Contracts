using Application.Catalog.Products;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Common.UOMs;

public class GetUOMsLov
{
    public class UOMsEnvelope
    {
        public List<UomDto> Uoms { get; set; }
        public int UomCount { get; set; }
    }

    public class Query : IRequest<Result<UOMsEnvelope>>
    {
        public ProductLovParams? Params { get; set; }
        public string Language { get; set; } // Added to hold Accept-Language header value
    }

    public class Handler : IRequestHandler<Query, Result<UOMsEnvelope>>
    {
        private readonly DataContext _context;
        private readonly ILogger<Handler> _logger;
        private readonly IMapper _mapper;

        public Handler(DataContext context, IMapper mapper, ILogger<Handler> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<UOMsEnvelope>> Handle(Query request, CancellationToken cancellationToken)
        {
            try
            {
                // REFACTOR: Validate input parameters
                // Purpose: Prevent null reference exceptions and ensure valid pagination
                // Context: Checks for null params and language to avoid errors
                if (request?.Params == null)
                {
                    _logger.LogWarning("Invalid request: Params is null");
                    return Result<UOMsEnvelope>.Failure("Invalid request parameters.");
                }

                if (string.IsNullOrEmpty(request.Language))
                {
                    _logger.LogWarning("Invalid request: Language is null or empty, defaulting to 'en'");
                    request.Language = "en";
                }

                // REFACTOR: Define allowed UOM IDs
                // Purpose: Restrict query to specific UOMs from provided JSON list
                // Context: Filters UOMs to match client-provided list
                var allowedUomIds = new List<string>
                {
                    "LN_m", "AR_m2", "VL_m3", "LN_m_linear", "LN_m_half", "LN_m_third",
                    "LN_mm", "LN_in", "VL_l", "WT_kg", "VL_bbl", "VL_bastila",
                    "WT_t", "TM_day", "TM_hr", "QT_thousand"
                };

                // REFACTOR: Determine language preference
                // Purpose: Select appropriate description field (English or Arabic) based on query language
                // Context: Uses Language property from Query, set by controller from Accept-Language header
                bool useArabic = request.Language.StartsWith("ar", StringComparison.OrdinalIgnoreCase);

                // REFACTOR: Build query with filtering and dynamic description
                // Purpose: Fetch UomId and description (English or Arabic) for allowed UOMs with pagination and search
                // Context: Filters by allowedUomIds, applies search on selected description, supports pagination
                var query = _context.Uoms
                    .AsQueryable()
                    .Where(u => allowedUomIds.Contains(u.UomId));

                if (!string.IsNullOrEmpty(request.Params.SearchTerm))
                {
                    query = useArabic
                        ? query.Where(u =>
                            u.DescriptionArabic != null && u.DescriptionArabic.Contains(request.Params.SearchTerm))
                        : query.Where(u => u.Description != null && u.Description.Contains(request.Params.SearchTerm));
                }

                var total = await query.CountAsync(cancellationToken);
                var uoms = await query
                    .OrderBy(u => useArabic ? u.DescriptionArabic : u.Description)
                    .Skip(request.Params.Skip)
                    .Take(request.Params.PageSize)
                    .Select(u => new UomDto
                    {
                        UomId = u.UomId,
                        Description = useArabic ? u.DescriptionArabic ?? u.Description : u.Description
                    })
                    .ToListAsync(cancellationToken);

                var uomEnvelope = new UOMsEnvelope
                {
                    Uoms = uoms,
                    UomCount = total
                };

                // REFACTOR: Log query execution details
                // Purpose: Track query performance and language used for debugging
                // Context: Logs search term, result count, and language from query
                _logger.LogInformation(
                    "Retrieved {UomCount} UOMs for searchTerm {SearchTerm} in language {Language}",
                    uomEnvelope.UomCount,
                    request.Params.SearchTerm ?? "none",
                    useArabic ? "Arabic" : "English");

                return Result<UOMsEnvelope>.Success(uomEnvelope);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving UOMs for searchTerm {SearchTerm}, language {Language}",
                    request?.Params?.SearchTerm ?? "none", request?.Language ?? "unknown");
                return Result<UOMsEnvelope>.Failure("Failed to retrieve UOMs.");
            }
        }
    }
}

public class UomDto
{
    public string UomId { get; set; }
    public string Description { get; set; }
}