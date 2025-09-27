using Application.Shipments.OrganizationGlSettings;
using AutoMapper;
using Domain;
using MediatR;
using Persistence;

namespace Application.Accounting.OrganizationGlSettings
{
    public class GetGlAccountHierarchyLov
    {
        private static List<GlAccountHierarchyViewLovDto> GetChildGlAccounts(string parentId, List<GlAccount> flatGlAccounts, string language)
        {
            // REFACTOR: Added language parameter to support "en" or "ar" for account name selection
            return flatGlAccounts
                .Where(account => account.ParentGlAccountId == parentId)
                .Select(account =>
                {
                    var children = GetChildGlAccounts(account.GlAccountId, flatGlAccounts, language);
                    return new GlAccountHierarchyViewLovDto
                    {
                        GlAccountId = account.GlAccountId,
                        GlAccountTypeId = account.GlAccountTypeId,
                        GlAccountClassId = account.GlAccountClassId,
                        GlResourceTypeId = account.GlResourceTypeId,
                        ParentGlAccountId = account.ParentGlAccountId,
                        AccountCode = account.AccountCode,
                        // REFACTOR: Use Arabic account name in Text if language is "ar"
                        Text = (language == "ar" ? account.AccountNameArabic ?? account.AccountName : account.AccountName) + " (" + account.GlAccountId + ")",
                        // REFACTOR: Select ParentAccountName based on language; use Arabic name if language is "ar"
                        ParentAccountName = flatGlAccounts
                            .Where(a => a.GlAccountId == account.ParentGlAccountId)
                            .Select(a => language == "ar" ? a.AccountNameArabic ?? a.AccountName : a.AccountName)
                            .FirstOrDefault(),
                        Items = children,
                        IsLeaf = !children.Any() // Set IsLeaf to true if there are no children
                    };
                })
                .ToList();
        }

        public class Query : IRequest<Result<List<GlAccountHierarchyViewLovDto>>>
        {
            // REFACTOR: Added Language property to support "en" or "ar" for account name selection
            public string Language { get; set; }
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
                    var flatGlAccounts = _context.GlAccounts.ToList(); // Fetch GL accounts from the database

                    // Build the hierarchy using LINQ
                    var hierarchicalGlAccounts = flatGlAccounts
                        .Where(account => account.ParentGlAccountId == null)
                        .Select(account =>
                        {
                            var children = GetChildGlAccounts(account.GlAccountId, flatGlAccounts, request.Language);
                            return new GlAccountHierarchyViewLovDto
                            {
                                GlAccountId = account.GlAccountId,
                                GlAccountTypeId = account.GlAccountTypeId,
                                GlAccountClassId = account.GlAccountClassId,
                                GlResourceTypeId = account.GlResourceTypeId,
                                ParentGlAccountId = account.ParentGlAccountId,
                                AccountCode = account.AccountCode,
                                // REFACTOR: Use Arabic account name in Text if language is "ar"
                                Text = (request.Language == "ar" ? account.AccountNameArabic ?? account.AccountName : account.AccountName) + " (" + account.GlAccountId + ")",
                                ParentAccountName = null, // Root nodes have no parent
                                Items = children,
                                IsLeaf = !children.Any() // Set IsLeaf for root nodes
                            };
                        })
                        .ToList();

                    return Result<List<GlAccountHierarchyViewLovDto>>.Success(hierarchicalGlAccounts);
                }
                catch (Exception ex)
                {
                    // Handle exception and return an error result
                    return Result<List<GlAccountHierarchyViewLovDto>>.Failure(ex.Message);
                }
            }
        }
    }
}