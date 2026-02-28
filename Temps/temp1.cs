using Application.Interfaces;           // assuming Result<T> is here
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Catalog.ProductCategories;

public class ListProductCategories
{
    public class Query : IRequest<Result<List<ProductCategoryMemberDto>>>
    {
        public string ProductId { get; set; } = string.Empty;
    }

    public class Handler : IRequestHandler<Query, Result<List<ProductCategoryMemberDto>>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<List<ProductCategoryMemberDto>>> Handle(
            Query request,
            CancellationToken cancellationToken)
        {
            var productCategoryMembers = await _context.ProductCategoryMembers
                .Where(z => z.ProductId == request.ProductId)
                // Optional: include only active/current categories
                // .Where(z => z.FromDate <= DateTime.UtcNow && (z.ThruDate == null || z.ThruDate > DateTime.UtcNow))
                .Join(
                    _context.ProductCategories,
                    member => member.ProductCategoryId,
                    category => category.ProductCategoryId,
                    (member, category) => new { member, category }
                )
                .Select(x => new ProductCategoryMemberDto
                {
                    ProductId         = x.member.ProductId,
                    ProductCategoryId = x.member.ProductCategoryId,
                    FromDate          = x.member.FromDate,
                    ThruDate          = x.member.ThruDate,
                    Comments          = x.member.Comments,
                    SequenceNum       = x.member.SequenceNum,
                    Quantity          = x.member.Quantity,

                    // ← New fields from joined ProductCategory
                    CategoryDescriptionArabic = x.category.DescriptionArabic,
                    // CategoryName           = x.category.CategoryName,           // if you want English too
                    // CategoryDescription    = x.category.Description,           // if needed
                })
                .ToListAsync(cancellationToken);

            return Result<List<ProductCategoryMemberDto>>.Success(productCategoryMembers);
        }
    }
}