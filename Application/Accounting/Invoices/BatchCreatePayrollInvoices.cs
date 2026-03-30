using Application.Accounting.Services;
using Application.Core;
using Application.Shipments.Invoices;
using MediatR;
using Persistence;

namespace Application.Accounting.Invoices;

public class BatchCreatePayrollInvoices
{
    public class Command : IRequest<Result<Unit>>
    {
        public List<EmployeePayrollRunDto> Employees { get; set; }
        public DateTime InvoiceDate { get; set; }
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

        public Handler(DataContext context, IInvoiceHelperService invoiceHelperService, IInvoiceUtilityService invoiceUtilityService)
        {
            _context = context;
            _invoiceHelperService = invoiceHelperService;
            _invoiceUtilityService = invoiceUtilityService;
        }

        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
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
                        Description = $"Payroll for {request.InvoiceDate:MMMM yyyy}"
                    };

                    var createdInvoice = await _invoiceHelperService.CreateInvoice(invoiceDto);
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
                    }

                    await _context.SaveChangesAsync(cancellationToken);

                    // 6. Set Invoice to READY (this triggers accounting posting)
                    await _invoiceUtilityService.SetInvoiceStatus(invoiceId, "INVOICE_READY", request.InvoiceDate);
                    await _context.SaveChangesAsync(cancellationToken);
                }

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