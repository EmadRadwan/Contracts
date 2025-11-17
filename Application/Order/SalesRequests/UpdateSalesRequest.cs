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
                .FirstOrDefaultAsync(x => x.SalesRequestId == dto.SalesRequestId, ct);

            if (sr == null)
                return Result<CreateSalesRequest.SalesRequestResponseDto>.Failure("Sales request not found");

            // -----------------------------------------------------------------
            // 2. Transaction
            // -----------------------------------------------------------------
            await using var transaction = await _context.Database.BeginTransactionAsync(ct);
            try
            {
                // -----------------------------------------------------------------
                // 3. Update scalar fields
                // -----------------------------------------------------------------
                sr.ProductId = dto.ProductId!;
                sr.SaleDate = dto.SaleDate!.Value;
                sr.FromPartyId = dto.FromPartyId!;
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