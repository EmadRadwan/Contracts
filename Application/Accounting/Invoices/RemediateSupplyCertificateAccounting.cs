using Application.Accounting.Services;
using Application.Facilities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Accounting.Invoices;

// One-off remediation for the SUPPLY_PROCUREMENT_CERTIFICATE accounting gap: certificates approved
// before the fix in ReceiveInventoryFromPurchaseOrder/_invoiceUtilityService either (a) never had their
// PO completed, so no invoice exists at all, or (b) have an invoice that was posted through the old
// isSupplyCertificate skip and so has zero AcctgTrans. This walks both cases and posts the missing
// AP liability using the exact same code paths the live approval flow now uses.
//
// PartyIdSuppliers is a required allowlist, not an "everything" switch — each supplier must be added
// explicitly after checking their certificates, since this posts real, IsPosted="Y" GL entries.
public class RemediateSupplyCertificateAccounting
{
    public class Command : IRequest<Result<RemediateSupplyCertificateAccountingResult>>
    {
        public List<string> PartyIdSuppliers { get; set; } = new();
    }

    public class RemediateSupplyCertificateAccountingResult
    {
        public List<string> InvoicesCreated { get; set; } = new();
        public List<string> InvoicesPosted { get; set; } = new();
        public List<string> Skipped { get; set; } = new();
        public List<string> Errors { get; set; } = new();
    }

    public class Handler : IRequestHandler<Command, Result<RemediateSupplyCertificateAccountingResult>>
    {
        private readonly DataContext _context;
        private readonly IMediator _mediator;
        private readonly IInvoiceUtilityService _invoiceUtilityService;
        private readonly IInvoiceHelperService _invoiceHelperService;
        private readonly IGeneralLedgerService _generalLedgerService;
        private readonly ILogger<Handler> _logger;

        public Handler(DataContext context, IMediator mediator, IInvoiceUtilityService invoiceUtilityService,
            IInvoiceHelperService invoiceHelperService, IGeneralLedgerService generalLedgerService,
            ILogger<Handler> logger)
        {
            _context = context;
            _mediator = mediator;
            _invoiceUtilityService = invoiceUtilityService;
            _invoiceHelperService = invoiceHelperService;
            _generalLedgerService = generalLedgerService;
            _logger = logger;
        }

