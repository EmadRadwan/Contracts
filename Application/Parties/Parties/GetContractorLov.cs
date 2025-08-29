using Application.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Parties.Parties;

public class GetContractorsLov
{
    public class ContractorsEnvelop
    {
        public List<PartyFromPartyIdDto>? Parties { get; set; }
        public int PartyCount { get; set; }
    }

    public class Query : IRequest<Result<ContractorsEnvelop>>
    {
        public PartyLovParams? Params { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<ContractorsEnvelop>>
    {
        private readonly DataContext _context;
        private readonly ILogger<Handler> _logger;
        private readonly IMapper _mapper;
        private readonly IUserAccessor _userAccessor;

        public Handler(DataContext context, IMapper mapper, IUserAccessor userAccessor, ILogger<Handler> logger)
        {
            _mapper = mapper;
            _context = context;
            _userAccessor = userAccessor;
            _logger = logger;
        }

        public async Task<Result<ContractorsEnvelop>> Handle(Query request, CancellationToken cancellationToken)
        {
            var query = _context.Parties
                .Where(x => x.MainRole == "Contractor")
                .AsQueryable();

            if (!string.IsNullOrEmpty(request.Params!.SearchTerm))
            {
                var lowerCaseSearchTerm = request.Params.SearchTerm.Trim().ToLower();

                query = query.Where(p => p.Description!.ToLower().Contains(lowerCaseSearchTerm));
            }


            var Partys = await query
                .OrderBy(x => x.Description)
                .Skip(request.Params.Skip)
                .Take(request.Params.PageSize)
                .ProjectTo<PartyFromPartyIdDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

            var PartyEnvelop = new ContractorsEnvelop
            {
                Parties = Partys,
                PartyCount = query.Count()
            };


            return Result<ContractorsEnvelop>.Success(PartyEnvelop);
        }
    }
}