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

                    // 3. Delete related aggregated Payments
                    var paymentIds = await _context.Payments
                        .Where(p => p.PaymentTypeId == "PAYROL_PAYMENT"
                                    && p.PartyIdFrom == request.OrganizationPartyId
                                    && p.PartyIdTo == PayrollConstants.StaffPartyId
                                    && p.EffectiveDate >= monthStart
                                    && p.EffectiveDate <= monthEnd)
                        .Select(p => p.PaymentId)
                        .ToListAsync(cancellationToken);

                    if (paymentIds.Any())
                    {
                        // Delete accounting entries for payments
                        await _context.AcctgTransEntries
                            .Where(ate => _context.AcctgTrans
                                .Where(at => at.PaymentId != null && paymentIds.Contains(at.PaymentId))
                                .Select(at => at.AcctgTransId)
                                .Contains(ate.AcctgTransId))
                            .ExecuteDeleteAsync(cancellationToken);

                        // Delete accounting attributes for payments
                        await _context.AcctgTransAttributes
                            .Where(ata => _context.AcctgTrans
                                .Where(at => at.PaymentId != null && paymentIds.Contains(at.PaymentId))
                                .Select(at => at.AcctgTransId)
                                .Contains(ata.AcctgTransId))
                            .ExecuteDeleteAsync(cancellationToken);

                        // Delete accounting transactions for payments
                        await _context.AcctgTrans
                            .Where(at => at.PaymentId != null && paymentIds.Contains(at.PaymentId))
                            .ExecuteDeleteAsync(cancellationToken);

                        // Delete financial account transactions for payments
                        await _context.FinAccountTrans
                            .Where(fat => fat.PaymentId != null && paymentIds.Contains(fat.PaymentId))
                            .ExecuteDeleteAsync(cancellationToken);

                        // Delete payment group memberships
                        await _context.PaymentGroupMembers
                            .Where(pgm => paymentIds.Contains(pgm.PaymentId))
                            .ExecuteDeleteAsync(cancellationToken);

                        // Finally delete the payments
                        await _context.Payments
                            .Where(p => paymentIds.Contains(p.PaymentId))
                            .ExecuteDeleteAsync(cancellationToken);
                    }

                    // 4. Delete related records and Invoices
                    await _context.InvoiceItems
                        .Where(ii => existingInvoiceIds.Contains(ii.InvoiceId))
                        .ExecuteDeleteAsync(cancellationToken);

                    await _context.InvoiceRoles
                        .Where(ir => existingInvoiceIds.Contains(ir.InvoiceId))
                        .ExecuteDeleteAsync(cancellationToken);

                    await _context.InvoiceAttributes
                        .Where(ia => existingInvoiceIds.Contains(ia.InvoiceId))
                        .ExecuteDeleteAsync(cancellationToken);

                    await _context.InvoiceStatuses
                        .Where(isr => existingInvoiceIds.Contains(isr.InvoiceId))
                        .ExecuteDeleteAsync(cancellationToken);

                    // Delete accounting entries and transactions
                    await _context.AcctgTransEntries
                        .Where(ate => _context.AcctgTrans
                            .Where(at => existingInvoiceIds.Contains(at.InvoiceId))
                            .Select(at => at.AcctgTransId)
                            .Contains(ate.AcctgTransId))
                        .ExecuteDeleteAsync(cancellationToken);

                    await _context.AcctgTransAttributes
                        .Where(ata => _context.AcctgTrans
                            .Where(at => existingInvoiceIds.Contains(at.InvoiceId))
                            .Select(at => at.AcctgTransId)
                            .Contains(ata.AcctgTransId))
                        .ExecuteDeleteAsync(cancellationToken);

                    await _context.AcctgTrans
                        .Where(at => existingInvoiceIds.Contains(at.InvoiceId))
                        .ExecuteDeleteAsync(cancellationToken);

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
