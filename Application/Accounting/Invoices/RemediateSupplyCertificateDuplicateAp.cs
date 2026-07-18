using Application.Accounting.Services;
using Application.Accounting.Services.Models;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Accounting.Invoices;

// One-off correction for the SUPPLY_PROCUREMENT_CERTIFICATE double-ACCOUNTS_PAYABLE bug (see
// RemediateSupplyCertificateAccounting.cs, which introduced it, and the fix in
// CreateAcctgTransForShipmentReceiptForProject, which now credits UNINVOICED_SHIP_RCPT instead of
// ACCOUNTS_PAYABLE at receipt time). Certificates approved before that fix have both a
// SHIPMENT_RECEIPT that credited ACCOUNTS_PAYABLE directly and a PURCHASE_INVOICE that credited
// ACCOUNTS_PAYABLE again for the same delivery.
//
// This does not edit or delete either original posting — both stay in the ledger exactly as they
// were entered. Instead, per affected certificate, it posts one new INTERNAL_ACCTG_TRANS entry dated
// today: Dr ACCOUNTS_PAYABLE / Cr UNINVOICED_SHIP_RCPT, for exactly the amount the certificate's
// SHIPMENT_RECEIPT(s) credited to ACCOUNTS_PAYABLE. That reclassifies the receipt-time credit onto
// the clearing account it should have used, leaving only the invoice's ACCOUNTS_PAYABLE credit
// standing on the vendor's ledger — matching what the corrected code produces for new certificates.
//
// PartyIdSuppliers is a required allowlist, not an "everything" switch — same convention as
// RemediateSupplyCertificateAccounting: each supplier must be added explicitly after review, since
// this posts real, IsPosted="Y" GL entries.
public class RemediateSupplyCertificateDuplicateAp
{
    // Stamped on the AcctgTrans header only — used purely to detect an already-corrected certificate
    // (see the alreadyCorrected check below). Never shown on GetGlAccountTransactionDetails, which
    // reads AcctgTransEntry.Description instead, so it can stay a plain machine-readable marker.
    private const string CorrectionMarker = "DUPLICATE_AP_CORRECTION_SUPPLY_CERTIFICATE";

    // What actually shows up in the vendor's transaction report (البيان column) for both entry lines.
    // The report already displays SHIPMENT_RECEIPT/PURCHASE_INVOICE next to this in نوع القيد, so a
    // reader can tell the lines apart — this just explains why the same amount reappears here.
    private const string EntryDisplayDescription =
        "تصحيح ازدواج القيد: تحويل من حساب الدائنون إلى حساب الإيصالات غير المفوترة";

    public class Command : IRequest<Result<RemediateSupplyCertificateDuplicateApResult>>
    {
        public List<string> PartyIdSuppliers { get; set; } = new();
    }

    public class RemediateSupplyCertificateDuplicateApResult
    {
        public List<string> Corrected { get; set; } = new();
        public List<string> Skipped { get; set; } = new();
        public List<string> Errors { get; set; } = new();
    }

    public class Handler : IRequestHandler<Command, Result<RemediateSupplyCertificateDuplicateApResult>>
    {
        private readonly DataContext _context;
        private readonly IGeneralLedgerService _generalLedgerService;
        private readonly ILogger<Handler> _logger;

        public Handler(DataContext context, IGeneralLedgerService generalLedgerService, ILogger<Handler> logger)
        {
            _context = context;
            _generalLedgerService = generalLedgerService;
            _logger = logger;
        }

