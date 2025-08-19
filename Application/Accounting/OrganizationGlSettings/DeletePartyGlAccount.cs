using Application.Accounting.Services;
using Application.Accounting.Services.Models;
using Domain;
using FluentValidation;
using MediatR;
using Persistence;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Accounting.OrganizationGlSettings
{
    public class DeletePartyGlAccount
    {
        public class Command : IRequest<Result<DeletePartyGlAccountResult>>
        {
            public string OrganizationPartyId { get; set; }
            public string PartyId { get; set; }
            public string RoleTypeId { get; set; }
            public string GlAccountTypeId { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.OrganizationPartyId)
                    .NotEmpty().WithMessage("ORGANIZATION_PARTY_ID must not be empty.");
                RuleFor(x => x.PartyId)
                    .NotEmpty().WithMessage("PARTY_ID must not be empty.");
                RuleFor(x => x.RoleTypeId)
                    .NotEmpty().WithMessage("ROLE_TYPE_ID must not be empty.");
                RuleFor(x => x.GlAccountTypeId)
                    .NotEmpty().WithMessage("GL_ACCOUNT_TYPE_ID must not be empty.");
            }
        }

        public class Handler : IRequestHandler<Command, Result<DeletePartyGlAccountResult>>
        {
            private readonly DataContext _context;

            public Handler(DataContext context)
            {
                _context = context;
            }

            public async Task<Result<DeletePartyGlAccountResult>> Handle(Command request, CancellationToken cancellationToken)
            {
                await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    // Retrieve the entity based on its composite key.
                    var entity = await _context.PartyGlAccounts.FindAsync(
                        new object[] { request.OrganizationPartyId, request.PartyId, request.RoleTypeId, request.GlAccountTypeId },
                        cancellationToken);

                    if (entity == null)
                    {
                        return Result<DeletePartyGlAccountResult>.Failure("Record not found");
                    }

                    // Remove the entity and persist changes.
                    _context.PartyGlAccounts.Remove(entity);
                    await _context.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);

                    var resultObj = new DeletePartyGlAccountResult
                    {
                        OrganizationPartyId = request.OrganizationPartyId,
                        PartyId = request.PartyId,
                        RoleTypeId = request.RoleTypeId,
                        GlAccountTypeId = request.GlAccountTypeId
                    };

                    return Result<DeletePartyGlAccountResult>.Success(resultObj);
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return Result<DeletePartyGlAccountResult>.Failure("Error deleting Party GL Account");
                }
            }
        }
    }

    public class DeletePartyGlAccountResult
    {
        public string OrganizationPartyId { get; set; }
        public string PartyId { get; set; }
        public string RoleTypeId { get; set; }
        public string GlAccountTypeId { get; set; }
    }
}
