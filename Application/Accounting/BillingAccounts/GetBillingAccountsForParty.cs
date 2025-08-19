using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Accounting.BillingAccounts
{
    public class GetBillingAccountsQuery : IRequest<List<BillingAccountForPartyDto>>
    {
        public string PartyId { get; set; }
    }

    public class BillingAccountForPartyDto
    {
        public string BillingAccountId { get; set; }
        public string Description { get; set; }
    }

    public class GetBillingAccountsQueryHandler : IRequestHandler<GetBillingAccountsQuery, List<BillingAccountForPartyDto>>
    {
        private readonly DataContext _context;

        public GetBillingAccountsQueryHandler(DataContext context)
        {
            _context = context;
        }

        public async Task<List<BillingAccountForPartyDto>> Handle(GetBillingAccountsQuery request, CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;

            var billingAccounts = await (from ba in _context.BillingAccounts
                    join br in _context.BillingAccountRoles
                        on ba.BillingAccountId equals br.BillingAccountId
                    where br.PartyId == request.PartyId &&
                          br.RoleTypeId == "BILL_TO_ROLE" &&
                          br.FromDate <= now &&
                          (br.ThruDate == null || br.ThruDate >= now)
                    select new BillingAccountForPartyDto
                    {
                        BillingAccountId = ba.BillingAccountId,
                        Description = ba.Description
                    })
                .ToListAsync(cancellationToken);

            return billingAccounts;
        }
    }
}