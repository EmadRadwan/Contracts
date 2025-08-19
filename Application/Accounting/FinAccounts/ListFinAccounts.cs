using Application.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Shipments.FinAccounts;

public class ListFinAccounts
{
    public class Query : IRequest<IQueryable<FinAccountRecord>>
    {
        public ODataQueryOptions<FinAccountRecord> Options { get; set; }
    }

    public class Handler : IRequestHandler<Query, IQueryable<FinAccountRecord>>
    {
        private readonly DataContext _context;
        private readonly ILogger<Handler> _logger;
        private readonly IMapper _mapper;
        private readonly IUserAccessor _userAccessor;

        public Handler(DataContext context, IMapper mapper, IUserAccessor userAccessor, ILogger<Handler> logger)
        {
            _mapper = mapper;
            _context = context;
            _userAccessor = userAccessor;
            _logger = logger;
        }

        public async Task<IQueryable<FinAccountRecord>> Handle(Query request,
            CancellationToken cancellationToken)
        {
            var query = (from fa in _context.FinAccounts
                join faType in _context.FinAccountTypes on fa.FinAccountTypeId equals faType.FinAccountTypeId
                join Uoms in _context.Uoms on fa.CurrencyUomId equals Uoms.UomId
                join Orgs in _context.Parties on fa.OrganizationPartyId equals Orgs.PartyId
                join Owners in _context.Parties on fa.OwnerPartyId equals Owners.PartyId
                select new FinAccountRecord
                {
                    FinAccountId = fa.FinAccountId,
                    FinAccountTypeId = fa.FinAccountTypeId,
                    FinAccountTypeDescription = faType.Description,
                    StatusId = fa.StatusId,
                    FinAccountName = fa.FinAccountName,
                    FinAccountCode = fa.FinAccountCode,
                    FinAccountPin = fa.FinAccountPin,
                    CurrencyUomId = fa.CurrencyUomId,
                    CurrencyUomDescription = Uoms.Description,
                    OrganizationPartyId = fa.OrganizationPartyId,
                    OrganizationPartyName = Orgs.Description,
                    OwnerPartyId = fa.OwnerPartyId,
                    OwnerPartyName = Owners.Description,
                    PostToGlAccountId = fa.PostToGlAccountId,
                    FromDate = fa.FromDate,
                    ThruDate = fa.ThruDate,
                    IsRefundable = fa.IsRefundable,
                    ReplenishPaymentId = fa.ReplenishPaymentId,
                    ReplenishLevel = fa.ReplenishLevel,
                    ActualBalance = fa.ActualBalance,
                    AvailableBalance = fa.AvailableBalance
                }).AsQueryable();


            return query;
        }
    }
}