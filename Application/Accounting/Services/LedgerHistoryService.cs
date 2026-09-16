using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Accounting.Services;

/// <summary>What still refers to an artifact in the ledger and the bank book.</summary>
public class LedgerHistory
{
    public int AcctgTransCount { get; set; }
    public int PostedAcctgTransCount { get; set; }
    public int FinAccountTransCount { get; set; }

    /// <summary>Bank transactions that are not cancelled; a cancelled one is history, not a live reference.</summary>
    public int LiveFinAccountTransCount { get; set; }

    public bool IsEmpty => AcctgTransCount == 0 && FinAccountTransCount == 0;

    /// <summary>True when the artifact has ever touched the ledger or the bank book: the auditor's boundary for "may not be physically deleted".</summary>
    public bool HasHistory => !IsEmpty;
}

/// <summary>
/// The "has ledger history" refusal generalised from <c>DeleteParty</c>, which was the one handler
/// already refusing to delete a party with accounting transactions. Every physical delete of a
/// financial artifact calls <see cref="EnsureNoHistoryAsync"/> first; anything with history is
/// voided or reversed instead.
/// </summary>
public interface ILedgerHistoryService
{
    Task<LedgerHistory> ForPaymentAsync(string paymentId, CancellationToken ct = default);
    Task<LedgerHistory> ForPaymentsAsync(IEnumerable<string> paymentIds, CancellationToken ct = default);
    Task<LedgerHistory> ForInvoiceAsync(string invoiceId, CancellationToken ct = default);
    Task<LedgerHistory> ForInvoicesAsync(IEnumerable<string> invoiceIds, CancellationToken ct = default);
    Task<LedgerHistory> ForWorkEffortAsync(string workEffortId, CancellationToken ct = default);
    Task<LedgerHistory> ForSalesRequestAsync(string salesRequestId, CancellationToken ct = default);

    /// <summary>Throws <see cref="LedgerHistoryExistsException"/> when the history is not empty.</summary>
    void EnsureNoHistory(LedgerHistory history, string artifactLabel);
}

public class LedgerHistoryExistsException : InvalidOperationException
{
    public LedgerHistory History { get; }

    public LedgerHistoryExistsException(LedgerHistory history, string artifactLabel)
        : base(
            $"لا يمكن حذف {artifactLabel} لوجود قيود محاسبية أو حركات بنكية مسجلة عليه " +
            $"({history.AcctgTransCount} قيد، {history.FinAccountTransCount} حركة بنكية). " +
            $"استخدم الإلغاء أو العكس بدلاً من الحذف. " +
            $"(Cannot delete {artifactLabel}: it has ledger history. Void or reverse it instead.)")
    {
        History = history;
    }
}

public class LedgerHistoryService : ILedgerHistoryService
{
    private readonly DataContext _context;

    public LedgerHistoryService(DataContext context)
    {
        _context = context;
    }

    public Task<LedgerHistory> ForPaymentAsync(string paymentId, CancellationToken ct = default)
        => ForPaymentsAsync(new[] { paymentId }, ct);

    public async Task<LedgerHistory> ForPaymentsAsync(IEnumerable<string> paymentIds, CancellationToken ct = default)
    {
        var ids = paymentIds.Where(i => !string.IsNullOrEmpty(i)).Distinct().ToList();
        var history = new LedgerHistory();
        if (ids.Count == 0) return history;

        var finTrans = await _context.FinAccountTrans
            .AsNoTracking()
            .Where(f => f.PaymentId != null && ids.Contains(f.PaymentId))
            .Select(f => new { f.FinAccountTransId, f.StatusId })
            .ToListAsync(ct);
        var finTransIds = finTrans.Select(f => f.FinAccountTransId).ToList();

        history.FinAccountTransCount = finTrans.Count;
        history.LiveFinAccountTransCount = finTrans.Count(f => f.StatusId != "FINACT_TRNS_CANCELED");

        var trans = await _context.AcctgTrans
            .AsNoTracking()
            .Where(t => (t.PaymentId != null && ids.Contains(t.PaymentId)) ||
                        (t.FinAccountTransId != null && finTransIds.Contains(t.FinAccountTransId)))
            .Select(t => t.IsPosted)
            .ToListAsync(ct);

        history.AcctgTransCount = trans.Count;
        history.PostedAcctgTransCount = trans.Count(p => p == "Y");
        return history;
    }

    public Task<LedgerHistory> ForInvoiceAsync(string invoiceId, CancellationToken ct = default)
        => ForInvoicesAsync(new[] { invoiceId }, ct);

    public async Task<LedgerHistory> ForInvoicesAsync(IEnumerable<string> invoiceIds, CancellationToken ct = default)
    {
        var ids = invoiceIds.Where(i => !string.IsNullOrEmpty(i)).Distinct().ToList();
        var history = new LedgerHistory();
        if (ids.Count == 0) return history;

        var trans = await _context.AcctgTrans
            .AsNoTracking()
            .Where(t => t.InvoiceId != null && ids.Contains(t.InvoiceId))
            .Select(t => t.IsPosted)
            .ToListAsync(ct);

        history.AcctgTransCount = trans.Count;
        history.PostedAcctgTransCount = trans.Count(p => p == "Y");
        return history;
    }

    public async Task<LedgerHistory> ForWorkEffortAsync(string workEffortId, CancellationToken ct = default)
    {
        var history = new LedgerHistory();
        var trans = await _context.AcctgTrans
            .AsNoTracking()
            .Where(t => t.WorkEffortId == workEffortId)
            .Select(t => t.IsPosted)
            .ToListAsync(ct);

        history.AcctgTransCount = trans.Count;
        history.PostedAcctgTransCount = trans.Count(p => p == "Y");
        return history;
    }

    public async Task<LedgerHistory> ForSalesRequestAsync(string salesRequestId, CancellationToken ct = default)
    {
        var history = new LedgerHistory();

        var paymentIds = await _context.Payments
            .AsNoTracking()
            .Where(p => p.SalesRequestId == salesRequestId)
            .Select(p => p.PaymentId)
            .ToListAsync(ct);

        var viaPayments = await ForPaymentsAsync(paymentIds, ct);

        var direct = await _context.AcctgTrans
            .AsNoTracking()
            .Where(t => t.SalesRequestId == salesRequestId)
            .Select(t => t.IsPosted)
            .ToListAsync(ct);

        history.AcctgTransCount = viaPayments.AcctgTransCount + direct.Count;
        history.PostedAcctgTransCount = viaPayments.PostedAcctgTransCount + direct.Count(p => p == "Y");
        history.FinAccountTransCount = viaPayments.FinAccountTransCount;
        history.LiveFinAccountTransCount = viaPayments.LiveFinAccountTransCount;
        return history;
    }

    public void EnsureNoHistory(LedgerHistory history, string artifactLabel)
    {
        if (history.HasHistory) throw new LedgerHistoryExistsException(history, artifactLabel);
    }
}
