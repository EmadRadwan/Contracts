using System.Text.RegularExpressions;
using Application.Accounting.Services;
using Application.Core;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Accounting.Payments;

/// <summary>
/// Deletes a draft payment that was entered twice by mistake, when the payment is linked to a project
/// certificate or a sales request. A plain Delete refuses those links (DeletePayment); this command
/// lets the extra copy go only when the linked document stays covered:
/// <list type="bullet">
/// <item>Certificate: another payment that isn't void must remain on the certificate's order.</item>
/// <item>Sales request: the payment must be a Duplicate copy ("نسخة من دفعة X") whose original X still
/// exists, isn't void and belongs to the same sales request. Installments link to payments by
/// SalesRequestId + comment only, so "another payment exists" would also match a real installment.</item>
/// </list>
/// Drafts only: no accounting transactions, no invoice/payment applications, no group, advance or
/// deduction links. The payment's own order preference is removed too when no other payment uses it
/// (Duplicate gives each copy a fresh preference), so the order is left as it was before the mistake.
/// The reason is part of the command, so AuditBehavior records it in AUDIT_ACTIVITY.
/// </summary>
public class DeleteDuplicatePayment
{
    public class Command : IRequest<Result<Unit>>
    {
        public string PaymentId { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
    }

    private static readonly Regex CopyPrefix = new(@"^\s*نسخة من دفعة\s+([^\s–-]+)", RegexOptions.Compiled);
    private static readonly string[] DeadStatuses = { "PMNT_VOID", "PMNT_CANCELLED" };

    public class Handler : IRequestHandler<Command, Result<Unit>>
    {
        private readonly DataContext _context;
        private readonly ILedgerHistoryService _ledgerHistory;
        private readonly IAccountingPeriodGuard _periodGuard;
        private readonly IMediator _mediator;
        private readonly ILogger<Handler> _logger;

