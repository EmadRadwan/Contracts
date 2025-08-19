using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Shipments.BillingAccounts;

public class ListPartyBillingAccounts
{
    public class Query : IRequest<Result<List<BillingAccountDto>>>
    {
        public string PartyId { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<List<BillingAccountDto>>>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public Handler(DataContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task<Result<List<BillingAccountDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var partyBillingAccounts = await (from ba in _context.BillingAccounts
                    join bar in _context.BillingAccountRoles on ba.BillingAccountId equals bar.BillingAccountId
                    join pty in _context.Parties on bar.PartyId equals pty.PartyId
                    join uom in _context.Uoms on ba.AccountCurrencyUomId equals uom.UomId
                    where ba.ThruDate == null && bar.PartyId == request.PartyId
                    select new BillingAccountDto
                    {
                        BillingAccountId = ba.BillingAccountId,
                        AccountLimit = ba.AccountLimit,
                        AccountCurrencyUomId = ba.AccountCurrencyUomId,
                        AccountCurrencyUomDescription = uom.Description,
                        PartyId = bar.PartyId,
                        PartyName = pty.Description,
                        FromDate = ba.FromDate,
                        ThruDate = ba.ThruDate
                    }
                ).ToListAsync(cancellationToken);

            return Result<List<BillingAccountDto>>.Success(partyBillingAccounts);
        }
    }
}