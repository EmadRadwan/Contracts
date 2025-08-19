using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Shipments.OrganizationGlSettings
{
    public class GetPartyGlAccounts
    {
        public class Query : IRequest<Result<List<GetPartyGlAccountDto>>>
        {
            public string CompanyId { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<List<GetPartyGlAccountDto>>>
        {
            private readonly DataContext _context;
            private readonly IMapper _mapper;

            public Handler(DataContext context, IMapper mapper)
            {
                _mapper = mapper;
                _context = context;
            }

            public async Task<Result<List<GetPartyGlAccountDto>>> Handle(Query request,
                CancellationToken cancellationToken)
            {
                var partyGlAccounts = await (from pga in _context.PartyGlAccounts
                    join a in _context.GlAccounts
                        on pga.GlAccountId equals a.GlAccountId
                    join p in _context.Parties
                        on pga.PartyId equals p.PartyId
                    where pga.OrganizationPartyId == request.CompanyId
                    select new GetPartyGlAccountDto
                    {
                        PartyId = p.PartyId,
                        PartyDescription = p.Description, // Assuming PartyName is the description field
                        GlAccountId = pga.GlAccountId,
                        GlAccountName = pga.GlAccountId + " - " + a.AccountName // Concatenate GlAccountId and GlAccountName
                    }).ToListAsync(cancellationToken);

                return Result<List<GetPartyGlAccountDto>>.Success(partyGlAccounts!);
            }
        }
    }
}