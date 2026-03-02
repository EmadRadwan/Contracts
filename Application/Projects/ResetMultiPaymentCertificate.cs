using Application.Core;
using MediatR;
using Persistence;
using Microsoft.EntityFrameworkCore;

namespace Application.Projects
{
    public class ResetMultiPaymentCertificate
    {
        public class Command : IRequest<Result<MultiPaymentCertificateDto>>
        {
            public string WorkEffortId { get; set; }
        }

        public class Handler : IRequestHandler<Command, Result<MultiPaymentCertificateDto>>
        {
            private readonly DataContext _context;

            public Handler(DataContext context)
            {
                _context = context;
            }

            public async Task<Result<MultiPaymentCertificateDto>> Handle(Command request, CancellationToken cancellationToken)
            {
                var certificate = await _context.WorkEfforts
                    .Include(w => w.AcctgTrans)
                    .ThenInclude(t => t.AcctgTransEntries)
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
                    // 1. Delete Accounting Transactions
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

                    // 2. Reset Status
                    certificate.CurrentStatusId = "WEPR_CREATED";
                    certificate.LastUpdatedStamp = DateTime.UtcNow;

                    foreach (var item in items)
                    {
                        item.CurrentStatusId = "WEPR_CREATED";
                        item.LastUpdatedStamp = DateTime.UtcNow;
                    }

                    var success = await _context.SaveChangesAsync(cancellationToken) > 0;

                    if (success)
                    {
                        await transaction.CommitAsync(cancellationToken);
                        
                        // Return the updated certificate DTO (simplified logic here, typically we'd map it)
                        // Re-using logic from Create/Approve or simplified
                        var statusDescriptions = new Dictionary<string, (string English, string Arabic)>
                        {
                            { "WEPR_CREATED", ("Created", "تم الإنشاء") },
                            { "WEPR_APPROVED", ("Approved", "تمت الموافقة") },
                            { "WEPR_COMPLETE", ("Complete", "مكتمل") }
                        };

                        var (statusDescription, statusDescriptionArabic) =
                            statusDescriptions.ContainsKey(certificate.CurrentStatusId)
                                ? statusDescriptions[certificate.CurrentStatusId]
                                : ("Unknown", "غير معروف");

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
                            // Items are not strictly needed for the form refresh as it refetches items anyway
                        };

                        return Result<MultiPaymentCertificateDto>.Success(resultDto);
                    }

                    await transaction.RollbackAsync(cancellationToken);
                    return Result<MultiPaymentCertificateDto>.Failure("Failed to reset the certificate");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return Result<MultiPaymentCertificateDto>.Failure($"Error resetting certificate: {ex.Message}");
                }
            }
        }
    }
}
