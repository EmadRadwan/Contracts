// REFACTOR: Add missing using statements
using Application.Core;
using Application.Interfaces;
using Domain;

namespace Application.Order.SalesRequests;

public class UpdateSalesRequest
{
    public class Command : IRequest<Result<CreateSalesRequest.SalesRequestResponseDto>>
    {
        public SalesRequestDto? SalesRequestDto { get; set; }
    }

    // REFACTOR: Add ApartmentReservedBySalesRequestId to response (critical for frontend validation)
    // Purpose: Frontend needs this to allow editing when apartment is reserved by THIS request
    // Why: Without it, validation blocks edits after creation (same bug as before)
    // Context: Must match Create handler
    public class SalesRequestResponseDto : CreateSalesRequest.SalesRequestResponseDto
    {
        public string? ApartmentReservedBySalesRequestId { get; set; }
    }

    public class Handler : IRequestHandler<Command, Result<SalesRequestResponseDto>>
    {
        private readonly DataContext _context;
        private readonly IUtilityService _utilityService; // may be needed later

        private const string ApartmentAvailableStatusId = "APARTMENT_AVAILABLE";
        private const string ApartmentReservedStatusId = "APARTMENT_RESERVED";

        public Handler(DataContext context, IUtilityService utilityService)
        {
            _context = context;
            _utilityService = utilityService;
        }

