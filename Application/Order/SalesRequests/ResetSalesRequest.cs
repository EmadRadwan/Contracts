using Application.Accounting.Payments;
using Application.Projects;
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

            // An approved commission owns real payments — some already disbursed — and its snapshot
            // of the sale (price, collected amount) is what those payments were computed from.
            // Resetting here would either wipe payments the commission still points at or leave it
            // pointing at a sale that no longer exists, so make the user unwind the commission
            // first. Pending commissions have generated nothing yet and don't block.
            var blockingCommission = await _context.SalesCommissions
                .Where(c => c.SalesRequestId == salesRequestId
                            && (c.StatusId == "COMMISSION_APPROVED" || c.StatusId == "COMMISSION_PAID"))
                .Select(c => c.SalesCommissionId)
                .FirstOrDefaultAsync(ct);

            if (blockingCommission != null)
                // Arabic: this rule is hit by end users during normal work and the toast shows the
                // message verbatim, so it follows the UI language rather than the handler's other
                // developer-facing strings.
                return Result<CreateSalesRequest.SalesRequestResponseDto>.Failure(
                    $"لا يمكن إعادة تعيين طلب المبيعات: توجد عمولة مبيعات معتمدة ({blockingCommission}) مرتبطة بهذا الطلب. " +
                    "يجب إعادة تعيين تلك العمولة أو حذفها أولاً، ثم إعادة تعيين طلب المبيعات.");

            await using var transaction = await _context.Database.BeginTransactionAsync(ct);

            try
            {
                // 2. Clean up artifacts created during approval
                // REFACTOR: this used to delete payments and only the AcctgTrans carrying
                //           SALES_REQUEST_ID, which left the ledger rows hanging off the payments
                //           behind and made SaveChanges fail on ACCTTX_PAYMENT. Share the delete
                //           path's cleanup so the two can't drift apart again.

                // Customer payments (advance, installments, maintenance deposit) and everything
                // hanging off them. Commission payments are excluded deliberately: an approved
                // commission is refused above, so anything left with that type belongs to a record
                // this handler must not silently destroy.
                var payments = await _context.Payments
                    .Where(p => p.SalesRequestId == salesRequestId
                                && p.PaymentTypeId != CommissionPaymentCleanup.CommissionPaymentTypeId)
                    .ToListAsync(ct);

                await PaymentArtifactCleanup.PurgePaymentsAsync(_context, payments, ct);

                // Ledger transactions booked against the request itself (APARTMENT_SALE_*,
                // APARTMENT_MAINTENANCE_DEPOSIT) — these carry no PAYMENT_ID, so the purge above
                // does not reach them.
                var acctgTransIds = await _context.AcctgTrans
                    .Where(t => t.SalesRequestId == salesRequestId)
                    .Select(t => t.AcctgTransId)
                    .ToListAsync(ct);

                await PaymentArtifactCleanup.PurgeAcctgTransAsync(_context, acctgTransIds, ct);

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
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(ct);
                // Innermost provider message — an FK violation's detail sits there, and the UI
                // shows this text verbatim.
                return Result<CreateSalesRequest.SalesRequestResponseDto>.Failure(
                    $"Failed to reset sales request: {ex.GetBaseException().Message}");
            }

            // 5. Return updated DTO
            var response = await BuildResponseDto(sr, ct);
            return Result<CreateSalesRequest.SalesRequestResponseDto>.Success(response);
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