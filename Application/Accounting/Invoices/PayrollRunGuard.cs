using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Accounting.Invoices;

/// <summary>
/// Pragmatic payroll policy agreed with the auditor (Sep 2026): a generated month may be wiped and
/// re-run only while both settlement payments are still PMNT_NOT_PAID and the period is open.
/// Once either payment is sent, the month is settled and corrections go through a per-invoice
/// reversal or the next run.
/// </summary>
public static class PayrollRunGuard
{
    public static async Task<List<string>> SentRunPaymentIdsAsync(DataContext context, string organizationPartyId,
        DateOnly monthStart, DateOnly monthEnd, CancellationToken ct)
    {
        return await context.Payments
            .AsNoTracking()
            .Where(p => p.PaymentTypeId == "PAYROL_PAYMENT"
                        && p.PartyIdFrom == organizationPartyId
                        && p.PartyIdTo == PayrollConstants.StaffPartyId
                        && p.EffectiveDate >= monthStart
                        && p.EffectiveDate <= monthEnd
                        && p.StatusId != "PMNT_NOT_PAID")
            .Select(p => p.PaymentId)
            .ToListAsync(ct);
    }

    public static string SettledMessage(DateOnly monthStart, IEnumerable<string> paymentIds) =>
        $"لا يمكن حذف أو إعادة تشغيل رواتب شهر {monthStart:yyyy-MM} لأن دفعة الرواتب أُرسلت بالفعل " +
        $"({string.Join("، ", paymentIds)}). الشهر مسوّى؛ سجّل التصحيح على الشهر التالي أو كقيد عكسي. " +
        $"(Payroll for {monthStart:yyyy-MM} is settled: a run payment has been sent. Correct next month or by reversal.)";
}
