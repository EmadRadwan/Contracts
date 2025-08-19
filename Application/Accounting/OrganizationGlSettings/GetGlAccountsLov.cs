using MediatR;
using Persistence;
using Domain;
using Microsoft.EntityFrameworkCore;
using System.Linq; // Ensure this is included

namespace Application.Shipments.GlobalGlSettings
{
    public class GetGlAccountsLov
    {
        public class Query : IRequest<Result<List<GlAccountDto>>>
        {
            public string Language { get; set; }
        }
        private static List<GlAccountDto> GetChildGlAccounts(string parentId,
            List<GlAccount> flatGlAccounts, string language)
        {
            return flatGlAccounts
                .Where(account => account.ParentGlAccountId == parentId)
                .Select(account => new GlAccountDto
                {
                    GlAccountId = account.GlAccountId,
                    GlAccountTypeId = account.GlAccountTypeId,
                    GlAccountTypeDescription = account.GlAccountType != null 
                        ? (language == "en" ? account.GlAccountType.Description : account.GlAccountType.DescriptionArabic) 
                        : "N/A",
                    GlAccountClassId = account.GlAccountClassId,
                    GlResourceTypeId = account.GlResourceTypeId,
                    GlResourceTypeDescription = account.GlAccountClass != null 
                        ? (language == "en" ? account.GlAccountClass.Description : account.GlAccountClass.DescriptionArabic) 
                        : "N/A",
                    ParentGlAccountId = account.ParentGlAccountId,
                    AccountCode = account.AccountCode,
                    AccountName = language == "en" ? account.AccountName : account.AccountNameArabic,
                    ParentAccountName = flatGlAccounts
                        .Where(a => a.GlAccountId == account.ParentGlAccountId)
                        .Select(a => language == "en" ? a.AccountName : a.AccountNameArabic) 
                        .FirstOrDefault() ?? "N/A",
                    Children = GetChildGlAccounts(account.GlAccountId, flatGlAccounts, language)
                })
                .ToList();
        }

        

        public class Handler : IRequestHandler<Query, Result<List<GlAccountDto>>>
        {
            private readonly DataContext _context;

            public Handler(DataContext context)
            {
                _context = context;
            }

            public async Task<Result<List<GlAccountDto>>> Handle(Query request, CancellationToken cancellationToken)
            {
                try
                {
                    // Fetch all accounts into memory
                    var flatGlAccounts = await _context.GlAccounts.ToListAsync(cancellationToken);
                    var language = request.Language;

                    // Check if flatGlAccounts is null or empty
                    if (flatGlAccounts == null || !flatGlAccounts.Any())
                    {
                        return Result<List<GlAccountDto>>.Failure("No GL accounts found.");
                    }

                    var allAccountsLov = flatGlAccounts
                        .Where(a => a.ParentGlAccountId == null)
                        .Select(account => new GlAccountDto
                        {
                            GlAccountId = account.GlAccountId,
                            GlAccountTypeId = account.GlAccountTypeId,
                            GlAccountTypeDescription = account.GlAccountType != null 
                                ? (language == "en" ? account.GlAccountType.Description : account.GlAccountType.DescriptionArabic) 
                                : "N/A",
                            GlAccountClassId = account.GlAccountClassId,
                            GlResourceTypeId = account.GlResourceTypeId,
                            GlResourceTypeDescription = account.GlAccountClass != null 
                                ? (language == "en" ? account.GlAccountClass.Description : account.GlAccountClass.DescriptionArabic) 
                                : "N/A",
                            ParentGlAccountId = account.ParentGlAccountId,
                            AccountCode = account.AccountCode,
                            AccountName = language == "en" ? account.AccountName : account.AccountNameArabic,
                            ParentAccountName = flatGlAccounts
                                .Where(a => a.GlAccountId == account.ParentGlAccountId)
                                .Select(a => language == "en" ? a.AccountName : a.AccountNameArabic) 
                                .FirstOrDefault() ?? "N/A", // Provide a default value if null
                            Children = GetChildGlAccounts(account.GlAccountId, flatGlAccounts, request.Language)
                        })
                        .ToList();

                    // Check if the resulting list is null or empty before returning
                    if (allAccountsLov == null || !allAccountsLov.Any())
                    {
                        return Result<List<GlAccountDto>>.Failure("No GL account LOV found.");
                    }

                    return Result<List<GlAccountDto>>.Success(allAccountsLov);
                }
                catch (Exception ex)
                {
                    // Log the exception message for debugging
                    return Result<List<GlAccountDto>>.Failure($"An error occurred: {ex.Message}");
                }
            }
        }
    }
}