using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Accounting.Payments;

/// <summary>
/// Blocks the "Override GL Account" from re-booking cost on a payment whose cost is already in the
/// ledger. A payment linked to an order (purchase order / project certificate, via its payment
/// preference) or applied to an invoice only settles a payable: the certificate/invoice already
/// debited the project or expense account. Overriding the payment's debit to that project account
/// books the same cost twice and leaves the supplier looking unpaid (Ladris October, Sep 2026:
/// O17328 put 85,000 twice on 124422; five more payments did the same on 124424/124425).
/// On such payments the override may only name an Accounts Payable account.
/// </summary>
public static class PaymentOverrideGlGuard
{
    public const string ErrorCode = "OVERRIDE_GL_ON_LINKED_PAYMENT";

    public const string ErrorMessage =
        "لا يمكن اختيار «حساب دفتر الأستاذ» لدفعة مرتبطة بمستخلص أو أمر شراء أو فاتورة: " +
        "التكلفة مسجلة بالفعل عند اعتماد المستخلص/الفاتورة، واختيار حساب المشروع أو المصروف يسجلها مرتين. " +
        "اترك الحقل فارغاً (أو اختر حساب موردين فقط).";

    /// <returns>null when allowed, otherwise the Arabic error message.</returns>
    public static async Task<string?> CheckAsync(DataContext context, string paymentId,
        string? paymentPreferenceId, string? overrideGlAccountId, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(overrideGlAccountId)) return null;

        var linkedToOrder = !string.IsNullOrEmpty(paymentPreferenceId) &&
                            await context.OrderPaymentPreferences.AsNoTracking()
                                .AnyAsync(opp => opp.OrderPaymentPreferenceId == paymentPreferenceId
                                                 && opp.OrderId != null, ct);

        var appliedToInvoice = !linkedToOrder &&
                               await context.PaymentApplications.AsNoTracking()
                                   .AnyAsync(pa => pa.PaymentId == paymentId && pa.InvoiceId != null, ct);

        if (!linkedToOrder && !appliedToInvoice) return null;

        // Pointing the debit at the supplier's own payable account is harmless (and used in practice).
        var isPayable = await context.GlAccounts.AsNoTracking()
            .AnyAsync(g => g.GlAccountId == overrideGlAccountId
                           && g.GlAccountTypeId == "ACCOUNTS_PAYABLE", ct);

        return isPayable ? null : ErrorMessage;
    }
}
