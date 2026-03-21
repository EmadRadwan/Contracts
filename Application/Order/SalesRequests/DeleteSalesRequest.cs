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
                // 3. Clean up related artifacts if the request was approved
                // -----------------------------------------------------------------
                if (sr.StatusId == "SALES_REQUEST_APPROVED")
                {
                    // Delete generated payments
                    var payments = await _context.Payments
                        .Where(p => p.SalesRequestId == sr.SalesRequestId)
                        .ToListAsync(ct);

                    if (payments.Any())
                    {
                        var paymentIds = payments.Select(p => p.PaymentId).ToList();

                        // Delete related FinAccountTran
                        var finAccountTrans = await _context.FinAccountTrans
                            .Where(fat => fat.PaymentId != null && paymentIds.Contains(fat.PaymentId))
                            .ToListAsync(ct);

                        if (finAccountTrans.Any())
                            _context.FinAccountTrans.RemoveRange(finAccountTrans);

                        _context.Payments.RemoveRange(payments);
                    }

                    // Delete accounting transaction + entries
                    var acctgTransList = await _context.AcctgTrans
                        .Include(t => t.AcctgTransEntries)
                        .Where(t => t.SalesRequestId == sr.SalesRequestId)
                        .ToListAsync(ct);

                    if (acctgTransList.Any())
                    {
                        // REFACTOR: Remove all entries from all transactions first
                        // Ensures no FK violations when deleting parent AcctgTran records.
                        foreach (var tran in acctgTransList)
                        {
                            _context.AcctgTransEntries.RemoveRange(tran.AcctgTransEntries);
                        }

                        // REFACTOR: Now safely remove all parent AcctgTran records
                        _context.AcctgTrans.RemoveRange(acctgTransList);
                    }
                }

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
                return Result<Unit>.Failure($"Failed to delete sales request: {ex.Message}");
            }
        }
    }
}