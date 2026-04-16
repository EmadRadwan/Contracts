using Application.Core;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Accounting.Invoices;

public class ListPayrollData
{
    public class Query : IRequest<Result<List<PayrollDataDto>>>
    {
        public DateOnly FromDate { get; set; }
        public DateOnly ToDate { get; set; }
        public string OrganizationPartyId { get; set; }
    }

    public class PayrollDataDto
    {
        public string InvoiceId { get; set; }
        public string EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public decimal BaseSalary { get; set; }
        public decimal AbsenceDays { get; set; }
        public decimal AbsenceValue { get; set; }
        public decimal OvertimeDays { get; set; }
        public decimal OvertimeValue { get; set; }
        public decimal NetSalary { get; set; }
        public decimal TotalAdvances { get; set; }
        public DateOnly InvoiceDate { get; set; }
        public string PreferredPayrollPaymentMethodId { get; set; }
        public string SalaryAccountNameArabic { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<List<PayrollDataDto>>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<List<PayrollDataDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            // STEP 1: Load invoices + employee + account info
            var invoices = await (
                    from inv in _context.Invoices
                    where inv.InvoiceTypeId == "PAYROL_INVOICE" &&
                          inv.InvoiceDate >= request.FromDate &&
                          inv.InvoiceDate <= request.ToDate &&
                          inv.PartyId == request.OrganizationPartyId
                    join emp in _context.Parties on inv.PartyIdFrom equals emp.PartyId
                    join pga in _context.PartyGlAccounts
                            .Where(p => p.RoleTypeId == "EMPLOYEE" && p.GlAccountTypeId == "ACCOUNTS_PAYABLE")
                        on emp.PartyId equals pga.PartyId into pgaGroup
                    from pga in pgaGroup.DefaultIfEmpty()
                    join gla in _context.GlAccounts on pga.GlAccountId equals gla.GlAccountId into glaGroup
                    from gla in glaGroup.DefaultIfEmpty()
                    join glaAdv in _context.GlAccounts on emp.GlAccountIdAdvancedPayment equals glaAdv.GlAccountId into
                        glaAdvGroup
                    from glaAdv in glaAdvGroup.DefaultIfEmpty()
                    select new
                    {
                        inv.InvoiceId,
                        inv.InvoiceDate,
                        EmployeeId = emp.PartyId,
                        EmployeeName = emp.Description,
                        emp.PreferredPayrollPaymentMethodId,
                        SalaryAccountNameArabic =
                            glaAdv != null ? glaAdv.AccountNameArabic : (gla != null ? gla.AccountNameArabic : "")
                    }
                )
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            // STEP 2: Load all invoice items in one query
            var invoiceIds = invoices.Select(i => i.InvoiceId).ToList();

            var items = await _context.InvoiceItems
                .Where(ii => invoiceIds.Contains(ii.InvoiceId))
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            // Group items by InvoiceId
            var itemsGrouped = items
                .GroupBy(i => i.InvoiceId)
                .ToDictionary(g => g.Key, g => g.ToList());

            // STEP 3: Build final result in memory
            var result = invoices.Select(r =>
            {
                var invItems = itemsGrouped.TryGetValue(r.InvoiceId, out var list)
                    ? list
                    : new List<Domain.InvoiceItem>();

                var baseSalary = invItems
                    .Where(i => i.InvoiceItemTypeId == "PAYROL_SALARY")
                    .Sum(i => (i.Amount ?? 0) * (i.Quantity ?? 0));

                var absenceValue = invItems
                    .Where(i => i.InvoiceItemTypeId == "PAYROL_DD_ABSENCE")
                    .Sum(i => i.Amount ?? 0);

                var absenceDays = invItems
                    .Where(i => i.InvoiceItemTypeId == "PAYROL_DD_ABSENCE")
                    .Sum(i => i.Quantity ?? 0);

                var overtimeValue = invItems
                    .Where(i => i.InvoiceItemTypeId == "PAYROL_OVERTIME")
                    .Sum(i => i.Amount ?? 0);

                var overtimeDays = invItems
                    .Where(i => i.InvoiceItemTypeId == "PAYROL_OVERTIME")
                    .Sum(i => i.Quantity ?? 0);

                var totalAdvances = invItems
                    .Where(i => i.InvoiceItemTypeId == "PAYROL_DD_ADVANCE")
                    .Sum(i => i.Amount ?? 0);

                var netSalary = baseSalary + overtimeValue - absenceValue - totalAdvances;

                return new PayrollDataDto
                {
                    InvoiceId = r.InvoiceId,
                    InvoiceDate = r.InvoiceDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
                    EmployeeId = r.EmployeeId,
                    EmployeeName = r.EmployeeName,
                    PreferredPayrollPaymentMethodId = r.PreferredPayrollPaymentMethodId,
                    SalaryAccountNameArabic = r.SalaryAccountNameArabic,
                    BaseSalary = baseSalary,
                    AbsenceDays = absenceDays,
                    AbsenceValue = absenceValue,
                    OvertimeDays = overtimeDays,
                    OvertimeValue = overtimeValue,
                    TotalAdvances = totalAdvances,
                    NetSalary = netSalary
                };
            }).ToList();

            return Result<List<PayrollDataDto>>.Success(result);
        }
    }
}