using Application.Core;
using Application.Interfaces;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Order.SalesRequests;

public class CreateSalesRequest
{
    // -----------------------------------------------------------------
    // Command
    // -----------------------------------------------------------------
    public class Command : IRequest<Result<SalesRequestResponseDto>>
    {
        public SalesRequestDto? SalesRequestDto { get; set; }
    }

    // -----------------------------------------------------------------
    // Flat Response DTO – all scalar values, no nested objects
    // -----------------------------------------------------------------
    public class SalesRequestResponseDto
    {
        public string SalesRequestId { get; set; } = string.Empty;

        // FromParty (flattened)
        public string FromPartyId { get; set; } = string.Empty;
        public string FromPartyName { get; set; } = string.Empty;
        public string FromPartyPhone { get; set; } = string.Empty;
        public string EmployeePartyId { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;

        // Apartment (flattened)
        public string ApartmentId { get; set; } = string.Empty;
        public string ApartmentName { get; set; } = string.Empty;
        public string ApartmentType { get; set; } = string.Empty;
        public string ProjectName { get; set; } = string.Empty;
        public string FloorNumber { get; set; } = string.Empty;
        public decimal ApartmentSpaceM2 { get; set; }
        public decimal? GardenSpaceM2 { get; set; }
        public decimal? GardenPricePerM2 { get; set; }
        public decimal ApartmentPricePerM2 { get; set; }
        public decimal? MaintenanceDeposit { get; set; }
        public bool? IsChequesDelivered { get; set; }


        public string ApartmentStatusId { get; set; } = string.Empty;
        public string ApartmentStatusDescription { get; set; } = string.Empty;
        public string? ApartmentReservedBySalesRequestId { get; set; }

        // Pricing & Payment
        public decimal TotalPrice { get; set; }
        public decimal? Discount { get; set; }
        public decimal AdvancePayment { get; set; }
        public int NumberOfInstallments { get; set; }
        public DateTime? DateOfFirstInstallment { get; set; }
        public int MonthsBetweenInstallments { get; set; }

        // Metadata
        public DateTime SaleDate { get; set; }
        public string? Comments { get; set; }
        public string StatusId { get; set; } = string.Empty;
        public string StatusDescription { get; set; } = string.Empty;

        public DateTime CreatedStamp { get; set; }
        public DateTime LastUpdatedStamp { get; set; }
    }

    // -----------------------------------------------------------------
    // Strongly-typed projection for apartment LOV
    // -----------------------------------------------------------------
    public class ApartmentLovProjection
    {
        public string ApartmentId { get; set; } = string.Empty;
        public string ApartmentName { get; set; } = string.Empty;
        public string ApartmentType { get; set; } = string.Empty;
        public string ProjectName { get; set; } = string.Empty;
        public string FloorNumber { get; set; } = string.Empty;
        public decimal ApartmentSpaceM2 { get; set; }
        public decimal? GardenSpaceM2 { get; set; }
        public decimal? GardenPricePerM2 { get; set; }
        public decimal ApartmentPricePerM2 { get; set; }
        public string ApartmentStatusId { get; set; } = string.Empty;
        public string ApartmentStatusDescription { get; set; } = string.Empty;
        public string ReservedBySalesRequestId { get; set; } = string.Empty;
    }

    // -----------------------------------------------------------------
    // Handler
    // -----------------------------------------------------------------
    public class Handler : IRequestHandler<Command, Result<SalesRequestResponseDto>>
    {
        private readonly DataContext _context;
        private readonly IUtilityService _utilityService;
        private readonly IUserAccessor _userAccessor;

        private const string SalesRequestCreatedStatusId = "SALES_REQUEST_CREATED";
        private const string ApartmentReservedStatusId = "APARTMENT_RESERVED"; // <-- NEW


        public Handler(DataContext context, IUserAccessor userAccessor, IUtilityService utilityService)
        {
            _context = context;
            _userAccessor = userAccessor;
            _utilityService = utilityService;
        }

