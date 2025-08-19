using Application.Shipments.OrganizationGlSettings;
using AutoMapper;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Accounting.OrganizationGlSettings
{
    public class GetGlAccountOrganizationHierarchyLov
    {
        // Helper: Recursively builds the hierarchy of GL accounts.
        private static List<GlAccountHierarchyViewLovDto> GetChildGlAccounts(string parentId, List<GlAccount> flatGlAccounts)
        {
            return flatGlAccounts
                .Where(account => account.ParentGlAccountId == parentId)
                .Select(account =>
                {
                    var children = GetChildGlAccounts(account.GlAccountId, flatGlAccounts);
                    return new GlAccountHierarchyViewLovDto
                    {
                        GlAccountId = account.GlAccountId,
                        GlAccountTypeId = account.GlAccountTypeId,
                        GlAccountClassId = account.GlAccountClassId,
                        GlResourceTypeId = account.GlResourceTypeId,
                        ParentGlAccountId = account.ParentGlAccountId,
                        AccountCode = account.AccountCode,
                        Text = $"{account.AccountName} ({account.GlAccountId})",
                        ParentAccountName = flatGlAccounts
                            .Where(a => a.GlAccountId == account.ParentGlAccountId)
                            .Select(a => a.AccountName)
                            .FirstOrDefault(),
                        Items = children,
                        IsLeaf = !children.Any()
                    };
                })
                .ToList();
        }

        public class Query : IRequest<Result<List<GlAccountHierarchyViewLovDto>>>
        {
            // Accepts the CompanyId (i.e. OrganizationPartyId)
            public string CompanyId { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<List<GlAccountHierarchyViewLovDto>>>
        {
            private readonly DataContext _context;
            private readonly IMapper _mapper;

            public Handler(DataContext context, IMapper mapper)
            {
                _mapper = mapper;
                _context = context;
            }

            public async Task<Result<List<GlAccountHierarchyViewLovDto>>> Handle(Query request, CancellationToken cancellationToken)
            {
                try
                {
                    // Query the OrganizationGlAccounts table using the CompanyId.
                    // Assume OrganizationGlAccounts has a navigation property "GlAccount" to the underlying GL account.
                    var flatGlAccounts = await _context.GlAccountOrganizations
                        .Include(x => x.GlAccount)
                        .Where(x => x.OrganizationPartyId == request.CompanyId)
                        .OrderBy(x => x.GlAccount.GlAccountClassId)
                        .Select(x => x.GlAccount)
                        .ToListAsync(cancellationToken);

                    // Build the hierarchical tree.
                    var hierarchicalGlAccounts = flatGlAccounts
                        .Where(account => account.ParentGlAccountId == null)
                        .Select(account =>
                        {
                            var children = GetChildGlAccounts(account.GlAccountId, flatGlAccounts);
                            return new GlAccountHierarchyViewLovDto
                            {
                                GlAccountId = account.GlAccountId,
                                GlAccountTypeId = account.GlAccountTypeId,
                                GlAccountClassId = account.GlAccountClassId,
                                GlResourceTypeId = account.GlResourceTypeId,
                                ParentGlAccountId = account.ParentGlAccountId,
                                AccountCode = account.AccountCode,
                                Text = $"{account.AccountName} ({account.GlAccountId})",
                                ParentAccountName = null, // Root nodes have no parent
                                Items = children,
                                IsLeaf = !children.Any()
                            };
                        })
                        .ToList();

                    return Result<List<GlAccountHierarchyViewLovDto>>.Success(hierarchicalGlAccounts);
                }
                catch (Exception ex)
                {
                    return Result<List<GlAccountHierarchyViewLovDto>>.Failure(ex.Message);
                }
            }
        }
    }
}
