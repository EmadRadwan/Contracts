using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Accounting.OrganizationGlSettings;

public class ListInternalAccountingOrganizationsLov
{
    public class Query : IRequest<Result<List<InternalAccountingOrganizationDto>>>
    {
    }

    public class Handler : IRequestHandler<Query, Result<List<InternalAccountingOrganizationDto>>>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public Handler(DataContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task<Result<List<InternalAccountingOrganizationDto>>> Handle(Query request,
            CancellationToken cancellationToken)
        {
            var internalOrganizations = await _context.Parties
            .Join(_context.PartyAcctgPreferences,
                p => p.PartyId,
                pap => pap.PartyId,
                (p, pap) => new { Party = p, Preferences = pap })
            .Join(_context.PartyRoles,
                pp => pp.Party.PartyId,
                pr => pr.PartyId,
                (pp, pr) => new { pp.Party, pp.Preferences, Role = pr })
            .Where(x => x.Preferences.EnableAccounting == "Y" && x.Role.RoleTypeId == "INTERNAL_ORGANIZATIO")
            .Select(x => new InternalAccountingOrganizationDto
            {
                PartyId = x.Party.PartyId,
                PartyName = x.Party.Description
            }).ToListAsync(cancellationToken);


            return Result<List<InternalAccountingOrganizationDto>>.Success(internalOrganizations!);
        }
    }
}