        public async Task<Result<SalesRequestResponseDto>> Handle(Command request, CancellationToken ct)
        {
            var dto = request.SalesRequestDto!;

            // -----------------------------------------------------------------
            // 1. Validate current user
            // -----------------------------------------------------------------
            // REFACTOR: Early-exit with meaningful message; no need to continue if user missing.
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == _userAccessor.GetUsername(), ct);

            if (user == null)
                return Result<SalesRequestResponseDto>.Failure("User not found");

            // -----------------------------------------------------------------
            // 2. Generate next SalesRequestId (sequence) – using existing utility
            // -----------------------------------------------------------------
            // REFACTOR: Kept exactly as requested – uses _utilityService.GetNextSequence("SalesRequest")
            //          This method must handle its own transaction/sequence logic safely.
            var salesRequestId = await _utilityService.GetNextSequence("SalesRequest");
            var now = DateTime.UtcNow;

            // -----------------------------------------------------------------
            // 3. Build domain entity
            // -----------------------------------------------------------------
            var sr = new SalesRequest
            {
                SalesRequestId = salesRequestId,
                ProductId = dto.ProductId!,
                SaleDate = dto.SaleDate!.Value,
                FromPartyId = dto.FromPartyId!,
                EmployeePartyId = dto.EmployeePartyId,
                ApartmentPricePerM2 = dto.ApartmentPricePerM2,
                GardenPricePerM2 = dto.GardenPricePerM2,
                Discount = dto.Discount,
                TotalPrice = dto.TotalPrice,
                IsChequesDelivered = dto.IsChequesDelivered,
                AdvancePayment = dto.AdvancePayment,
                NumberOfInstallments = dto.NumberOfInstallments,
                DateOfFirstInstallment = dto.DateOfFirstInstallment,
                MonthsBetweenInstallments = dto.MonthsBetweenInstallments,
                MaintenanceDeposit = dto.MaintenanceDeposit,
                StatusId = SalesRequestCreatedStatusId,
                Comments = dto.Comments,
                CreatedStamp = now,
                LastUpdatedStamp = now
            };

            _context.SalesRequests.Add(sr);
            
            if (dto.CustomInstallments != null && dto.CustomInstallments.Any())
            {
                var sorted = dto.CustomInstallments
                    .OrderBy(i => i.DueDate)
                    .ThenBy(i => i.InstallmentNumber)
                    .ToList();

                for (int i = 0; i < sorted.Count; i++)
                {
                    sr.Installments.Add(new SalesRequestInstallment
                    {
                        SalesRequestId = salesRequestId,
                        InstallmentNumber = i + 1,
                        DueDate = sorted[i].DueDate,
                        Amount = sorted[i].Amount,
                        IsAdvance = sorted[i].IsAdvance  // ← persist the flag
                    });
                }
            }

            var apartment = await _context.Products
                .FirstOrDefaultAsync(p => p.ProductId == dto.ProductId! && p.ProductTypeId == "APARTMENT", ct);

            if (apartment == null)
                return Result<SalesRequestResponseDto>.Failure("Apartment not found");

            // REFACTOR: Change status to APARTMENT_RESERVED.
            //           Improves domain consistency: apartment becomes reserved the moment the request is created.
            apartment.ApartmentStatusId = ApartmentReservedStatusId;
            apartment.ReservedBySalesRequestId = salesRequestId;


            // -----------------------------------------------------------------
            // 4. Persist everything in ONE transaction
            // -----------------------------------------------------------------
            // REFACTOR: Removed explicit transaction – EF Core uses implicit transaction per SaveChangesAsync().
            //          Simpler, safer, and avoids "already committed/rolled back" errors.
            try
            {
                var saved = await _context.SaveChangesAsync(ct) > 0;
                if (!saved)
                    return Result<SalesRequestResponseDto>.Failure("Failed to persist sales request");
            }
            catch (Exception ex)
            {
                // REFACTOR: No manual rollback needed – EF automatically rolls back on exception.
                return Result<SalesRequestResponseDto>.Failure($"Database error: {ex.Message}");
            }

