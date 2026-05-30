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
            {
                return Results<Unit>.Failure("Employee advance not found.", "ADVANCE_NOT_FOUND");
            }

            // Optional: Prevent deletion if already paid or deductions started
            if (advance.StatusId == "ADVANCE_FULLY_PAID" || advance.PayrollInvoiceId != null)
            {
                return Results<Unit>.Failure("Cannot delete a paid or processed advance.", "ADVANCE_FULLY_PAID");
            }
            
            if (advance.EmployeeAdvanceSchedules.Any(s => s.DeductedAmount > 0 || s.PayrolInvoiceId != null))
            {
                return Results<Unit>.Failure("Cannot delete advance after deductions have started or processed in payroll.", "DEDUCTIONS_STARTED");
            }

            // Remove schedules first
            if (advance.EmployeeAdvanceSchedules.Any())
            {
                _context.EmployeeAdvanceSchedules.RemoveRange(advance.EmployeeAdvanceSchedules);
            }

            // Remove related payment if exists
            if (!string.IsNullOrEmpty(advance.PaymentId))
            {
                var payment = await _context.Payments.FindAsync(new object[] { advance.PaymentId }, ct);
                if (payment != null)
                {
                    // Check if payment can be deleted (only if PMNT_NOT_PAID or similar logic)
                    if (payment.StatusId == "PMNT_NOT_PAID")
                    {
                        _context.Payments.Remove(payment);
                    }
                }
            }

            _context.EmployeeAdvances.Remove(advance);

            var success = await _context.SaveChangesAsync(ct) > 0;

            if (!success) return Results<Unit>.Failure("Failed to delete employee advance");

            return Results<Unit>.Success(Unit.Value);
        }
    }
}
