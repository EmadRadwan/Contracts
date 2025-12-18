using Application.Core;
using Application.Interfaces;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Order.ReserveRequests;

public class CreateReserveRequest
{
    // -----------------------------------------------------------------
    // Command
    // -----------------------------------------------------------------
    public class Command : IRequest<Result<ReserveRequestResponseDto>>
    {
        public ReserveRequestDto? ReserveRequestDto { get; set; }
    }

    // -----------------------------------------------------------------
    // Request DTO
    // -----------------------------------------------------------------
    public class ReserveRequestDto
    {
        public string ReserveRequestId { get; set; } = string.Empty;
        public string ProductId { get; set; } = string.Empty;
        public string FromPartyId { get; set; } = string.Empty;
        public string? EmployeePartyId { get; set; }
        public DateTime ReserveDate { get; set; }
        public decimal ReserveAmount { get; set; }
        public string PayMethod { get; set; } = string.Empty;
        public string? ChequeStatus { get; set; }
        public string? Comments { get; set; }
    }

    // -----------------------------------------------------------------
    // Flat Response DTO
    // -----------------------------------------------------------------
    public class ReserveRequestResponseDto
    {
        public string ReserveRequestId { get; set; } = string.Empty;

        public string FromPartyId { get; set; } = string.Empty;
        public string FromPartyName { get; set; } = string.Empty;

        public string EmployeePartyId { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;

        public string ApartmentId { get; set; } = string.Empty;
        public string ApartmentName { get; set; } = string.Empty;
        public string ProjectName { get; set; } = string.Empty;
        public string FloorNumber { get; set; } = string.Empty;
        public decimal ApartmentSpaceM2 { get; set; }
        public decimal? GardenSpaceM2 { get; set; }
        public string ApartmentStatusDescription { get; set; } = string.Empty;

        public DateTime ReserveDate { get; set; }
        public decimal ReserveAmount { get; set; }
        public string PayMethod { get; set; } = string.Empty;
        public string? ChequeStatus { get; set; }
        public string? Comments { get; set; }

        public string StatusId { get; set; } = string.Empty;
        public string StatusDescription { get; set; } = string.Empty;
        public DateTime CreatedStamp { get; set; }
        public DateTime LastUpdatedStamp { get; set; }
    }

    // -----------------------------------------------------------------
    // Projection class used by GetApartmentLovProjection
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

    // -----------------------------------------------------------------
    // Handler
    // -----------------------------------------------------------------
    public class Handler : IRequestHandler<Command, Result<ReserveRequestResponseDto>>
    {
        private readonly DataContext _context;
        private readonly IUtilityService _utilityService;
        private readonly IUserAccessor _userAccessor;

        private const string ReserveRequestCreatedStatusId = "RESERVE_REQUEST_CREATED";
        private const string ApartmentReservedStatusId = "APARTMENT_RESERVED";

        public Handler(DataContext context, IUserAccessor userAccessor, IUtilityService utilityService)
        {
            _context = context;
            _userAccessor = userAccessor;
            _utilityService = utilityService;
        }

        public async Task<Result<ReserveRequestResponseDto>> Handle(Command request, CancellationToken ct)
        {
            var dto = request.ReserveRequestDto!;

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == _userAccessor.GetUsername(), ct);
            if (user == null)
                return Result<ReserveRequestResponseDto>.Failure("User not found");

            var reserveRequestId = await _utilityService.GetNextSequence("ReserveRequest");
            var now = DateTime.UtcNow;

            var apartment = await _context.Products
                .FirstOrDefaultAsync(p => p.ProductId == dto.ProductId && p.ProductTypeId == "APARTMENT", ct);

            if (apartment == null)
                return Result<ReserveRequestResponseDto>.Failure("Apartment not found");

            apartment.ApartmentStatusId = ApartmentReservedStatusId;
            apartment.ReservedBySalesRequestId = null;

            var rr = new ReserveRequest
            {
                ReserveRequestId = reserveRequestId,
                ProductId = dto.ProductId,
                FromPartyId = dto.FromPartyId,
                EmployeePartyId = dto.EmployeePartyId,
                ReserveDate = dto.ReserveDate,
                ReserveAmount = dto.ReserveAmount,
                PayMethod = dto.PayMethod,
                ChequeStatus = dto.PayMethod == "CHEQUE" ? dto.ChequeStatus : null,
                Comments = dto.Comments,
                //StatusId = ReserveRequestCreatedStatusId,
                CreatedStamp = now,
                LastUpdatedStamp = now
            };

            _context.ReserveRequests.Add(rr);

            try
            {
                var saved = await _context.SaveChangesAsync(ct) > 0;
                if (!saved)
                    return Result<ReserveRequestResponseDto>.Failure("Failed to create reserve request");
            }
            catch (Exception ex)
            {
                return Result<ReserveRequestResponseDto>.Failure($"Database error: {ex.Message}");
            }

            // -----------------------------------------------------------------
            // Load related data sequentially
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

            // # REFACTOR: Moved GetApartmentLovProjection inside this class to avoid cross-namespace reference.
            // Keeps the exact same logic as SalesRequest but eliminates "Cannot resolve symbol" error.
            var apartmentLov = await GetApartmentLovProjection(dto.ProductId, ct)
                               ?? new ApartmentLovProjection();

            var statusDesc = await _context.StatusItems
                .Where(s => s.StatusId == ReserveRequestCreatedStatusId)
                .Select(s => s.Description ?? s.StatusId)
                .FirstOrDefaultAsync(ct) ?? ReserveRequestCreatedStatusId;

            var response = new ReserveRequestResponseDto
            {
                ReserveRequestId = reserveRequestId,
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
                StatusId = ReserveRequestCreatedStatusId,
                StatusDescription = statusDesc,
                CreatedStamp = now,
                LastUpdatedStamp = now
            };

            return Result<ReserveRequestResponseDto>.Success(response);
        }

        // -----------------------------------------------------------------
        // Reusable apartment LOV projection (copied from SalesRequest to resolve symbol error)
        // -----------------------------------------------------------------
        // # REFACTOR: Duplicated method here to eliminate dependency on CreateSalesRequest class.
        // Ensures both features have identical apartment display logic without cross-feature coupling.
        private async Task<ApartmentLovProjection?> GetApartmentLovProjection(string productId, CancellationToken ct)
        {
            var projectLookup = await _context.WorkEfforts
                .Where(w => w.WorkEffortTypeId == "PROJECT")
                .GroupBy(w => w.WorkEffortId)
                .Select(g => new
                {
                    ProjectId = g.Key, ProjectName = g.OrderByDescending(x => x.WorkEffortId)
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
                ? pn
                : string.Empty;
            var statusDesc =
                raw.ApartmentStatusId != null && statusLookup.TryGetValue(raw.ApartmentStatusId, out var sd)
                    ? sd
                    : raw.ApartmentStatusId ?? string.Empty;
            var floorLabel = raw.FloorNumber != null && floorMap.TryGetValue(raw.FloorNumber, out var fl)
                ? fl
                : raw.FloorNumber ?? string.Empty;

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