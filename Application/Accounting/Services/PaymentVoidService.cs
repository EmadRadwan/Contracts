using Application.Core;
using Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Accounting.Services;

public class PaymentVoidOutcome
{
    public string PaymentId { get; set; } = null!;
    public string PreviousStatusId { get; set; } = null!;
    public string StatusId { get; set; } = "PMNT_VOID";
    public List<ReversedAcctgTrans> ReversedTransactions { get; set; } = new();
    public int RemovedPaymentApplications { get; set; }
    public int CancelledFinAccountTrans { get; set; }
}

/// <summary>
/// The void primitive. A payment that should never have existed is not deleted: its status
/// becomes PMNT_VOID, every posted transaction it produced is reversed by a linked contra
/// transaction, its bank transactions are cancelled (kept, excluded from balances), and its
/// applications to invoices are removed so those invoices return to READY. The row stays.
///
/// Supersedes the OFBiz-port <c>PaymentHelperService.VoidPayment</c>, which reversed only
/// transactions with no invoice id, ignored bank transactions, and threw away the reversal id.
/// That method now delegates here.
///
/// Rows are changed on the DbContext; the caller owns SaveChanges and the surrounding transaction.
/// </summary>
public interface IPaymentVoidService
{
    Task<GeneralServiceResult<PaymentVoidOutcome>> VoidPaymentAsync(string paymentId, string reason,
        CancellationToken ct = default);
}

public class PaymentVoidService : IPaymentVoidService
{
    public static readonly string[] VoidableStatuses = { "PMNT_NOT_PAID", "PMNT_SENT", "PMNT_RECEIVED" };

    private readonly DataContext _context;
    private readonly IAcctgTransReversalService _reversal;
    private readonly IAccountingPeriodGuard _periodGuard;
    private readonly IPaymentApplicationService _paymentApplicationService;
    private readonly Lazy<IPaymentHelperService> _paymentHelperService;
    private readonly IUserAccessor _userAccessor;
    private readonly ILogger<PaymentVoidService> _logger;

    public PaymentVoidService(DataContext context, IAcctgTransReversalService reversal,
        IAccountingPeriodGuard periodGuard, IPaymentApplicationService paymentApplicationService,
        Lazy<IPaymentHelperService> paymentHelperService, IUserAccessor userAccessor,
        ILogger<PaymentVoidService> logger)
    {
        _context = context;
        _reversal = reversal;
        _periodGuard = periodGuard;
        _paymentApplicationService = paymentApplicationService;
        _paymentHelperService = paymentHelperService;
        _userAccessor = userAccessor;
        _logger = logger;
    }

    public async Task<GeneralServiceResult<PaymentVoidOutcome>> VoidPaymentAsync(string paymentId, string reason,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(paymentId))
            return GeneralServiceResult<PaymentVoidOutcome>.Error("paymentId is required.");

        var payment = await _context.Payments.FirstOrDefaultAsync(p => p.PaymentId == paymentId, ct);
        if (payment == null)
            return GeneralServiceResult<PaymentVoidOutcome>.Error(
                $"الدفعة {paymentId} غير موجودة. (Payment {paymentId} not found.)");

        var previousStatus = payment.StatusId ?? string.Empty;
        if (previousStatus == "PMNT_VOID")
            return GeneralServiceResult<PaymentVoidOutcome>.Error(
                $"الدفعة {paymentId} ملغاة بالفعل. (Payment {paymentId} is already void.)");

        if (!VoidableStatuses.Contains(previousStatus))
            return GeneralServiceResult<PaymentVoidOutcome>.Error(
                $"لا يمكن إلغاء دفعة في الحالة {previousStatus}. " +
                $"(A payment in status {previousStatus} cannot be voided; only not-paid, sent or received payments can.)");

        // Closed-period control on the way out (the evidence being cancelled) and on the way in
        // (the reversal being written today).
        await _periodGuard.EnsureOpenForPaymentsAsync(new[] { paymentId }, ct);
        await _periodGuard.EnsureOpenAsync(DateTime.UtcNow.Date, null, ct);

