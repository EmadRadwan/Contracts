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
    public class DeleteFixedAssetTypeGlAccount
    {
        public class Command : IRequest<Result<DeleteFixedAssetTypeGlAccountResult>>
        {
            public string FinAccountTypeId { get; set; }
            public string OrganizationPartyId { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.FinAccountTypeId)
                    .NotEmpty().WithMessage("FIN_ACCOUNT_TYPE_ID must not be empty.");
                RuleFor(x => x.OrganizationPartyId)
                    .NotEmpty().WithMessage("ORGANIZATION_PARTY_ID must not be empty.");
            }
        }

        public class Handler : IRequestHandler<Command, Result<DeleteFixedAssetTypeGlAccountResult>>
        {
            private readonly DataContext _context;

            public Handler(DataContext context)
            {
                _context = context;
            }

            public async Task<Result<DeleteFixedAssetTypeGlAccountResult>> Handle(Command request, CancellationToken cancellationToken)
            {
                await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    // Retrieve the entity based on its composite key.
                    var entity = await _context.FixedAssetTypeGlAccounts.FindAsync(
                        new object[] { request.FinAccountTypeId, request.OrganizationPartyId },
                        cancellationToken);

                    if (entity == null)
                    {
                        return Result<DeleteFixedAssetTypeGlAccountResult>.Failure("Record not found");
                    }

                    // Remove the entity and persist changes.
                    _context.FixedAssetTypeGlAccounts.Remove(entity);
                    await _context.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);

                    var resultObj = new DeleteFixedAssetTypeGlAccountResult
                    {
                        FinAccountTypeId = request.FinAccountTypeId,
                        OrganizationPartyId = request.OrganizationPartyId
                    };

                    return Result<DeleteFixedAssetTypeGlAccountResult>.Success(resultObj);
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return Result<DeleteFixedAssetTypeGlAccountResult>.Failure("Error deleting Fixed Asset Type GL Account");
                }
            }
        }
    }

    public class DeleteFixedAssetTypeGlAccountResult
    {
        public string FinAccountTypeId { get; set; }
        public string OrganizationPartyId { get; set; }
    }
}
