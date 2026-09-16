using Application.Accounting.Services;
using Application.Core;
using Application.Shipments.Invoices;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Microsoft.Extensions.Logging;

namespace Application.Accounting.Invoices;

public class ResetInvoice
{
    public class Command : IRequest<Results<InvoiceDto3>>
    {
        public string InvoiceId { get; set; } = string.Empty;
    }

    public class Handler : IRequestHandler<Command, Results<InvoiceDto3>>
    {
        private readonly DataContext _context;
        private readonly IAccountingPeriodGuard _periodGuard;
        private readonly ILogger<Handler> _logger;

        public Handler(DataContext context, ILogger<Handler> logger, IAccountingPeriodGuard periodGuard)
        {
            _context = context;
            _periodGuard = periodGuard;
            _logger = logger;
        }

        public async Task<Results<InvoiceDto3>> Handle(Command request, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(request.InvoiceId))
            {
                return Results<InvoiceDto3>.Failure("Invoice ID is required", "INVALID_INPUT");
            }

            await using var transaction = await _context.Database.BeginTransactionAsync(ct);

            try
            {
                // 1. Fetch the current invoice
                var original = await _context.Invoices
                    .AsNoTracking()
                    .Include(i => i.InvoiceType)
                    .Include(i => i.Status)
                    .FirstOrDefaultAsync(i => i.InvoiceId == request.InvoiceId, ct);

                if (original == null)
                {
                    return Results<InvoiceDto3>.Failure("Invoice not found", "INVOICE_NOT_FOUND");
                }

                // Guard: based on issue description, it might be in INVOICE_READY
                // but let's be flexible or follow the requested status check.
                // The issue says: "visibility of the new menu item will depend on having the permission and the invoice must be in statusId == 'INVOICE_READY'"
                if (original.StatusId != "INVOICE_READY")
                {
                    return Results<InvoiceDto3>.Failure(
                        $"Cannot reset invoice in status '{original.StatusId}'. Only 'INVOICE_READY' status can be reset.",
                        "INVALID_INVOICE_STATUS"
                    );
                }

                // Pragmatic policy (agreed Sep 2026): an accrual may be unposted, edited and re-posted
                // only while no payment against it has left draft. Once a payment is sent or
                // received the month is settled; corrections go through a reversal or the next run.
                var settledBy = await (from pa in _context.PaymentApplications
                        join pm in _context.Payments on pa.PaymentId equals pm.PaymentId
                        where pa.InvoiceId == request.InvoiceId && pm.StatusId != "PMNT_NOT_PAID"
                        select pm.PaymentId)
                    .Distinct()
                    .ToListAsync(ct);
                if (settledBy.Any())
                {
                    return Results<InvoiceDto3>.Failure(
                        $"لا يمكن إعادة تعيين الفاتورة لأن دفعة مرسلة أو مستلمة مسجلة عليها ({string.Join("، ", settledBy)}). " +
                        "ألغِ الدفعة أو اعكس قيدها أولاً. " +
                        "(A sent or received payment is applied to this invoice; void or reverse it first.)",
                        "INVOICE_SETTLED");
                }

                // Closed-period control on the accrual about to be removed.
                await _periodGuard.EnsureOpenForInvoicesAsync(new[] { request.InvoiceId }, ct);

                // 2. Reset core fields on the invoice
                var invoiceToUpdate = new Invoice { InvoiceId = request.InvoiceId };
                _context.Attach(invoiceToUpdate);

                invoiceToUpdate.StatusId = "INVOICE_IN_PROCESS";
                invoiceToUpdate.PaidDate = null;
                invoiceToUpdate.LastUpdatedStamp = DateTime.UtcNow;
                invoiceToUpdate.LastUpdatedTxStamp = DateTime.UtcNow;

                // 3. Status history is audit evidence and is kept; the reset is recorded as one more step.
                //    (This used to delete every INVOICE_STATUS row other than IN_PROCESS.)
                var now = DateTime.UtcNow;
                _context.InvoiceStatuses.Add(new InvoiceStatus
                {
                    InvoiceId = request.InvoiceId,
                    StatusId = "INVOICE_IN_PROCESS",
                    StatusDate = now,
                    // INVOICE_STATUS.CHANGE_BY_USER_LOGIN_ID is an FK to the OFBiz USER_LOGIN table
                    // (GUID keys); the app's identity usernames are not rows there, and every existing
                    // history row leaves it null. Authorship is on AUDIT_ACTIVITY instead.
                    ChangeByUserLoginId = null,
                    CreatedStamp = now,
                    LastUpdatedStamp = now
                });

                // 4. Delete related accounting transactions (entries first, then headers)
                // Delete related accounting transactions to the invoice from AcctgTrans and AcctgTransEntries tables
                await _context.AcctgTransEntries
                    .Where(ate => _context.AcctgTrans
                        .Any(at => at.AcctgTransId == ate.AcctgTransId &&
                                   at.InvoiceId == request.InvoiceId))
                    .ExecuteDeleteAsync(ct);

                await _context.AcctgTrans
                    .Where(at => at.InvoiceId == request.InvoiceId)
                    .ExecuteDeleteAsync(ct);

                // 5. Look at PaymentApplications table and delete records from there related to this invoice
                await _context.PaymentApplications
                    .Where(pa => pa.InvoiceId == request.InvoiceId)
                    .ExecuteDeleteAsync(ct);

                // 6. Save all changes
                await _context.SaveChangesAsync(ct);

                // 7. Reload the updated invoice entity to return it
                var updatedInvoice = await _context.Invoices
                    .AsNoTracking()
                    .Include(i => i.InvoiceType)
                    .Include(i => i.Status)
                    .FirstOrDefaultAsync(i => i.InvoiceId == request.InvoiceId, ct);

                if (updatedInvoice == null)
                {
                    throw new InvalidOperationException("Failed to reload updated invoice after reset");
                }

                // 8. Load party names for DTO
                var fromPartyName = await _context.Parties
                    .Where(p => p.PartyId == updatedInvoice.PartyIdFrom)
                    .Select(p => p.Description)
                    .FirstOrDefaultAsync(ct);

                var toPartyName = await _context.Parties
                    .Where(p => p.PartyId == updatedInvoice.PartyId)
                    .Select(p => p.Description)
                    .FirstOrDefaultAsync(ct);

                // 9. Build DTO to return
                var invoiceDto = new InvoiceDto3
                {
                    InvoiceId = updatedInvoice.InvoiceId,
                    InvoiceTypeId = updatedInvoice.InvoiceTypeId,
                    InvoiceTypeDescription = updatedInvoice.InvoiceType?.Description,
                    PartyIdFrom = updatedInvoice.PartyIdFrom,
                    FromPartyName = fromPartyName,
                    PartyId = updatedInvoice.PartyId,
                    ToPartyName = toPartyName,
                    RoleTypeId = updatedInvoice.RoleTypeId,
                    StatusId = updatedInvoice.StatusId,
                    StatusDescription = "In Process", // or load from status item
                    BillingAccountId = updatedInvoice.BillingAccountId,
                    ContactMechId = updatedInvoice.ContactMechId,
                    InvoiceDate = updatedInvoice.InvoiceDate,
                    DueDate = updatedInvoice.DueDate,
                    PaidDate = updatedInvoice.PaidDate,
                    InvoiceMessage = updatedInvoice.InvoiceMessage,
                    ReferenceNumber = updatedInvoice.ReferenceNumber,
                    Description = updatedInvoice.Description,
                    CurrencyUomId = updatedInvoice.CurrencyUomId,
                };

                await transaction.CommitAsync(ct);

                _logger.LogInformation("Invoice {InvoiceId} successfully reset", request.InvoiceId);

                return Results<InvoiceDto3>.Success(invoiceDto);
            }
            catch (ClosedAccountingPeriodException ex)
            {
                await transaction.RollbackAsync(ct);
                return Results<InvoiceDto3>.Failure(ex.Message, "PERIOD_CLOSED");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(ct);
                _logger.LogError(ex, "Unexpected error resetting invoice {InvoiceId}", request.InvoiceId);
                return Results<InvoiceDto3>.Failure(
                    "An unexpected error occurred while resetting the invoice.",
                    "UNEXPECTED_ERROR"
                );
            }
        }
    }
}
