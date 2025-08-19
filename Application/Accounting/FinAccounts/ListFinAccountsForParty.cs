using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Accounting.FinAccounts
{
    public class GetFinAccountsQuery : IRequest<List<FinAccountDto>>
    {
        public string PartyId { get; set; }
    }

    public class FinAccountDto
    {
        public string FinAccountId { get; set; }
        public string FinAccountName { get; set; }
    }

    public class GetFinAccountsQueryHandler : IRequestHandler<GetFinAccountsQuery, List<FinAccountDto>>
    {
        private readonly DataContext _context;

        public GetFinAccountsQueryHandler(DataContext context)
        {
            _context = context;
        }

        public async Task<List<FinAccountDto>> Handle(GetFinAccountsQuery request, CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;

            var finAccounts = await (from fa in _context.FinAccounts
                    join fr in _context.FinAccountRoles
                        on fa.FinAccountId equals fr.FinAccountId
                    where fr.PartyId == request.PartyId &&
                          fa.FinAccountTypeId == "STORE_CREDIT_ACCT" &&
                          fa.StatusId == "FNACT_ACTIVE" &&
                          fr.RoleTypeId == "OWNER" &&
                          fr.FromDate <= now &&
                          (fr.ThruDate == null || fr.ThruDate >= now)
                    select new FinAccountDto
                    {
                        FinAccountId = fa.FinAccountId,
                        FinAccountName = fa.FinAccountName
                    })
                .ToListAsync(cancellationToken);

            return finAccounts;
        }
    }
}