using FluentValidation;
using MediatR;
using Persistence;
using Domain;
using Application.Core;

namespace Application.Accounting.OrganizationGlSettings;

public class AssignGlAccountToOrganization
{
    public class Command : IRequest<Results<AssignGlAccountToOrganizationResult>>
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

    public class Handler : IRequestHandler<Command, Results<AssignGlAccountToOrganizationResult>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Results<AssignGlAccountToOrganizationResult>> Handle(Command request, CancellationToken cancellationToken)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                // Check if the record already exists
                var existing = await _context.GlAccountOrganizations.FindAsync(
                    new object[] { request.GlAccountId, request.CompanyId },
                    cancellationToken);

                if (existing != null)
                {
                    return Results<AssignGlAccountToOrganizationResult>.Failure("Record already exists", "ALREADY_EXISTS");
                }

                var parentRecord = await _context.GlAccounts.FindAsync(request.GlAccountId, cancellationToken);
                if (parentRecord == null)
                {
                    return Results<AssignGlAccountToOrganizationResult>.Failure("GL Account not found", "GL_ACCOUNT_NOT_FOUND");
                }

                var parentAssignedToCompany = await _context.GlAccountOrganizations.FindAsync(
                    new object[] { parentRecord.GlAccountId, request.CompanyId },
                    cancellationToken);

                if (parentAssignedToCompany == null)
                {
                    return Results<AssignGlAccountToOrganizationResult>.Failure("Parent GL Account is not assigned to the specified Company", "PARENT_GL_ACCOUNT_NOT_ASSIGNED");
                }

                var entity = new GlAccountOrganization
                {
                    GlAccountId = request.GlAccountId,
                    OrganizationPartyId = request.CompanyId,
                    FromDate = DateTime.UtcNow,
                    CreatedStamp = DateTime.UtcNow,
                    CreatedTxStamp = DateTime.UtcNow
                };

                _context.GlAccountOrganizations.Add(entity);
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                var result = new AssignGlAccountToOrganizationResult
                {
                    GlAccountId = entity.GlAccountId,
                    OrganizationPartyId = entity.OrganizationPartyId
                };

                return Results<AssignGlAccountToOrganizationResult>.Success(result);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Results<AssignGlAccountToOrganizationResult>.Failure("Error creating GlAccountOrganization", "ERROR_CREATING_RECORD");
            }
        }
    }
}

public class AssignGlAccountToOrganizationResult
{
    public string GlAccountId { get; set; } = null!;
    public string OrganizationPartyId { get; set; } = null!;
}
