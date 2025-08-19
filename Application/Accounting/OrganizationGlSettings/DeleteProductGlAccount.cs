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
    public class DeleteProductGlAccount
    {
        public class Command : IRequest<Result<DeleteProductGlAccountResult>>
        {
            public string ProductId { get; set; }
            public string OrganizationPartyId { get; set; }
            public string GlAccountTypeId { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.ProductId)
                    .NotEmpty().WithMessage("PRODUCT_ID must not be empty.");
                RuleFor(x => x.OrganizationPartyId)
                    .NotEmpty().WithMessage("ORGANIZATION_PARTY_ID must not be empty.");
                RuleFor(x => x.GlAccountTypeId)
                    .NotEmpty().WithMessage("GL_ACCOUNT_TYPE_ID must not be empty.");
            }
        }

        public class Handler : IRequestHandler<Command, Result<DeleteProductGlAccountResult>>
        {
            private readonly DataContext _context;

            public Handler(DataContext context)
            {
                _context = context;
            }

            public async Task<Result<DeleteProductGlAccountResult>> Handle(Command request, CancellationToken cancellationToken)
            {
                await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    // Retrieve the entity based on its composite key.
                    var entity = await _context.ProductGlAccounts.FindAsync(
                        new object[] { request.ProductId, request.OrganizationPartyId, request.GlAccountTypeId },
                        cancellationToken);

                    if (entity == null)
                    {
                        return Result<DeleteProductGlAccountResult>.Failure("Record not found");
                    }

                    // Remove the entity and persist changes.
                    _context.ProductGlAccounts.Remove(entity);
                    await _context.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);

                    var resultObj = new DeleteProductGlAccountResult
                    {
                        ProductId = request.ProductId,
                        OrganizationPartyId = request.OrganizationPartyId,
                        GlAccountTypeId = request.GlAccountTypeId
                    };

                    return Result<DeleteProductGlAccountResult>.Success(resultObj);
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return Result<DeleteProductGlAccountResult>.Failure("Error deleting Product GL Account");
                }
            }
        }
    }

    public class DeleteProductGlAccountResult
    {
        public string ProductId { get; set; }
        public string OrganizationPartyId { get; set; }
        public string GlAccountTypeId { get; set; }
    }
}