        public async Task<Result<RemediateSupplyCertificateDuplicateApResult>> Handle(Command request,
            CancellationToken cancellationToken)
        {
            var result = new RemediateSupplyCertificateDuplicateApResult();

            if (request.PartyIdSuppliers == null || request.PartyIdSuppliers.Count == 0)
            {
                return Result<RemediateSupplyCertificateDuplicateApResult>.Failure(
                    "At least one PartyIdSupplier is required — this correction does not run unscoped.");
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

                    // 1. Must have a posted SHIPMENT_RECEIPT — that's the entry we may need to reclass.
                    var shipmentReceiptTransIds = await _context.AcctgTrans
                        .Where(t => t.WorkEffortId == certificate.WorkEffortId
                                    && t.AcctgTransTypeId == "SHIPMENT_RECEIPT"
                                    && t.IsPosted == "Y")
                        .Select(t => t.AcctgTransId)
                        .ToListAsync(cancellationToken);

                    if (shipmentReceiptTransIds.Count == 0)
                    {
                        result.Skipped.Add($"{certificate.CertificateNumber}: no posted SHIPMENT_RECEIPT found.");
                        if (ownsTransaction) { await transaction!.CommitAsync(cancellationToken); committed = true; }
                        continue;
                    }

                    // 2. Must also have a posted PURCHASE_INVOICE — otherwise the receipt's credit is not
                    //    duplicated by anything and must be left alone.
                    var hasPostedInvoice = await _context.AcctgTrans.AnyAsync(t =>
                            t.WorkEffortId == certificate.WorkEffortId &&
                            t.AcctgTransTypeId == "PURCHASE_INVOICE" &&
                            t.IsPosted == "Y",
                        cancellationToken);

                    if (!hasPostedInvoice)
                    {
                        result.Skipped.Add(
                            $"{certificate.CertificateNumber}: no posted PURCHASE_INVOICE found — receipt is not duplicated.");
                        if (ownsTransaction) { await transaction!.CommitAsync(cancellationToken); committed = true; }
                        continue;
                    }

                    // 3. Idempotency — don't double-correct if this has already run for this certificate.
                    var alreadyCorrected = await _context.AcctgTrans.AnyAsync(t =>
                            t.WorkEffortId == certificate.WorkEffortId &&
                            t.AcctgTransTypeId == "INTERNAL_ACCTG_TRANS" &&
                            t.Description == CorrectionMarker,
                        cancellationToken);

                    if (alreadyCorrected)
                    {
                        result.Skipped.Add($"{certificate.CertificateNumber}: correction already posted.");
                        if (ownsTransaction) { await transaction!.CommitAsync(cancellationToken); committed = true; }
                        continue;
                    }

                    // 4. Sum exactly what the SHIPMENT_RECEIPT(s) credited to ACCOUNTS_PAYABLE — this,
                    //    not the invoice total (which can include tax/variance the receipt never saw), is
                    //    the amount that needs to move off the vendor's payable.
                    var duplicatedAmount = await _context.AcctgTransEntries
                        .Where(e => shipmentReceiptTransIds.Contains(e.AcctgTransId)
                                    && e.DebitCreditFlag == "C"
                                    && e.GlAccountTypeId == "ACCOUNTS_PAYABLE")
                        .SumAsync(e => (decimal?)e.Amount, cancellationToken) ?? 0m;

                    if (duplicatedAmount <= 0)
                    {
                        result.Skipped.Add(
                            $"{certificate.CertificateNumber}: no ACCOUNTS_PAYABLE credit found on its receipt(s).");
                        if (ownsTransaction) { await transaction!.CommitAsync(cancellationToken); committed = true; }
                        continue;
                    }

                    // 5. Post the reclass: Dr ACCOUNTS_PAYABLE (moves the receipt-time credit off the
                    //    vendor's ledger) / Cr UNINVOICED_SHIP_RCPT (where it should have posted). The
                    //    invoice's own ACCOUNTS_PAYABLE credit is left standing, untouched.
                    var entries = new List<AcctgTransEntry>
                    {
                        new()
                        {
                            AcctgTransEntrySeqId = "1",
                            AcctgTransEntryTypeId = "_NA_",
                            DebitCreditFlag = "D",
                            OrganizationPartyId = "Company",
                            GlAccountTypeId = "ACCOUNTS_PAYABLE",
                            PartyId = certificate.PartyIdSupplier,
                            RoleTypeId = "BILL_FROM_VENDOR",
                            OrigAmount = duplicatedAmount,
                            ReconcileStatusId = "AES_NOT_RECONCILED",
                            Description = EntryDisplayDescription
                        },
                        new()
                        {
                            AcctgTransEntrySeqId = "2",
                            AcctgTransEntryTypeId = "_NA_",
                            DebitCreditFlag = "C",
                            OrganizationPartyId = "Company",
                            GlAccountTypeId = "UNINVOICED_SHIP_RCPT",
                            PartyId = certificate.PartyIdSupplier,
                            RoleTypeId = "BILL_FROM_VENDOR",
                            OrigAmount = duplicatedAmount,
                            ReconcileStatusId = "AES_NOT_RECONCILED",
                            Description = EntryDisplayDescription
                        }
                    };

                    var acctgTransId = await _generalLedgerService.CreateAcctgTransAndEntries(
                        new CreateAcctgTransAndEntriesParams
                        {
                            GlFiscalTypeId = "ACTUAL",
                            AcctgTransTypeId = "INTERNAL_ACCTG_TRANS",
                            Description = CorrectionMarker,
                            PartyId = certificate.PartyIdSupplier,
                            RoleTypeId = "BILL_FROM_VENDOR",
                            WorkEffortId = certificate.WorkEffortId,
                            TransactionDate = DateOnly.FromDateTime(DateTime.UtcNow),
                            AcctgTransEntries = entries
                        });

                    if (string.IsNullOrEmpty(acctgTransId))
                    {
                        result.Errors.Add($"{certificate.CertificateNumber}: correcting entry was not created.");
                        if (ownsTransaction) await transaction!.RollbackAsync(cancellationToken);
                        continue;
                    }

                    var saved = await _context.SaveChangesAsync(cancellationToken);
                    if (saved == 0)
                    {
                        result.Errors.Add(
                            $"{certificate.CertificateNumber}: SaveChangesAsync persisted zero changes.");
                        if (ownsTransaction) await transaction!.RollbackAsync(cancellationToken);
                        continue;
                    }

                    if (ownsTransaction)
                    {
                        await transaction!.CommitAsync(cancellationToken);
                        committed = true;
                    }

                    result.Corrected.Add(
                        $"{certificate.CertificateNumber} ({certificate.PartyIdSupplier}): {duplicatedAmount:N2} reclassed via {acctgTransId}.");
                }
                catch (Exception ex)
                {
                    if (ownsTransaction && transaction != null && !committed)
                        await transaction.RollbackAsync(cancellationToken);
                    _logger.LogError(ex, "Duplicate-AP correction failed for certificate {WorkEffortId}",
                        certificate.WorkEffortId);
                    result.Errors.Add($"{certificate.CertificateNumber}: {ex.Message}");
                }
                finally
                {
                    if (ownsTransaction && transaction != null) await transaction.DisposeAsync();
                }
            }

            return Result<RemediateSupplyCertificateDuplicateApResult>.Success(result);
        }
    }
}
