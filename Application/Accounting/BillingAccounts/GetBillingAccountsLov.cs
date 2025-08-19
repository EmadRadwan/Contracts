using Application.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Shipments.BillingAccounts;

public class GetBillingAccountsLov
{
    public class BillingAccountsEnvelop
    {
        public List<BillingAccountLovDto> BillingAccounts { get; set; }
        public int BillingAccountCount { get; set; }
    }

    public class Query : IRequest<Result<BillingAccountsEnvelop>>
    {
        public BillingAccountLovParams? Params { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<BillingAccountsEnvelop>>
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

        public async Task<Result<BillingAccountsEnvelop>> Handle(Query request, CancellationToken cancellationToken)
        {
            var query = from ba in _context.BillingAccounts
                join c in _context.ContactMeches on ba.ContactMechId equals c.ContactMechId
                join pcm in _context.PartyContactMeches on c.ContactMechId equals pcm.ContactMechId
                join p in _context.Parties on pcm.PartyId equals p.PartyId
                where p.Description.Contains(request.Params.SearchTerm) || request.Params.SearchTerm == null
                select new BillingAccountLovDto
                {
                    BillingAccountId = ba.BillingAccountId,
                    ContactMechName = p.Description
                };


            var billingAccounts = await query
                .OrderBy(x => x.ContactMechName)
                .Skip(request.Params.Skip)
                .Take(request.Params.PageSize)
                .ToListAsync(cancellationToken: cancellationToken);

            var billingAccountsEnvelop = new BillingAccountsEnvelop
            {
                BillingAccounts = billingAccounts,
                BillingAccountCount = query.Count()
            };


            return Result<BillingAccountsEnvelop>.Success(billingAccountsEnvelop);
        }
    }
}