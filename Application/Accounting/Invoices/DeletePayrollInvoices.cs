using Application.Core;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Accounting.Invoices;

public class DeletePayrollInvoices
{
    public class Command : IRequest<Result<Unit>>
    {
        public DateTime InvoiceDate { get; set; }
        public string OrganizationPartyId { get; set; }
    }

    public class Handler : IRequestHandler<Command, Result<Unit>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                DateOnly monthStart = new DateOnly(request.InvoiceDate.Year, request.InvoiceDate.Month, 1);
                DateOnly monthEnd = monthStart.AddMonths(1).AddDays(-1);

                var existingInvoiceIds = await _context.Invoices
                    .Where(i => i.InvoiceTypeId == "PAYROL_INVOICE"
                                && i.InvoiceDate >= monthStart
                                && i.InvoiceDate <= monthEnd
                                && i.PartyId == request.OrganizationPartyId)
                    .Select(i => i.InvoiceId)
                    .ToListAsync(cancellationToken);

                if (existingInvoiceIds.Any())
                {
                    // 1. Revert Long-term Advance Schedules
                    await _context.EmployeeAdvanceSchedules
                        .Where(s => existingInvoiceIds.Contains(s.PayrolInvoiceId))
                        .ExecuteUpdateAsync(s => s
                            .SetProperty(x => x.DeductedAmount, 0m)
                            .SetProperty(x => x.PayrolInvoiceId, (string?)null)
                            .SetProperty(x => x.StatusId, "SCHEDULED")
                            .SetProperty(x => x.LastUpdatedStamp, DateTime.UtcNow)
                            .SetProperty(x => x.Notes, (string?)null), cancellationToken);

                    // 2. Revert Short-term Advances
                    await _context.EmployeeAdvances
                        .Where(a => existingInvoiceIds.Contains(a.PayrollInvoiceId)
                                    && a.AdvanceTypeId == "EMPLOYEE_ADVANCE")
                        .ExecuteUpdateAsync(a => a
                            .SetProperty(x => x.PayrollInvoiceId, (string?)null)
                            .SetProperty(x => x.StatusId, "ADVANCE_APPROVED")
                            .SetProperty(x => x.LastUpdatedStamp, DateTime.UtcNow), cancellationToken);

                    // 3. Delete related records and Invoices
                    await _context.InvoiceItems
                        .Where(ii => existingInvoiceIds.Contains(ii.InvoiceId))
                        .ExecuteDeleteAsync(cancellationToken);

                    await _context.InvoiceRoles
                        .Where(ir => existingInvoiceIds.Contains(ir.InvoiceId))
                        .ExecuteDeleteAsync(cancellationToken);

                    await _context.InvoiceStatuses
                        .Where(isr => existingInvoiceIds.Contains(isr.InvoiceId))
                        .ExecuteDeleteAsync(cancellationToken);

                    // Delete accounting entries and transactions
                    var transIds = await _context.AcctgTrans
                        .Where(at => existingInvoiceIds.Contains(at.InvoiceId))
                        .Select(at => at.AcctgTransId)
                        .ToListAsync(cancellationToken);

                    if (transIds.Any())
                    {
                        await _context.AcctgTransEntries
                            .Where(ate => transIds.Contains(ate.AcctgTransId))
                            .ExecuteDeleteAsync(cancellationToken);

                        await _context.AcctgTrans
                            .Where(at => transIds.Contains(at.AcctgTransId))
                            .ExecuteDeleteAsync(cancellationToken);
                    }

                    await _context.Invoices
                        .Where(i => existingInvoiceIds.Contains(i.InvoiceId))
                        .ExecuteDeleteAsync(cancellationToken);
                }

                await transaction.CommitAsync(cancellationToken);
                return Result<Unit>.Success(Unit.Value);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Result<Unit>.Failure($"Error deleting payroll invoices: {ex.Message}");
            }
        }
    }
}
