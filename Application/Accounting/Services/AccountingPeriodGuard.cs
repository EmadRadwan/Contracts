using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Accounting.Services;

/// <summary>
/// Thrown when a write would land in, or remove evidence from, a closed accounting period.
/// Handlers that return Result<T> catch this and surface <see cref="Exception.Message"/>.
/// </summary>
public class ClosedAccountingPeriodException : InvalidOperationException
{
    public string CustomTimePeriodId { get; }
    public string? PeriodName { get; }
    public DateTime TransactionDate { get; }

    public ClosedAccountingPeriodException(string customTimePeriodId, string? periodName, DateTime transactionDate)
        : base(
            $"لا يمكن التعديل على قيود بتاريخ {transactionDate:yyyy-MM-dd} لأن الفترة المحاسبية " +
            $"{(string.IsNullOrWhiteSpace(periodName) ? customTimePeriodId : periodName)} مقفلة. " +
            $"سجّل التصحيح بتاريخ يقع في فترة مفتوحة. " +
            $"(Accounting period {customTimePeriodId} covering {transactionDate:yyyy-MM-dd} is closed.)")
    {
        CustomTimePeriodId = customTimePeriodId;
        PeriodName = periodName;
        TransactionDate = transactionDate;
    }
}

/// <summary>
/// The period control the September 2026 report found missing: the only period check in the
/// codebase lived in <c>PostAcctgTrans</c>, appended a warning instead of blocking, and was
/// bypassed because <c>CreateAcctgTransAndEntries</c> stamps IS_POSTED='Y' at creation.
///
/// This guard THROWS. It is called from the single creation choke point
/// (<see cref="AcctgTransService.CreateAcctgTrans"/>), from the reversal service, and from every
/// handler that deletes or resets posted ledger rows — so a closed period is closed on the way
/// in and on the way out.
///
/// A date with no defined period is allowed through (only closed periods block); defining
/// periods for a new year must never take the system down.
/// </summary>
public interface IAccountingPeriodGuard
{
    /// <summary>Throws <see cref="ClosedAccountingPeriodException"/> if any fiscal period covering the date is closed.</summary>
    Task EnsureOpenAsync(DateTime transactionDate, string? organizationPartyId = null, CancellationToken ct = default);

    /// <summary>Non-throwing form of <see cref="EnsureOpenAsync"/>; null when open.</summary>
    Task<ClosedAccountingPeriodException?> CheckAsync(DateTime transactionDate, string? organizationPartyId = null, CancellationToken ct = default);

    /// <summary>Guards the transaction dates of the given accounting transactions (used before deleting or reversing them).</summary>
    Task EnsureOpenForAcctgTransAsync(IEnumerable<string> acctgTransIds, CancellationToken ct = default);

    /// <summary>Guards every posted transaction anchored to the payment (by payment id or by its bank transactions).</summary>
    Task EnsureOpenForPaymentsAsync(IEnumerable<string> paymentIds, CancellationToken ct = default);

    Task EnsureOpenForInvoicesAsync(IEnumerable<string> invoiceIds, CancellationToken ct = default);

    Task EnsureOpenForWorkEffortsAsync(IEnumerable<string> workEffortIds, CancellationToken ct = default);

    Task EnsureOpenForSalesRequestsAsync(IEnumerable<string> salesRequestIds, CancellationToken ct = default);
}

public class AccountingPeriodGuard : IAccountingPeriodGuard
{
    /// <summary>Same list PostAcctgTrans uses; other period types (e.g. sales forecasts) never close the ledger.</summary>
    public static readonly string[] FiscalPeriodTypes =
    {
        "FISCAL_YEAR", "FISCAL_QUARTER", "FISCAL_MONTH", "FISCAL_WEEK", "FISCAL_BIWEEK"
    };

    private readonly DataContext _context;

    public AccountingPeriodGuard(DataContext context)
    {
        _context = context;
    }

    public async Task<ClosedAccountingPeriodException?> CheckAsync(DateTime transactionDate,
        string? organizationPartyId = null, CancellationToken ct = default)
    {
        var date = transactionDate.Date;

        var query = _context.CustomTimePeriods
            .AsNoTracking()
            .Where(p => p.IsClosed == "Y"
                        && FiscalPeriodTypes.Contains(p.PeriodTypeId)
                        && p.FromDate <= date
                        && (p.ThruDate == null || p.ThruDate >= date));

        // When an organization is named, periods of that organization plus organization-less
        // periods apply. When none is named, any closed period covering the date blocks —
        // the safer reading for a single-organization ledger.
        if (!string.IsNullOrEmpty(organizationPartyId))
        {
            query = query.Where(p => p.OrganizationPartyId == organizationPartyId
                                     || p.OrganizationPartyId == null
                                     || p.OrganizationPartyId == "_NA_");
        }

        var closed = await query
            .OrderBy(p => p.FromDate)
            .Select(p => new { p.CustomTimePeriodId, p.PeriodName })
            .FirstOrDefaultAsync(ct);

        return closed == null
            ? null
            : new ClosedAccountingPeriodException(closed.CustomTimePeriodId, closed.PeriodName, date);
    }

