using MediatR;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Order.SalesRequests;

public class ListReserveRequestsQuery
{
    // -----------------------------------------------------------------
    // 1. Raw projection – only EF-translatable columns
    // -----------------------------------------------------------------
    private class RawReserveRequest
    {
        public string ReserveRequestId { get; set; } = null!;
        public string ProductId { get; set; } = null!;
        public string FromPartyId { get; set; } = null!;
        public string? EmployeePartyId { get; set; }

        public DateTime? ReserveDate { get; set; }
        public decimal? ReserveAmount { get; set; }
        public string? Comments { get; set; }
        public string? PayMethod { get; set; }
        public string? StatusId { get; set; }

        public DateTime? CreatedStamp { get; set; }
        public DateTime? LastUpdatedStamp { get; set; }

        // Navigation-related fields
        public string ProductName { get; set; } = null!;
        public string? ProjectId { get; set; }
        public string? FloorNumber { get; set; }
        public decimal? ApartmentSpaceM2 { get; set; }
        public string? ApartmentStatusId { get; set; }
        public string Description { get; set; } = null!; // ProductType.Description
        public string? DescriptionArabic { get; set; }

        public string? CustomerDescription { get; set; }
        public string? EmployeeDescription { get; set; }
        public string? FromPartyPhone { get; set; }
    }

    // -----------------------------------------------------------------
    // 2. Final record returned to client (after in-memory enrichment)
    // -----------------------------------------------------------------
    public class ReserveRequestRecord
    {
        public string ReserveRequestId { get; set; } = null!;
        public string ApartmentId { get; set; } = null!;
        public string ApartmentName { get; set; } = null!;
        public string ProductTypeDescription { get; set; } = null!;
        public string ProjectName { get; set; } = null!;
        public string FloorNumber { get; set; } = null!;
        public decimal ApartmentSpaceM2 { get; set; }

        public string FromPartyId { get; set; } = null!;
        public string FromPartyName { get; set; } = null!;
        public string? FromPartyPhone { get; set; }

        public string? EmployeePartyId { get; set; }
        public string EmployeeName { get; set; } = null!;

        public DateTime? ReserveDate { get; set; }
        public decimal? ReserveAmount { get; set; }
        public string? PayMethod { get; set; }
        public string? Comments { get; set; }

        public string StatusId { get; set; } = null!;
        public string StatusDescription { get; set; } = null!;

        public DateTime? CreatedStamp { get; set; }
        public DateTime? LastUpdatedStamp { get; set; }
    }

    // -----------------------------------------------------------------
    // 3. Query + Handler
    // -----------------------------------------------------------------
    public class Query : IRequest<IQueryable<ReserveRequestRecord>>
    {
        public ODataQueryOptions<ReserveRequestRecord> Options { get; set; } = null!;
        public string Language { get; set; } = "en";
    }

