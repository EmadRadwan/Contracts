// Application/Accounting/Invoices/ListPayrollData2.cs
using Application.Core;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Accounting.Invoices;

public class ListPayrollData2
{
    public class Query : IRequest<Result<List<PayrollData2Dto>>>
    {
        public DateOnly? FromDate { get; set; }
        public DateOnly? ToDate { get; set; }
        public string OrganizationPartyId { get; set; }
    }

    public class PayrollData2Dto
    {
        public int Serial { get; set; }
        public string EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string JobTitle { get; set; }               // المهنة
        public string WorkLocation { get; set; }           // مكان العمل
        public decimal BaseSalary { get; set; }
        public decimal AttendanceRate { get; set; }        // معدل الحضور %
        public decimal DailyRate { get; set; }
        public decimal OvertimeDays { get; set; }
        public decimal OvertimeValue { get; set; }
        public decimal Bonuses { get; set; } = 0;
        public decimal Allowances { get; set; } = 0;
        public decimal GrossTotal { get; set; }
        public decimal Advances { get; set; }
        public decimal AbsenceDays { get; set; }
        public decimal AbsenceValue { get; set; }
        public decimal LateDeduction { get; set; } = 0;
        public decimal OtherDeductions { get; set; } = 0;
        public decimal TotalDeductions { get; set; }
        public decimal NetSalary { get; set; }
        public string Notes { get; set; } = "";
        public string PaymentMethod { get; set; }
        public DateOnly? InvoiceDate { get; set; }
        public string InvoiceId { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<List<PayrollData2Dto>>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context) => _context = context;

        public async Task<Result<List<PayrollData2Dto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var invoices = await (
                from inv in _context.Invoices
                where inv.InvoiceTypeId == "PAYROL_INVOICE"
                   && inv.InvoiceDate >= request.FromDate
                   && inv.InvoiceDate <= request.ToDate
                   && inv.PartyId == request.OrganizationPartyId

                join emp in _context.Parties on inv.PartyIdFrom equals emp.PartyId
                join posFul in _context.EmplPositionFulfillments.Where(f => f.ThruDate == null)
                    on emp.PartyId equals posFul.PartyId into pf
                from posFul in pf.DefaultIfEmpty()
                join pos in _context.EmplPositions on posFul.EmplPositionId equals pos.EmplPositionId into p
                from pos in p.DefaultIfEmpty()
                join posType in _context.EmplPositionTypes on pos.EmplPositionTypeId equals posType.EmplPositionTypeId into pt
                from posType in pt.DefaultIfEmpty()

                select new
                {
                    inv.InvoiceId,
                    inv.InvoiceDate,
                    EmployeeId = emp.PartyId,
                    EmployeeName = emp.Description,
                    JobTitle = posType != null ? posType.Description : "",
                    WorkLocation = emp.MainRole ?? "الشركة", // You can enhance this later
                    PreferredPayrollPaymentMethodId = emp.PreferredPayrollPaymentMethodId
                }).AsNoTracking().ToListAsync(cancellationToken);

            var invoiceIds = invoices.Select(i => i.InvoiceId).ToList();

            var items = await _context.InvoiceItems
                .Where(i => invoiceIds.Contains(i.InvoiceId))
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var itemsByInvoice = items.GroupBy(i => i.InvoiceId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var result = new List<PayrollData2Dto>();

            foreach (var inv in invoices)
            {
                var invItems = itemsByInvoice.GetValueOrDefault(inv.InvoiceId) ?? new List<Domain.InvoiceItem>();

                var baseSalary = invItems.Where(i => i.InvoiceItemTypeId == "PAYROL_SALARY")
                    .Sum(i => (i.Amount ?? 0) * (i.Quantity ?? 1));

                var overtimeDays = invItems.Where(i => i.InvoiceItemTypeId == "PAYROL_OVERTIME")
                    .Sum(i => i.Quantity ?? 0);
                var overtimeValue = invItems.Where(i => i.InvoiceItemTypeId == "PAYROL_OVERTIME")
                    .Sum(i => i.Amount ?? 0);

                var absenceDays = invItems.Where(i => i.InvoiceItemTypeId == "PAYROL_DD_ABSENCE")
                    .Sum(i => i.Quantity ?? 0);
                var absenceValue = invItems.Where(i => i.InvoiceItemTypeId == "PAYROL_DD_ABSENCE")
                    .Sum(i => i.Amount ?? 0);

                var advances = invItems.Where(i => i.InvoiceItemTypeId == "PAYROL_DD_ADVANCE")
                    .Sum(i => i.Amount ?? 0);

                var netSalary = baseSalary + overtimeValue - absenceValue - advances;

                var attendanceRate = 30m > 0 ? Math.Round((30 - absenceDays) / 30 * 100, 4) : 100m;
                var dailyRate = baseSalary > 0 ? Math.Round(baseSalary / 30, 4) : 0;

                result.Add(new PayrollData2Dto
                {
                    Serial = 0, // will be set in frontend
                    EmployeeId = inv.EmployeeId,
                    EmployeeName = inv.EmployeeName,
                    JobTitle = inv.JobTitle,
                    WorkLocation = inv.WorkLocation,
                    BaseSalary = baseSalary,
                    AttendanceRate = attendanceRate,
                    DailyRate = dailyRate,
                    OvertimeDays = overtimeDays,
                    OvertimeValue = overtimeValue,
                    GrossTotal = baseSalary + overtimeValue,
                    Advances = advances,
                    AbsenceDays = absenceDays,
                    AbsenceValue = absenceValue,
                    TotalDeductions = advances + absenceValue,
                    NetSalary = netSalary,
                    PaymentMethod = inv.PreferredPayrollPaymentMethodId == "BANK_TRANSFER" ? "تحويل بنكي" :
                                   inv.PreferredPayrollPaymentMethodId == "CASH" ? "يصرف نقدا" : inv.PreferredPayrollPaymentMethodId,
                    InvoiceDate = inv.InvoiceDate,
                    InvoiceId = inv.InvoiceId,
                    Notes = ""
                });
            }

            // Sort by name
            var sorted = result.OrderBy(x => x.EmployeeName).ToList();
            for (int i = 0; i < sorted.Count; i++)
                sorted[i].Serial = i + 1;

            return Result<List<PayrollData2Dto>>.Success(sorted);
        }
    }
}