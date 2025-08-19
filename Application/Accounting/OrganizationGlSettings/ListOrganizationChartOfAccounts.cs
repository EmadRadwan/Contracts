using Application.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Shipments.OrganizationGlSettings;

public class ListOrganizationChartOfAccounts
{
    public class Query : IRequest<IQueryable<OrganizationGlAccountRecord>>
    {
        public ODataQueryOptions<OrganizationGlAccountRecord> Options { get; set; }
        public string? CompanyId { get; set; }
    }

    public class Handler : IRequestHandler<Query, IQueryable<OrganizationGlAccountRecord>>
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

        public async Task<IQueryable<OrganizationGlAccountRecord>> Handle(Query request,
            CancellationToken cancellationToken)
        {
            var accountsQuery = _context.GlAccounts
                // Inner join: only include accounts with a matching organization record
                .Join(_context.GlAccountOrganizations,
                    account => account.GlAccountId,
                    accountOrg => accountOrg.GlAccountId,
                    (account, accountOrg) => new { account, accountOrg })
                // Filter to include only records for the specific CompanyId
                .Where(x => x.accountOrg.OrganizationPartyId == request.CompanyId)
                // Left join with GlAccountTypes to allow missing type data
                .GroupJoin(_context.GlAccountTypes,
                    x => x.account.GlAccountTypeId,
                    accountType => accountType.GlAccountTypeId,
                    (x, accountTypes) => new { x.account, x.accountOrg, accountTypes })
                .SelectMany(
                    x => x.accountTypes.DefaultIfEmpty(),
                    (x, accountType) => new { x.account, x.accountOrg, accountType })
                // Left join with GlAccountClasses to allow missing class data
                .GroupJoin(_context.GlAccountClasses,
                    x => x.account.GlAccountClassId,
                    accountClass => accountClass.GlAccountClassId,
                    (x, accountClasses) => new { x.account, x.accountOrg, x.accountType, accountClasses })
                .SelectMany(
                    x => x.accountClasses.DefaultIfEmpty(),
                    (x, accountClass) => new OrganizationGlAccountRecord
                    {
                        // Use the Organization record's GlAccountId
                        GlAccountId = x.accountOrg.GlAccountId,
                        GlAccountTypeId = x.account.GlAccountTypeId,
                        OrganizationPartyId = x.accountOrg.OrganizationPartyId,
                        GlAccountTypeDescription = x.accountType.Description ?? "",
                        GlAccountClassId = x.account.GlAccountClassId,
                        GlResourceTypeId = x.account.GlResourceTypeId,
                        GlResourceTypeDescription = accountClass.Description ?? "",
                        ParentGlAccountId = x.account.ParentGlAccountId,
                        AccountCode = x.account.AccountCode,
                        AccountName = x.account.AccountName,
                        Description = x.account.Description,
                        ParentAccountName = _context.GlAccounts
                            .Where(a => a.GlAccountId == x.account.ParentGlAccountId)
                            .Select(a => a.AccountName)
                            .FirstOrDefault() ?? ""
                    });

            return accountsQuery;
        }
    }
}