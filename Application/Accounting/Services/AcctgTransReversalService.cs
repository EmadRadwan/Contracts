using Application.Accounting.Services.Models;
using Application.Core;
using Application.Interfaces;
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Accounting.Services;

/// <summary>Attribute names that link a reversal to its original on ACCTG_TRANS_ATTRIBUTE (no schema change).</summary>
public static class AcctgTransReversal
{
    /// <summary>On the reversal: the id of the transaction it cancels.</summary>
    public const string ReversalOf = "REVERSAL_OF";

    /// <summary>On the original: the id of the reversal that cancelled it. Presence means "already reversed".</summary>
    public const string ReversedBy = "REVERSED_BY";

    /// <summary>On the reversal: free-text reason supplied by the caller.</summary>
    public const string ReversalReason = "REVERSAL_REASON";
}

public class ReverseAcctgTransOptions
{
    /// <summary>Why the transaction is being reversed. Stored on the reversal and prefixed into its description.</summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// Date of the contra transaction. When null the original's own date is used if its period is
    /// still open (keeps the period's totals net zero), otherwise today. Either way the date must
    /// fall in an open period or the reversal is refused.
    /// </summary>
    public DateOnly? ReversalDate { get; set; }
}

public class ReversedAcctgTrans
{
    public string OriginalAcctgTransId { get; set; } = null!;
    public string? ReversalAcctgTransId { get; set; }

    /// <summary>True when the original already carried a REVERSED_BY link and was left alone.</summary>
    public bool SkippedAlreadyReversed { get; set; }
}

/// <summary>
/// The reversal primitive from the September 2026 report. A posted transaction is never deleted;
/// it is cancelled by a linked contra transaction with debits and credits swapped, and both stay
/// visible in the ledger.
///
/// Compared with the older <c>CopyAcctgTransAndEntries(id, revert: true)</c>: the link is recorded
/// in both directions, the reason is kept, every entry field is carried (party, organisation,
/// currency, description), the date is chosen deliberately, the period guard runs, and a
/// transaction cannot be reversed twice.
///
/// Rows are added to the DbContext; the caller owns SaveChanges and the surrounding transaction,
/// matching <see cref="IAcctgTransService.CreateAcctgTrans"/>.
/// </summary>
public interface IAcctgTransReversalService
{
    Task<GeneralServiceResult<string>> ReverseAsync(string acctgTransId, ReverseAcctgTransOptions options,
        CancellationToken ct = default);

    /// <summary>
    /// Reverses every transaction in the set. Already-reversed transactions are skipped and reported,
    /// not treated as errors, so a void that is retried after a partial failure completes cleanly.
    /// </summary>
    Task<GeneralServiceResult<List<ReversedAcctgTrans>>> ReverseManyAsync(IEnumerable<string> acctgTransIds,
        ReverseAcctgTransOptions options, CancellationToken ct = default);

    Task<bool> IsReversedAsync(string acctgTransId, CancellationToken ct = default);

    /// <summary>Ids of posted, not-yet-reversed transactions anchored to the payment (by payment id or its bank transactions).</summary>
    Task<List<string>> FindReversibleForPaymentAsync(string paymentId, CancellationToken ct = default);

    Task<List<string>> FindReversibleForInvoiceAsync(string invoiceId, CancellationToken ct = default);

    Task<List<string>> FindReversibleForWorkEffortAsync(string workEffortId, CancellationToken ct = default);

    Task<List<string>> FindReversibleForSalesRequestAsync(string salesRequestId, CancellationToken ct = default);
}

public class AcctgTransReversalService : IAcctgTransReversalService
{
    private readonly DataContext _context;
    private readonly IAcctgTransService _acctgTransService;
    private readonly IAccountingPeriodGuard _periodGuard;
    private readonly IUserAccessor _userAccessor;
    private readonly ILogger<AcctgTransReversalService> _logger;

    public AcctgTransReversalService(DataContext context, IAcctgTransService acctgTransService,
        IAccountingPeriodGuard periodGuard, IUserAccessor userAccessor, ILogger<AcctgTransReversalService> logger)
    {
        _context = context;
        _acctgTransService = acctgTransService;
        _periodGuard = periodGuard;
        _userAccessor = userAccessor;
        _logger = logger;
    }

