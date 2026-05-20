using Application.Accounting.FinAccounts;
using Application.Accounting.Services;
using Application.Shipments.Invoices;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Accounting.Invoices;

public class BatchCreatePayrollInvoices
{
    public class Command : IRequest<Result<Unit>>
    {
        public List<EmployeePayrollRunDto> Employees { get; set; }
        public DateOnly InvoiceDate { get; set; }
        public string OrganizationPartyId { get; set; }
    }

    public class EmployeePayrollRunDto
    {
        public string EmployeeId { get; set; }
        public decimal BaseSalary { get; set; }
        public decimal AbsenceDays { get; set; }
        public decimal AbsenceValue { get; set; }
        public decimal OvertimeDays { get; set; }
        public decimal OvertimeValue { get; set; }
        public List<AdvanceDeductionDto> Advances { get; set; }
        public string PreferredPayrollPaymentMethodId { get; set; }
        public decimal NetSalary { get; set; }
    }

    public class AdvanceDeductionDto
    {
        public string AdvanceId { get; set; }
        public string AdvanceTypeId { get; set; }
        public decimal Amount { get; set; }
    }

    public class Handler : IRequestHandler<Command, Result<Unit>>
    {
        private readonly DataContext _context;
        private readonly IInvoiceHelperService _invoiceHelperService;
        private readonly IInvoiceUtilityService _invoiceUtilityService;
        private readonly IPaymentHelperService _paymentHelperService;

        public Handler(DataContext context, IInvoiceHelperService invoiceHelperService,
            IInvoiceUtilityService invoiceUtilityService, IPaymentHelperService paymentHelperService)
        {
            _context = context;
            _invoiceHelperService = invoiceHelperService;
            _invoiceUtilityService = invoiceUtilityService;
            _paymentHelperService =
                paymentHelperService ?? throw new ArgumentNullException(nameof(paymentHelperService));
        }

        /// <summary>
        /// Reverts all deductions made by the given payroll invoices.
        /// Used before re-running batch payroll for the same month to avoid double deduction.
        /// </summary>
        private async Task RevertPayrollDeductionsForInvoices(List<string> invoiceIds)
        {
            if (invoiceIds == null || !invoiceIds.Any())
                return;

            // 1. Revert Long-term Advance Schedules
            await _context.EmployeeAdvanceSchedules
                .Where(s => invoiceIds.Contains(s.PayrolInvoiceId))
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.DeductedAmount, 0m)
                    .SetProperty(x => x.PayrolInvoiceId, (string?)null)
                    .SetProperty(x => x.StatusId, "SCHEDULED")
                    .SetProperty(x => x.LastUpdatedStamp, DateTime.UtcNow)
                    .SetProperty(x => x.Notes, (string?)null));

