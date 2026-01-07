// Application/Parties/UpdateMainRole.cs
using Application.Core;
using Application.Interfaces;
using MediatR;
using Persistence;
using Domain; // assuming Party is in Domain

namespace Application.Parties.Parties
{
    public class UpdateMainRole
    {
        public class Command : IRequest<Result<Unit>>
        {
            public string PartyId { get; set; } = string.Empty;
            public string MainRole { get; set; } = string.Empty;
        }

        public class Handler : IRequestHandler<Command, Result<Unit>>
        {
            private readonly DataContext _context;
            private readonly IUserAccessor _userAccessor;

            public Handler(DataContext context, IUserAccessor userAccessor)
            {
                _context = context;
                _userAccessor = userAccessor;
            }

            public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
            {
                var party = await _context.Parties.FindAsync(request.PartyId);

                if (party == null)
                    return Result<Unit>.Failure("Party not found");

                // Optional: Validate allowed roles
                var allowedRoles = new[] { "CUSTOMER", "SUPPLIER", "EMPLOYEE", "CONTRACTOR" };
                if (!allowedRoles.Contains(request.MainRole))
                    return Result<Unit>.Failure("Invalid main role");

                // Update only MainRole and timestamp
                party.MainRole = request.MainRole;
                party.LastUpdatedStamp = DateTime.UtcNow;

                var success = await _context.SaveChangesAsync(cancellationToken) > 0;

                if (!success)
                    return Result<Unit>.Failure("Failed to update main role");

                return Result<Unit>.Success(Unit.Value);
            }
        }
    }
    
    public class UpdateMainRoleDto
    {
        public string MainRole { get; set; } = string.Empty;
    }
}