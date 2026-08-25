using Application.Accounting.Payments;
using Application.Projects;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Order.SalesRequests;

public class DeleteSalesRequest
{
    // -----------------------------------------------------------------
    // Command – returns Unit because nothing is returned on success
    // -----------------------------------------------------------------
    public class Command : IRequest<Result<Unit>>
    {
        public string SalesRequestId { get; set; } = null!;
    }

    // -----------------------------------------------------------------
    // Handler
    // -----------------------------------------------------------------
    public class Handler : IRequestHandler<Command, Result<Unit>>
    {
        private readonly DataContext _context;

        private const string ApartmentAvailableStatusId = "APARTMENT_AVAILABLE"; // adjust if needed

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<Unit>> Handle(Command request, CancellationToken ct)
        {
            var salesRequestId = request.SalesRequestId;

            await using var transaction = await _context.Database.BeginTransactionAsync(ct);

            try
            {
                // 1. Load SalesRequest
                var sr = await _context.SalesRequests
                    .Include(s => s.Installments) // ← NEW: Load custom installments
                    .FirstOrDefaultAsync(x => x.SalesRequestId == salesRequestId, ct);

                if (sr == null)
                    return Result<Unit>.Failure("Sales request not found");

                // Optional: prevent deletion if already approved and payments exist, etc.
                // You can add more business rules here if needed.

                // 2. Load related apartment to clear reservation
                var apartment = await _context.Products
                    .FirstOrDefaultAsync(p => p.ProductId == sr.ProductId && p.ProductTypeId == "APARTMENT", ct);

                if (apartment == null)
                    return Result<Unit>.Failure("Associated apartment not found");

                // -----------------------------------------------------------------
                // 3. Clean up related artifacts
                // -----------------------------------------------------------------
                // REFACTOR: the cleanup used to run only for SALES_REQUEST_APPROVED. Artifacts can
                //           outlive that status (a commission approved against the request, a
                //           half-finished reset), and the FKs below are all NO ACTION, so anything
                //           left behind rejects the whole delete. Purge unconditionally instead —
                //           the queries are no-ops when there is nothing to remove.

                // 3a. Commission first. SALES_COMMISSION.FK_SALES_COMM_SR is Restrict, so an
                //     approved commission blocks the delete outright, and the ledger rows its
                //     payments produced carry no SALES_REQUEST_ID — only the commission cleanup
                //     knows how to find them (via PAYMENT_ID / FIN_ACCOUNT_TRANS_ID).
                await CommissionPaymentCleanup.PurgeAsync(_context, salesRequestId, ct);

                var commissions = await _context.SalesCommissions
                    .Where(c => c.SalesRequestId == salesRequestId)
                    .ToListAsync(ct);

                if (commissions.Any())
                    _context.SalesCommissions.RemoveRange(commissions);

                // 3b. Customer payments (advance, installments, maintenance deposit) and every
                //     artifact hanging off them.
                var payments = await _context.Payments
                    .Where(p => p.SalesRequestId == salesRequestId
                                && p.PaymentTypeId != CommissionPaymentCleanup.CommissionPaymentTypeId)
                    .ToListAsync(ct);

                await PaymentArtifactCleanup.PurgePaymentsAsync(_context, payments, ct);

                // 3c. Ledger transactions booked against the request itself (APARTMENT_SALE_*,
                //     APARTMENT_MAINTENANCE_DEPOSIT) — these carry no PAYMENT_ID, so 3b misses them.
                var acctgTransIds = await _context.AcctgTrans
                    .Where(t => t.SalesRequestId == salesRequestId)
                    .Select(t => t.AcctgTransId)
                    .ToListAsync(ct);

                await PaymentArtifactCleanup.PurgeAcctgTransAsync(_context, acctgTransIds, ct);

                if (sr.Installments.Any())
                {
                    _context.SalesRequestInstallments.RemoveRange(sr.Installments);
                }

                // -----------------------------------------------------------------
                // 4. Clear apartment reservation
                // -----------------------------------------------------------------
                // REFACTOR: Restore apartment to available state before deleting the request.
                //           Ensures no dangling reservation remains after hard delete.
                apartment.ApartmentStatusId = ApartmentAvailableStatusId;
                apartment.ReservedBySalesRequestId = null;

                // -----------------------------------------------------------------
                // 5. Hard delete the SalesRequest
                // -----------------------------------------------------------------
                // REFACTOR: Physical removal instead of soft-delete (status change).
                //           Meets requirement: no response DTO needed because entity is gone.
                _context.SalesRequests.Remove(sr);

                // -----------------------------------------------------------------
                // 6. Persist all changes atomically
                // -----------------------------------------------------------------
                var saved = await _context.SaveChangesAsync(ct) > 0;
                if (!saved)
                {
                    await transaction.RollbackAsync(ct);
                    return Result<Unit>.Failure("Failed to delete sales request");
                }

                await transaction.CommitAsync(ct);

                // Success with no content
                return Result<Unit>.Success(Unit.Value);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(ct);
                // Surface the innermost provider message — an FK violation's detail sits there, and
                // the UI shows this text verbatim.
                var detail = ex.GetBaseException().Message;
                return Result<Unit>.Failure($"Failed to delete sales request: {detail}");
            }
        }
    }
}
