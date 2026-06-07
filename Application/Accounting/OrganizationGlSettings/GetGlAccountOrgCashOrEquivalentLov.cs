using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Accounting.OrganizationGlSettings
{
    public class GetGlAccountOrgCashOrEquivalentLov
    {
        public class Query : IRequest<Result<List<GlAccountCashEquivalentDto>>>
        {
            public string CompanyId { get; set; } // OrganizationPartyId
            public string Language { get; set; } = "en"; // Optional: default to English
        }

        // Simple flat DTO — no hierarchy needed
        public class GlAccountCashEquivalentDto
        {
            public string GlAccountId { get; set; }
            public string GlAccountTypeId { get; set; }
            public string GlAccountClassId { get; set; }
            public string ParentGlAccountId { get; set; }
            public string AccountCode { get; set; }
            public string AccountName { get; set; } // Will be language-aware
            public string Description { get; set; }
            public string AccountNameArabic { get; set; }
            public string OrganizationPartyId { get; set; }

            // Display text for dropdowns (e.g., "Cash in Bank - ABC (1001001)")
            public string DisplayText => $"{AccountName} ({GlAccountId})";
        }

        public class Handler : IRequestHandler<Query, Result<List<GlAccountCashEquivalentDto>>>
        {
            private readonly DataContext _context;
            private readonly IMapper _mapper;

            public Handler(DataContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public async Task<Result<List<GlAccountCashEquivalentDto>>> Handle(Query request,
                CancellationToken cancellationToken)
            {
                try
                {
                    // REFACTOR: Simplified to flat list, exactly matching your SQL
                    // REASON: No hierarchy needed → much faster, cleaner, and predictable
                    var language = request.Language?.ToLower() ?? "en";

                    var cashAccounts = await _context.GlAccountOrganizations
                        .Include(x => x.GlAccount)
                        .Where(x =>
                            x.OrganizationPartyId == request.CompanyId &&
                            (x.GlAccount.GlAccountClassId == "CASH_EQUIVALENT" ||
                             x.GlAccount.GlAccountId == "302000" ||
                             x.GlAccount.GlAccountId == "303000"))
                        .OrderBy(x => x.GlAccount.GlAccountId)
                        .ThenBy(x => x.OrganizationPartyId)
                        .Select(x => new GlAccountCashEquivalentDto
                        {
                            GlAccountId = x.GlAccount.GlAccountId,
                            GlAccountTypeId = x.GlAccount.GlAccountTypeId,
                            GlAccountClassId = x.GlAccount.GlAccountClassId,
                            ParentGlAccountId = x.GlAccount.ParentGlAccountId,
                            AccountCode = x.GlAccount.AccountCode,
                            Description = x.GlAccount.Description,
                            AccountNameArabic = x.GlAccount.AccountNameArabic,
                            OrganizationPartyId = x.OrganizationPartyId,

                            // REFACTOR: Language-aware name selection
                            // REASON: Support Arabic/English display in dropdowns
                            AccountName = language == "ar"
                                ? (x.GlAccount.AccountNameArabic ?? x.GlAccount.AccountName)
                                : x.GlAccount.AccountName
                        })
                        .ToListAsync(cancellationToken);

                    return Result<List<GlAccountCashEquivalentDto>>.Success(cashAccounts);
                }
                catch (Exception ex)
                {
                    return Result<List<GlAccountCashEquivalentDto>>.Failure(ex.Message);
                }
            }
        }
    }
}