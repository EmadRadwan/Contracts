using Application.Interfaces;
using Application.ProductCategories;
using Domain;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Catalog.ProductCategories;

public class CreateProductCategory
{
    public class Command : IRequest<Result<ProductCategoryMemberDto>>
    {
        public ProductCategoryMemberDto Member { get; set; } = null!;
    }
    

    public class Handler : IRequestHandler<Command, Result<ProductCategoryMemberDto>>
    {
        private readonly DataContext _context;
        private readonly IUserAccessor _userAccessor;

        public Handler(DataContext context, IUserAccessor userAccessor)
        {
            _context = context;
            _userAccessor = userAccessor;
        }

        public async Task<Result<ProductCategoryMemberDto>> Handle(
            Command request,
            CancellationToken cancellationToken)
        {
            var currentUserName = _userAccessor.GetUsername();

            // Optional: you can keep or remove this user check
            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.UserName == currentUserName, cancellationToken);

            // if (user == null)
            //     return Result<ProductCategoryMemberDto>.Failure("User not found");

            var now = DateTime.UtcNow;  // Recommended: use UTC for audit fields

            // Manual mapping: DTO → Entity
            var entity = new ProductCategoryMember
            {
                ProductId         = request.Member.ProductId,
                ProductCategoryId = request.Member.ProductCategoryId,
                FromDate          = request.Member.FromDate,
                ThruDate          = request.Member.ThruDate,

                // Audit fields — always controlled by backend
                CreatedStamp      = now,
                CreatedTxStamp    = now,
                LastUpdatedStamp  = now,
                LastUpdatedTxStamp = now
            };

            _context.ProductCategoryMembers.Add(entity);

            var success = await _context.SaveChangesAsync(cancellationToken) > 0;

            if (!success)
            {
                return Result<ProductCategoryMemberDto>.Failure(
                    "Failed to create product category assignment");
            }

            // Manual mapping: Entity → DTO (for response)
            var responseDto = new ProductCategoryMemberDto
            {
                ProductId         = entity.ProductId,
                ProductCategoryId = entity.ProductCategoryId,
                FromDate          = entity.FromDate,
                ThruDate          = entity.ThruDate,
                
            };

            return Result<ProductCategoryMemberDto>.Success(responseDto);
        }
    }
}