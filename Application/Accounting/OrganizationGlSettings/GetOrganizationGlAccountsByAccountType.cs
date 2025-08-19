using MediatR;
using Persistence;
using Microsoft.EntityFrameworkCore;

namespace Application.Shipments.OrganizationGlSettings;

public class GetOrganizationGlAccountsByAccountType
{
    public class Query : IRequest<Result<List<OrganizationGlAccountRecord>>>
    {
        public string CompanyId { get; set; }
        public string Type { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<List<OrganizationGlAccountRecord>>>
    {
        private readonly DataContext _context;
        
        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<List<OrganizationGlAccountRecord>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var accountsQuery = _context.GlAccounts
                .Join(_context.GlAccountOrganizations,
                    account => account.GlAccountId,
                    accountOrg => accountOrg.GlAccountId,
                    (account, accountOrg) => new { account, accountOrg })
                .Where(account => account.accountOrg.OrganizationPartyId == request.CompanyId && account.account.GlAccountTypeId == request.Type)
                .Select(account => new OrganizationGlAccountRecord
                {
                    GlAccountId = account.accountOrg.GlAccountId,
                    GlAccountTypeId = account.account.GlAccountTypeId,
                    OrganizationPartyId = account.accountOrg.OrganizationPartyId,
                    GlAccountTypeDescription = account.account.GlAccountType.Description,
                    GlAccountClassId = account.account.GlAccountClassId,
                    GlResourceTypeId = account.account.GlResourceTypeId,
                    GlResourceTypeDescription = account.account.GlAccountClass.Description,
                    ParentGlAccountId = account.account.ParentGlAccountId,
                    AccountCode = account.account.AccountCode,
                    AccountName = account.account.AccountName,
                    Description = account.account.Description,
                    ParentAccountName = _context.GlAccounts
                        .Where(a => a.GlAccountId == account.account.ParentGlAccountId)
                        .Select(a => a.AccountName)
                        .FirstOrDefault()
                });

            var result = await accountsQuery.ToListAsync(cancellationToken);

            return Result<List<OrganizationGlAccountRecord>>.Success(result);
        }
    }
}
