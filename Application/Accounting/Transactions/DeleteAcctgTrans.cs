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

            public Handler(DataContext context)
            {
                _context = context;
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

                    // Optional: Prevent deletion if posted (uncomment if needed)
                    // if (header.IsPosted == "Y")
                    //     return Result<Unit>.Failure("Cannot delete posted transactions");

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