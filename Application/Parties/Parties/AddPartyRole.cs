using Application.Core;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Parties.Parties;

public class AddPartyRole
{
    public class Command : IRequest<Result<Unit>>
    {
        public string PartyId { get; set; } = null!;
        public string RoleTypeId { get; set; } = null!;
    }

    public class Handler : IRequestHandler<Command, Result<Unit>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var party = await _context.Parties.FindAsync(new object[] { request.PartyId }, cancellationToken);
            if (party == null) return Result<Unit>.Failure("Party not found");

            var roleType = await _context.RoleTypes.FindAsync(new object[] { request.RoleTypeId }, cancellationToken);
            if (roleType == null) return Result<Unit>.Failure("Role type not found");

            var exists = await _context.PartyRoles
                .AnyAsync(pr => pr.PartyId == request.PartyId && pr.RoleTypeId == request.RoleTypeId, cancellationToken);
            
            if (exists) return Result<Unit>.Failure("Party already has this role");

            var partyRole = new PartyRole
            {
                PartyId = request.PartyId,
                RoleTypeId = request.RoleTypeId,
                CreatedStamp = DateTime.UtcNow,
                LastUpdatedStamp = DateTime.UtcNow
            };

            _context.PartyRoles.Add(partyRole);

            var result = await _context.SaveChangesAsync(cancellationToken) > 0;

            if (!result) return Result<Unit>.Failure("Failed to add role to party");

            return Result<Unit>.Success(Unit.Value);
        }
    }
}
