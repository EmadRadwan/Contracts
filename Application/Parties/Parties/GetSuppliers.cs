using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Parties.Parties;

public class GetSuppliers
{
    public class Query : IRequest<Result<List<PartyDto>>>
    {
    }


    public class Handler : IRequestHandler<Query, Result<List<PartyDto>>>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;


        public Handler(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<List<PartyDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var query = _context.Parties
                .Where(y => y.MainRole == "SUPPLIER")
                .OrderBy(x => x.Description)
                .ProjectTo<PartyDto>(_mapper.ConfigurationProvider)
                .AsQueryable();


            var partyTypes = await query
                .ToListAsync();

            return Result<List<PartyDto>>.Success(partyTypes);
        }
    }
}