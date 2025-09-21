using Application.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Parties.Parties;

public class GetSuppliersLov
{
    public class SuppliersEnvelop
    {
        public List<PartyFromPartyIdDto>? Parties { get; set; }
        public int PartyCount { get; set; }
    }

    public class PartyFromPartyIdDto
    {
        public string fromPartyId { get; set; }
        public string fromPartyName { get; set; }
    }

    public class Query : IRequest<Result<SuppliersEnvelop>>
    {
        public PartyLovParams? Params { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<SuppliersEnvelop>>
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

        public async Task<Result<SuppliersEnvelop>> Handle(Query request, CancellationToken cancellationToken)
        {
            // REFACTOR: Validate input
            // Purpose: Prevent null reference exceptions
            // Context: Ensure request parameters are valid
            if (request?.Params == null)
            {
                _logger.LogWarning("Invalid request: Params is null");
                return Result<SuppliersEnvelop>.Failure("Invalid request parameters.");
            }

            var query = _context.Parties
                .Where(x => x.MainRole == "SUPPLIER")
                .AsQueryable();

            // REFACTOR: Update search logic
            // Purpose: Allow searching by both fromPartyId and fromPartyName
            // Context: Modified to include PartyId in search conditions
            if (!string.IsNullOrEmpty(request.Params.SearchTerm))
            {
                var lowerCaseSearchTerm = request.Params.SearchTerm.Trim().ToLower();
                query = query.Where(p => 
                    p.Description!.ToLower().Contains(lowerCaseSearchTerm) ||
                    p.PartyId.ToLower().Contains(lowerCaseSearchTerm));
            }

            var total = await query.CountAsync(cancellationToken);

            var parties = await query
                .OrderBy(x => x.Description)
                .Skip(request.Params.Skip)
                .Take(request.Params.PageSize)
                .ProjectTo<PartyFromPartyIdDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            var partyEnvelop = new SuppliersEnvelop
            {
                Parties = parties,
                PartyCount = total
            };

            return Result<SuppliersEnvelop>.Success(partyEnvelop);
        }
    }
}