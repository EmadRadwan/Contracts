using MediatR;
using Persistence;
using Application.Core;

namespace Application.Catalog.ProductCategories;

public class DeleteProductCategory
{
    public class Command : IRequest<Result<Unit>>
    {
        public string ProductId { get; set; }
        public string ProductCategoryId { get; set; }
        public DateTime FromDate { get; set; }
    }

    public class Handler : IRequestHandler<Command, Result<Unit>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var productCategoryMember = await _context.ProductCategoryMembers.FindAsync(
                request.ProductCategoryId,
                request.ProductId,
                request.FromDate);

            if (productCategoryMember == null) return null;

            _context.ProductCategoryMembers.Remove(productCategoryMember);

            var result = await _context.SaveChangesAsync() > 0;

            if (!result) return Result<Unit>.Failure("Failed to delete the product category member");

            return Result<Unit>.Success(Unit.Value);
        }
    }
}
