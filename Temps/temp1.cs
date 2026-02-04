using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Shipments.OrganizationGlSettings  // ← consider moving to Accounting namespace?
{
    public class GetPartyGlAccounts
    {
        public class Query : IRequest<Result<List<GetPartyGlAccountDto>>>
        {
            public string CompanyId { get; set; } = null!;
        }

        public class Handler : IRequestHandler<Query, Result<List<GetPartyGlAccountDto>>>
        {
            private readonly DataContext _context;
            private readonly IMapper _mapper;

            public Handler(DataContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public async Task<Result<List<GetPartyGlAccountDto>>> Handle(
                Query request,
                CancellationToken cancellationToken)
            {
                if (string.IsNullOrWhiteSpace(request.CompanyId))
                {
                    return Result<List<GetPartyGlAccountDto>>.Failure("Company ID is required");
                }

                var query = from pga in _context.PartyGlAccounts
                            join party in _context.Parties 
                                on pga.PartyId equals party.PartyId

                            join role in _context.RoleTypes   // ← added join for role description
                                on pga.RoleTypeId equals role.RoleTypeId

                            join glAcct in _context.GlAccounts
                                on pga.GlAccountId equals glAcct.GlAccountId into glAcctJoin
                                from glAcct in glAcctJoin.DefaultIfEmpty()   // LEFT JOIN (GlAccountId is nullable)

                            join glType in _context.GlAccountTypes
                                on pga.GlAccountTypeId equals glType.GlAccountTypeId   // ← use type from PartyGlAccount

                            where pga.OrganizationPartyId == request.CompanyId

                            select new GetPartyGlAccountDto
                            {
                                PartyId              = pga.PartyId,
                                PartyDescription     = party.Description ?? party.PartyName ?? "Unknown Party",

                                RoleTypeId           = pga.RoleTypeId,
                                RoleDescription      = role.Description ?? role.RoleTypeId,  // fallback

                                GlAccountTypeId      = pga.GlAccountTypeId,
                                GlAccountTypeDescription = glType.Description ?? "Unknown Type",

                                GlAccountId          = pga.GlAccountId,
                                GlAccountName        = glAcct != null 
                                    ? $"{glAcct.GlAccountId} - {glAcct.AccountName ?? glAcct.AccountNameArabic ?? "Unnamed"}"
                                    : "—",  // or "Not Assigned"

                                // Optional extras
                                // CreatedStamp     = pga.CreatedStamp,
                                // LastUpdatedStamp = pga.LastUpdatedStamp
                            };

                var partyGlAccounts = await query
                    .OrderBy(x => x.PartyDescription)
                    .ThenBy(x => x.RoleDescription)
                    .ToListAsync(cancellationToken);

                return Result<List<GetPartyGlAccountDto>>.Success(partyGlAccounts);
            }
        }
    }
}