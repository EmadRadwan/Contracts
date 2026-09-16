using Application.Accounting.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Accounting.Transactions
{
    public class DeleteAcctgTrans
    {
        public class Command : IRequest<Result<Unit>>
        {
            public string AcctgTransId { get; set; }
        }

        public class Handler : IRequestHandler<Command, Result<Unit>>
        {
            private readonly DataContext _context;
        private readonly IAccountingPeriodGuard _periodGuard;

            public Handler(DataContext context, IAccountingPeriodGuard periodGuard)
            {
                _context = context;
            _periodGuard = periodGuard;
            }

            public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
            {
                await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

                try
                {
                    // Find header
                    var header = await _context.AcctgTrans
                        .FirstOrDefaultAsync(t => t.AcctgTransId == request.AcctgTransId, cancellationToken);

                    if (header == null)
                        return Result<Unit>.Failure("Transaction not found");

                    // Step 3 (auditor soft-delete requirement): a posted transaction is never deleted.
                    // It is cancelled by a linked reversal through ReverseAcctgTrans.
                    if (header.IsPosted == "Y")
                        return Result<Unit>.Failure(
                            $"القيد {request.AcctgTransId} مرحّل ولا يمكن حذفه. استخدم إجراء \"عكس القيد\" بدلاً من الحذف. " +
                            "(Posted transactions are reversed, not deleted.)");

                    // Closed-period control on the unposted draft being removed.
                    await _periodGuard.EnsureOpenForAcctgTransAsync(new[] { request.AcctgTransId }, cancellationToken);

                    // Delete all entries
                    var entries = await _context.AcctgTransEntries
                        .Where(e => e.AcctgTransId == request.AcctgTransId)
                        .ToListAsync(cancellationToken);

                    _context.AcctgTransEntries.RemoveRange(entries);

                    // Delete header
                    _context.AcctgTrans.Remove(header);

                    await _context.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);

                    return Result<Unit>.Success(Unit.Value);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return Result<Unit>.Failure($"Failed to delete transaction: {ex.Message}");
                }
            }
        }
    }
}