    public async Task EnsureOpenAsync(DateTime transactionDate, string? organizationPartyId = null,
        CancellationToken ct = default)
    {
        var violation = await CheckAsync(transactionDate, organizationPartyId, ct);
        if (violation != null) throw violation;
    }

    public async Task EnsureOpenForAcctgTransAsync(IEnumerable<string> acctgTransIds, CancellationToken ct = default)
    {
        var ids = acctgTransIds.Where(id => !string.IsNullOrEmpty(id)).Distinct().ToList();
        if (ids.Count == 0) return;

        var dates = await _context.AcctgTrans
            .AsNoTracking()
            .Where(t => ids.Contains(t.AcctgTransId) && t.TransactionDate != null)
            .Select(t => t.TransactionDate!.Value)
            .Distinct()
            .ToListAsync(ct);

        await EnsureAllOpenAsync(dates, ct);
    }

    public async Task EnsureOpenForPaymentsAsync(IEnumerable<string> paymentIds, CancellationToken ct = default)
    {
        var ids = paymentIds.Where(id => !string.IsNullOrEmpty(id)).Distinct().ToList();
        if (ids.Count == 0) return;

        var finAccountTransIds = await _context.FinAccountTrans
            .AsNoTracking()
            .Where(f => f.PaymentId != null && ids.Contains(f.PaymentId))
            .Select(f => f.FinAccountTransId)
            .ToListAsync(ct);

        var dates = await _context.AcctgTrans
            .AsNoTracking()
            .Where(t => t.TransactionDate != null &&
                        ((t.PaymentId != null && ids.Contains(t.PaymentId)) ||
                         (t.FinAccountTransId != null && finAccountTransIds.Contains(t.FinAccountTransId))))
            .Select(t => t.TransactionDate!.Value)
            .Distinct()
            .ToListAsync(ct);

        await EnsureAllOpenAsync(dates, ct);
    }

    public async Task EnsureOpenForInvoicesAsync(IEnumerable<string> invoiceIds, CancellationToken ct = default)
    {
        var ids = invoiceIds.Where(id => !string.IsNullOrEmpty(id)).Distinct().ToList();
        if (ids.Count == 0) return;

        var dates = await _context.AcctgTrans
            .AsNoTracking()
            .Where(t => t.TransactionDate != null && t.InvoiceId != null && ids.Contains(t.InvoiceId))
            .Select(t => t.TransactionDate!.Value)
            .Distinct()
            .ToListAsync(ct);

        await EnsureAllOpenAsync(dates, ct);
    }

    public async Task EnsureOpenForWorkEffortsAsync(IEnumerable<string> workEffortIds, CancellationToken ct = default)
    {
        var ids = workEffortIds.Where(id => !string.IsNullOrEmpty(id)).Distinct().ToList();
        if (ids.Count == 0) return;

        var dates = await _context.AcctgTrans
            .AsNoTracking()
            .Where(t => t.TransactionDate != null && t.WorkEffortId != null && ids.Contains(t.WorkEffortId))
            .Select(t => t.TransactionDate!.Value)
            .Distinct()
            .ToListAsync(ct);

        await EnsureAllOpenAsync(dates, ct);
    }

    public async Task EnsureOpenForSalesRequestsAsync(IEnumerable<string> salesRequestIds, CancellationToken ct = default)
    {
        var ids = salesRequestIds.Where(id => !string.IsNullOrEmpty(id)).Distinct().ToList();
        if (ids.Count == 0) return;

        var dates = await _context.AcctgTrans
            .AsNoTracking()
            .Where(t => t.TransactionDate != null && t.SalesRequestId != null && ids.Contains(t.SalesRequestId))
            .Select(t => t.TransactionDate!.Value)
            .Distinct()
            .ToListAsync(ct);

        await EnsureAllOpenAsync(dates, ct);
    }

    private async Task EnsureAllOpenAsync(IEnumerable<DateTime> dates, CancellationToken ct)
    {
        // Distinct by day: one query per distinct date, and there are rarely more than a handful.
        foreach (var date in dates.Select(d => d.Date).Distinct().OrderBy(d => d))
        {
            await EnsureOpenAsync(date, null, ct);
        }
    }
}
