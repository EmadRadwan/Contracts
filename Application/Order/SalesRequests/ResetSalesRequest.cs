using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Order.SalesRequests;

public class ResetSalesRequest
{
    public class Command : IRequest<Result<CreateSalesRequest.SalesRequestResponseDto>>
    {
        public string SalesRequestId { get; set; } = null!;
    }

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
            var salesRequestId = request.SalesRequestId;

            await using var transaction = await _context.Database.BeginTransactionAsync(ct);

            try
            {
                // 1. Load SalesRequest
                var sr = await _context.SalesRequests
                    .Include(s => s.Installments)
                    .Include(s => s.Product)
                    .FirstOrDefaultAsync(x => x.SalesRequestId == salesRequestId, ct);

                if (sr == null)
                    return Result<CreateSalesRequest.SalesRequestResponseDto>.Failure("Sales request not found");

                if (sr.StatusId != "SALES_REQUEST_APPROVED")
                    return Result<CreateSalesRequest.SalesRequestResponseDto>.Failure(
                        "Only approved sales requests can be reset.");

                // 2. Clean up artifacts created during approval

                // Delete Payments and related FinAccountTran
                var payments = await _context.Payments
                    .Where(p => p.SalesRequestId == sr.SalesRequestId)
                    .ToListAsync(ct);

                if (payments.Any())
                {
                    var paymentIds = payments.Select(p => p.PaymentId).ToList();

                    var finAccountTrans = await _context.FinAccountTrans
                        .Where(fat => paymentIds.Contains(fat.PaymentId))
                        .ToListAsync(ct);

                    // ── Critical step: break the cycle by nulling one direction ──
                    // Usually safest to null the FK pointing from the "dependent" side
                    // (try PaymentId first; if it fails → try nulling FinAccountTransId instead)
                    foreach (var tran in finAccountTrans)
                    {
                        tran.PaymentId = null;   // ← this removes the reference FinAccountTran → Payment
                    }

                    // Optional: also null the reverse if needed (rarely required)
                    // foreach (var p in payments)
                    // {
                    //     p.FinAccountTransId = null;
                    // }

                    _context.FinAccountTrans.RemoveRange(finAccountTrans);
                    _context.Payments.RemoveRange(payments);
                }

                // Delete AcctgTrans and entries
                var acctgTransList = await _context.AcctgTrans
                    .Include(t => t.AcctgTransEntries)
                    .Where(t => t.SalesRequestId == sr.SalesRequestId)
                    .ToListAsync(ct);

                if (acctgTransList.Any())
                {
                    foreach (var tran in acctgTransList)
                    {
                        _context.AcctgTransEntries.RemoveRange(tran.AcctgTransEntries);
                    }

                    _context.AcctgTrans.RemoveRange(acctgTransList);
                }

                // 3. Revert statuses
                sr.StatusId = "SALES_REQUEST_CREATED";
                sr.LastUpdatedStamp = DateTime.UtcNow;

                if (sr.Product != null)
                {
                    sr.Product.ApartmentStatusId = "APARTMENT_RESERVED";
                }

                // 4. Persist all changes in one transaction
                var saved = await _context.SaveChangesAsync(ct) > 0;
                if (!saved)
                {
                    await transaction.RollbackAsync(ct);
                    return Result<CreateSalesRequest.SalesRequestResponseDto>.Failure("Failed to reset sales request");
                }

                await transaction.CommitAsync(ct);

                // 5. Return updated DTO
                var response = await BuildResponseDto(sr, ct);
                return Result<CreateSalesRequest.SalesRequestResponseDto>.Success(response);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(ct);
                return Result<CreateSalesRequest.SalesRequestResponseDto>.Failure(
                    $"Failed to reset sales request: {ex.Message}");
            }
        }

        private async Task<CreateSalesRequest.SalesRequestResponseDto> BuildResponseDto(SalesRequest sr,
            CancellationToken ct)
        {
            var fromParty = await _context.Parties
                .Where(p => p.PartyId == sr.FromPartyId)
                .Select(p => new { p.PartyId, p.Description, Phone = string.Empty })
                .FirstOrDefaultAsync(ct);

            var employee = await _context.Parties
                .Where(p => p.PartyId == sr.EmployeePartyId)
                .Select(p => new { p.PartyId, p.Description })
                .FirstOrDefaultAsync(ct);

            var apartment = await CreateSalesRequest.Handler.GetApartmentLovProjection(_context, sr.ProductId!, ct)
                            ?? new CreateSalesRequest.ApartmentLovProjection { ApartmentId = sr.ProductId! };

            var statusDesc = await _context.StatusItems
                .Where(s => s.StatusId == sr.StatusId)
                .Select(s => s.Description ?? s.StatusId)
                .FirstOrDefaultAsync(ct) ?? "Created";

            return new CreateSalesRequest.SalesRequestResponseDto
            {
                SalesRequestId = sr.SalesRequestId,
                FromPartyId = fromParty?.PartyId ?? sr.FromPartyId!,
                FromPartyName = fromParty?.Description ?? string.Empty,
                FromPartyPhone = fromParty?.Phone ?? string.Empty,
                EmployeePartyId = employee?.PartyId ?? sr.EmployeePartyId ?? string.Empty,
                EmployeeName = employee?.Description ?? string.Empty,
                ApartmentId = apartment.ApartmentId,
                ApartmentName = apartment.ApartmentName,
                ProjectName = apartment.ProjectName,
                FloorNumber = apartment.FloorNumber,
                ApartmentSpaceM2 = apartment.ApartmentSpaceM2,
                GardenSpaceM2 = apartment.GardenSpaceM2,
                ApartmentPricePerM2 = apartment.ApartmentPricePerM2,
                GardenPricePerM2 = apartment.GardenPricePerM2 ?? sr.GardenPricePerM2,
                TotalPrice = sr.TotalPrice ?? 0m,
                Discount = sr.Discount,
                AdvancePayment = sr.AdvancePayment ?? 0m,
                NumberOfInstallments = (int)sr.NumberOfInstallments,
                DateOfFirstInstallment = sr.DateOfFirstInstallment,
                MonthsBetweenInstallments = (int)sr.MonthsBetweenInstallments,
                IsChequesDelivered = sr.IsChequesDelivered,
                MaintenanceDeposit = sr.MaintenanceDeposit,
                SaleDate = sr.SaleDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
                Comments = sr.Comments,
                StatusId = sr.StatusId,
                StatusDescription = statusDesc,
                CreatedStamp = (DateTime)sr.CreatedStamp,
                LastUpdatedStamp = (DateTime)sr.LastUpdatedStamp
            };
        }
    }
}