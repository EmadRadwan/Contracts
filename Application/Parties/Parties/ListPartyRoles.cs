using Application.Core;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Parties.Parties;

public class ListPartyRoles
{
    public class Query : IRequest<Result<List<RoleDto>>>
    {
        public string PartyId { get; set; } = null!;
    }

    public class Handler : IRequestHandler<Query, Result<List<RoleDto>>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<List<RoleDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var roles = await _context.PartyRoles
                .Where(pr => pr.PartyId == request.PartyId)
                .Select(pr => new RoleDto
                {
                    RoleTypeId = pr.RoleTypeId,
                    RoleName = pr.RoleType.Description
                })
                .ToListAsync(cancellationToken);

            return Result<List<RoleDto>>.Success(roles);
        }
    }
}
