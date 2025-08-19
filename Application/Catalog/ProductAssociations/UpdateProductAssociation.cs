using Application.Interfaces;
using Domain;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Persistence;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Catalog.ProductAssociations;

public class UpdateProductAssociation
{
    // REFACTOR: Define DTO to match ProductAssociationsForm interface for update
    public class ProductAssociationDto
    {
        public string ProductId { get; set; } = null!;
        public string ProductIdTo { get; set; } = null!;
        public string ProductAssocTypeId { get; set; } = null!;
        public DateTime? FromDate { get; set; }
        public DateTime? ThruDate { get; set; }
        public string? Reason { get; set; }
        public decimal? Quantity { get; set; }
        public int? SequenceNum { get; set; }
    }

    public class Command : IRequest<Result<ProductAssociationDto>>
    {
        public ProductAssociationDto? ProductAssociationDto { get; set; }
    }

    // REFACTOR: Validate input data to ensure required fields and referential integrity
    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(x => x.ProductAssociationDto).NotNull().WithMessage("Product association data is required");
            RuleFor(x => x.ProductAssociationDto!.ProductId).NotEmpty().WithMessage("Product ID is required");
            RuleFor(x => x.ProductAssociationDto!.ProductIdTo).NotEmpty()
                .WithMessage("Associated Product ID is required");
            RuleFor(x => x.ProductAssociationDto!.ProductAssocTypeId).NotEmpty()
                .WithMessage("Product association type ID is required");
            RuleFor(x => x.ProductAssociationDto!.FromDate).NotEmpty().WithMessage("From date is required");
            RuleFor(x => x.ProductAssociationDto!.Quantity).GreaterThan(0)
                .When(x => x.ProductAssociationDto!.Quantity.HasValue).WithMessage("Quantity must be greater than 0");
            RuleFor(x => x.ProductAssociationDto!.SequenceNum).GreaterThanOrEqualTo(0)
                .When(x => x.ProductAssociationDto!.SequenceNum.HasValue)
                .WithMessage("Sequence number must be non-negative");
        }
    }

    public class Handler : IRequestHandler<Command, Result<ProductAssociationDto>>
    {
        private readonly DataContext _context;
        private readonly IUserAccessor _userAccessor;

        public Handler(DataContext context, IUserAccessor userAccessor)
        {
            _context = context;
            _userAccessor = userAccessor;
        }

        public async Task<Result<ProductAssociationDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            // REFACTOR: Validate user
            var user = await _context.Users.FirstOrDefaultAsync(x =>
                x.UserName == _userAccessor.GetUsername(), cancellationToken);
            if (user == null)
            {
                return Result<ProductAssociationDto>.Failure("User not found");
            }

            // REFACTOR: Validate input DTO
            if (request.ProductAssociationDto == null)
            {
                return Result<ProductAssociationDto>.Failure("Product association data is required");
            }

            // REFACTOR: Check if productId exists
            var productExists = await _context.Products
                .AnyAsync(x => x.ProductId == request.ProductAssociationDto.ProductId, cancellationToken);
            if (!productExists)
            {
                return Result<ProductAssociationDto>.Failure("Product ID does not exist");
            }

            // REFACTOR: Check if productIdTo exists
            var productToExists = await _context.Products
                .AnyAsync(x => x.ProductId == request.ProductAssociationDto.ProductIdTo, cancellationToken);
            if (!productToExists)
            {
                return Result<ProductAssociationDto>.Failure("Associated Product ID does not exist");
            }

            // REFACTOR: Check if productAssocTypeId exists
            var assocTypeExists = await _context.ProductAssocTypes
                .AnyAsync(x => x.ProductAssocTypeId == request.ProductAssociationDto.ProductAssocTypeId,
                    cancellationToken);
            if (!assocTypeExists)
            {
                return Result<ProductAssociationDto>.Failure("Product association type ID does not exist");
            }

            // REFACTOR: Find existing association
            var existingAssoc = await _context.ProductAssocs
                .FirstOrDefaultAsync(x =>
                    x.ProductId == request.ProductAssociationDto.ProductId &&
                    x.ProductIdTo == request.ProductAssociationDto.ProductIdTo &&
                    x.ProductAssocTypeId == request.ProductAssociationDto.ProductAssocTypeId &&
                    x.FromDate == request.ProductAssociationDto.FromDate,
                    cancellationToken);
            if (existingAssoc == null)
            {
                return Result<ProductAssociationDto>.Failure("Product association does not exist");
            }

            // REFACTOR: Begin transaction for data consistency
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                // REFACTOR: Update ProductAssoc entity manually without AutoMapper
                existingAssoc.ProductId = request.ProductAssociationDto.ProductId;
                existingAssoc.ProductIdTo = request.ProductAssociationDto.ProductIdTo;
                existingAssoc.ProductAssocTypeId = request.ProductAssociationDto.ProductAssocTypeId;
                existingAssoc.FromDate = (DateTime)request.ProductAssociationDto.FromDate;
                existingAssoc.ThruDate = request.ProductAssociationDto.ThruDate;
                existingAssoc.Reason = request.ProductAssociationDto.Reason;
                existingAssoc.Quantity = request.ProductAssociationDto.Quantity;
                existingAssoc.SequenceNum = request.ProductAssociationDto.SequenceNum;
                existingAssoc.LastUpdatedStamp = DateTime.UtcNow;

                // REFACTOR: Save changes
                var result = await _context.SaveChangesAsync(cancellationToken) > 0;
                if (!result)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return Result<ProductAssociationDto>.Failure("Failed to update product association");
                }

                // REFACTOR: Commit transaction
                await transaction.CommitAsync(cancellationToken);

                // REFACTOR: Manually map ProductAssoc to ProductAssociationDto for response
                var productAssocDto = new ProductAssociationDto
                {
                    ProductId = existingAssoc.ProductId,
                    ProductIdTo = existingAssoc.ProductIdTo,
                    ProductAssocTypeId = existingAssoc.ProductAssocTypeId,
                    FromDate = existingAssoc.FromDate,
                    ThruDate = existingAssoc.ThruDate,
                    Reason = existingAssoc.Reason,
                    Quantity = existingAssoc.Quantity,
                    SequenceNum = existingAssoc.SequenceNum
                };

                return Result<ProductAssociationDto>.Success(productAssocDto);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Result<ProductAssociationDto>.Failure($"Failed to update product association: {ex.Message}");
            }
        }
    }
}

