using Microsoft.EntityFrameworkCore;
using MediatR;
using Persistence;

namespace Application.Order.SalesRequests;

// Variant of ListSalesRequestsByDateRange that also lists APARTMENT products with no
// SalesRequest at all and not already marked SOLD, so the report can show current
// available/reserved inventory alongside units actually sold in the period. The
// "available" rows are not date-filtered - they reflect current inventory state, not
// a dated event - and are tagged IsSold = false so the report can format them distinctly.
public class ListSalesRequestsAndAvailableApartmentsByDateRange
{
    public class Query : IRequest<List<SalesRequestOrApartmentRecord>>
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string Language { get; set; } = "en";
    }

    public class Handler : IRequestHandler<Query, List<SalesRequestOrApartmentRecord>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<List<SalesRequestOrApartmentRecord>> Handle(Query request, CancellationToken ct)
        {
            var language = request.Language;

            // 1. Load lookup dictionaries (shared with ListSalesRequestsByDateRange)
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
                    elementSelector: s => language == "ar" ? (s.DescriptionArabic ?? s.Description) : s.Description,
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

            var notSoldLabel = language == "ar" ? "غير مباع" : "Not Sold";

            // 2. Sold rows - same shape/filter as ListSalesRequestsByDateRange
            var soldQuery = from sr in _context.SalesRequests
                            join prod in _context.Products on sr.ProductId equals prod.ProductId
                            join pt in _context.ProductTypes on prod.ProductTypeId equals pt.ProductTypeId
                            join customer in _context.Parties on sr.FromPartyId equals customer.PartyId into custGroup
                                from cust in custGroup.DefaultIfEmpty()
                            join employee in _context.Parties on sr.EmployeePartyId equals employee.PartyId into empGroup
                                from emp in empGroup.DefaultIfEmpty()
                            where sr.StatusId == "SALES_REQUEST_APPROVED"
                                  && sr.CreatedStamp >= request.FromDate && sr.CreatedStamp <= request.ToDate
                            select new SalesRequestOrApartmentRecord
                            {
                                IsSold                     = true,
                                SalesRequestId             = sr.SalesRequestId,
                                ApartmentId                = sr.ProductId,
                                ApartmentName              = prod.ProductName ?? "",
                                ProductTypeDescription     = language == "ar" ? (pt.DescriptionArabic ?? pt.Description) : pt.Description,
                                FromPartyId                = sr.FromPartyId,
                                FromPartyName              = cust != null ? cust.Description ?? "" : "",
                                EmployeePartyId            = sr.EmployeePartyId,
                                EmployeeName               = emp != null ? emp.Description ?? "" : "",
                                BuildingNumber             = prod.BuildingNumber ?? "",
                                ProjectName                = SalesRequestProjectionHelpers.GetProjectName(prod.ProjectId, projectNameLookup),
                                FloorNumber                = SalesRequestProjectionHelpers.GetFloorName(prod.FloorNumber, floorMap),
                                ApartmentSpaceM2           = prod.ApartmentSpaceM2 ?? 0m,
                                GardenSpaceM2              = prod.GardenSpaceM2,
                                ApartmentStatusDescription = SalesRequestProjectionHelpers.GetApartmentStatusDescription(prod.ApartmentStatusId, apartmentStatusLookup),
                                MaintenanceDeposit         = sr.MaintenanceDeposit,
                                IsChequesDelivered         = sr.IsChequesDelivered,
                                StatusId                   = sr.StatusId ?? "",
                                StatusDescription          = SalesRequestProjectionHelpers.GetSalesRequestStatusDescription(sr.StatusId, salesRequestStatusLookup),
                                TotalPrice                 = sr.TotalPrice,
                                AdvancePayment             = sr.AdvancePayment,
                                AdvancePercent             = sr.AdvancePercent,
                                MaintenancePercent         = sr.MaintenancePercent,
                                SaleDate                   = sr.SaleDate,
                                Comments                   = sr.Comments,
                                CreatedStamp               = sr.CreatedStamp,
                                LastUpdatedStamp           = sr.LastUpdatedStamp,
                                ApartmentPricePerM2        = sr.ApartmentPricePerM2,
                                GardenPricePerM2           = sr.GardenPricePerM2,
                                Discount                   = sr.Discount,
                                NumberOfInstallments       = sr.NumberOfInstallments,
                                DateOfFirstInstallment     = sr.DateOfFirstInstallment,
                                MonthsBetweenInstallments  = sr.MonthsBetweenInstallments
                            };

            var soldRecords = await soldQuery.ToListAsync(ct);

            // 3. Available/reserved rows - APARTMENT products with no SalesRequest at all AND
            // not already marked SOLD on the product itself. Both checks matter: a product can
            // be ApartmentStatusId=SOLD with zero SalesRequest rows (e.g. imported as sold, or a
            // duplicate of another product that carries the real SalesRequest) - without the
            // status check such a row would wrongly show "not sold" next to its own "Sold" status.
            // Not date-filtered: this represents current inventory, not a dated event.
            var availableQuery = from prod in _context.Products
                                 join pt in _context.ProductTypes on prod.ProductTypeId equals pt.ProductTypeId
                                 where prod.ProductTypeId == "APARTMENT"
                                       && !prod.SalesRequests.Any()
                                       && prod.ApartmentStatusId != "APARTMENT_SOLD"
                                 select new SalesRequestOrApartmentRecord
                                 {
                                     IsSold                     = false,
                                     SalesRequestId             = "",
                                     ApartmentId                = prod.ProductId,
                                     ApartmentName              = prod.ProductName ?? "",
                                     ProductTypeDescription     = language == "ar" ? (pt.DescriptionArabic ?? pt.Description) : pt.Description,
                                     FromPartyId                = "",
                                     FromPartyName              = "",
                                     EmployeePartyId            = "",
                                     EmployeeName               = "",
                                     BuildingNumber             = prod.BuildingNumber ?? "",
                                     ProjectName                = SalesRequestProjectionHelpers.GetProjectName(prod.ProjectId, projectNameLookup),
                                     FloorNumber                = SalesRequestProjectionHelpers.GetFloorName(prod.FloorNumber, floorMap),
                                     ApartmentSpaceM2           = prod.ApartmentSpaceM2 ?? 0m,
                                     GardenSpaceM2              = prod.GardenSpaceM2,
                                     ApartmentStatusDescription = SalesRequestProjectionHelpers.GetApartmentStatusDescription(prod.ApartmentStatusId, apartmentStatusLookup),
                                     MaintenanceDeposit         = null,
                                     IsChequesDelivered         = null,
                                     StatusId                   = "",
                                     StatusDescription          = notSoldLabel,
                                     TotalPrice                 = null,
                                     AdvancePayment             = null,
                                     AdvancePercent             = null,
                                     MaintenancePercent         = null,
                                     SaleDate                   = null,
                                     Comments                   = prod.Comments,
                                     CreatedStamp               = prod.CreatedStamp,
                                     LastUpdatedStamp           = prod.LastUpdatedStamp,
                                     ApartmentPricePerM2        = prod.ApartmentPricePerM2,
                                     GardenPricePerM2           = prod.GardenPricePerM2,
                                     Discount                   = null,
                                     NumberOfInstallments       = null,
                                     DateOfFirstInstallment     = null,
                                     MonthsBetweenInstallments  = null
                                 };

            var availableRecords = await availableQuery.ToListAsync(ct);

            return soldRecords
                .Concat(availableRecords)
                .OrderBy(x => x.ProjectName)
                .ThenBy(x => x.BuildingNumber)
                .ThenBy(x => x.FloorNumber)
                .ThenBy(x => x.ApartmentName)
                .ToList();
        }
    }
}
