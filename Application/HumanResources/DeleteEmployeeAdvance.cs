using Application.Core;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.HumanResources;

public class DeleteEmployeeAdvance
{
    public class Command : IRequest<Results<Unit>>
    {
        public string AdvanceId { get; set; } = null!;
        public bool DropPayment { get; set; } = false;
    }

    public class Handler : IRequestHandler<Command, Results<Unit>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Results<Unit>> Handle(Command request, CancellationToken ct)
        {
            var advance = await _context.EmployeeAdvances
                .Include(a => a.EmployeeAdvanceSchedules)
                .FirstOrDefaultAsync(x => x.AdvanceId == request.AdvanceId, ct);

            if (advance == null)
                return Results<Unit>.Failure("السلفة غير موجودة.", "ADVANCE_NOT_FOUND");

            if (advance.PayrollInvoiceId != null)
                return Results<Unit>.Failure("لا يمكن حذف السلفة لأنها مدرجة في كشف راتب.", "ADVANCE_IN_PAYROLL");

            if (advance.EmployeeAdvanceSchedules.Any(s => s.DeductedAmount > 0 || s.PayrolInvoiceId != null))
                return Results<Unit>.Failure("لا يمكن حذف السلفة بعد بدء الاستقطاع أو معالجتها في الرواتب.", "DEDUCTIONS_STARTED");

            // Remove schedules first
            if (advance.EmployeeAdvanceSchedules.Any())
                _context.EmployeeAdvanceSchedules.RemoveRange(advance.EmployeeAdvanceSchedules);

            // Handle linked payment
            if (!string.IsNullOrEmpty(advance.PaymentId))
            {
                var payment = await _context.Payments.FindAsync(new object[] { advance.PaymentId }, ct);
                if (payment != null)
                {
                    var shouldDeletePayment = payment.StatusId == "PMNT_NOT_PAID" || request.DropPayment;

                    if (shouldDeletePayment)
                    {
                        // Clean accounting entries before deleting payment
                        var acctgTransIds = await _context.AcctgTrans
                            .Where(t => t.PaymentId == advance.PaymentId)
                            .Select(t => t.AcctgTransId)
                            .ToListAsync(ct);

                        if (acctgTransIds.Any())
                        {
                            var entries = await _context.AcctgTransEntries
                                .Where(e => acctgTransIds.Contains(e.AcctgTransId))
                                .ToListAsync(ct);
                            if (entries.Any())
                                _context.AcctgTransEntries.RemoveRange(entries);

                            var trans = await _context.AcctgTrans
                                .Where(t => acctgTransIds.Contains(t.AcctgTransId))
                                .ToListAsync(ct);
                            if (trans.Any())
                                _context.AcctgTrans.RemoveRange(trans);
                        }

                        var finTrans = await _context.FinAccountTrans
                            .Where(f => f.PaymentId == advance.PaymentId)
                            .ToListAsync(ct);
                        if (finTrans.Any())
                            _context.FinAccountTrans.RemoveRange(finTrans);

                        _context.Payments.Remove(payment);
                    }
                }
            }

            _context.EmployeeAdvances.Remove(advance);

            var success = await _context.SaveChangesAsync(ct) > 0;

            if (!success) return Results<Unit>.Failure("فشل في حذف السلفة.");

            return Results<Unit>.Success(Unit.Value);
        }
    }
}
