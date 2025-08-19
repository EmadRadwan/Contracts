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
    public class RemoveGlAccountTypeDefault
    {
        public class Command : IRequest<Result<RemoveGlAccountTypeDefaultResult>>
        {
            public string GlAccountTypeId { get; set; }
            public string OrganizationPartyId { get; set; }
            public string GlAccountId { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.GlAccountTypeId)
                    .NotEmpty().WithMessage("GL_ACCOUNT_TYPE_ID must not be empty.");
                RuleFor(x => x.OrganizationPartyId)
                    .NotEmpty().WithMessage("ORGANIZATION_PARTY_ID must not be empty.");
                RuleFor(x => x.GlAccountId)
                    .NotEmpty().WithMessage("GL_ACCOUNT_ID must not be empty.");
            }
        }

        public class Handler : IRequestHandler<Command, Result<RemoveGlAccountTypeDefaultResult>>
        {
            private readonly DataContext _context;

            public Handler(DataContext context)
            {
                _context = context;
            }

            public async Task<Result<RemoveGlAccountTypeDefaultResult>> Handle(Command request, CancellationToken cancellationToken)
            {
                await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    // Retrieve the entity based on its composite key.
                    var entity = await _context.GlAccountTypeDefaults.FindAsync(new object[] { request.GlAccountTypeId, request.OrganizationPartyId, request.GlAccountId }, cancellationToken);
                    if (entity == null)
                    {
                        return Result<RemoveGlAccountTypeDefaultResult>.Failure("Record not found");
                    }

                    // Remove the entity and persist changes.
                    _context.GlAccountTypeDefaults.Remove(entity);
                    await _context.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);

                    var resultObj = new RemoveGlAccountTypeDefaultResult
                    {
                        GlAccountTypeId = request.GlAccountTypeId,
                        OrganizationPartyId = request.OrganizationPartyId,
                        GlAccountId = request.GlAccountId
                    };

                    return Result<RemoveGlAccountTypeDefaultResult>.Success(resultObj);
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return Result<RemoveGlAccountTypeDefaultResult>.Failure("Error removing GL Account Type Default");
                }
            }
        }
    }

    public class RemoveGlAccountTypeDefaultResult
    {
        public string GlAccountTypeId { get; set; }
        public string OrganizationPartyId { get; set; }
        public string GlAccountId { get; set; }
    }
}