        public async Task<Result<RemediateSupplyCertificateAccountingResult>> Handle(Command request,
            CancellationToken cancellationToken)
        {
            var result = new RemediateSupplyCertificateAccountingResult();

            if (request.PartyIdSuppliers == null || request.PartyIdSuppliers.Count == 0)
            {
                return Result<RemediateSupplyCertificateAccountingResult>.Failure(
                    "At least one PartyIdSupplier is required — this remediation does not run unscoped.");
            }

            var certificates = await _context.WorkEfforts
                .Where(we => we.WorkEffortTypeId == "PROJECT_CERTIFICATE"
                             && we.CertificateCategory == "SUPPLY_PROCUREMENT_CERTIFICATE"
                             && we.PartyIdSupplier != null
                             && request.PartyIdSuppliers.Contains(we.PartyIdSupplier)
                             && we.RelatedOrderId != null)
                .ToListAsync(cancellationToken);

            foreach (var certificate in certificates)
            {
                var orderId = certificate.RelatedOrderId;

                // Wrap invoice creation + GL posting for this certificate in one explicit transaction.
                // ChangeInvoiceStatus.cs (the existing, working invoice-status endpoint) does the same —
                // SetInvoiceStatus/CreateAcctgTrans*/CreateAcctgTransEntry only add to the change tracker,
                // they never call SaveChangesAsync themselves, and a bare SaveChangesAsync without an
                // explicit BeginTransactionAsync/CommitAsync around it did not reliably persist here.
                // ProcessWorkmanCertificatePurchaseOrder already checks CurrentTransaction == null before
                // opening its own, so it correctly joins this transaction instead of committing separately.
                IDbContextTransaction? transaction = null;
                var ownsTransaction = false;
                var committed = false;

                try
                {
                    if (_context.Database.CurrentTransaction == null)
                    {
                        transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                        ownsTransaction = true;
                    }

                    // 1. Find (or create) the invoice for this certificate's PO.
                    var invoiceId = await _context.OrderItemBillings
                        .Where(oib => oib.OrderId == orderId)
                        .Select(oib => oib.InvoiceId)
                        .Distinct()
                        .FirstOrDefaultAsync(cancellationToken);

                    if (string.IsNullOrEmpty(invoiceId))
                    {
                        // Pre-fix state: PO was approved/received but never completed, so
                        // CreateInvoiceFromOrder was never triggered. Complete it now via the
                        // same command the live approval flow uses.
                        var completeResult = await _mediator.Send(
                            new ProcessWorkmanCertificatePurchaseOrder.Command
                                { WorkEffortId = certificate.WorkEffortId },
                            cancellationToken);

                        if (!completeResult.IsSuccess)
                        {
                            result.Errors.Add(
                                $"{certificate.WorkEffortId} ({certificate.CertificateNumber}): failed to complete order {orderId} — {completeResult.Error}");
                            if (ownsTransaction) await transaction!.RollbackAsync(cancellationToken);
                            continue;
                        }

                        invoiceId = await _context.OrderItemBillings
                            .Where(oib => oib.OrderId == orderId)
                            .Select(oib => oib.InvoiceId)
                            .Distinct()
                            .FirstOrDefaultAsync(cancellationToken);

                        if (string.IsNullOrEmpty(invoiceId))
                        {
                            // The order was likely already at ORDER_COMPLETED from before this remediation
                            // existed (confirmed via ORDER_STATUS history for the first batch this hit) —
                            // ChangeOrderStatus short-circuits and skips CreateInvoiceFromOrder when the
                            // order is already at the target status, so completing it "again" above was a
                            // no-op. Call invoicing directly instead of going through the status change.
                            invoiceId = await _invoiceHelperService.CreateInvoiceFromOrder(orderId);

                            if (string.IsNullOrEmpty(invoiceId))
                            {
                                result.Errors.Add(
                                    $"{certificate.WorkEffortId} ({certificate.CertificateNumber}): order {orderId} completed but CreateInvoiceFromOrder produced no invoice.");
                                if (ownsTransaction) await transaction!.RollbackAsync(cancellationToken);
                                continue;
                            }

                            // CRITICAL: flush before any posting logic below queries invoice items.
                            // CreateInvoiceFromOrder only adds Invoice/InvoiceItem/OrderItemBilling to the
                            // change tracker — unlike ProcessWorkmanCertificatePurchaseOrder (used in the
                            // branch above), which happens to SaveChanges internally right after. Without
                            // this, CreateAcctgTransForPurchaseInvoice's own database query for invoice
                            // items finds zero rows (they're only in the tracker, not yet in the DB) and
                            // posts a degenerate, unbalanced, zero-amount entry instead — this is exactly
                            // what happened on the first run of this fallback and had to be corrected by
                            // hand afterward.
                            await _context.SaveChangesAsync(cancellationToken);
                        }

                        result.InvoicesCreated.Add(invoiceId);
                    }

                    // 2. Skip invoices that already have posted accounting (nothing to remediate).
                    var alreadyPosted =
                        await _context.AcctgTrans.AnyAsync(t => t.InvoiceId == invoiceId, cancellationToken);
                    if (alreadyPosted)
                    {
                        result.Skipped.Add($"{invoiceId}: already has accounting transactions.");
                        if (ownsTransaction)
                        {
                            await transaction!.CommitAsync(cancellationToken);
                            committed = true;
                        }
                        continue;
                    }

                    var invoice = await _context.Invoices.FindAsync(new object[] { invoiceId }, cancellationToken);
                    if (invoice == null)
                    {
                        result.Errors.Add($"{certificate.WorkEffortId}: invoice {invoiceId} not found.");
                        if (ownsTransaction) await transaction!.RollbackAsync(cancellationToken);
                        continue;
                    }

                    if (invoice.StatusId == "INVOICE_READY" || invoice.StatusId == "INVOICE_PAID")
                    {
                        // Already approved under the old skip-accounting stub — post directly.
                        // SetInvoiceStatus would no-op here since oldStatusId == newStatusId.
                        var acctgTransId = await _generalLedgerService.CreateAcctgTransForPurchaseInvoice(invoiceId);
                        if (string.IsNullOrEmpty(acctgTransId))
                        {
                            result.Errors.Add(
                                $"{invoiceId}: CreateAcctgTransForPurchaseInvoice returned no transaction.");
                            if (ownsTransaction) await transaction!.RollbackAsync(cancellationToken);
                            continue;
                        }

                        result.InvoicesPosted.Add(invoiceId);
                    }
                    else
                    {
                        // Freshly created (or still INVOICE_IN_PROCESS) — advance it to INVOICE_READY,
                        // which now correctly triggers GL posting for PINV_CERTIFICATE_SUPPLY_ITEM.
                        await _invoiceUtilityService.SetInvoiceStatus(invoiceId, "INVOICE_READY");
                        result.InvoicesPosted.Add(invoiceId);
                    }

                    // Neither SetInvoiceStatus nor CreateAcctgTrans[...]/CreateAcctgTransEntry save on their
                    // own — they only add to the change tracker. Must flush + commit explicitly: a bare
                    // SaveChangesAsync without wrapping BeginTransactionAsync/CommitAsync did not reliably
                    // persist here, matching why ChangeInvoiceStatus.cs wraps the same call this way.
                    var saved = await _context.SaveChangesAsync(cancellationToken);
                    if (saved == 0)
                    {
                        result.Errors.Add(
                            $"{invoiceId}: SaveChangesAsync persisted zero changes — accounting may not have been posted.");
                        if (ownsTransaction) await transaction!.RollbackAsync(cancellationToken);
                        continue;
                    }

                    if (ownsTransaction)
                    {
                        await transaction!.CommitAsync(cancellationToken);
                        committed = true;
                    }

                    // SetInvoiceStatus swallows accounting failures internally (logs, doesn't throw), so
                    // don't trust "no exception" as proof of success — verify a transaction actually landed,
                    // after commit so this reflects durably persisted state.
                    var posted = await _context.AcctgTrans.AnyAsync(t => t.InvoiceId == invoiceId, cancellationToken);
                    if (!posted)
                    {
                        result.InvoicesPosted.Remove(invoiceId);
                        result.Errors.Add(
                            $"{invoiceId}: no AcctgTrans found after posting attempt — check logs for a swallowed error.");
                    }
                }
                catch (Exception ex)
                {
                    if (ownsTransaction && transaction != null && !committed)
                        await transaction.RollbackAsync(cancellationToken);
                    _logger.LogError(ex, "Remediation failed for certificate {WorkEffortId} (order {OrderId})",
                        certificate.WorkEffortId, orderId);
                    result.Errors.Add($"{certificate.WorkEffortId} (order {orderId}): {ex.Message}");
                }
                finally
                {
                    if (ownsTransaction && transaction != null) await transaction.DisposeAsync();
                }
            }

            return Result<RemediateSupplyCertificateAccountingResult>.Success(result);
        }
    }
}
