using Application.Accounting.Services;
using Application.Core;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.HumanResources;

/// <summary>
/// Removes an employee advance that has not started being deducted.
///
/// Step 3 of the auditor's soft-delete requirement (Sep 2026): the outcome depends on whether the
/// advance's payment has reached the ledger.
///  - Payment still a draft (or none): the advance, its schedule and the draft payment are deleted.
///  - Payment sent: the advance is kept and marked ADVANCE_CANCELLED, and the payment is voided,
///    which reverses its ledger entries and cancels its bank row. Nothing posted is removed.
/// The old <c>DropPayment</c> flag is accepted for compatibility and ignored.
/// </summary>
public class DeleteEmployeeAdvance
{
    public class Command : IRequest<Results<Unit>>
    {
        public string AdvanceId { get; set; } = null!;
        public bool DropPayment { get; set; } = false;
        public string? Reason { get; set; }
    }

    public class Handler : IRequestHandler<Command, Results<Unit>>
    {
        private readonly DataContext _context;
        private readonly IAccountingPeriodGuard _periodGuard;
        private readonly IPaymentVoidService _voidService;

        public Handler(DataContext context, IAccountingPeriodGuard periodGuard, IPaymentVoidService voidService)
        {
            _context = context;
            _periodGuard = periodGuard;
            _voidService = voidService;
        }

        public async Task<Results<Unit>> Handle(Command request, CancellationToken ct)
        {
            var advance = await _context.EmployeeAdvances
                .Include(a => a.EmployeeAdvanceSchedules)
                .FirstOrDefaultAsync(x => x.AdvanceId == request.AdvanceId, ct);

            if (advance == null)
                return Results<Unit>.Failure("السلفة غير موجودة.", "ADVANCE_NOT_FOUND");

            if (advance.StatusId == "ADVANCE_CANCELLED")
                return Results<Unit>.Failure("السلفة ملغاة بالفعل.", "ADVANCE_ALREADY_CANCELLED");

            if (advance.PayrollInvoiceId != null)
                return Results<Unit>.Failure("لا يمكن حذف السلفة لأنها مدرجة في كشف راتب.", "ADVANCE_IN_PAYROLL");

            if (advance.EmployeeAdvanceSchedules.Any(s => s.DeductedAmount > 0 || s.PayrolInvoiceId != null))
                return Results<Unit>.Failure("لا يمكن حذف السلفة بعد بدء الاستقطاع أو معالجتها في الرواتب.", "DEDUCTIONS_STARTED");

            await using var transaction = await _context.Database.BeginTransactionAsync(ct);
            try
            {
                var payment = string.IsNullOrEmpty(advance.PaymentId)
                    ? null
                    : await _context.Payments.FirstOrDefaultAsync(p => p.PaymentId == advance.PaymentId, ct);

                var paymentReachedLedger = payment != null && payment.StatusId != "PMNT_NOT_PAID";

                // The schedule is a plan, not a financial record; it goes either way.
                if (advance.EmployeeAdvanceSchedules.Any())
                    _context.EmployeeAdvanceSchedules.RemoveRange(advance.EmployeeAdvanceSchedules);

                if (paymentReachedLedger)
                {
                    // Void, never delete: reversal + bank row cancelled + status PMNT_VOID.
                    var reason = string.IsNullOrWhiteSpace(request.Reason)
                        ? $"Employee advance {advance.AdvanceId} cancelled"
                        : request.Reason.Trim();

                    var voided = await _voidService.VoidPaymentAsync(payment!.PaymentId, reason, ct);
                    if (!voided.IsSuccess)
                    {
                        await transaction.RollbackAsync(ct);
                        return Results<Unit>.Failure(voided.ErrorMessage, "PAYMENT_VOID_FAILED");
                    }

                    advance.StatusId = "ADVANCE_CANCELLED";
                    advance.LastUpdatedStamp = DateTime.UtcNow;
                }
                else
                {
                    if (payment != null)
                    {
                        // A draft payment carries no posted rows; its bank row was created with it and goes with it.
                        await _periodGuard.EnsureOpenForPaymentsAsync(new[] { payment.PaymentId }, ct);

                        var draftTrans = await _context.AcctgTrans
                            .Include(t => t.AcctgTransEntries)
                            .Where(t => t.PaymentId == payment.PaymentId)
                            .ToListAsync(ct);
                        if (draftTrans.Any(t => t.IsPosted == "Y"))
                        {
                            await transaction.RollbackAsync(ct);
                            return Results<Unit>.Failure(
                                "الدفعة المرتبطة بالسلفة لها قيود مرحّلة؛ ألغِ الدفعة بدلاً من حذف السلفة.",
                                "PAYMENT_POSTED");
                        }
                        foreach (var t in draftTrans)
                        {
                            _context.AcctgTransEntries.RemoveRange(t.AcctgTransEntries);
                            _context.AcctgTrans.Remove(t);
                        }

                        var finTrans = await _context.FinAccountTrans
                            .Where(f => f.PaymentId == payment.PaymentId)
                            .ToListAsync(ct);
                        _context.FinAccountTrans.RemoveRange(finTrans);
                        _context.Payments.Remove(payment);
                    }

                    _context.EmployeeAdvances.Remove(advance);
                }

                var success = await _context.SaveChangesAsync(ct) > 0;
                if (!success)
                {
                    await transaction.RollbackAsync(ct);
                    return Results<Unit>.Failure("فشل في حذف السلفة.");
                }

                await transaction.CommitAsync(ct);
                return Results<Unit>.Success(Unit.Value);
            }
            catch (ClosedAccountingPeriodException ex)
            {
                await transaction.RollbackAsync(ct);
                return Results<Unit>.Failure(ex.Message, "PERIOD_CLOSED");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(ct);
                return Results<Unit>.Failure($"فشل في حذف السلفة: {ex.GetBaseException().Message}", "UNEXPECTED_ERROR");
            }
        }
    }
}