        public Handler(DataContext context, ILedgerHistoryService ledgerHistory, IAccountingPeriodGuard periodGuard,
            IMediator mediator, ILogger<Handler> logger)
        {
            _context = context;
            _ledgerHistory = ledgerHistory;
            _periodGuard = periodGuard;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<Result<Unit>> Handle(Command request, CancellationToken ct)
        {
            var reason = request.Reason?.Trim() ?? string.Empty;
            if (reason.Length < 5)
                return Result<Unit>.Failure("يرجى كتابة سبب الحذف (5 أحرف على الأقل). (A reason is required.)");

            var payment = await _context.Payments.FirstOrDefaultAsync(p => p.PaymentId == request.PaymentId, ct);
            if (payment == null)
                return Result<Unit>.Failure($"الدفعة {request.PaymentId} غير موجودة. (Payment not found.)");

            // ── Which document is it linked to? ──
            string? certOrderId = null;
            string? certNumber = null;
            if (!string.IsNullOrEmpty(payment.PaymentPreferenceId))
            {
                var cert = await (from opp in _context.OrderPaymentPreferences
                        where opp.OrderPaymentPreferenceId == payment.PaymentPreferenceId && opp.OrderId != null
                        from we in _context.WorkEfforts
                        where we.RelatedOrderId == opp.OrderId && we.WorkEffortTypeId == "PROJECT_CERTIFICATE"
                        select new { opp.OrderId, we.CertificateNumber })
                    .FirstOrDefaultAsync(ct);
                certOrderId = cert?.OrderId;
                certNumber = cert?.CertificateNumber;
            }

            var isSalesRequest = !string.IsNullOrWhiteSpace(payment.SalesRequestId);

            // Not linked to a certificate or sales request: the ordinary delete rules apply unchanged.
            if (certOrderId == null && !isSalesRequest)
                return await _mediator.Send(new DeletePayment.Command { PaymentId = request.PaymentId }, ct);

            // ── Draft only, nothing posted or attached ──
            if (payment.StatusId != "PMNT_NOT_PAID")
                return Result<Unit>.Failure(
                    $"لا يمكن حذف الدفعة {payment.PaymentId} لأنها ليست غير مدفوعة. استخدم \"إلغاء الدفعة\" من شاشة الدفعة. " +
                    "(Only not-paid drafts can be deleted; void the payment instead.)");

            var history = await _ledgerHistory.ForPaymentAsync(payment.PaymentId, ct);
            if (history.AcctgTransCount > 0)
                return Result<Unit>.Failure(
                    $"لا يمكن حذف الدفعة {payment.PaymentId} لأن لها قيوداً محاسبية. استخدم \"إلغاء الدفعة\". " +
                    "(The payment has accounting transactions; void it instead.)");

            if (await _context.PaymentApplications.AnyAsync(
                    pa => pa.PaymentId == payment.PaymentId || pa.ToPaymentId == payment.PaymentId, ct))
                return Result<Unit>.Failure(
                    $"لا يمكن حذف الدفعة {payment.PaymentId} لأنها مسجلة على فاتورة أو دفعة أخرى. " +
                    "(The payment is applied to an invoice or another payment.)");

            if (await _context.PaymentGroupMembers.AnyAsync(m => m.PaymentId == payment.PaymentId, ct))
                return Result<Unit>.Failure(
                    $"لا يمكن حذف الدفعة {payment.PaymentId} لأنها ضمن مجموعة دفعات. (The payment belongs to a payment group.)");

            if (await _context.EmployeeAdvances.AnyAsync(a => a.PaymentId == payment.PaymentId, ct)
                || await _context.Deductions.AnyAsync(d => d.PaymentId == payment.PaymentId, ct))
                return Result<Unit>.Failure(
                    $"لا يمكن حذف الدفعة {payment.PaymentId} لأنها مرتبطة بسلفة أو خصم. (Linked to an advance or deduction.)");

            // ── The linked document must stay covered ──
            if (certOrderId != null)
            {
                var otherPaymentOnCertificate = await (from p in _context.Payments
                        join opp in _context.OrderPaymentPreferences
                            on p.PaymentPreferenceId equals opp.OrderPaymentPreferenceId
                        where opp.OrderId == certOrderId
                              && p.PaymentId != payment.PaymentId
                              && !DeadStatuses.Contains(p.StatusId)
                        select p.PaymentId)
                    .FirstOrDefaultAsync(ct);

                if (otherPaymentOnCertificate == null)
                    return Result<Unit>.Failure(
                        $"لا يمكن حذف الدفعة {payment.PaymentId} لأنها الدفعة الوحيدة على الشهادة {certNumber}. " +
                        "لحذفها يجب إعادة تعيين الشهادة أو حذفها. " +
                        "(It is the certificate's only payment; reset or delete the certificate instead.)");
            }

            if (isSalesRequest)
            {
                var match = CopyPrefix.Match(payment.Comments ?? string.Empty);
                var originalId = match.Success ? match.Groups[1].Value : null;

                var originalOk = originalId != null && await _context.Payments.AnyAsync(p =>
                    p.PaymentId == originalId
                    && p.SalesRequestId == payment.SalesRequestId
                    && !DeadStatuses.Contains(p.StatusId), ct);

                if (!originalOk)
                    return Result<Unit>.Failure(
                        $"لا يمكن حذف الدفعة {payment.PaymentId} لأنها قسط من طلب المبيعات {payment.SalesRequestId} " +
                        "وليست نسخة مكررة من دفعة قائمة. " +
                        "(Only a Duplicate copy whose original still exists on the same sales request can be deleted.)");
            }

            try
            {
                await _periodGuard.EnsureOpenForPaymentsAsync(new[] { payment.PaymentId }, ct);
            }
            catch (ClosedAccountingPeriodException ex)
            {
                return Result<Unit>.Failure(ex.Message);
            }

            // ── Delete ──
            await using var transaction = await _context.Database.BeginTransactionAsync(ct);
            try
            {
                var paymentId = payment.PaymentId;
                var preferenceId = payment.PaymentPreferenceId;

                _context.FinAccountTrans.RemoveRange(
                    await _context.FinAccountTrans.Where(f => f.PaymentId == paymentId).ToListAsync(ct));
                _context.PaymentAttributes.RemoveRange(
                    await _context.PaymentAttributes.Where(a => a.PaymentId == paymentId).ToListAsync(ct));
                _context.PaymentContents.RemoveRange(
                    await _context.PaymentContents.Where(c => c.PaymentId == paymentId).ToListAsync(ct));
                _context.PaymentBudgetAllocations.RemoveRange(
                    await _context.PaymentBudgetAllocations.Where(b => b.PaymentId == paymentId).ToListAsync(ct));
                _context.Payments.Remove(payment);
                await _context.SaveChangesAsync(ct);

                // The payment's own preference goes with it, unless another payment shares it.
                var removedPreference = false;
                if (!string.IsNullOrEmpty(preferenceId)
                    && !await _context.Payments.AnyAsync(p => p.PaymentPreferenceId == preferenceId, ct))
                {
                    var preference = await _context.OrderPaymentPreferences
                        .FirstOrDefaultAsync(o => o.OrderPaymentPreferenceId == preferenceId, ct);
                    if (preference != null)
                    {
                        _context.OrderPaymentPreferences.Remove(preference);
                        await _context.SaveChangesAsync(ct);
                        removedPreference = true;
                    }
                }

                await transaction.CommitAsync(ct);

                _logger.LogWarning(
                    "Duplicate payment {PaymentId} deleted (certificate {Certificate}, sales request {SalesRequest}, preference removed: {Pref}). Reason: {Reason}",
                    paymentId, certNumber, payment.SalesRequestId, removedPreference, reason);

                return Result<Unit>.Success(Unit.Value);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(ct);
                _logger.LogError(ex, "Deleting duplicate payment {PaymentId} failed", request.PaymentId);
                return Result<Unit>.Failure("حدث خطأ أثناء حذف الدفعة: " + ex.Message);
            }
        }
    }
}
