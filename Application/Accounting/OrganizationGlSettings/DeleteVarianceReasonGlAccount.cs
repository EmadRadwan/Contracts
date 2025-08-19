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
    public class DeleteVarianceReasonGlAccount
    {
        public class Command : IRequest<Result<DeleteVarianceReasonGlAccountResult>>
        {
            public string VarianceReasonId { get; set; }
            public string OrganizationPartyId { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.VarianceReasonId)
                    .NotEmpty().WithMessage("VARIANCE_REASON_ID must not be empty.");
                RuleFor(x => x.OrganizationPartyId)
                    .NotEmpty().WithMessage("ORGANIZATION_PARTY_ID must not be empty.");
            }
        }

        public class Handler : IRequestHandler<Command, Result<DeleteVarianceReasonGlAccountResult>>
        {
            private readonly DataContext _context;

            public Handler(DataContext context)
            {
                _context = context;
            }

            public async Task<Result<DeleteVarianceReasonGlAccountResult>> Handle(Command request, CancellationToken cancellationToken)
            {
                await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    // Retrieve the entity based on its composite key.
                    var entity = await _context.VarianceReasonGlAccounts.FindAsync(
                        new object[] { request.VarianceReasonId, request.OrganizationPartyId },
                        cancellationToken);

                    if (entity == null)
                    {
                        return Result<DeleteVarianceReasonGlAccountResult>.Failure("Record not found");
                    }

                    // Remove the entity and persist changes.
                    _context.VarianceReasonGlAccounts.Remove(entity);
                    await _context.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);

                    var resultObj = new DeleteVarianceReasonGlAccountResult
                    {
                        VarianceReasonId = request.VarianceReasonId,
                        OrganizationPartyId = request.OrganizationPartyId
                    };

                    return Result<DeleteVarianceReasonGlAccountResult>.Success(resultObj);
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return Result<DeleteVarianceReasonGlAccountResult>.Failure("Error deleting Variance Reason GL Account");
                }
            }
        }
    }

    public class DeleteVarianceReasonGlAccountResult
    {
        public string VarianceReasonId { get; set; }
        public string OrganizationPartyId { get; set; }
    }
}