    public class Handler : IRequestHandler<Query, IQueryable<ReserveRequestRecord>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context) => _context = context;

        public async Task<IQueryable<ReserveRequestRecord>> Handle(Query request, CancellationToken ct)
        {
            var language = request.Language;

            // -------------------------------------------------------------
            // 1. Load lookup dictionaries once (in-memory)
            // -------------------------------------------------------------
            var projectNameLookup = await _context.WorkEfforts
                .Where(w => w.WorkEffortTypeId == "PROJECT")
                .GroupBy(w => w.WorkEffortId)
                .Select(g => new
                {
                    ProjectId = g.Key,
                    ProjectName = g.OrderByDescending(w => w.WorkEffortId)
                        .Select(w => w.ProjectName)
                        .FirstOrDefault()
                })
                .ToDictionaryAsync(x => x.ProjectId, x => x.ProjectName ?? "", ct);

            var apartmentStatusLookup = await _context.StatusItems
                .Where(s => s.StatusTypeId == "APARTMENT_STATUS")
                .ToDictionaryAsync(s => s.StatusId, s => s.Description ?? s.StatusId, ct);

            var reserveRequestStatusLookup = await _context.StatusItems
                .Where(s => s.StatusTypeId == "RESERVE_REQUEST_STATUS") // adjust if different
                .ToDictionaryAsync(s => s.StatusId, s => language == "ar"
                    ? s.DescriptionArabic ?? s.Description ?? s.StatusId
                    : s.Description ?? s.StatusId, ct);

            var floorMap = new Dictionary<string, string>
            {
                { "0", "الطابق الأرضي" }, { "1", "الطابق الأول" }, { "2", "الطابق الثاني" },
                { "3", "الطابق الثالث" }, { "4", "الطابق الرابع" }, { "5", "الطابق الخامس" },
                { "6", "الطابق السادس" }
            };

            // -------------------------------------------------------------
            // 2. DB query – only raw columns (EF-translatable)
            // -------------------------------------------------------------
            var dbQuery = from rr in _context.ReserveRequests
                join p in _context.Products on rr.ProductId equals p.ProductId
                join pt in _context.ProductTypes on p.ProductTypeId equals pt.ProductTypeId
                join c in _context.Parties on rr.FromPartyId equals c.PartyId into customerGrp
                from c in customerGrp.DefaultIfEmpty()
                join e in _context.Parties on rr.EmployeePartyId equals e.PartyId into employeeGrp
                from e in employeeGrp.DefaultIfEmpty()
                select new RawReserveRequest
                {
                    ReserveRequestId = rr.ReserveRequestId,
                    ProductId = rr.ProductId,
                    FromPartyId = rr.FromPartyId,
                    EmployeePartyId = rr.EmployeePartyId,

                    ReserveDate = rr.ReserveDate,
                    ReserveAmount = rr.ReserveAmount,
                    Comments = rr.Comments,
                    PayMethod = rr.PayMethod,
                    StatusId = rr.StatusId,

                    CreatedStamp = rr.CreatedStamp,
                    LastUpdatedStamp = rr.LastUpdatedStamp,

                    ProductName = p.ProductName,
                    ProjectId = p.ProjectId,
                    FloorNumber = p.FloorNumber,
                    ApartmentSpaceM2 = p.ApartmentSpaceM2,
                    ApartmentStatusId = p.ApartmentStatusId,
                    Description = pt.Description,
                    DescriptionArabic = pt.DescriptionArabic,

                    CustomerDescription = c != null ? c.Description : null,
                    EmployeeDescription = e != null ? e.Description : null,
                };

            // -------------------------------------------------------------
            // 3. Materialize early to allow in-memory lookups
            // -------------------------------------------------------------
            var materialized = await dbQuery.ToListAsync(ct);

            // -------------------------------------------------------------
            // 4. Final projection with in-memory enrichment
            // -------------------------------------------------------------
            var records = materialized.Select(x => new ReserveRequestRecord
            {
                ReserveRequestId = x.ReserveRequestId,
                ApartmentId = x.ProductId,
                ApartmentName = x.ProductName,
                ProductTypeDescription = language == "ar" ? x.DescriptionArabic ?? x.Description : x.Description,
                ProjectName = x.ProjectId != null && projectNameLookup.TryGetValue(x.ProjectId, out var pn)
                    ? pn
                    : string.Empty,
                FloorNumber = x.FloorNumber != null && floorMap.TryGetValue(x.FloorNumber, out var fn)
                    ? fn
                    : x.FloorNumber ?? string.Empty,
                ApartmentSpaceM2 = x.ApartmentSpaceM2 ?? 0m,

                FromPartyId = x.FromPartyId,
                FromPartyName = x.CustomerDescription ?? string.Empty,
                FromPartyPhone = x.FromPartyPhone,

                EmployeePartyId = x.EmployeePartyId,
                EmployeeName = x.EmployeeDescription ?? string.Empty,

                ReserveDate = x.ReserveDate,
                ReserveAmount = x.ReserveAmount,
                PayMethod = x.PayMethod,
                Comments = x.Comments,

                StatusId = x.StatusId ?? string.Empty,
                StatusDescription = x.StatusId != null && reserveRequestStatusLookup.TryGetValue(x.StatusId, out var sd)
                    ? sd
                    : x.StatusId ?? string.Empty,

                CreatedStamp = x.CreatedStamp,
                LastUpdatedStamp = x.LastUpdatedStamp
            }).AsQueryable();

            // -------------------------------------------------------------
            // 5. Apply OData ($filter, $orderby, $skip, $top, etc.)
            // -------------------------------------------------------------
            var final = request.Options.ApplyTo(records) as IQueryable<ReserveRequestRecord>
                        ?? records;

            return final;
        }
    }
}