        public async Task<Result<SalesRequestResponseDto>> Handle(Command request, CancellationToken ct)
        {
            var dto = request.SalesRequestDto!;

            // REFACTOR: Load existing sales request with current apartment
            // Why: We need old ProductId to release reservation if apartment changes
            var sr = await _context.SalesRequests
                .Include(s => s.Product) // eager load current apartment
                .FirstOrDefaultAsync(x => x.SalesRequestId == dto.SalesRequestId, ct);

            if (sr == null)
                return Result<SalesRequestResponseDto>.Failure("Sales request not found");

            var oldProductId = sr.ProductId;
            var newProductId = dto.ProductId!;

            // REFACTOR: Remove explicit transaction – EF Core SaveChangesAsync() is atomic
            // Why: Simpler, safer, avoids manual rollback issues
            // Context: One SaveChanges = one transaction
            try
            {
                // -----------------------------------------------------------------
                // 1. Handle apartment change – release old, reserve new
                // -----------------------------------------------------------------
                if (oldProductId != newProductId)
                {
                    // Release old apartment (if it was reserved)
                    if (sr.Product != null && sr.Product.ApartmentStatusId == ApartmentReservedStatusId)
                    {
                        sr.Product.ApartmentStatusId = ApartmentAvailableStatusId;
                        sr.Product.ReservedBySalesRequestId = null;
                    }

                    // Load and validate new apartment
                    var newApartment = await _context.Products
                        .FirstOrDefaultAsync(p => p.ProductId == newProductId && p.ProductTypeId == "APARTMENT", ct);

                    if (newApartment == null)
                        return Result<SalesRequestResponseDto>.Failure("Selected apartment not found");

                    // Validate availability: must be AVAILABLE or reserved by THIS request
                    if (newApartment.ApartmentStatusId != ApartmentAvailableStatusId &&
                        newApartment.ReservedBySalesRequestId != sr.SalesRequestId)
                    {
                        return Result<SalesRequestResponseDto>.Failure(
                            "Cannot update: the selected apartment is already reserved or sold.");
                    }

                    // Reserve the new apartment
                    newApartment.ApartmentStatusId = ApartmentReservedStatusId;
                    newApartment.ReservedBySalesRequestId = sr.SalesRequestId;
                }

                // -----------------------------------------------------------------
                // 2. Update scalar fields
                // -----------------------------------------------------------------
                sr.ProductId = newProductId;
                sr.SaleDate = dto.SaleDate!.Value;
                sr.FromPartyId = dto.FromPartyId!;
                sr.EmployeePartyId = dto.EmployeePartyId;
                sr.ApartmentPricePerM2 = dto.ApartmentPricePerM2;
                sr.GardenPricePerM2 = dto.GardenPricePerM2;
                sr.Discount = dto.Discount;
                sr.TotalPrice = dto.TotalPrice;
                sr.AdvancePayment = dto.AdvancePayment;
                sr.NumberOfInstallments = dto.NumberOfInstallments;
                sr.DateOfFirstInstallment = dto.DateOfFirstInstallment;
                sr.MonthsBetweenInstallments = dto.MonthsBetweenInstallments;
                sr.MaintenanceDeposit = dto.MaintenanceDeposit;
                sr.Comments = dto.Comments;
                sr.LastUpdatedStamp = DateTime.UtcNow;

                // -----------------------------------------------------------------
                // 3. Persist changes
                // -----------------------------------------------------------------
                var saved = await _context.SaveChangesAsync(ct) > 0;
                if (!saved)
                    return Result<SalesRequestResponseDto>.Failure("Failed to update sales request");

                // -----------------------------------------------------------------
                // 4. Load fresh display data (same as Create)
                // -----------------------------------------------------------------
                var fromParty = await _context.Parties
                    .Where(p => p.PartyId == dto.FromPartyId)
                    .Select(p => new { p.PartyId, p.Description, Phone = string.Empty })
                    .FirstOrDefaultAsync(ct);

                var employee = await _context.Parties
                    .Where(p => p.PartyId == dto.EmployeePartyId)
                    .Select(p => new { p.PartyId, p.Description })
                    .FirstOrDefaultAsync(ct);

                // REFACTOR: Use same projection method – but make it instance method or shared
                var apartmentLov = await CreateSalesRequest.Handler.GetApartmentLovProjection(_context, newProductId, ct)
                    ?? new CreateSalesRequest.ApartmentLovProjection { /* fallback */ };

                var statusDesc = await _context.StatusItems
                    .Where(s => s.StatusId == sr.StatusId)
                    .Select(s => s.Description ?? s.StatusId)
                    .FirstOrDefaultAsync(ct) ?? sr.StatusId;

                // -----------------------------------------------------------------
                // 5. Build response – now includes reservation info
                // -----------------------------------------------------------------
                var response = new SalesRequestResponseDto
                {
                    SalesRequestId = sr.SalesRequestId,
                    FromPartyId = fromParty?.PartyId ?? dto.FromPartyId!,
                    FromPartyName = fromParty?.Description ?? string.Empty,
                    FromPartyPhone = fromParty?.Phone ?? string.Empty,
                    EmployeePartyId = employee?.PartyId ?? dto.EmployeePartyId ?? string.Empty,
                    EmployeeName = employee?.Description ?? string.Empty,

                    ApartmentId = apartmentLov.ApartmentId,
                    ApartmentName = apartmentLov.ApartmentName,
                    ApartmentType = apartmentLov.ApartmentType,
                    ProjectName = apartmentLov.ProjectName,
                    FloorNumber = apartmentLov.FloorNumber,
                    ApartmentSpaceM2 = apartmentLov.ApartmentSpaceM2,
                    GardenSpaceM2 = apartmentLov.GardenSpaceM2,
                    GardenPricePerM2 = apartmentLov.GardenPricePerM2 ?? dto.GardenPricePerM2,
                    ApartmentPricePerM2 = apartmentLov.ApartmentPricePerM2,
                    ApartmentStatusId = apartmentLov.ApartmentStatusId,
                    ApartmentStatusDescription = apartmentLov.ApartmentStatusDescription,

                    // REFACTOR: Critical – return reservation owner
                    ApartmentReservedBySalesRequestId = apartmentLov.ReservedBySalesRequestId,

                    TotalPrice = (decimal)dto.TotalPrice,
                    Discount = dto.Discount,
                    AdvancePayment = (decimal)dto.AdvancePayment,
                    NumberOfInstallments = (int)dto.NumberOfInstallments,
                    DateOfFirstInstallment = dto.DateOfFirstInstallment,
                    MonthsBetweenInstallments = (int)dto.MonthsBetweenInstallments,
                    MaintenanceDeposit = dto.MaintenanceDeposit,
                    SaleDate = dto.SaleDate!.Value,
                    Comments = dto.Comments,
                    StatusId = sr.StatusId,
                    StatusDescription = statusDesc,
                    CreatedStamp = sr.CreatedStamp,
                    LastUpdatedStamp = sr.LastUpdatedStamp
                };

                return Result<SalesRequestResponseDto>.Success(response);
            }
            catch (Exception ex)
            {
                // REFACTOR: No manual rollback needed – exception aborts SaveChanges
                return Result<SalesRequestResponseDto>.Failure($"Failed to update sales request: {ex.Message}");
            }
        }
    }
}