using Application.Core;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.HumanResources;

public class ListPayrollAdvances
{
    public class Query : IRequest<Results<EmployeeAdvancesResponse>>
    {
        public DateTime InvoiceDate { get; set; }
        public string OrganizationPartyId { get; set; }
        public string Language { get; set; } = "en";
    }

    public class Handler : IRequestHandler<Query, Results<EmployeeAdvancesResponse>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Results<EmployeeAdvancesResponse>> Handle(Query request, CancellationToken ct)
        {
            var language = request.Language;
            
            // Define month boundaries
            var year = request.InvoiceDate.Year;
            var month = request.InvoiceDate.Month;
            
            var monthStart = new DateOnly(year, month, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);

            var query = _context.EmployeeAdvances
                .Include(a => a.EmployeeAdvanceSchedules)
                .Where(adv => 
                    // Case 1: Short-term advances in this month
                    (adv.AdvanceTypeId == "EMPLOYEE_ADVANCE" && 
                     adv.AdvanceDate >= monthStart && adv.AdvanceDate <= monthEnd &&
                     (adv.StatusId == "ADVANCE_APPROVED" || adv.StatusId == "ADVANCE_ACTIVE" || adv.StatusId == "ADVANCE_FULLY_PAID" || adv.StatusId == "ADVANCE_PARTIALLY_PAID")) ||
                    // Case 2: Long-term advances with a schedule in this month
                    (adv.AdvanceTypeId == "EMPLOYEE_LONG_TERM_ADVANCE" &&
                     adv.EmployeeAdvanceSchedules.Any(s => 
                        s.DueDate >= monthStart && s.DueDate <= monthEnd &&
                        (s.StatusId == "SCHEDULED" || s.StatusId == "PAID")))
                )
                .OrderByDescending(adv => adv.AdvanceDate)
                .Select(adv => new EmployeeAdvanceRecord
                {
                    AdvanceId = adv.AdvanceId,
                    PartyId = adv.PartyId,
                    EmployeeName = _context.Parties.Where(p => p.PartyId == adv.PartyId).Select(p => p.Description).FirstOrDefault() ?? adv.PartyId,
                    PaymentId = adv.PaymentId,
                    AdvanceDate = adv.AdvanceDate,
                    AdvanceTypeId = adv.AdvanceTypeId,
                    AdvanceTypeDescription = adv.AdvanceTypeId == "EMPLOYEE_ADVANCE" ? "سلفة راتب" : "سلفة طويلة الأجل",
                    Amount = adv.Amount,
                    InstallmentCount = adv.InstallmentCount,
                    StartDate = adv.StartDate,
                    StatusId = adv.StatusId,
                    StatusDescription = _context.StatusItems.Where(s => s.StatusId == adv.StatusId)
                        .Select(s => language == "ar" ? s.DescriptionArabic : s.Description).FirstOrDefault() ?? adv.StatusId,
                    Description = adv.Description,
                    Schedules = adv.EmployeeAdvanceSchedules
                        .Where(s => s.DueDate >= monthStart && s.DueDate <= monthEnd)
                        .Select(s => new EmployeeAdvanceScheduleRecord
                        {
                            ScheduleId = s.ScheduleId,
                            InstallmentNumber = s.InstallmentNumber,
                            DueDate = s.DueDate,
                            ScheduledAmount = s.ScheduledAmount,
                            DeductedAmount = s.DeductedAmount,
                            StatusId = s.StatusId,
                            PayrolInvoiceId = s.PayrolInvoiceId,
                            Notes = s.Notes
                        }).ToList()
                });

            var data = await query.ToListAsync(ct);

            return Results<EmployeeAdvancesResponse>.Success(new EmployeeAdvancesResponse
            {
                Data = data,
                Total = data.Count
            });
        }
    }
}