            // -----------------------------------------------------------------
            // 5. Load auxiliary data **sequentially** – NO Task.WhenAll, NO parallel EF calls
            // -----------------------------------------------------------------
            // REFACTOR: All EF queries are awaited one-by-one to prevent:
            //          "A second operation was started on this context instance before a previous operation completed."
            var fromParty = await _context.Parties
                .Where(p => p.PartyId == dto.FromPartyId)
                .Select(p => new { p.PartyId, p.Description, Phone = string.Empty })
                .FirstOrDefaultAsync(ct);
            
            var employee = await _context.Parties
                .Where(p => p.PartyId == dto.EmployeePartyId)
                .Select(p => new { p.PartyId, p.Description })
                .FirstOrDefaultAsync(ct);

            // REFACTOR: GetApartmentLovProjection now uses the same context safely (sequential awaits inside)
            var apartmentLov = await GetApartmentLovProjection(_context, dto.ProductId!, ct)
                               ?? new ApartmentLovProjection
                               {
                                   ApartmentId = dto.ProductId!,
                                   ApartmentName = string.Empty,
                                   ApartmentType = "APARTMENT",
                                   ProjectName = string.Empty,
                                   FloorNumber = string.Empty,
                                   ApartmentSpaceM2 = 0m,
                                   GardenSpaceM2 = null,
                                   GardenPricePerM2 = dto.GardenPricePerM2,
                                   ApartmentPricePerM2 = (decimal)dto.ApartmentPricePerM2,
                                   ApartmentStatusId = string.Empty,
                                   ApartmentStatusDescription = string.Empty
                               };

            var statusDesc = await _context.StatusItems
                .Where(s => s.StatusId == SalesRequestCreatedStatusId)
                .Select(s => s.Description ?? s.StatusId)
                .FirstOrDefaultAsync(ct) ?? SalesRequestCreatedStatusId;

            // -----------------------------------------------------------------
            // 6. Build flat response DTO
            // -----------------------------------------------------------------
            // REFACTOR: All values are safely non-null; removed unnecessary casts.
            var response = new SalesRequestResponseDto
            {
                SalesRequestId = salesRequestId,

                // FromParty
                FromPartyId = fromParty?.PartyId ?? dto.FromPartyId!,
                FromPartyName = fromParty?.Description ?? string.Empty,
                FromPartyPhone = fromParty?.Phone ?? string.Empty,
                EmployeePartyId = employee?.PartyId ?? dto.EmployeePartyId ?? string.Empty,
                EmployeeName = employee?.Description ?? string.Empty,

                // Apartment (consistent with LOV)
                ApartmentId = apartmentLov.ApartmentId,
                ApartmentName = apartmentLov.ApartmentName,
                ApartmentType = apartmentLov.ApartmentType,
                ProjectName = apartmentLov.ProjectName,
                FloorNumber = apartmentLov.FloorNumber,
                ApartmentSpaceM2 = apartmentLov.ApartmentSpaceM2,
                GardenSpaceM2 = apartmentLov.GardenSpaceM2,
                ApartmentStatusId = apartmentLov.ApartmentStatusId,
                ApartmentStatusDescription = apartmentLov.ApartmentStatusDescription,

                ApartmentPricePerM2 = (decimal)dto.ApartmentPricePerM2,  // ← was apartmentLov.ApartmentPricePerM2
                GardenPricePerM2 = dto.GardenPricePerM2,        // ← was apartmentLov.GardenPricePerM2 ?? dto...

                // Pricing & Payment
                TotalPrice = (decimal)dto.TotalPrice,
                Discount = dto.Discount,
                AdvancePayment = (decimal)dto.AdvancePayment,
                NumberOfInstallments = (int)dto.NumberOfInstallments,
                DateOfFirstInstallment = dto.DateOfFirstInstallment,
                MonthsBetweenInstallments = (int)dto.MonthsBetweenInstallments,
                MaintenanceDeposit = dto.MaintenanceDeposit,
                ApartmentReservedBySalesRequestId = apartmentLov.ReservedBySalesRequestId,


                // Metadata
                SaleDate = dto.SaleDate!.Value,
                Comments = dto.Comments,
                StatusId = SalesRequestCreatedStatusId,
                IsChequesDelivered = dto.IsChequesDelivered,
                StatusDescription = statusDesc,
                CreatedStamp = now,
                LastUpdatedStamp = now
            };

