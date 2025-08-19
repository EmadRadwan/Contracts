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
    public class DeleteCreditCardTypeGlAccount
    {
        public class Command : IRequest<Result<DeleteCreditCardTypeGlAccountResult>>
        {
            public string CardType { get; set; }
            public string OrganizationPartyId { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.CardType)
                    .NotEmpty().WithMessage("CARD_TYPE must not be empty.");
                RuleFor(x => x.OrganizationPartyId)
                    .NotEmpty().WithMessage("ORGANIZATION_PARTY_ID must not be empty.");
            }
        }

        public class Handler : IRequestHandler<Command, Result<DeleteCreditCardTypeGlAccountResult>>
        {
            private readonly DataContext _context;

            public Handler(DataContext context)
            {
                _context = context;
            }

            public async Task<Result<DeleteCreditCardTypeGlAccountResult>> Handle(Command request, CancellationToken cancellationToken)
            {
                await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    // Retrieve the entity based on its composite key.
                    var entity = await _context.CreditCardTypeGlAccounts.FindAsync(
                        new object[] { request.CardType, request.OrganizationPartyId },
                        cancellationToken);

                    if (entity == null)
                    {
                        return Result<DeleteCreditCardTypeGlAccountResult>.Failure("Record not found");
                    }

                    // Remove the entity and persist changes.
                    _context.CreditCardTypeGlAccounts.Remove(entity);
                    await _context.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);

                    var resultObj = new DeleteCreditCardTypeGlAccountResult
                    {
                        CardType = request.CardType,
                        OrganizationPartyId = request.OrganizationPartyId
                    };

                    return Result<DeleteCreditCardTypeGlAccountResult>.Success(resultObj);
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return Result<DeleteCreditCardTypeGlAccountResult>.Failure("Error deleting Credit Card GL Account");
                }
            }
        }
    }

    public class DeleteCreditCardTypeGlAccountResult
    {
        public string CardType { get; set; }
        public string OrganizationPartyId { get; set; }
    }
}