            // 2. Revert Short-term Advances (using correct property name: PayrolInvoiceId)
            await _context.EmployeeAdvances
                .Where(a => invoiceIds.Contains(a.PayrollInvoiceId)
                            && a.AdvanceTypeId == "EMPLOYEE_ADVANCE")
                .ExecuteUpdateAsync(a => a
                    .SetProperty(x => x.PayrollInvoiceId, (string?)null)
                    .SetProperty(x => x.StatusId, "ADVANCE_APPROVED")
                    .SetProperty(x => x.LastUpdatedStamp, DateTime.UtcNow));
        }

        /// <summary>
        /// Deletes payroll invoices and all related records using ExecuteDeleteAsync (EF Core 7+)
        /// </summary>
        private async Task DeletePayrollInvoicesSafelyAsync(List<string> invoiceIds, string organizationPartyId, DateOnly monthStart, DateOnly monthEnd, CancellationToken ct)
        {
            if (!invoiceIds.Any()) return;

            // 1. Find and delete related aggregated Payments
            var paymentIds = await _context.Payments
                .Where(p => p.PaymentTypeId == "PAYROL_PAYMENT"
                            && p.PartyIdFrom == organizationPartyId
                            && p.PartyIdTo == "276"
                            && p.EffectiveDate >= monthStart
                            && p.EffectiveDate <= monthEnd)
                .Select(p => p.PaymentId)
                .ToListAsync(ct);

            if (paymentIds.Any())
            {
                // Delete accounting entries for payments
                await _context.AcctgTransEntries
                    .Where(ate => _context.AcctgTrans
                        .Where(at => at.PaymentId != null && paymentIds.Contains(at.PaymentId))
                        .Select(at => at.AcctgTransId)
                        .Contains(ate.AcctgTransId))
                    .ExecuteDeleteAsync(ct);

                // Delete accounting attributes for payments
                await _context.AcctgTransAttributes
                    .Where(ata => _context.AcctgTrans
                        .Where(at => at.PaymentId != null && paymentIds.Contains(at.PaymentId))
                        .Select(at => at.AcctgTransId)
                        .Contains(ata.AcctgTransId))
                    .ExecuteDeleteAsync(ct);

                // Delete accounting transactions for payments
                await _context.AcctgTrans
                    .Where(at => at.PaymentId != null && paymentIds.Contains(at.PaymentId))
                    .ExecuteDeleteAsync(ct);

                // Delete financial account transactions for payments
                await _context.FinAccountTrans
                    .Where(fat => fat.PaymentId != null && paymentIds.Contains(fat.PaymentId))
                    .ExecuteDeleteAsync(ct);

                // Delete payment group memberships
                await _context.PaymentGroupMembers
                    .Where(pgm => paymentIds.Contains(pgm.PaymentId))
                    .ExecuteDeleteAsync(ct);

                // Finally delete the payments
                await _context.Payments
                    .Where(p => paymentIds.Contains(p.PaymentId))
                    .ExecuteDeleteAsync(ct);
            }

            // 2. Delete Invoices and related records
            // Delete child records first (order is important)
            await _context.InvoiceItems
                .Where(ii => invoiceIds.Contains(ii.InvoiceId))
                .ExecuteDeleteAsync(ct);

            await _context.Set<InvoiceRole>() // Assuming you have InvoiceRole entity
                .Where(ir => invoiceIds.Contains(ir.InvoiceId))
                .ExecuteDeleteAsync(ct);

            await _context.InvoiceAttributes
                .Where(ia => invoiceIds.Contains(ia.InvoiceId))
                .ExecuteDeleteAsync(ct);

            await _context.Set<InvoiceStatus>()
                .Where(isr => invoiceIds.Contains(isr.InvoiceId))
                .ExecuteDeleteAsync(ct);

            // Delete accounting entries first, then transactions
            await _context.AcctgTransEntries
                .Where(ate => _context.AcctgTrans
                    .Where(at => invoiceIds.Contains(at.InvoiceId))
                    .Select(at => at.AcctgTransId)
                    .Contains(ate.AcctgTransId))
                .ExecuteDeleteAsync(ct);

            await _context.AcctgTransAttributes
                .Where(ata => _context.AcctgTrans
                    .Where(at => invoiceIds.Contains(at.InvoiceId))
                    .Select(at => at.AcctgTransId)
                    .Contains(ata.AcctgTransId))
                .ExecuteDeleteAsync(ct);

            await _context.AcctgTrans
                .Where(at => invoiceIds.Contains(at.InvoiceId))
                .ExecuteDeleteAsync(ct);

            // Finally delete the invoices
            await _context.Invoices
                .Where(i => invoiceIds.Contains(i.InvoiceId))
                .ExecuteDeleteAsync(ct);
        }

        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            if (request.Employees == null)
            {
                return Result<Unit>.Failure("Employees list is null.");
            }

            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                DateOnly monthStart = new DateOnly(request.InvoiceDate.Year, request.InvoiceDate.Month, 1);
                DateOnly monthEnd = monthStart.AddMonths(1).AddDays(-1);

                // ===================================================================
                // STEP 1: Find existing payroll invoices for this month
                // ===================================================================
                var existingInvoiceIds = await _context.Invoices
                    .Where(i => i.InvoiceTypeId == "PAYROL_INVOICE"
                                && i.InvoiceDate >= monthStart
                                && i.InvoiceDate <= monthEnd
                                && request.Employees.Select(e => e.EmployeeId).Contains(i.PartyIdFrom))
                    .Select(i => i.InvoiceId)
                    .ToListAsync(cancellationToken);

                if (existingInvoiceIds.Any())
                {
                    // Revert deductions on advances and schedules first
                    await RevertPayrollDeductionsForInvoices(existingInvoiceIds);

                    // Then safely delete old payroll invoices + all related artifacts using ExecuteDeleteAsync
                    await DeletePayrollInvoicesSafelyAsync(existingInvoiceIds, request.OrganizationPartyId, monthStart, monthEnd, cancellationToken);
                }

                foreach (var emp in request.Employees)
                {
                    // 1. Create Invoice Header
                    var invoiceDto = new InvoiceDto3
                    {
                        InvoiceTypeId = "PAYROL_INVOICE",
                        PartyId = request.OrganizationPartyId,
                        PartyIdFrom = emp.EmployeeId,
                        StatusId = "INVOICE_IN_PROCESS",
                        CurrencyUomId = "EGP", // Default for payroll as seen in existing logic
                        InvoiceDate = request.InvoiceDate,
                        Description = $"Payroll for {request.InvoiceDate.ToString("MMMM yyyy")}"
                    };

                    var createdInvoice = await _invoiceHelperService.CreateInvoice(invoiceDto);
                    await _context.SaveChangesAsync(
                        cancellationToken); // Ensure invoice is persisted for FK constraints
                    var invoiceId = createdInvoice.InvoiceId;
                    var itemSeqId = 1;

                    // 2. Add Salary Item
                    if (emp.BaseSalary > 0)
                    {
                        await _invoiceHelperService.CreateInvoiceItem(new InvoiceItemParameters
                        {
                            InvoiceId = invoiceId,
                            InvoiceItemSeqId = (itemSeqId++).ToString("D5"),
                            InvoiceItemTypeId = "PAYROL_SALARY",
                            Amount = emp.BaseSalary,
                            Quantity = 1,
                            Description = "Monthly Base Salary"
                        });
                    }

                    // 3. Add Absence Item
                    if (emp.AbsenceValue > 0)
                    {
                        await _invoiceHelperService.CreateInvoiceItem(new InvoiceItemParameters
                        {
                            InvoiceId = invoiceId,
                            InvoiceItemSeqId = (itemSeqId++).ToString("D5"),
                            InvoiceItemTypeId = "PAYROL_DD_ABSENCE",
                            Amount = emp.AbsenceValue,
                            Quantity = emp.AbsenceDays,
                            Description = $"Absence: {emp.AbsenceDays} days"
                        });
                    }

                    // 4. Add Overtime Item
                    if (emp.OvertimeValue > 0)
                    {
                        await _invoiceHelperService.CreateInvoiceItem(new InvoiceItemParameters
                        {
                            InvoiceId = invoiceId,
                            InvoiceItemSeqId = (itemSeqId++).ToString("D5"),
                            InvoiceItemTypeId = "PAYROL_OVERTIME",
                            Amount = emp.OvertimeValue,
                            Quantity = emp.OvertimeDays,
                            Description = $"Overtime: {emp.OvertimeDays} days"
                        });
                    }

                    // 5. Add Advance Items
                    if (emp.Advances != null && emp.Advances.Any())
                    {
                        var totalAdvances = emp.Advances.Sum(a => a.Amount);
                        var advanceDesc = string.Join(", ", emp.Advances.Select(a => $"Adv #{a.AdvanceId}"));

                        await _invoiceHelperService.CreateInvoiceItem(new InvoiceItemParameters
                        {
                            InvoiceId = invoiceId,
                            InvoiceItemSeqId = (itemSeqId++).ToString("D5"),
                            InvoiceItemTypeId = "PAYROL_DD_ADVANCE",
                            Amount = totalAdvances,
                            Quantity = 1,
                            Description = advanceDesc
                        });

                        // Update Advance Status to PAID
                        foreach (var adv in emp.Advances)
                        {
                            if (adv.AdvanceTypeId == "EMPLOYEE_ADVANCE")
                            {
                                await _context.EmployeeAdvances
                                    .Where(a => a.AdvanceId == adv.AdvanceId)
                                    .ExecuteUpdateAsync(a => a
                                        .SetProperty(x => x.StatusId, "PAID")
                                        .SetProperty(x => x.PayrollInvoiceId, invoiceId)
                                        .SetProperty(x => x.LastUpdatedStamp, DateTime.UtcNow), cancellationToken);
                            }
                            else if (adv.AdvanceTypeId == "EMPLOYEE_LONG_TERM_ADVANCE")
                            {
                                await _context.EmployeeAdvanceSchedules
                                    .Where(s => s.AdvanceId == adv.AdvanceId && s.PayrolInvoiceId == null &&
                                                s.StatusId == "SCHEDULED"
                                                && s.DueDate.HasValue &&
                                                s.DueDate.Value.Month == request.InvoiceDate.Month &&
                                                s.DueDate.Value.Year == request.InvoiceDate.Year)
                                    .Take(1)
                                    .ExecuteUpdateAsync(s => s
                                        .SetProperty(x => x.StatusId, "PAID")
                                        .SetProperty(x => x.PayrolInvoiceId, invoiceId)
                                        .SetProperty(x => x.DeductedAmount, adv.Amount)
                                        .SetProperty(x => x.LastUpdatedStamp, DateTime.UtcNow), cancellationToken);
                            }
                        }
                    }

                    await _context.SaveChangesAsync(cancellationToken);

                    // 6. Set Invoice to READY (this triggers accounting posting)
                    await _invoiceUtilityService.SetInvoiceStatus(invoiceId, "INVOICE_READY", request.InvoiceDate);
                }

                // ===================================================================
                // STEP 3: Create Payments based on Preferred Payment Method
                // ===================================================================
                var cashTotal = request.Employees
                    .Where(e => e.PreferredPayrollPaymentMethodId == "CASH")
                    .Sum(e => e.NetSalary);

                var bankTransferTotal = request.Employees
                    .Where(e => e.PreferredPayrollPaymentMethodId == "BANK_TRANSFER")
                    .Sum(e => e.NetSalary);

                if (cashTotal > 0)
                {
                    var paymentResult = await _paymentHelperService.CreatePaymentAndFinAccountTrans(
                        new CreatePaymentAndFinAccountTransRequest
                        {
                            PaymentTypeId = "PAYROL_PAYMENT",
                            PartyIdFrom = request.OrganizationPartyId,
                            PartyIdTo = "276",
                            PaymentMethodId = "CASH",
                            Amount = cashTotal,
                            StatusId = "PMNT_NOT_PAID",
                            IsBankTransfer = false,
                            PaymentDate = request.InvoiceDate,
                            Comments = $"دفعة رواتب نقدية - {request.InvoiceDate.ToString("MM/yyyy")}"
                        });

                    if (!paymentResult.IsSuccess)
                    {
                        throw new Exception($"Failed to create Cash payment: {paymentResult.Error}");
                    }
                }

                if (bankTransferTotal > 0)
                {
                    var paymentResult = await _paymentHelperService.CreatePaymentAndFinAccountTrans(
                        new CreatePaymentAndFinAccountTransRequest
                        {
                            PaymentTypeId = "PAYROL_PAYMENT",
                            PartyIdFrom = request.OrganizationPartyId,
                            PartyIdTo = "276",
                            PaymentMethodId = "CIB_CHECKING",
                            Amount = bankTransferTotal,
                            StatusId = "PMNT_NOT_PAID",
                            IsBankTransfer = true,
                            PaymentDate = request.InvoiceDate,
                            Comments = $"تحويل رواتب بنكي - {request.InvoiceDate.ToString("MM/yyyy")}"
                        });

                    if (!paymentResult.IsSuccess)
                    {
                        throw new Exception($"Failed to create Bank Transfer payment: {paymentResult.Error}");
                    }
                }

                await _context.SaveChangesAsync(cancellationToken);

                await transaction.CommitAsync(cancellationToken);

                return Result<Unit>.Success(Unit.Value);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Result<Unit>.Failure($"Error during batch payroll run: {ex.Message}");
            }
        }
    }
}