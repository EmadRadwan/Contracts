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
                            && p.PartyIdTo == PayrollConstants.StaffPartyId
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

                var employeeInvoiceTotals = new Dictionary<string, decimal>();

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

                    // 1b. Snapshot the payment method this invoice was run under, so the aggregate
                    // payment it belongs to is a recorded fact rather than something re-derived from
                    // Party.PreferredPayrollPaymentMethodId months later. Editing an employee's
                    // preference after the run must not move a historical invoice between the cash
                    // and bank settlements — which is exactly what used to leave an already-created
                    // payment unable to post. Read back by
                    // GeneralLedgerService.CreateAcctgTransAndEntriesForPayrollPayment.
                    //
                    // Stored verbatim, NOT normalised to one of the two methods: the two payment
                    // totals below are summed with exact equality on CASH / BANK_TRANSFER, so an
                    // employee whose preference is unset belongs to neither payment. Defaulting a
                    // blank to BANK_TRANSFER here would make the posting debit an invoice that no
                    // payment amount covers, and the transaction would not balance.
                    _context.InvoiceAttributes.Add(new InvoiceAttribute
                    {
                        InvoiceId = invoiceId,
                        AttrName = PayrollConstants.PaymentMethodAttrName,
                        AttrValue = emp.PreferredPayrollPaymentMethodId,
                        AttrDescription = "Payroll payment method at run time",
                        CreatedStamp = DateTime.UtcNow,
                        LastUpdatedStamp = DateTime.UtcNow
                    });

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
                    // GetInvoiceTotal / CreateAcctgTransForPayrollInvoice sum Quantity * Amount, so Amount must be
                    // the per-day rate (not the already-computed total) or the days factor gets counted twice
                    // (dailyRate * days * days). Deriving the rate as AbsenceValue / AbsenceDays keeps Quantity
                    // holding the real day count (other reports read Quantity as days) while still reconstructing
                    // AbsenceValue exactly via Quantity * Amount — including when a manager has overridden
                    // AbsenceValue without changing AbsenceDays. Falls back to Quantity=1 when AbsenceDays is 0
                    // (e.g. a value entered with no corresponding days) so the amount isn't multiplied away.
                    if (emp.AbsenceValue > 0)
                    {
                        var absenceQuantity = emp.AbsenceDays > 0 ? emp.AbsenceDays : 1m;
                        await _invoiceHelperService.CreateInvoiceItem(new InvoiceItemParameters
                        {
                            InvoiceId = invoiceId,
                            InvoiceItemSeqId = (itemSeqId++).ToString("D5"),
                            InvoiceItemTypeId = "PAYROL_DD_ABSENCE",
                            Amount = emp.AbsenceValue / absenceQuantity,
                            Quantity = absenceQuantity,
                            Description = $"Absence: {emp.AbsenceDays} days"
                        });
                    }

                    // 4. Add Overtime Item (same per-day-rate reasoning as the absence item above)
                    if (emp.OvertimeValue > 0)
                    {
                        var overtimeQuantity = emp.OvertimeDays > 0 ? emp.OvertimeDays : 1m;
                        await _invoiceHelperService.CreateInvoiceItem(new InvoiceItemParameters
                        {
                            InvoiceId = invoiceId,
                            InvoiceItemSeqId = (itemSeqId++).ToString("D5"),
                            InvoiceItemTypeId = "PAYROL_OVERTIME",
                            Amount = emp.OvertimeValue / overtimeQuantity,
                            Quantity = overtimeQuantity,
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

                        // Update Advance Status
                        foreach (var adv in emp.Advances)
                        {
                            if (adv.AdvanceTypeId == "EMPLOYEE_ADVANCE")
                            {
                                await _context.EmployeeAdvances
                                    .Where(a => a.AdvanceId == adv.AdvanceId)
                                    .ExecuteUpdateAsync(a => a
                                        .SetProperty(x => x.StatusId, "ADVANCE_FULLY_PAID")
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

                                // Update parent advance status based on remaining schedules
                                var hasRemaining = await _context.EmployeeAdvanceSchedules
                                    .AnyAsync(s => s.AdvanceId == adv.AdvanceId && s.StatusId == "SCHEDULED", cancellationToken);

                                await _context.EmployeeAdvances
                                    .Where(a => a.AdvanceId == adv.AdvanceId)
                                    .ExecuteUpdateAsync(a => a
                                        .SetProperty(x => x.StatusId, hasRemaining ? "ADVANCE_PARTIALLY_PAID" : "ADVANCE_FULLY_PAID")
                                        .SetProperty(x => x.LastUpdatedStamp, DateTime.UtcNow), cancellationToken);
                            }
                        }
                    }

                    await _context.SaveChangesAsync(cancellationToken);

                    // 6. Set Invoice to READY (this triggers accounting posting)
                    await _invoiceUtilityService.SetInvoiceStatus(invoiceId, "INVOICE_READY", request.InvoiceDate);

                    // Record the actual persisted invoice total (not the client-submitted NetSalary) so the
                    // payment amount below always matches what CreateAcctgTransAndEntriesForPayrollPayment
                    // will later sum from the same invoices — otherwise the payment (credit) and the
                    // per-employee accrued-salary debits it posts can silently diverge.
                    employeeInvoiceTotals[emp.EmployeeId] = await _invoiceUtilityService.GetInvoiceTotal(invoiceId, true);
                }

                // ===================================================================
                // STEP 3: Create Payments based on Preferred Payment Method
                // ===================================================================
                var cashTotal = request.Employees
                    .Where(e => e.PreferredPayrollPaymentMethodId == PayrollConstants.CashMethod)
                    .Sum(e => employeeInvoiceTotals.GetValueOrDefault(e.EmployeeId));

                var bankTransferTotal = request.Employees
                    .Where(e => e.PreferredPayrollPaymentMethodId == PayrollConstants.BankTransferMethod)
                    .Sum(e => employeeInvoiceTotals.GetValueOrDefault(e.EmployeeId));

                if (cashTotal > 0)
                {
                    var paymentResult = await _paymentHelperService.CreatePaymentAndFinAccountTrans(
                        new CreatePaymentAndFinAccountTransRequest
                        {
                            PaymentTypeId = "PAYROL_PAYMENT",
                            PartyIdFrom = request.OrganizationPartyId,
                            PartyIdTo = PayrollConstants.StaffPartyId,
                            PaymentMethodId = PayrollConstants.CashMethod,
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
                            PartyIdTo = PayrollConstants.StaffPartyId,
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