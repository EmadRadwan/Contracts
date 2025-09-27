using Application.Interfaces;
using Application.Shipments.OrganizationGlSettings;
using MediatR;
using Persistence;

namespace Application.Accounting.OrganizationGlSettings
{
    public class ListFullOrganizationChartOfAccounts
    {
        public class Query : IRequest<IEnumerable<OrganizationGlAccountRecord>>
        {
            public string? CompanyId { get; set; }

            // REFACTOR: Added Language property to support "en" or "ar" for account name selection
            public string Language { get; set; }
        }

        public class Handler : IRequestHandler<Query, IEnumerable<OrganizationGlAccountRecord>>
        {
            private readonly DataContext _context;

            public Handler(DataContext context)
            {
                _context = context;
            }

            public async Task<IEnumerable<OrganizationGlAccountRecord>> Handle(Query request,
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
                            GlAccountTypeDescription = x.accountType.Description ?? string.Empty,
                            GlAccountClassId = x.account.GlAccountClassId,
                            GlResourceTypeId = x.account.GlResourceTypeId,
                            GlResourceTypeDescription = accountClass.Description ?? string.Empty,
                            ParentGlAccountId = x.account.ParentGlAccountId,
                            AccountCode = x.account.AccountCode,
                            // REFACTOR: Select AccountName based on Language; use Arabic name if Language is "ar"
                            AccountName = request.Language == "ar"
                                ? x.account.AccountNameArabic ?? x.account.AccountName
                                : x.account.AccountName,
                            Description = x.account.Description,
                            ParentAccountName = _context.GlAccounts
                                .Where(a => a.GlAccountId == x.account.ParentGlAccountId)
                                .Select(a =>
                                    request.Language == "ar" ? a.AccountNameArabic ?? a.AccountName : a.AccountName)
                                .FirstOrDefault() ?? string.Empty
                        });

                // No paging, so we simply execute and return the complete list
                return await Task.FromResult(accountsQuery.ToList());
            }
        }
    }
}