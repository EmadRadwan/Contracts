using Application.Accounting.Services;
using Application.Core;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Projects
{
    /// <summary>
    /// Returns an approved multi-payment certificate to Created so it can be corrected and
    /// re-approved. Step 3 of the auditor's soft-delete requirement: the disbursement transaction
    /// posted at approval is reversed by a linked contra entry, not deleted. Re-approval posts a
    /// fresh transaction; the ledger keeps all three.
    /// </summary>
    public class ResetMultiPaymentCertificate
    {
        public class Command : IRequest<Result<MultiPaymentCertificateDto>>
        {
            public string WorkEffortId { get; set; }
            public string? Reason { get; set; }
        }

        public class Handler : IRequestHandler<Command, Result<MultiPaymentCertificateDto>>
        {
            private readonly DataContext _context;
            private readonly IAccountingPeriodGuard _periodGuard;
            private readonly IAcctgTransReversalService _reversal;

            public Handler(DataContext context, IAccountingPeriodGuard periodGuard, IAcctgTransReversalService reversal)
            {
                _context = context;
                _periodGuard = periodGuard;
                _reversal = reversal;
            }

            public async Task<Result<MultiPaymentCertificateDto>> Handle(Command request, CancellationToken cancellationToken)
            {
                var certificate = await _context.WorkEfforts
                    .FirstOrDefaultAsync(x => x.WorkEffortId == request.WorkEffortId && x.WorkEffortTypeId == "PAYMENT_CERTIFICATE", cancellationToken);

                if (certificate == null) return null;

                if (certificate.CurrentStatusId != "WEPR_APPROVED")
                {
                    return Result<MultiPaymentCertificateDto>.Failure("Only approved certificates can be reset");
                }

                var items = await _context.WorkEfforts
                    .Where(x => x.WorkEffortParentId == request.WorkEffortId && x.WorkEffortTypeId == "PAYMENT_CERTIFICATE_ITEM")
                    .ToListAsync(cancellationToken);

                await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    await _periodGuard.EnsureOpenForWorkEffortsAsync(new[] { request.WorkEffortId }, cancellationToken);

                    var reason = string.IsNullOrWhiteSpace(request.Reason)
                        ? $"Multi-payment certificate {certificate.CertificateNumber ?? request.WorkEffortId} reset"
                        : request.Reason.Trim();

                    var reversible = await _reversal.FindReversibleForWorkEffortAsync(request.WorkEffortId, cancellationToken);
                    var reversed = await _reversal.ReverseManyAsync(reversible,
                        new ReverseAcctgTransOptions { Reason = reason }, cancellationToken);
                    if (!reversed.IsSuccess)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return Result<MultiPaymentCertificateDto>.Failure(reversed.ErrorMessage);
                    }

                    // Unposted drafts (none in practice) go as drafts.
                    var drafts = await _context.AcctgTrans
                        .Include(t => t.AcctgTransEntries)
                        .Where(t => t.WorkEffortId == request.WorkEffortId && t.IsPosted != "Y")
                        .ToListAsync(cancellationToken);
                    foreach (var draft in drafts)
                    {
                        _context.AcctgTransEntries.RemoveRange(draft.AcctgTransEntries);
                        _context.AcctgTrans.Remove(draft);
                    }

                    certificate.CurrentStatusId = "WEPR_CREATED";
                    certificate.LastUpdatedStamp = DateTime.UtcNow;
                    foreach (var item in items)
                    {
                        item.CurrentStatusId = "WEPR_CREATED";
                        item.LastUpdatedStamp = DateTime.UtcNow;
                    }

                    var success = await _context.SaveChangesAsync(cancellationToken) > 0;
                    if (!success)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return Result<MultiPaymentCertificateDto>.Failure("Failed to reset the certificate");
                    }

                    await transaction.CommitAsync(cancellationToken);

                    var (statusDescription, statusDescriptionArabic) = MultiPaymentCertificateStatus.Describe(certificate.CurrentStatusId);

                    var employeeParty = certificate.PartyIdEmployee != null
                        ? await _context.Parties
                            .Where(p => p.PartyId == certificate.PartyIdEmployee)
                            .Select(p => new { p.PartyId, p.Description })
                            .FirstOrDefaultAsync(cancellationToken)
                        : null;

                    var resultDto = new MultiPaymentCertificateDto
                    {
                        WorkEffortId = certificate.WorkEffortId,
                        Date = certificate.EstimatedStartDate,
                        Description = certificate.Description,
                        Notes = certificate.Notes,
                        PartyIdEmployee = certificate.PartyIdEmployee,
                        PartyName = employeeParty?.Description,
                        CurrentStatusId = certificate.CurrentStatusId,
                        StatusDescription = statusDescription,
                        StatusDescriptionArabic = statusDescriptionArabic,
                        GlAccountId = certificate.GlAccountId,
                    };

                    return Result<MultiPaymentCertificateDto>.Success(resultDto);
                }
                catch (ClosedAccountingPeriodException ex)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return Result<MultiPaymentCertificateDto>.Failure(ex.Message);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return Result<MultiPaymentCertificateDto>.Failure($"Error resetting certificate: {ex.Message}");
                }
            }
        }
    }

    /// <summary>Status labels shared by the multi-payment certificate handlers.</summary>
    public static class MultiPaymentCertificateStatus
    {
        public static (string English, string Arabic) Describe(string? statusId) => statusId switch
        {
            "WEPR_CREATED" => ("Created", "تم الإنشاء"),
            "WEPR_APPROVED" => ("Approved", "تمت الموافقة"),
            "WEPR_COMPLETE" => ("Complete", "مكتمل"),
            "WEPR_CANCELLED" => ("Cancelled", "ملغاة"),
            _ => ("Unknown", "غير معروف")
        };
    }
}
