using Application.Accounting.Services;
using Application.Core;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Projects
{
    /// <summary>
    /// Removes a multi-payment certificate. Step 3 of the auditor's soft-delete requirement:
    ///  - never posted to the ledger: the certificate and its items are physically deleted;
    ///  - ever posted (approved now, or approved and reset earlier): the certificate is kept and
    ///    marked WEPR_CANCELLED, and any still-live posting is reversed by a linked contra entry.
    /// </summary>
    public class DeleteMultiPaymentCertificate
    {
        public class Command : IRequest<Result<Unit>>
        {
            public string WorkEffortId { get; set; }
            public string? Reason { get; set; }
        }

        public class Handler : IRequestHandler<Command, Result<Unit>>
        {
            private readonly DataContext _context;
            private readonly IAccountingPeriodGuard _periodGuard;
            private readonly ILedgerHistoryService _ledgerHistory;
            private readonly IAcctgTransReversalService _reversal;

            public Handler(DataContext context, IAccountingPeriodGuard periodGuard, ILedgerHistoryService ledgerHistory,
                IAcctgTransReversalService reversal)
            {
                _context = context;
                _periodGuard = periodGuard;
                _ledgerHistory = ledgerHistory;
                _reversal = reversal;
            }

            public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
            {
                var certificate = await _context.WorkEfforts
                    .FirstOrDefaultAsync(x => x.WorkEffortId == request.WorkEffortId && x.WorkEffortTypeId == "PAYMENT_CERTIFICATE", cancellationToken);

                if (certificate == null) return null;

                if (certificate.CurrentStatusId == "WEPR_CANCELLED")
                    return Result<Unit>.Failure("الشهادة ملغاة بالفعل. (Certificate is already cancelled.)");

                var items = await _context.WorkEfforts
                    .Where(x => x.WorkEffortParentId == request.WorkEffortId && x.WorkEffortTypeId == "PAYMENT_CERTIFICATE_ITEM")
                    .ToListAsync(cancellationToken);

                await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    var history = await _ledgerHistory.ForWorkEffortAsync(request.WorkEffortId, cancellationToken);

                    if (history.HasHistory)
                    {
                        // Void path: keep the row, cancel the posting.
                        await _periodGuard.EnsureOpenForWorkEffortsAsync(new[] { request.WorkEffortId }, cancellationToken);

                        var reason = string.IsNullOrWhiteSpace(request.Reason)
                            ? $"Multi-payment certificate {certificate.CertificateNumber ?? request.WorkEffortId} cancelled"
                            : request.Reason.Trim();

                        var reversible = await _reversal.FindReversibleForWorkEffortAsync(request.WorkEffortId, cancellationToken);
                        var reversed = await _reversal.ReverseManyAsync(reversible,
                            new ReverseAcctgTransOptions { Reason = reason }, cancellationToken);
                        if (!reversed.IsSuccess)
                        {
                            await transaction.RollbackAsync(cancellationToken);
                            return Result<Unit>.Failure(reversed.ErrorMessage);
                        }

                        var stamp = DateTime.UtcNow;
                        certificate.CurrentStatusId = "WEPR_CANCELLED";
                        certificate.LastUpdatedStamp = stamp;
                        foreach (var item in items)
                        {
                            item.CurrentStatusId = "WEPR_CANCELLED";
                            item.LastUpdatedStamp = stamp;
                        }
                    }
                    else
                    {
                        // Never reached the ledger: a draft, deleted outright.
                        if (items.Any())
                            _context.WorkEfforts.RemoveRange(items);
                        _context.WorkEfforts.Remove(certificate);
                    }

                    var success = await _context.SaveChangesAsync(cancellationToken) > 0;
                    if (!success)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return Result<Unit>.Failure("Failed to delete the certificate");
                    }

                    await transaction.CommitAsync(cancellationToken);
                    return Result<Unit>.Success(Unit.Value);
                }
                catch (ClosedAccountingPeriodException ex)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return Result<Unit>.Failure(ex.Message);
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
