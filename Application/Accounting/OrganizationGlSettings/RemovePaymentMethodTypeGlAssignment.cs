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
    public class RemovePaymentMethodTypeGlAssignment
    {
        public class Command : IRequest<Result<RemovePaymentMethodTypeGlAssignmentResult>>
        {
            public string PaymentMethodTypeId { get; set; }
            public string OrganizationPartyId { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.PaymentMethodTypeId)
                    .NotEmpty().WithMessage("PAYMENT_METHOD_TYPE_ID must not be empty.");
                RuleFor(x => x.OrganizationPartyId)
                    .NotEmpty().WithMessage("ORGANIZATION_PARTY_ID must not be empty.");
            }
        }

        public class Handler : IRequestHandler<Command, Result<RemovePaymentMethodTypeGlAssignmentResult>>
        {
            private readonly DataContext _context;

            public Handler(DataContext context)
            {
                _context = context;
            }

            public async Task<Result<RemovePaymentMethodTypeGlAssignmentResult>> Handle(Command request, CancellationToken cancellationToken)
            {
                await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    // Retrieve the entity based on its composite key.
                    var entity = await _context.PaymentMethodTypeGlAccounts.FindAsync(
                        new object[] { request.PaymentMethodTypeId, request.OrganizationPartyId },
                        cancellationToken);

                    if (entity == null)
                    {
                        return Result<RemovePaymentMethodTypeGlAssignmentResult>.Failure("Record not found");
                    }

                    // Remove the entity and persist changes.
                    _context.PaymentMethodTypeGlAccounts.Remove(entity);
                    await _context.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);

                    var resultObj = new RemovePaymentMethodTypeGlAssignmentResult
                    {
                        PaymentMethodTypeId = request.PaymentMethodTypeId,
                        OrganizationPartyId = request.OrganizationPartyId
                    };

                    return Result<RemovePaymentMethodTypeGlAssignmentResult>.Success(resultObj);
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return Result<RemovePaymentMethodTypeGlAssignmentResult>.Failure("Error removing Payment Method Type GL Assignment");
                }
            }
        }
    }

    public class RemovePaymentMethodTypeGlAssignmentResult
    {
        public string PaymentMethodTypeId { get; set; }
        public string OrganizationPartyId { get; set; }
    }
}
