using Application.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Shipments.OrganizationGlSettings;

public class ListGlAccountTypeDefaults
{
    public class Query : IRequest<IQueryable<GlAccountTypeDefaultRecord>>
    {
        public ODataQueryOptions<GlAccountTypeDefaultRecord> Options { get; set; }
        public string? CompanyId { get; set; }
    }

    public class Handler : IRequestHandler<Query, IQueryable<GlAccountTypeDefaultRecord>>
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

        public async Task<IQueryable<GlAccountTypeDefaultRecord>> Handle(Query request,
            CancellationToken cancellationToken)
        {
            var accountsQuery = _context.GlAccounts
                .Join(_context.GlAccountTypeDefaults,
                    account => account.GlAccountId,
                    accountDef => accountDef.GlAccountId,
                    (account, accountDef) => new { account, accountDef })
                .Where(account => account.accountDef.OrganizationPartyId == request.CompanyId)
                .Select(account => new GlAccountTypeDefaultRecord
                {
                    GlAccountId = account.accountDef.GlAccountId,
                    GlAccountTypeId = account.account.GlAccountTypeId,
                    GlAccountTypeDescription = account.account.GlAccountType.Description,
                    OrganizationPartyId = account.accountDef.OrganizationPartyId,
                    AccountName = _context.GlAccounts
                        .Where(a => a.GlAccountId == account.account.ParentGlAccountId)
                        .Select(a => a.AccountName)
                        .FirstOrDefault()
                });

            return accountsQuery;
        }
    }
}