    public async Task<GeneralServiceResult<string>> ReverseAsync(string acctgTransId, ReverseAcctgTransOptions options,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(acctgTransId))
            return GeneralServiceResult<string>.Error("acctgTransId is required.");

        var original = await _context.AcctgTrans
            .Include(t => t.AcctgTransEntries)
            .FirstOrDefaultAsync(t => t.AcctgTransId == acctgTransId, ct);

        if (original == null)
            return GeneralServiceResult<string>.Error($"القيد {acctgTransId} غير موجود. (Accounting transaction not found.)");

        if (original.IsPosted != "Y")
            return GeneralServiceResult<string>.Error(
                $"القيد {acctgTransId} غير مرحّل؛ القيود غير المرحّلة تُحذف ولا تُعكس. " +
                $"(Transaction {acctgTransId} is not posted; unposted transactions are deleted, not reversed.)");

        if (original.AcctgTransEntries == null || original.AcctgTransEntries.Count == 0)
            return GeneralServiceResult<string>.Error(
                $"القيد {acctgTransId} لا يحتوي على بنود. (Transaction {acctgTransId} has no entries.)");

        if (await IsReversedAsync(acctgTransId, ct))
        {
            var by = await GetAttributeAsync(acctgTransId, AcctgTransReversal.ReversedBy, ct);
            return GeneralServiceResult<string>.Error(
                $"القيد {acctgTransId} سبق عكسه بالقيد {by}. (Transaction {acctgTransId} was already reversed by {by}.)");
        }

        // A reversal is closed history too. Reversing it would re-open the original by the back
        // door with no trace on the original itself; re-opening is done by re-posting.
        var reversalOf = await GetAttributeAsync(acctgTransId, AcctgTransReversal.ReversalOf, ct);
        if (!string.IsNullOrEmpty(reversalOf))
        {
            return GeneralServiceResult<string>.Error(
                $"القيد {acctgTransId} هو قيد عكسي للقيد {reversalOf} ولا يمكن عكسه. لإعادة القيد الأصلي، أعد ترحيله. " +
                $"(Transaction {acctgTransId} is itself a reversal of {reversalOf} and cannot be reversed; re-post instead.)");
        }

        // ----- Date: original's date while its period is open, else today; always guarded -----
        var originalDate = original.TransactionDate?.Date ?? DateTime.UtcNow.Date;
        DateTime reversalDate;
        if (options.ReversalDate.HasValue)
        {
            reversalDate = options.ReversalDate.Value.ToStartOfDay();
        }
        else
        {
            var originalClosed = await _periodGuard.CheckAsync(originalDate, null, ct);
            reversalDate = originalClosed == null ? originalDate : DateTime.UtcNow.Date;
        }

        // Evidence in a closed period cannot be cancelled from inside that period either way:
        // the reversal date itself must be open.
        await _periodGuard.EnsureOpenAsync(reversalDate, null, ct);

        var reason = string.IsNullOrWhiteSpace(options.Reason) ? "Reversal" : options.Reason.Trim();
        var stamp = DateTime.UtcNow;
        var user = SafeUsername();

        // ----- Header -----
        var headerParams = new CreateAcctgTransParams
        {
            AcctgTransTypeId = original.AcctgTransTypeId,
            GlFiscalTypeId = original.GlFiscalTypeId,
            IsPosted = "Y",
            PostedDate = stamp,
            TransactionDate = DateOnly.FromDateTime(reversalDate),
            Description = BuildDescription(original, reason),
            InvoiceId = original.InvoiceId,
            PaymentId = original.PaymentId,
            FinAccountTransId = original.FinAccountTransId,
            SalesRequestId = original.SalesRequestId,
            WorkEffortId = original.WorkEffortId,
            ShipmentId = original.ShipmentId,
            ReceiptId = original.ReceiptId,
            PartyId = original.PartyId,
            RoleTypeId = original.RoleTypeId,
            CostCenterId = original.CostCenterId,
            FixedAssetId = original.FixedAssetId,
            InventoryItemId = original.InventoryItemId,
            PhysicalInventoryId = original.PhysicalInventoryId,
            GlJournalId = original.GlJournalId,
            VoucherRef = original.VoucherRef,
            VoucherDate = original.VoucherDate,
            GroupStatusId = original.GroupStatusId,
            TheirAcctgTransId = original.TheirAcctgTransId,
            CreatedByUserLogin = user,
            LastModifiedByUserLogin = user
        };

