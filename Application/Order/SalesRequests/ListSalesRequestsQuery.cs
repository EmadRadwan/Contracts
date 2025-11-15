using MediatR;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using Persistence;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Order.SalesRequests;

class RawSalesRequest
{
    public string SalesRequestId { get; set; } = null!;
    public string ProductId { get; set; } = null!;
    public string FromPartyId { get; set; } = null!;
    public decimal? ApartmentPricePerM2 { get; set; }
    public decimal? GardenPricePerM2 { get; set; }
    public decimal? Discount { get; set; }
    public decimal? TotalPrice { get; set; }
    public decimal? AdvancePayment { get; set; }
    public int? NumberOfInstallments { get; set; }
    public DateTime? DateOfFirstInstallment { get; set; }
    public int? DurationBetweenInstallments { get; set; }
    public DateTime? SaleDate { get; set; }
    public string? Comments { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }

    public string ProductName { get; set; } = null!;
    public string? ProjectId { get; set; }
    public string? FloorNumber { get; set; }
    public decimal? ApartmentSpaceM2 { get; set; }
    public decimal? GardenSpaceM2 { get; set; }
    public string? ApartmentStatusId { get; set; }

    public string Description { get; set; } = null!;
    public string? DescriptionArabic { get; set; }

    public string? PartyDescription { get; set; }
}

public class ListSalesRequestsQuery
{
    public class Query : IRequest<IQueryable<SalesRequestRecord>>
    {
        public ODataQueryOptions<SalesRequestRecord> Options { get; set; } = null!;
        public string Language { get; set; } = "en";
    }

    public class Handler : IRequestHandler<Query, IQueryable<SalesRequestRecord>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context) => _context = context;

        public async Task<IQueryable<SalesRequestRecord>> Handle(Query request,
                                                                CancellationToken ct)
        {
            var language = request.Language;

            // -------------------------------------------------------------
            // 1. Load lookup dictionaries once (in-memory)
            // -------------------------------------------------------------
            // REFACTOR: Load once per request → O(1) per row, avoids N+1
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

            var statusLookup = await _context.StatusItems
                .Where(s => s.StatusTypeId == "APARTMENT_STATUS")
                .ToDictionaryAsync(s => s.StatusId, s => s.Description ?? s.StatusId, ct);

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

            // -------------------------------------------------------------
            // 2. DB query – only raw columns (EF-translatable)
            // -------------------------------------------------------------
            // REFACTOR: Left join for Party (phone not in table)
            var dbQuery = from sr in _context.SalesRequests
                          join p in _context.Products on sr.ProductId equals p.ProductId
                          join pt in _context.ProductTypes on p.ProductTypeId equals pt.ProductTypeId
                          join c in _context.Parties on sr.FromPartyId equals c.PartyId into partyGrp
                          from c in partyGrp.DefaultIfEmpty()
                          select new RawSalesRequest
                          {
                              SalesRequestId = sr.SalesRequestId,
                              ProductId = sr.ProductId,
                              FromPartyId = sr.FromPartyId,
                              ApartmentPricePerM2 = sr.ApartmentPricePerM2,
                              GardenPricePerM2 = sr.GardenPricePerM2,
                              Discount = sr.Discount,
                              TotalPrice = sr.TotalPrice,
                              AdvancePayment = sr.AdvancePayment,
                              NumberOfInstallments = sr.NumberOfInstallments,
                              DateOfFirstInstallment = sr.DateOfFirstInstallment,
                              DurationBetweenInstallments = sr.DurationBetweenInstallments,
                              SaleDate = sr.SaleDate,
                              Comments = sr.Comments,
                              CreatedStamp = sr.CreatedStamp,
                              LastUpdatedStamp = sr.LastUpdatedStamp,

                              ProductName = p.ProductName,
                              ProjectId = p.ProjectId,
                              FloorNumber = p.FloorNumber,
                              ApartmentSpaceM2 = p.ApartmentSpaceM2,
                              GardenSpaceM2 = p.GardenSpaceM2,
                              ApartmentStatusId = p.ApartmentStatusId,

                              Description = pt.Description,
                              DescriptionArabic = pt.DescriptionArabic,

                              PartyDescription = c != null ? c.Description : null
                          };

            // -------------------------------------------------------------
            // 3. Materialize to List<RawSalesRequest>
            // -------------------------------------------------------------
            // REFACTOR: Materialize early to allow TryGetValue in-memory
            var materialized = await dbQuery.ToListAsync(ct);

            // -------------------------------------------------------------
            // 4. Project into SalesRequestRecord (in-memory)
            // -------------------------------------------------------------
            // REFACTOR: All TryGetValue calls are now in-memory → no EF error
            var records = materialized
                .Select(x => new SalesRequestRecord
                {
                    SalesRequestId = x.SalesRequestId,
                    ApartmentId = x.ProductId,
                    ApartmentName = x.ProductName,

                    ProductTypeDescription = language == "ar"
                        ? x.DescriptionArabic ?? x.Description
                        : x.Description,

                    FromPartyId = x.FromPartyId,
                    FromPartyName = x.PartyDescription ?? string.Empty,

                    ApartmentPricePerM2 = x.ApartmentPricePerM2,
                    GardenPricePerM2 = x.GardenPricePerM2,
                    Discount = x.Discount,
                    TotalPrice = x.TotalPrice,
                    AdvancePayment = x.AdvancePayment,
                    NumberOfInstallments = x.NumberOfInstallments,
                    DateOfFirstInstallment = x.DateOfFirstInstallment,
                    DurationBetweenInstallments = x.DurationBetweenInstallments,

                    ProjectName = x.ProjectId != null && projectNameLookup.TryGetValue(x.ProjectId, out var pn)
                        ? pn
                        : string.Empty,

                    FloorNumber = x.FloorNumber != null && floorMap.TryGetValue(x.FloorNumber, out var fn)
                        ? fn
                        : x.FloorNumber ?? string.Empty,

                    ApartmentSpaceM2 = x.ApartmentSpaceM2 ?? 0m,
                    GardenSpaceM2 = x.GardenSpaceM2,

                    ApartmentStatusDescription = x.ApartmentStatusId != null && statusLookup.TryGetValue(x.ApartmentStatusId, out var sd)
                        ? sd
                        : x.ApartmentStatusId ?? string.Empty,

                    SaleDate = x.SaleDate,
                    Comments = x.Comments,
                    CreatedStamp = x.CreatedStamp,
                    LastUpdatedStamp = x.LastUpdatedStamp
                })
                .AsQueryable();

            // -------------------------------------------------------------
            // 5. Apply OData to IQueryable<SalesRequestRecord>
            // -------------------------------------------------------------
            // REFACTOR: Apply OData after projecting to correct type
            var final = request.Options.ApplyTo(records) as IQueryable<SalesRequestRecord>
                        ?? records;

            return final;
        }
    }
}