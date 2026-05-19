using Application.Core;
using Application.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Parties.Parties;

public class GetDepartmentsLov
{
    public class PartiesEnvelope
    {
        public List<PartyFromPartyIdDto> Parties { get; set; } = new();
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

        public Handler(DataContext context, IMapper mapper, ILogger<Handler> logger)
        {
            _mapper = mapper;
            _context = context;
            _logger = logger;
        }

        public async Task<Result<PartiesEnvelope>> Handle(Query request, CancellationToken cancellationToken)
        {
            var queryByName = _context.Parties
                .Where(x => x.PartyTypeId == "TEAM")
                .Select(x => new PartyFromPartyIdDto
                {
                    FromPartyId = x.PartyId,
                    FromPartyName = x.Description ?? string.Empty,
                    FromPartyPhone = string.Empty
                })
                .AsQueryable();

            var query = queryByName;

            if (request.Params != null && !string.IsNullOrEmpty(request.Params.SearchTerm))
            {
                var lowerCaseSearchTerm = request.Params.SearchTerm.Trim().ToLower();
                query = query.Where(p => p.FromPartyName.ToLower().Contains(lowerCaseSearchTerm));
            }

            var partyCount = await query.CountAsync(cancellationToken);

            var parties = await query
                .OrderBy(x => x.FromPartyName)
                .Skip(request.Params?.Skip ?? 0)
                .Take(request.Params?.PageSize ?? 20)
                .ToListAsync(cancellationToken);

            var partyEnvelope = new PartiesEnvelope
            {
                Parties = parties,
                PartyCount = partyCount
            };

            return Result<PartiesEnvelope>.Success(partyEnvelope);
        }
    }
}