        var reversalId = await _acctgTransService.CreateAcctgTrans(headerParams);
        if (string.IsNullOrEmpty(reversalId))
            return GeneralServiceResult<string>.Error("Failed to create the reversing transaction header.");

        // CreateAcctgTrans leaves CreatedByUserLogin null; stamp the tracked entity so the reversal names its author.
        var tracked = _context.AcctgTrans.Local.FirstOrDefault(t => t.AcctgTransId == reversalId);
        if (tracked != null)
        {
            tracked.CreatedByUserLogin = user;
            tracked.LastModifiedByUserLogin = user;
        }

        // ----- Entries: every field carried, only the side flips -----
        decimal debits = 0m, credits = 0m;
        foreach (var e in original.AcctgTransEntries.OrderBy(e => e.AcctgTransEntrySeqId))
        {
            var flag = e.DebitCreditFlag == "D" ? "C" : "D";
            var amount = e.Amount ?? 0m;
            if (flag == "D") debits += amount; else credits += amount;

            await _acctgTransService.CreateAcctgTransEntry(new AcctgTransEntry
            {
                AcctgTransId = reversalId,
                AcctgTransEntrySeqId = e.AcctgTransEntrySeqId,
                AcctgTransEntryTypeId = e.AcctgTransEntryTypeId,
                Description = e.Description,
                VoucherRef = e.VoucherRef,
                PartyId = e.PartyId,
                RoleTypeId = e.RoleTypeId,
                TheirPartyId = e.TheirPartyId,
                ProductId = e.ProductId,
                TheirProductId = e.TheirProductId,
                InventoryItemId = e.InventoryItemId,
                GlAccountTypeId = e.GlAccountTypeId,
                GlAccountId = e.GlAccountId,
                OrganizationPartyId = e.OrganizationPartyId,
                Amount = e.Amount,
                CurrencyUomId = e.CurrencyUomId,
                OrigAmount = e.OrigAmount,
                OrigCurrencyUomId = e.OrigCurrencyUomId,
                DebitCreditFlag = flag,
                DueDate = e.DueDate,
                GroupId = e.GroupId,
                TaxId = e.TaxId,
                ReconcileStatusId = "AES_NOT_RECONCILED",
                SettlementTermId = e.SettlementTermId,
                IsSummary = e.IsSummary,
                CreatedStamp = stamp,
                LastUpdatedStamp = stamp
            });
        }

        // A reversal of a balanced transaction is balanced by construction; an unbalanced original
        // (the payroll triple-booking class of bug) must not be propagated silently.
        if (Math.Abs(debits - credits) >= 0.01m)
        {
            throw new InvalidOperationException(
                $"Reversal of {acctgTransId} would be unbalanced (debits {debits:N2} vs credits {credits:N2}). " +
                "The original transaction is unbalanced; correct it by hand before reversing.");
        }

        // ----- Links, both directions -----
        _context.AcctgTransAttributes.AddRange(
            Attr(reversalId, AcctgTransReversal.ReversalOf, acctgTransId, "Reverses this transaction", stamp),
            Attr(reversalId, AcctgTransReversal.ReversalReason, Truncate(reason, 255), null, stamp),
            Attr(acctgTransId, AcctgTransReversal.ReversedBy, reversalId, "Cancelled by this reversal", stamp));

        original.LastModifiedDate = stamp;
        original.LastModifiedByUserLogin = user;
        original.LastUpdatedStamp = stamp;

        _logger.LogInformation("Reversed AcctgTrans {Original} with {Reversal} dated {Date:yyyy-MM-dd} ({Reason})",
            acctgTransId, reversalId, reversalDate, reason);

