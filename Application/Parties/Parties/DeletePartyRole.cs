using Application.Core;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Parties.Parties;

public class DeletePartyRole
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
            var partyRole = await _context.PartyRoles
                .FirstOrDefaultAsync(pr => pr.PartyId == request.PartyId && pr.RoleTypeId == request.RoleTypeId, cancellationToken);

            if (partyRole == null) return Result<Unit>.Failure("Party role not found");

            _context.PartyRoles.Remove(partyRole);

            var result = await _context.SaveChangesAsync(cancellationToken) > 0;

            if (!result) return Result<Unit>.Failure("Failed to delete role from party");

            return Result<Unit>.Success(Unit.Value);
        }
    }
}
