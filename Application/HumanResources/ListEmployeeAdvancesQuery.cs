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

            var statusLookup = await _context.StatusItems
                .Where(s => s.StatusTypeId == "EMPLOYEE_ADVANCE_STATUS")
                .ToDictionaryAsync(
                    s => s.StatusId,
                    s => language == "ar" 
                        ? (s.DescriptionArabic ?? s.Description ?? s.StatusId) 
                        : (s.Description ?? s.StatusId),
                    ct);

            var query = from adv in _context.EmployeeAdvances
                join party in _context.Parties on adv.PartyId equals party.PartyId into p
                from party in p.DefaultIfEmpty()
                select new EmployeeAdvanceRecord
                {
                    AdvanceId         = adv.AdvanceId,
                    PartyId   = adv.PartyId,
                    EmployeeName      = party != null ? party.Description : adv.PartyId,
                    PaymentId         = adv.PaymentId,
                    AdvanceDate       = adv.AdvanceDate,
                    Amount            = adv.Amount,
                    InstallmentCount  = adv.InstallmentCount,
                    StartDate         = adv.StartDate,
                    StatusId          = adv.StatusId,
                    StatusDescription = statusLookup.GetValueOrDefault(adv.StatusId, adv.StatusId),
                    Description       = adv.Description,
                };

            return query;
        }
    }
}