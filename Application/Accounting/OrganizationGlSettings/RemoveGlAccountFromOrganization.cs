using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Application.Core;

namespace Application.Accounting.OrganizationGlSettings;

public class RemoveGlAccountFromOrganization
{
    public class Command : IRequest<Results<Unit>>
    {
        public string GlAccountId { get; set; } = null!;
        public string CompanyId { get; set; } = null!;
    }

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(x => x.GlAccountId)
                .NotEmpty().WithMessage("GL_ACCOUNT_ID must not be empty.");
            RuleFor(x => x.CompanyId)
                .NotEmpty().WithMessage("COMPANY_ID must not be empty.");
        }
    }

    public class Handler : IRequestHandler<Command, Results<Unit>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Results<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            // Check if the account is assigned to the organization
            var existingAssignment = await _context.GlAccountOrganizations
                .FirstOrDefaultAsync(x => x.GlAccountId == request.GlAccountId
                                       && x.OrganizationPartyId == request.CompanyId,
                    cancellationToken);

            if (existingAssignment == null)
            {
                return Results<Unit>.Failure("GL Account is not assigned to this organization", "NOT_ASSIGNED");
            }

            // Get all child accounts of the input account
            var childAccountIds = await _context.GlAccounts
                .Where(a => a.ParentGlAccountId == request.GlAccountId)
                .Select(a => a.GlAccountId)
                .ToListAsync(cancellationToken);

            // Check if any child accounts are assigned to the organization
            if (childAccountIds.Any())
            {
                var hasAssignedChildren = await _context.GlAccountOrganizations
                    .AnyAsync(x => childAccountIds.Contains(x.GlAccountId)
                                && x.OrganizationPartyId == request.CompanyId,
                        cancellationToken);

                if (hasAssignedChildren)
                {
                    return Results<Unit>.Failure(
                        "Cannot remove this account because it has child accounts assigned to the organization. Please remove child accounts first.",
                        "HAS_ASSIGNED_CHILDREN");
                }
            }

            // Remove the assignment
            _context.GlAccountOrganizations.Remove(existingAssignment);
            await _context.SaveChangesAsync(cancellationToken);

            return Results<Unit>.Success(Unit.Value);
        }
    }
}
