using Application.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Shipments.BillingAccounts;

public class ListBillingAccounts
{
    public class Query : IRequest<IQueryable<BillingAccountRecord>>
    {
        public ODataQueryOptions<BillingAccountRecord> Options { get; set; }
    }

    public class Handler : IRequestHandler<Query, IQueryable<BillingAccountRecord>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<IQueryable<BillingAccountRecord>> Handle(Query request,
            CancellationToken cancellationToken)
        {
            var query = (from ba in _context.BillingAccounts
                    join bar in _context.BillingAccountRoles on ba.BillingAccountId equals bar.BillingAccountId
                    join pty in _context.Parties on bar.PartyId equals pty.PartyId
                    join uom in _context.Uoms on ba.AccountCurrencyUomId equals uom.UomId
                    where ba.ThruDate == null
                    select new BillingAccountRecord
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
                ).AsQueryable();

            return query;
        }
    }
}