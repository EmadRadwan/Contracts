using MediatR;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.HumanResources;

public class ListEmployeeAdvancesQuery
{
    public class Query : IRequest<IQueryable<EmployeeAdvanceRecord>>
    {
        public ODataQueryOptions<EmployeeAdvanceRecord> Options { get; set; } = null!;
        public string Language { get; set; } = "en";
    }

    public class Handler : IRequestHandler<Query, IQueryable<EmployeeAdvanceRecord>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context) => _context = context;

        public async Task<IQueryable<EmployeeAdvanceRecord>> Handle(Query request, CancellationToken ct)
        {
            var language = request.Language;

            var query = from adv in _context.EmployeeAdvances
                join party in _context.Parties on adv.PartyId equals party.PartyId into p
                from party in p.DefaultIfEmpty()
                join status in _context.StatusItems on adv.StatusId equals status.StatusId into s
                from status in s.DefaultIfEmpty()
                select new EmployeeAdvanceRecord
                {
                    AdvanceId         = adv.AdvanceId,
                    PartyId   = adv.PartyId,
                    EmployeeName      = party != null ? party.Description : adv.PartyId,
                    PaymentId         = adv.PaymentId,
                    AdvanceDate       = adv.AdvanceDate,
                    AdvanceTypeId     = adv.AdvanceTypeId,
                    AdvanceTypeDescription = adv.AdvanceTypeId == "EMPLOYEE_ADVANCE" ? "سلفة راتب" : "سلفة طويلة الأجل",
                    Amount            = adv.Amount,
                    InstallmentCount  = adv.InstallmentCount,
                    StartDate         = adv.StartDate,
                    StatusId          = adv.StatusId,
                    StatusDescription = status != null ? (language == "ar" ? status.DescriptionArabic : status.Description) : adv.StatusId,
                    Description       = adv.Description,
                };

            return query;
        }
    }
}