using Microsoft.EntityFrameworkCore;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Persistence;  // your DbContext namespace

namespace Application.Order.SalesRequests
{
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

            public Handler(DataContext context)
            {
                _context = context;
            }

            public async Task<IQueryable<SalesRequestRecord>> Handle(Query request, CancellationToken ct)
            {
                var language = request.Language;

                // ────────────────────────────────────────────────────────────────
                // 1. Load all necessary lookup dictionaries (small, in-memory)
                //    Done once per request – very fast
                // ────────────────────────────────────────────────────────────────
                var projectNameLookup = await _context.WorkEfforts
                    .Where(w => w.WorkEffortTypeId == "PROJECT")
                    .ToDictionaryAsync(
                        w => w.WorkEffortId,
                        w => w.ProjectName ?? "",
                        ct);

                var apartmentStatusLookup = await _context.StatusItems
                    .Where(s => s.StatusTypeId == "APARTMENT_STATUS")
                    .ToDictionaryAsync(
                        s => s.StatusId,
                        s => s.Description ?? s.StatusId,
                        ct);

                var salesRequestStatusLookup = await _context.StatusItems
                    .Where(s => s.StatusTypeId == "SALES_REQUEST_STATUS")
                    .ToDictionaryAsync(
                        s => s.StatusId,
                        s => language == "ar" ? (s.DescriptionArabic ?? s.Description) : s.Description,
                        ct);

                var floorMap = new Dictionary<string, string>
                {
                    { "0", "الطابق الأرضي" },
                    { "1", "الطابق الأول" },
                    { "2", "الطابق الثاني" },
                    { "3", "الطابق الثالث" },
                    { "4", "الطابق الرابع" },
                    { "5", "الطابق الخامس" },
                    { "6", "الطابق السادس" }
                    // add more floors if needed
                };

                // ────────────────────────────────────────────────────────────────
                // 2. Build the base query – still pure IQueryable
                //    All joins and projections happen in SQL where possible
                // ────────────────────────────────────────────────────────────────
                var query = from sr in _context.SalesRequests
                            join prod in _context.Products on sr.ProductId equals prod.ProductId
                            join pt in _context.ProductTypes on prod.ProductTypeId equals pt.ProductTypeId
                            join status in _context.StatusItems 
                                on prod.ApartmentStatusId equals status.StatusId into statusJoin
                                from aptStatus in statusJoin.DefaultIfEmpty()
                            join customer in _context.Parties on sr.FromPartyId equals customer.PartyId into custJoin
                                from cust in custJoin.DefaultIfEmpty()
                            join employee in _context.Parties on sr.EmployeePartyId equals employee.PartyId into empJoin
                                from emp in empJoin.DefaultIfEmpty()
                            select new
                            {
                                // Raw data – keep only what's needed for projection
                                sr.SalesRequestId,
                                sr.ProductId,
                                prod.ProductName,
                                prod.ProjectId,
                                prod.FloorNumber,
                                prod.ApartmentSpaceM2,
                                prod.GardenSpaceM2,
                                prod.ApartmentStatusId,
                                pt.Description,
                                pt.DescriptionArabic,
                                CustomerDescription = cust != null ? cust.Description : null,
                                EmployeeDescription = emp != null ? emp.Description : null,
                                sr.FromPartyId,
                                sr.EmployeePartyId,
                                sr.StatusId,
                                sr.TotalPrice,
                                sr.AdvancePayment,
                                sr.SaleDate,
                                sr.Comments,
                                sr.CreatedStamp,
                                sr.LastUpdatedStamp,
                                sr.ApartmentPricePerM2,
                                sr.GardenPricePerM2,
                                sr.Discount,
                                sr.NumberOfInstallments,
                                sr.DateOfFirstInstallment,
                                sr.MonthsBetweenInstallments,
                                sr.MaintenanceDeposit,
                                sr.IsChequesDelivered
                            };

                // ────────────────────────────────────────────────────────────────
                // 3. Final projection to SalesRequestRecord – still IQueryable
                //    Lookups are resolved in-memory during enumeration (EF will handle)
                // ────────────────────────────────────────────────────────────────
                var recordsQuery = query.Select(x => new SalesRequestRecord
                {
                    SalesRequestId     = x.SalesRequestId,
                    ApartmentId        = x.ProductId,
                    ApartmentName      = x.ProductName,
                    ProductTypeDescription = language == "ar" ? (x.DescriptionArabic ?? x.Description) : x.Description,
                    FromPartyId        = x.FromPartyId,
                    FromPartyName      = x.CustomerDescription ?? "",
                    EmployeePartyId    = x.EmployeePartyId,
                    EmployeeName       = x.EmployeeDescription ?? "",
                    ProjectName = x.ProjectId != null && projectNameLookup.TryGetValue(x.ProjectId, out var pn)
                        ? pn
                        : string.Empty,

                    FloorNumber = x.FloorNumber != null && floorMap.TryGetValue(x.FloorNumber, out var fn)
                        ? fn
                        : x.FloorNumber ?? string.Empty,
                    ApartmentSpaceM2   = x.ApartmentSpaceM2 ?? 0m,
                    GardenSpaceM2      = x.GardenSpaceM2,
                    ApartmentStatusDescription = x.ApartmentStatusId != null 
                        && apartmentStatusLookup.TryGetValue(x.ApartmentStatusId, out var asd) 
                        ? asd : (x.ApartmentStatusId ?? ""),
                    MaintenanceDeposit = x.MaintenanceDeposit,
                    IsChequesDelivered = x.IsChequesDelivered,
                    StatusId           = x.StatusId ?? "",
                    StatusDescription  = x.StatusId != null 
                        && salesRequestStatusLookup.TryGetValue(x.StatusId, out var srd) 
                        ? srd : (x.StatusId ?? ""),
                    TotalPrice         = x.TotalPrice,
                    AdvancePayment     = x.AdvancePayment,
                    SaleDate           = x.SaleDate,
                    Comments           = x.Comments,
                    CreatedStamp       = x.CreatedStamp,
                    LastUpdatedStamp   = x.LastUpdatedStamp,
                    ApartmentPricePerM2 = x.ApartmentPricePerM2,
                    GardenPricePerM2   = x.GardenPricePerM2,
                    Discount           = x.Discount,
                    NumberOfInstallments = x.NumberOfInstallments,
                    DateOfFirstInstallment = x.DateOfFirstInstallment,
                    MonthsBetweenInstallments = x.MonthsBetweenInstallments
                });

                // Return IQueryable – controller + [EnableQuery] will apply $filter, $orderby, $top, $skip, $count
                return recordsQuery;
            }
        }
    }
}