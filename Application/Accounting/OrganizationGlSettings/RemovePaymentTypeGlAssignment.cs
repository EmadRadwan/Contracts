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
    public class RemovePaymentTypeGlAssignment
    {
        public class Command : IRequest<Result<RemovePaymentTypeGlAssignmentResult>>
        {
            public string PaymentTypeId { get; set; }
            public string OrganizationPartyId { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.PaymentTypeId)
                    .NotEmpty().WithMessage("PAYMENT_TYPE_ID must not be empty.");
                RuleFor(x => x.OrganizationPartyId)
                    .NotEmpty().WithMessage("ORGANIZATION_PARTY_ID must not be empty.");
            }
        }

        public class Handler : IRequestHandler<Command, Result<RemovePaymentTypeGlAssignmentResult>>
        {
            private readonly DataContext _context;

            public Handler(DataContext context)
            {
                _context = context;
            }

            public async Task<Result<RemovePaymentTypeGlAssignmentResult>> Handle(Command request, CancellationToken cancellationToken)
            {
                await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    // Retrieve the PaymentGlAccountTypeMap entity based on its composite key.
                    var entity = await _context.PaymentGlAccountTypeMaps.FindAsync(
                        new object[] { request.PaymentTypeId, request.OrganizationPartyId },
                        cancellationToken);

                    if (entity == null)
                    {
                        return Result<RemovePaymentTypeGlAssignmentResult>.Failure("Record not found");
                    }

                    // Remove the entity and persist changes.
                    _context.PaymentGlAccountTypeMaps.Remove(entity);
                    await _context.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);

                    var resultObj = new RemovePaymentTypeGlAssignmentResult
                    {
                        PaymentTypeId = request.PaymentTypeId,
                        OrganizationPartyId = request.OrganizationPartyId
                    };

                    return Result<RemovePaymentTypeGlAssignmentResult>.Success(resultObj);
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return Result<RemovePaymentTypeGlAssignmentResult>.Failure("Error removing Payment GL Account Type Map");
                }
            }
        }
    }

    public class RemovePaymentTypeGlAssignmentResult
    {
        public string PaymentTypeId { get; set; }
        public string OrganizationPartyId { get; set; }
    }
}
