using MediatR;
using Persistence;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Application.Catalog.Products
{
    public class GetProductQuantityUom
    {
        public class Query : IRequest<Result<UomDto>>
        {
            public string ProductId { get; set; }
        }

        public class UomDto
        {
            public string Description { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<UomDto>>
        {
            private readonly DataContext _context;

            public Handler(DataContext context)
            {
                _context = context;
            }

            public async Task<Result<UomDto>> Handle(Query request, CancellationToken cancellationToken)
            {
                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.ProductId == request.ProductId, cancellationToken);

                if (product == null)
                {
                    return Result<UomDto>.Failure("Product not found");
                }

                var uom = await _context.Uoms
                    .FirstOrDefaultAsync(u => u.UomId == product.QuantityUomId, cancellationToken);

                if (uom == null)
                {
                    return Result<UomDto>.Failure("UOM not found");
                }

                var uomDto = new UomDto
                {
                    Description = uom.Description
                };

                return Result<UomDto>.Success(uomDto);
            }
        }
    }
}