        return GeneralServiceResult<string>.Success(reversalId);
    }

    public async Task<GeneralServiceResult<List<ReversedAcctgTrans>>> ReverseManyAsync(
        IEnumerable<string> acctgTransIds, ReverseAcctgTransOptions options, CancellationToken ct = default)
    {
        var results = new List<ReversedAcctgTrans>();
        foreach (var id in acctgTransIds.Where(i => !string.IsNullOrEmpty(i)).Distinct())
        {
            if (await IsReversedAsync(id, ct))
            {
                results.Add(new ReversedAcctgTrans { OriginalAcctgTransId = id, SkippedAlreadyReversed = true });
                continue;
            }

            var r = await ReverseAsync(id, options, ct);
            if (!r.IsSuccess)
                return GeneralServiceResult<List<ReversedAcctgTrans>>.Error(r.ErrorMessage);

            results.Add(new ReversedAcctgTrans { OriginalAcctgTransId = id, ReversalAcctgTransId = r.ResultData });
        }

        return GeneralServiceResult<List<ReversedAcctgTrans>>.Success(results);
    }

    public async Task<bool> IsReversedAsync(string acctgTransId, CancellationToken ct = default)
    {
        // Check the tracker too, so a set reversed inside one unit of work cannot double up.
        if (_context.AcctgTransAttributes.Local.Any(a =>
                a.AcctgTransId == acctgTransId && a.AttrName == AcctgTransReversal.ReversedBy))
            return true;

        return await _context.AcctgTransAttributes
            .AnyAsync(a => a.AcctgTransId == acctgTransId && a.AttrName == AcctgTransReversal.ReversedBy, ct);
    }

    public async Task<List<string>> FindReversibleForPaymentAsync(string paymentId, CancellationToken ct = default)
    {
        var finAccountTransIds = await _context.FinAccountTrans
            .Where(f => f.PaymentId == paymentId)
            .Select(f => f.FinAccountTransId)
            .ToListAsync(ct);

        return await ReversibleWhere(t =>
            t.PaymentId == paymentId ||
            (t.FinAccountTransId != null && finAccountTransIds.Contains(t.FinAccountTransId)), ct);
    }

    public Task<List<string>> FindReversibleForInvoiceAsync(string invoiceId, CancellationToken ct = default)
        => ReversibleWhere(t => t.InvoiceId == invoiceId, ct);

    public Task<List<string>> FindReversibleForWorkEffortAsync(string workEffortId, CancellationToken ct = default)
        => ReversibleWhere(t => t.WorkEffortId == workEffortId, ct);

    public Task<List<string>> FindReversibleForSalesRequestAsync(string salesRequestId, CancellationToken ct = default)
        => ReversibleWhere(t => t.SalesRequestId == salesRequestId, ct);

    // ----- helpers -----

    private async Task<List<string>> ReversibleWhere(
        System.Linq.Expressions.Expression<Func<AcctgTran, bool>> anchor, CancellationToken ct)
    {
        var reversalMarkers = _context.AcctgTransAttributes
            .Where(a => a.AttrName == AcctgTransReversal.ReversedBy || a.AttrName == AcctgTransReversal.ReversalOf)
            .Select(a => a.AcctgTransId);

        // Posted, not already reversed, and not itself a reversal (reversing a reversal re-opens
        // the original by the back door; that is a deliberate act, not a side effect of a void).
        return await _context.AcctgTrans
            .Where(anchor)
            .Where(t => t.IsPosted == "Y" && !reversalMarkers.Contains(t.AcctgTransId))
            .OrderBy(t => t.AcctgTransId)
            .Select(t => t.AcctgTransId)
            .ToListAsync(ct);
    }

    private async Task<string?> GetAttributeAsync(string acctgTransId, string attrName, CancellationToken ct)
    {
        var local = _context.AcctgTransAttributes.Local
            .FirstOrDefault(a => a.AcctgTransId == acctgTransId && a.AttrName == attrName);
        if (local != null) return local.AttrValue;

        return await _context.AcctgTransAttributes
            .Where(a => a.AcctgTransId == acctgTransId && a.AttrName == attrName)
            .Select(a => a.AttrValue)
            .FirstOrDefaultAsync(ct);
    }

    private static AcctgTransAttribute Attr(string acctgTransId, string name, string? value, string? description,
        DateTime stamp) => new()
    {
        AcctgTransId = acctgTransId,
        AttrName = name,
        AttrValue = value,
        AttrDescription = description,
        CreatedStamp = stamp,
        LastUpdatedStamp = stamp
    };

    private static string BuildDescription(AcctgTran original, string reason)
    {
        var text = $"Reversal of {original.AcctgTransId} ({reason})";
        if (!string.IsNullOrWhiteSpace(original.Description))
            text += $" - {original.Description}";
        return Truncate(text, 1500);
    }

    private static string Truncate(string s, int max) => s.Length <= max ? s : s[..max];

    private string? SafeUsername()
    {
        try { return _userAccessor.GetUsername(); }
        catch { return null; } // background jobs and tests have no HttpContext
    }
}
