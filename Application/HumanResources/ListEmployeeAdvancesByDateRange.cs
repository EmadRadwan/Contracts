using Application.HumanResources;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using FluentValidation;
using Application.Core;

namespace Application.HumanResources;

public class ListEmployeeAdvancesByDateRange
{
    public class Query : IRequest<Results<EmployeeAdvancesResponse>>
    {
        public DateOnly? FromDate { get; set; }
        public DateOnly? ToDate { get; set; }
        public string Language { get; set; } = "en";
    }

    public class QueryValidator : AbstractValidator<Query>
    {
        public QueryValidator()
        {
            RuleFor(x => x.FromDate).NotEmpty();
            RuleFor(x => x.ToDate).NotEmpty();
        }
    }

    public class Handler : IRequestHandler<Query, Results<EmployeeAdvancesResponse>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context) => _context = context;

        public async Task<Results<EmployeeAdvancesResponse>> Handle(Query request, CancellationToken ct)
        {
            var language = request.Language;

            var query = from adv in _context.EmployeeAdvances
                join party in _context.Parties on adv.PartyId equals party.PartyId into p
                from party in p.DefaultIfEmpty()
                join status in _context.StatusItems on adv.StatusId equals status.StatusId into s
                from status in s.DefaultIfEmpty()
                where adv.AdvanceDate >= request.FromDate && adv.AdvanceDate <= request.ToDate
                orderby adv.AdvanceDate descending
                select new EmployeeAdvanceRecord
                {
                    AdvanceId         = adv.AdvanceId,
                    PartyId           = adv.PartyId,
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
                    Description       = adv.Description
                };

            var data = await query.ToListAsync(ct);

            return Results<EmployeeAdvancesResponse>.Success(new EmployeeAdvancesResponse
            {
                Data = data,
                Total = data.Count
            });
        }
    }
}

public class EmployeeAdvancesResponse
{
    public List<EmployeeAdvanceRecord> Data { get; set; }
    public int Total { get; set; }
}
