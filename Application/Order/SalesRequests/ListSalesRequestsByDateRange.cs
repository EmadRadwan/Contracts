using Microsoft.EntityFrameworkCore;
using MediatR;
using Persistence;

namespace Application.Order.SalesRequests;

public class ListSalesRequestsByDateRange
{
    public class Query : IRequest<List<SalesRequestRecord>>
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string Language { get; set; } = "en";
    }

    public class Handler : IRequestHandler<Query, List<SalesRequestRecord>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<List<SalesRequestRecord>> Handle(Query request, CancellationToken ct)
        {
            var language = request.Language;

            // 1. Load lookup dictionaries
            var projectNameLookup = await _context.WorkEfforts
                .Where(w => w.WorkEffortTypeId == "PROJECT")
                .ToDictionaryAsync(
                    keySelector: w => w.WorkEffortId,
                    elementSelector: w => w.ProjectName ?? "",
                    cancellationToken: ct);

            var apartmentStatusLookup = await _context.StatusItems
                .Where(s => s.StatusTypeId == "APARTMENT_STATUS")
                .ToDictionaryAsync(
                    keySelector: s => s.StatusId,
                    elementSelector: s => s.Description ?? s.StatusId,
                    cancellationToken: ct);

            var salesRequestStatusLookup = await _context.StatusItems
                .Where(s => s.StatusTypeId == "SALES_REQUEST_STATUS")
                .ToDictionaryAsync(
                    keySelector: s => s.StatusId,
                    elementSelector: s => language == "ar" ? (s.DescriptionArabic ?? s.Description) : s.Description,
                    cancellationToken: ct);

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

            // 2. Base query - filtered by Date
            var baseQuery = from sr in _context.SalesRequests
                            join prod in _context.Products on sr.ProductId equals prod.ProductId
                            join pt in _context.ProductTypes on prod.ProductTypeId equals pt.ProductTypeId
                            join aptStatus in _context.StatusItems
                                on prod.ApartmentStatusId equals aptStatus.StatusId into aptStatusGroup
                                from aptStatus in aptStatusGroup.DefaultIfEmpty()
                            join customer in _context.Parties on sr.FromPartyId equals customer.PartyId into custGroup
                                from cust in custGroup.DefaultIfEmpty()
                            join employee in _context.Parties on sr.EmployeePartyId equals employee.PartyId into empGroup
                                from emp in empGroup.DefaultIfEmpty()
                            where sr.StatusId == "SALES_REQUEST_APPROVED"
                                  && sr.SaleDate >= DateOnly.FromDateTime(request.FromDate) && sr.SaleDate <= DateOnly.FromDateTime(request.ToDate)
                            select new
                            {
                                sr.SalesRequestId,
                                ApartmentId        = sr.ProductId,
                                prod.ProductName,
                                prod.ProjectId,
                                prod.FloorNumber,
                                    prod.BuildingNumber,           // ← Added here
                                prod.ApartmentSpaceM2,
                                prod.GardenSpaceM2,
                                prod.ApartmentStatusId,
                                pt.Description,
                                pt.DescriptionArabic,
                                FromPartyName      = cust != null ? cust.Description : null,
                                EmployeeName       = emp != null ? emp.Description : null,
                                sr.FromPartyId,
                                sr.EmployeePartyId,
                                sr.StatusId,
                                sr.TotalPrice,
                                sr.AdvancePayment,
                                sr.AdvancePercent,
                                sr.MaintenancePercent,
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

            // 3. Final projection
            var records = await baseQuery.Select(x => new SalesRequestRecord
            {
                SalesRequestId             = x.SalesRequestId,
                ApartmentId                = x.ApartmentId,
                ApartmentName              = x.ProductName,
                ProductTypeDescription     = language == "ar" ? (x.DescriptionArabic ?? x.Description) : x.Description,
                FromPartyId                = x.FromPartyId,
                FromPartyName              = x.FromPartyName ?? "",
                EmployeePartyId            = x.EmployeePartyId,
                EmployeeName               = x.EmployeeName ?? "",
                    BuildingNumber             = x.BuildingNumber ?? "",        // ← Added here
                ProjectName                = SalesRequestProjectionHelpers.GetProjectName(x.ProjectId, projectNameLookup),
                FloorNumber                = SalesRequestProjectionHelpers.GetFloorName(x.FloorNumber, floorMap),
                ApartmentSpaceM2           = x.ApartmentSpaceM2 ?? 0m,
                GardenSpaceM2              = x.GardenSpaceM2,
                ApartmentStatusDescription = SalesRequestProjectionHelpers.GetApartmentStatusDescription(x.ApartmentStatusId, apartmentStatusLookup),
                MaintenanceDeposit         = x.MaintenanceDeposit,
                IsChequesDelivered         = x.IsChequesDelivered,
                StatusId                   = x.StatusId ?? "",
                StatusDescription          = SalesRequestProjectionHelpers.GetSalesRequestStatusDescription(x.StatusId, salesRequestStatusLookup),
                TotalPrice                 = x.TotalPrice,
                AdvancePayment             = x.AdvancePayment,
                AdvancePercent             = x.AdvancePercent,
                MaintenancePercent         = x.MaintenancePercent,
                SaleDate                   = x.SaleDate,
                Comments                   = x.Comments,
                CreatedStamp               = x.CreatedStamp,
                LastUpdatedStamp           = x.LastUpdatedStamp,
                ApartmentPricePerM2        = x.ApartmentPricePerM2,
                GardenPricePerM2           = x.GardenPricePerM2,
                Discount                   = x.Discount,
                NumberOfInstallments       = x.NumberOfInstallments,
                DateOfFirstInstallment     = x.DateOfFirstInstallment,
                MonthsBetweenInstallments  = x.MonthsBetweenInstallments
            }).ToListAsync(ct);

            return records;
        }
    }
}
