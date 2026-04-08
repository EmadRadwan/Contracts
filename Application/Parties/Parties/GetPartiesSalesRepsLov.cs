using Application.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Parties.Parties;

public class GetPartiesSalesRepsLov
{
    public class PartiesEnvelope
    {
        public List<PartyFromPartyIdDto> Parties { get; set; }
        public int PartyCount { get; set; }
    }

    public class Query : IRequest<Result<PartiesEnvelope>>
    {
        public PartyLovParams? Params { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<PartiesEnvelope>>
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

        public async Task<Result<PartiesEnvelope>> Handle(Query request, CancellationToken cancellationToken)
        {
            // REFACTOR: Removed queryByPhone and related logic since phone search is no longer needed.
            // Simplified to use only queryByName, improving code clarity and maintainability.
            var queryByName = _context.Parties
                .Where(x => x.MainRole == "SALES_REP")
                .Select(x => new PartyFromPartyIdDto
                {
                    FromPartyId = x.PartyId,
                    FromPartyName = x.Description,
                    FromPartyPhone = string.Empty
                })
                .AsQueryable();

            var query = Enumerable.Empty<PartyFromPartyIdDto>().AsQueryable();

            // REFACTOR: Simplified search logic to handle only name-based search.
            // Removed phone number parsing and search, as it's no longer required.
            if (!string.IsNullOrEmpty(request.Params.SearchTerm))
            {
                var lowerCaseSearchTerm = request.Params.SearchTerm.Trim().ToLower();
                query = queryByName.Where(p => p.FromPartyName.ToLower().Contains(lowerCaseSearchTerm));
            }
            else
            {
                query = queryByName;
            }

            // REFACTOR: Unchanged pagination and result construction, as they are unaffected by removing phone search.
            var parties = await query
                .OrderBy(x => x.FromPartyName)
                .Skip(request.Params.Skip)
                .Take(request.Params.PageSize)
                .ToListAsync();

            var partyEnvelope = new PartiesEnvelope
            {
                Parties = parties,
                PartyCount = query.Count()
            };

            return Result<PartiesEnvelope>.Success(partyEnvelope);
        }
    }
}