        var outcome = new PaymentVoidOutcome { PaymentId = paymentId, PreviousStatusId = previousStatus };
        var cleanReason = string.IsNullOrWhiteSpace(reason) ? "Payment voided" : reason.Trim();
        var stamp = DateTime.UtcNow;
        var user = SafeUsername();

        // 1. Applications: detach from invoices (returns INVOICE_PAID invoices to READY) and from other payments.
        var applications = await _context.PaymentApplications
            .Where(pa => pa.PaymentId == paymentId || pa.ToPaymentId == paymentId)
            .Select(pa => pa.PaymentApplicationId)
            .ToListAsync(ct);

        foreach (var applicationId in applications)
        {
            var removed = await _paymentApplicationService.RemovePaymentApplication(applicationId);
            if (!removed.IsSuccess)
                return GeneralServiceResult<PaymentVoidOutcome>.Error(removed.ErrorMessage);
            outcome.RemovedPaymentApplications++;
        }

        // 2. Ledger: reverse every posted transaction this payment produced, including those anchored
        //    through its bank transactions rather than its own id.
        var reversible = await _reversal.FindReversibleForPaymentAsync(paymentId, ct);
        var reversed = await _reversal.ReverseManyAsync(reversible,
            new ReverseAcctgTransOptions { Reason = $"Void payment {paymentId}: {cleanReason}" }, ct);
        if (!reversed.IsSuccess)
            return GeneralServiceResult<PaymentVoidOutcome>.Error(reversed.ErrorMessage);
        outcome.ReversedTransactions = reversed.ResultData;

        // 3. Bank book: cancel, never delete. Bank listings and totals already exclude FINACT_TRNS_CANCELED.
        var finAccountTrans = await _context.FinAccountTrans
            .Where(f => f.PaymentId == paymentId && f.StatusId != "FINACT_TRNS_CANCELED")
            .ToListAsync(ct);

        foreach (var fat in finAccountTrans)
        {
            fat.StatusId = "FINACT_TRNS_CANCELED";
            fat.Comments = Truncate(Prefix($"Voided {stamp:yyyy-MM-dd}: {cleanReason}", fat.Comments), 255);
            fat.LastUpdatedStamp = stamp;
            outcome.CancelledFinAccountTrans++;
        }

        // 4. Order-side preference, when the payment came from a sales/purchase order.
        if (!string.IsNullOrEmpty(payment.PaymentPreferenceId))
        {
            var preference = await _context.OrderPaymentPreferences
                .FirstOrDefaultAsync(opp => opp.OrderPaymentPreferenceId == payment.PaymentPreferenceId, ct);
            if (preference != null)
            {
                preference.StatusId = "PAYMENT_CANCELLED";
                preference.LastModifiedDate = stamp;
            }
        }

        // 5. Status, through the validated transition table (NOT_PAID/SENT/RECEIVED -> PMNT_VOID are all seeded).
        var statusChange = await _paymentHelperService.Value.SetPaymentStatus(paymentId, "PMNT_VOID");
        if (!statusChange.Success)
            return GeneralServiceResult<PaymentVoidOutcome>.Error(
                statusChange.ErrorMessage ?? $"Could not set payment {paymentId} to PMNT_VOID.");

        payment.Comments = Truncate(
            Prefix($"[VOID {stamp:yyyy-MM-dd}{(user == null ? "" : " by " + user)}] {cleanReason}", payment.Comments),
            2500);
        payment.LastUpdatedStamp = stamp;

        _logger.LogInformation(
            "Voided payment {PaymentId} (was {Status}): {Reversed} transactions reversed, {Fat} bank transactions cancelled, {Apps} applications removed",
            paymentId, previousStatus, outcome.ReversedTransactions.Count(r => r.ReversalAcctgTransId != null),
            outcome.CancelledFinAccountTrans, outcome.RemovedPaymentApplications);

        return GeneralServiceResult<PaymentVoidOutcome>.Success(outcome);
    }

    private static string Prefix(string line, string? existing)
        => string.IsNullOrWhiteSpace(existing) ? line : $"{line}\n{existing}";

    private static string Truncate(string s, int max) => s.Length <= max ? s : s[..max];

    private string? SafeUsername()
    {
        try { return _userAccessor.GetUsername(); }
        catch { return null; }
    }
}
