using Application.Core;
using Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Order.ReserveRequests;

public class EditReserveRequest
{
    // -----------------------------------------------------------------
    // Command
    // -----------------------------------------------------------------
    public class Command : IRequest<Result<CreateReserveRequest.ReserveRequestResponseDto>>
    {
        public CreateReserveRequest.ReserveRequestDto? ReserveRequestDto { get; set; }
    }

    // -----------------------------------------------------------------
    // Reuse the same DTOs from Create (no changes needed)
    // -----------------------------------------------------------------
    // ReserveRequestDto and ReserveRequestResponseDto are already defined in CreateReserveRequest.cs
    // We reuse them to keep consistency.

    // -----------------------------------------------------------------
    // Handler
    // -----------------------------------------------------------------
    public class Handler : IRequestHandler<Command, Result<CreateReserveRequest.ReserveRequestResponseDto>>
    {
        private readonly DataContext _context;
        private readonly IUtilityService _utilityService;
        private readonly IUserAccessor _userAccessor;

        private const string ApartmentReservedStatusId = "APARTMENT_RESERVED";

        public Handler(DataContext context, IUserAccessor userAccessor, IUtilityService utilityService)
        {
            _context = context;
            _userAccessor = userAccessor;
            _utilityService = utilityService;
        }

        public async Task<Result<CreateReserveRequest.ReserveRequestResponseDto>> Handle(Command request, CancellationToken ct)
        {
            var dto = request.ReserveRequestDto!;

            // REFACTOR: Validate user exists (same as create)
            // Purpose: Security – ensure request comes from authenticated user
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == _userAccessor.GetUsername(), ct);
            if (user == null)
                return Result<CreateReserveRequest.ReserveRequestResponseDto>.Failure("User not found");

            // REFACTOR: Find existing ReserveRequest with related data in one query
            // Purpose: Efficiency + avoid N+1; load apartment and parties early
            var existingRR = await _context.ReserveRequests
                .Include(rr => rr.Product)
                .FirstOrDefaultAsync(rr => rr.ReserveRequestId == dto.ReserveRequestId, ct);

            if (existingRR == null)
                return Result<CreateReserveRequest.ReserveRequestResponseDto>.Failure("Reserve request not found");

            // REFACTOR: Prevent changing apartment after creation
            // Purpose: Business rule – reservation is tied to one apartment
            if (existingRR.ProductId != dto.ProductId)
                return Result<CreateReserveRequest.ReserveRequestResponseDto>.Failure("Cannot change the reserved apartment");

            var now = DateTime.UtcNow;

            // Update scalar fields
            existingRR.FromPartyId = dto.FromPartyId;
            existingRR.EmployeePartyId = dto.EmployeePartyId;
            existingRR.ReserveDate = dto.ReserveDate;
            existingRR.ReserveAmount = dto.ReserveAmount;
            existingRR.PayMethod = dto.PayMethod;
            existingRR.ChequeStatus = dto.PayMethod == "CHEQUE" ? dto.ChequeStatus : null;
            existingRR.Comments = dto.Comments;
            existingRR.LastUpdatedStamp = now;

            try
            {
                var saved = await _context.SaveChangesAsync(ct) > 0;
                if (!saved)
                    return Result<CreateReserveRequest.ReserveRequestResponseDto>.Failure("Failed to update reserve request");
            }
            catch (Exception ex)
            {
                return Result<CreateReserveRequest.ReserveRequestResponseDto>.Failure($"Database error: {ex.Message}");
            }

            // -----------------------------------------------------------------
            // Load denormalized data for response (same logic as Create)
            // -----------------------------------------------------------------
            var customer = await _context.Parties
                .Where(p => p.PartyId == dto.FromPartyId)
                .Select(p => new { p.PartyId, p.Description })
                .FirstOrDefaultAsync(ct);

            var employee = dto.EmployeePartyId != null
                ? await _context.Parties
                    .Where(p => p.PartyId == dto.EmployeePartyId)
                    .Select(p => new { p.PartyId, p.Description })
                    .FirstOrDefaultAsync(ct)
                : null;

            // REFACTOR: Reuse the same apartment projection method (duplicated to avoid coupling)
            // Purpose: Identical display logic as Create and List views
            var apartmentLov = await GetApartmentLovProjection(dto.ProductId, ct)
                               ?? new ApartmentLovProjection();

            var statusDesc = await _context.StatusItems
                .Where(s => s.StatusId == existingRR.StatusId)
                .Select(s => s.Description ?? s.StatusId)
                .FirstOrDefaultAsync(ct) ?? existingRR.StatusId ?? "Unknown";

            var response = new CreateReserveRequest.ReserveRequestResponseDto
            {
                ReserveRequestId = existingRR.ReserveRequestId,
                FromPartyId = customer?.PartyId ?? dto.FromPartyId,
                FromPartyName = customer?.Description ?? string.Empty,
                EmployeePartyId = employee?.PartyId ?? string.Empty,
                EmployeeName = employee?.Description ?? string.Empty,
                ApartmentId = apartmentLov.ApartmentId,
                ApartmentName = apartmentLov.ApartmentName,
                ProjectName = apartmentLov.ProjectName,
                FloorNumber = apartmentLov.FloorNumber,
                ApartmentSpaceM2 = apartmentLov.ApartmentSpaceM2,
                GardenSpaceM2 = apartmentLov.GardenSpaceM2,
                ApartmentStatusDescription = apartmentLov.ApartmentStatusDescription,
                ReserveDate = dto.ReserveDate,
                ReserveAmount = dto.ReserveAmount,
                PayMethod = dto.PayMethod,
                ChequeStatus = dto.ChequeStatus,
                Comments = dto.Comments,
                StatusId = existingRR.StatusId ?? string.Empty,
                StatusDescription = statusDesc,
                LastUpdatedStamp = now
            };

            return Result<CreateReserveRequest.ReserveRequestResponseDto>.Success(response);
        }

        // -----------------------------------------------------------------
        // Identical to CreateReserveRequest – duplicated to keep features independent
        // -----------------------------------------------------------------
        private class ApartmentLovProjection
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

        private async Task<ApartmentLovProjection?> GetApartmentLovProjection(string productId, CancellationToken ct)
        {
            var projectLookup = await _context.WorkEfforts
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

            var raw = await _context.Products
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

            var projectName = raw.ProjectId != null && projectLookup.TryGetValue(raw.ProjectId, out var pn)
                ? pn : string.Empty;
            var statusDesc = raw.ApartmentStatusId != null && statusLookup.TryGetValue(raw.ApartmentStatusId, out var sd)
                ? sd : raw.ApartmentStatusId ?? string.Empty;
            var floorLabel = raw.FloorNumber != null && floorMap.TryGetValue(raw.FloorNumber, out var fl)
                ? fl : raw.FloorNumber ?? string.Empty;

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