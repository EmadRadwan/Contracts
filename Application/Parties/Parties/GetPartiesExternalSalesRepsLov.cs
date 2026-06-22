using Application.Core;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Parties.Parties;

public class GetPartiesExternalSalesRepsLov
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

        public Handler(DataContext context) => _context = context;

        public async Task<Result<PartiesEnvelope>> Handle(Query request, CancellationToken cancellationToken)
        {
            var queryByName = _context.Parties
                .Where(x => x.MainRole == "EXTERNAL_SALES_REP" ||
                            _context.PartyRoles.Any(pr => pr.PartyId == x.PartyId && pr.RoleTypeId == "EXTERNAL_SALES_REP"))
                .Select(x => new PartyFromPartyIdDto
                {
                    FromPartyId = x.PartyId,
                    FromPartyName = x.Description,
                    FromPartyPhone = string.Empty
                })
                .AsQueryable();

            var query = queryByName;

            if (!string.IsNullOrEmpty(request.Params?.SearchTerm))
            {
                var lower = request.Params.SearchTerm.Trim().ToLower();
                query = queryByName.Where(p => p.FromPartyName.ToLower().Contains(lower));
            }

            var parties = await query
                .OrderBy(x => x.FromPartyName)
                .Skip(request.Params?.Skip ?? 0)
                .Take(request.Params?.PageSize ?? 10)
                .ToListAsync(cancellationToken);

            return Result<PartiesEnvelope>.Success(new PartiesEnvelope
            {
                Parties = parties,
                PartyCount = query.Count()
            });
        }
    }
}
