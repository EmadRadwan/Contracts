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
    public class DeleteProductCategoryGlAccount
    {
        public class Command : IRequest<Result<DeleteProductCategoryGlAccountResult>>
        {
            public string ProductCategoryId { get; set; }
            public string OrganizationPartyId { get; set; }
            public string GlAccountTypeId { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.ProductCategoryId)
                    .NotEmpty().WithMessage("PRODUCT_CATEGORY_ID must not be empty.");
                RuleFor(x => x.OrganizationPartyId)
                    .NotEmpty().WithMessage("ORGANIZATION_PARTY_ID must not be empty.");
                RuleFor(x => x.GlAccountTypeId)
                    .NotEmpty().WithMessage("GL_ACCOUNT_TYPE_ID must not be empty.");
            }
        }

        public class Handler : IRequestHandler<Command, Result<DeleteProductCategoryGlAccountResult>>
        {
            private readonly DataContext _context;

            public Handler(DataContext context)
            {
                _context = context;
            }

            public async Task<Result<DeleteProductCategoryGlAccountResult>> Handle(Command request, CancellationToken cancellationToken)
            {
                await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    // Retrieve the entity based on its composite key.
                    var entity = await _context.ProductCategoryGlAccounts.FindAsync(
                        new object[] { request.ProductCategoryId, request.OrganizationPartyId, request.GlAccountTypeId },
                        cancellationToken);

                    if (entity == null)
                    {
                        return Result<DeleteProductCategoryGlAccountResult>.Failure("Record not found");
                    }

                    // Remove the entity and persist changes.
                    _context.ProductCategoryGlAccounts.Remove(entity);
                    await _context.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);

                    var resultObj = new DeleteProductCategoryGlAccountResult
                    {
                        ProductCategoryId = request.ProductCategoryId,
                        OrganizationPartyId = request.OrganizationPartyId,
                        GlAccountTypeId = request.GlAccountTypeId
                    };

                    return Result<DeleteProductCategoryGlAccountResult>.Success(resultObj);
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return Result<DeleteProductCategoryGlAccountResult>.Failure("Error deleting Product Category GL Account");
                }
            }
        }
    }

    public class DeleteProductCategoryGlAccountResult
    {
        public string ProductCategoryId { get; set; }
        public string OrganizationPartyId { get; set; }
        public string GlAccountTypeId { get; set; }
    }
}