            return Result<SalesRequestResponseDto>.Success(response);
        }

        // -----------------------------------------------------------------
        // Reusable apartment LOV projection (same logic as GetSimpleApartmentsLov)
        // -----------------------------------------------------------------
        // REFACTOR: All internal EF queries are awaited **sequentially** to avoid DbContext concurrency errors.
        //          Dictionaries are loaded once per call and used in-memory.
        //          No Task.WhenAll or parallel execution.
        public static async Task<ApartmentLovProjection?> GetApartmentLovProjection(
            DataContext ctx, string productId, CancellationToken ct)
        {
            // 1. Load project lookup dictionary
            var projectLookup = await ctx.WorkEfforts
                .Where(w => w.WorkEffortTypeId == "PROJECT")
                .GroupBy(w => w.WorkEffortId)
                .Select(g => new
                {
                    ProjectId = g.Key,
                    ProjectName = g.OrderByDescending(x => x.WorkEffortId)
                        .Select(x => x.ProjectName)
                        .FirstOrDefault()
                })
                .ToDictionaryAsync(x => x.ProjectId, x => x.ProjectName ?? string.Empty, ct);

            // 2. Load status lookup dictionary
            var statusLookup = await ctx.StatusItems
                .Where(s => s.StatusTypeId == "APARTMENT_STATUS")
                .ToDictionaryAsync(s => s.StatusId, s => s.Description ?? s.StatusId, ct);

            // 3. Static floor map (no DB)
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

            // 4. Pull raw product data
            var raw = await ctx.Products
                .Where(p => p.ProductId == productId && p.ProductTypeId == "APARTMENT")
                .Select(p => new
                {
                    p.ProductId,
                    p.ProductName,
                    p.ProductTypeId,
                    p.ProjectId,
                    p.FloorNumber,
                    p.ApartmentSpaceM2,
                    p.GardenSpaceM2,
                    p.GardenPricePerM2,
                    p.ApartmentPricePerM2,
                    p.ApartmentStatusId,
                    p.ReservedBySalesRequestId
                })
                .FirstOrDefaultAsync(ct);

            if (raw == null) return null;

            // 5. Apply lookups in-memory (after materialization)
            var projectName = raw.ProjectId != null && projectLookup.TryGetValue(raw.ProjectId, out var pn)
                ? pn
                : string.Empty;

            var statusDesc = raw.ApartmentStatusId != null &&
                             statusLookup.TryGetValue(raw.ApartmentStatusId, out var sd)
                ? sd
                : raw.ApartmentStatusId ?? string.Empty;

            var floorLabel = raw.FloorNumber != null && floorMap.TryGetValue(raw.FloorNumber, out var fl)
                ? fl
                : raw.FloorNumber ?? string.Empty;

            // 6. Return final projection
            return new ApartmentLovProjection
            {
                ApartmentId = raw.ProductId,
                ApartmentName = raw.ProductName ?? string.Empty,
                ApartmentType = raw.ProductTypeId ?? string.Empty,
                ProjectName = projectName,
                FloorNumber = floorLabel,
                ApartmentSpaceM2 = raw.ApartmentSpaceM2 ?? 0m,
                GardenSpaceM2 = raw.GardenSpaceM2,
                GardenPricePerM2 = raw.GardenPricePerM2,
                ApartmentPricePerM2 = raw.ApartmentPricePerM2 ?? 0m,
                ApartmentStatusId = raw.ApartmentStatusId ?? string.Empty,
                ApartmentStatusDescription = statusDesc,
                ReservedBySalesRequestId = raw.ReservedBySalesRequestId
            };
        }
    }
}