using AutoMapper;
using MediatR;
using Persistence;

namespace Application.Parties.Parties;

public class ListCompanies
{
    public class Query : IRequest<Result<List<OrganizationPartyDto>>>
    {
    }

    public class Handler : IRequestHandler<Query, Result<List<OrganizationPartyDto>>>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public Handler(DataContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task<Result<List<OrganizationPartyDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            // list all companies by selecting from the Parties table where 
            // there's a record in the PartyRoles table with a role of 'INTERNAL_ORGANIZATIO'
            var query = from p in _context.Parties
                join pr in _context.PartyRoles on p.PartyId equals pr.PartyId
                where pr.RoleTypeId == "INTERNAL_ORGANIZATIO"
                select new OrganizationPartyDto
                {
                    OrganizationPartyId = p.PartyId,
                    OrganizationPartyName = p.Description
                };

            List<OrganizationPartyDto> results = query.ToList();


            return Result<List<OrganizationPartyDto>>.Success(results);
        }
    }
}