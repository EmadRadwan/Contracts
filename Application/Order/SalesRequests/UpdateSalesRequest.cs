using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Order.SalesRequests;

public class UpdateSalesRequest
{
    // -----------------------------------------------------------------
    // Command (re-uses the same DTO as Create)
    // -----------------------------------------------------------------
    public class Command : IRequest<Result<CreateSalesRequest.SalesRequestResponseDto>>
    {
        public SalesRequestDto? SalesRequestDto { get; set; }
    }

    // -----------------------------------------------------------------
    // Handler
    // -----------------------------------------------------------------
    public class Handler : IRequestHandler<Command, Result<CreateSalesRequest.SalesRequestResponseDto>>
    {
        private readonly DataContext _context;
        private const string ApartmentAvailableStatusId = "APARTMENT_AVAILABLE";
        private const string ApartmentReservedStatusId = "APARTMENT_RESERVED";


        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<CreateSalesRequest.SalesRequestResponseDto>> Handle(Command request,
            CancellationToken ct)
        {
            var dto = request.SalesRequestDto!;

            // -----------------------------------------------------------------
            // 1. Load existing entity (optimistic concurrency not needed now)
            // -----------------------------------------------------------------
            var sr = await _context.SalesRequests
                .Include(s => s.Product) // eager load current apartment
                .FirstOrDefaultAsync(x => x.SalesRequestId == dto.SalesRequestId, ct);

            if (sr == null)
                return Result<CreateSalesRequest.SalesRequestResponseDto>.Failure("Sales request not found");
            
            var oldProductId = sr.ProductId;
            var newProductId = dto.ProductId!;


            // -----------------------------------------------------------------
            // 2. Transaction
            // -----------------------------------------------------------------
            await using var transaction = await _context.Database.BeginTransactionAsync(ct);
            
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
                    return Result<CreateSalesRequest.SalesRequestResponseDto>.Failure("Selected apartment not found");

                // Validate availability: must be AVAILABLE or reserved by THIS request
                if (newApartment.ApartmentStatusId != ApartmentAvailableStatusId &&
                    newApartment.ReservedBySalesRequestId != sr.SalesRequestId)
                {
                    return Result<CreateSalesRequest.SalesRequestResponseDto>.Failure(
                        "Cannot update: the selected apartment is already reserved or sold.");
                }

                // Reserve the new apartment
                newApartment.ApartmentStatusId = ApartmentReservedStatusId;
                newApartment.ReservedBySalesRequestId = sr.SalesRequestId;
            }

            
            try
            {
                // -----------------------------------------------------------------
                // 3. Update scalar fields
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
                sr.LastUpdatedStamp = DateTime.UtcNow; // REFACTOR: keep audit trail

                var saved = await _context.SaveChangesAsync(ct) > 0;
                if (!saved)
                {
                    await transaction.RollbackAsync(ct);
                    return Result<CreateSalesRequest.SalesRequestResponseDto>.Failure("Failed to update sales request");
                }

                await transaction.CommitAsync(ct);

                // -----------------------------------------------------------------
                // 4. Re-load party & apartment data (same logic as Create)
                // -----------------------------------------------------------------
                var fromParty = await _context.Parties
                    .Where(p => p.PartyId == dto.FromPartyId)
                    .Select(p => new { p.PartyId, p.Description, Phone = string.Empty })
                    .FirstOrDefaultAsync(ct);
                
                var employee = await _context.Parties
                    .Where(p => p.PartyId == dto.EmployeePartyId)
                    .Select(p => new { p.PartyId, p.Description })
                    .FirstOrDefaultAsync(ct);

                var apartment = await CreateSalesRequest.Handler.GetApartmentLovProjection(_context, dto.ProductId!, ct)
                                ?? new CreateSalesRequest.ApartmentLovProjection
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
                    .Where(s => s.StatusId == sr.StatusId) // use the current status (unchanged)
                    .Select(s => s.Description ?? s.StatusId)
                    .FirstOrDefaultAsync(ct) ?? sr.StatusId;

                // -----------------------------------------------------------------
                // 5. Build flat response DTO (identical to Create)
                // -----------------------------------------------------------------
                var response = new CreateSalesRequest.SalesRequestResponseDto
                {
                    SalesRequestId = sr.SalesRequestId,

                    FromPartyId = fromParty?.PartyId ?? dto.FromPartyId!,
                    FromPartyName = fromParty?.Description ?? string.Empty,
                    FromPartyPhone = fromParty?.Phone ?? string.Empty,
                    EmployeePartyId = employee?.PartyId ?? dto.EmployeePartyId ?? string.Empty,
                    EmployeeName = employee?.Description ?? string.Empty,
                    ApartmentId = apartment.ApartmentId,
                    ApartmentName = apartment.ApartmentName,
                    ApartmentType = apartment.ApartmentType,
                    ProjectName = apartment.ProjectName,
                    FloorNumber = apartment.FloorNumber,
                    ApartmentSpaceM2 = apartment.ApartmentSpaceM2,
                    GardenSpaceM2 = apartment.GardenSpaceM2,
                    GardenPricePerM2 = apartment.GardenPricePerM2 ?? dto.GardenPricePerM2,
                    ApartmentPricePerM2 = apartment.ApartmentPricePerM2,
                    ApartmentStatusId = apartment.ApartmentStatusId,
                    ApartmentStatusDescription = apartment.ApartmentStatusDescription,
                    
                    ApartmentReservedBySalesRequestId = apartment.ReservedBySalesRequestId,


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
                };

                return Result<CreateSalesRequest.SalesRequestResponseDto>.Success(response);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(ct);
                return Result<CreateSalesRequest.SalesRequestResponseDto>.Failure(
                    $"Failed to update sales request: {ex.Message}");
            }
        }
    }
}