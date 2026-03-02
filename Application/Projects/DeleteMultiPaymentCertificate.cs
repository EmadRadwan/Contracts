using Application.Core;
using MediatR;
using Persistence;
using Microsoft.EntityFrameworkCore;

namespace Application.Projects
{
    public class DeleteMultiPaymentCertificate
    {
        public class Command : IRequest<Result<Unit>>
        {
            public string WorkEffortId { get; set; }
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
                var certificate = await _context.WorkEfforts
                    .Include(w => w.AcctgTrans)
                    .ThenInclude(t => t.AcctgTransEntries)
                    .FirstOrDefaultAsync(x => x.WorkEffortId == request.WorkEffortId && x.WorkEffortTypeId == "PAYMENT_CERTIFICATE", cancellationToken);

                if (certificate == null) return null;

                // Also find items (children)
                var items = await _context.WorkEfforts
                    .Where(x => x.WorkEffortParentId == request.WorkEffortId && x.WorkEffortTypeId == "PAYMENT_CERTIFICATE_ITEM")
                    .ToListAsync(cancellationToken);

                await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    // 1. Delete Accounting Transactions if they exist
                    if (certificate.AcctgTrans != null && certificate.AcctgTrans.Any())
                    {
                        foreach (var acctgTrans in certificate.AcctgTrans)
                        {
                            if (acctgTrans.AcctgTransEntries != null)
                            {
                                _context.AcctgTransEntries.RemoveRange(acctgTrans.AcctgTransEntries);
                            }
                        }
                        _context.AcctgTrans.RemoveRange(certificate.AcctgTrans);
                    }

                    // 2. Delete Items
                    if (items.Any())
                    {
                        _context.WorkEfforts.RemoveRange(items);
                    }

                    // 3. Delete Certificate itself
                    _context.WorkEfforts.Remove(certificate);

                    var success = await _context.SaveChangesAsync(cancellationToken) > 0;

                    if (success)
                    {
                        await transaction.CommitAsync(cancellationToken);
                        return Result<Unit>.Success(Unit.Value);
                    }

                    await transaction.RollbackAsync(cancellationToken);
                    return Result<Unit>.Failure("Failed to delete the certificate");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return Result<Unit>.Failure($"Error deleting certificate: {ex.Message}");
                }
            }
        }
    }
}
