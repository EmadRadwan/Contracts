using Application.Core;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Parties.Parties;

public class ListEmployeesWithSalary
{
    public class Query : IRequest<Result<List<EmployeeSalaryDto>>>
    {
    }

    public class EmployeeSalaryDto
    {
        public string PartyId { get; set; }
        public string Name { get; set; }
        public decimal MonthlyBaseSalary { get; set; }
        public string StatusId { get; set; }
        public string SalaryAccountNameArabic { get; set; }
        public string GlAccountIdAdvancedPayment { get; set; }
        public string AdvancedPaymentAccountNameArabic { get; set; }
        public string PreferredPayrollPaymentMethodId { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<List<EmployeeSalaryDto>>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<List<EmployeeSalaryDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var query = from prty in _context.Parties
                where prty.MainRole == "EMPLOYEE" && prty.StatusId == "PARTY_ENABLED"
                && !_context.PartyRoles.Any(pr => pr.PartyId == prty.PartyId && pr.RoleTypeId == "PREVIOUS_EMPLOYEE")
                
                // Latest Monthly Base Salary
                join ra in _context.RateAmounts
                        .Where(r => r.PeriodTypeId == "RATE_MONTH")
                    on prty.PartyId equals ra.PartyId into raGroup
                from ra in raGroup.DefaultIfEmpty()

                // Salary Account info
                join pga in _context.PartyGlAccounts.Where(p => p.RoleTypeId == "EMPLOYEE" && p.GlAccountTypeId == "ACCOUNTS_PAYABLE")
                    on prty.PartyId equals pga.PartyId into pgaGroup
                from pga in pgaGroup.DefaultIfEmpty()
                join gla in _context.GlAccounts on pga.GlAccountId equals gla.GlAccountId into glaGroup
                from gla in glaGroup.DefaultIfEmpty()

                // Advanced Payment Account info
                join glaAdv in _context.GlAccounts on prty.GlAccountIdAdvancedPayment equals glaAdv.GlAccountId into glaAdvGroup
                from glaAdv in glaAdvGroup.DefaultIfEmpty()
                
                select new
                {
                    prty.PartyId,
                    prty.Description,
                    prty.StatusId,
                    RateAmount = ra,
                    SalaryAccountNameArabic = gla.AccountNameArabic,
                    GlAccountIdAdvancedPayment = prty.GlAccountIdAdvancedPayment,
                    AdvancedPaymentAccountNameArabic = glaAdv.AccountNameArabic,
                    PreferredPayrollPaymentMethodId = prty.PreferredPayrollPaymentMethodId
                };

            var rawResults = await query
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            // Since an employee might have multiple rates, we need to pick the latest one per employee
            // although the join above might return duplicates if we don't group or filter by date
            var employees = rawResults
                .GroupBy(r => r.PartyId)
                .Select(g => {
                    var latestRate = g.OrderByDescending(r => r.RateAmount != null ? r.RateAmount.FromDate : DateTime.MinValue)
                                      .FirstOrDefault();
                    return new EmployeeSalaryDto
                    {
                        PartyId = g.Key,
                        Name = latestRate.Description,
                        StatusId = latestRate.StatusId,
                        MonthlyBaseSalary = latestRate.RateAmount?.Amount ?? 0,
                        SalaryAccountNameArabic = latestRate.SalaryAccountNameArabic,
                        GlAccountIdAdvancedPayment = latestRate.GlAccountIdAdvancedPayment,
                        AdvancedPaymentAccountNameArabic = latestRate.AdvancedPaymentAccountNameArabic,
                        PreferredPayrollPaymentMethodId = latestRate.PreferredPayrollPaymentMethodId
                    };
                })
                .ToList();

            return Result<List<EmployeeSalaryDto>>.Success(employees);
        }
    }
}
