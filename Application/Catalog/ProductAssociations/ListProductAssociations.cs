using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Catalog.ProductAssocsiations
{
    // REFACTOR: Define DTO to match OFBiz ProductAssoc entity, including all fields required by React component
    public class ProductAssociationDto
    {
        public string ProductId { get; set; }
        public string ProductIdTo { get; set; }
        public string ProductNameTo { get; set; }
        public string ProductAssocTypeId { get; set; }
        public string ProductAssocTypeDescription { get; set; } // Localized description from ProductAssocType
        public DateTime? FromDate { get; set; }
        public DateTime? ThruDate { get; set; }
        public string Reason { get; set; }
        public decimal? Quantity { get; set; }
        public int? SequenceNum { get; set; }
    }

    public class ListProductAssociations
    {
        public class Query : IRequest<Result<List<ProductAssociationDto>>>
        {
            public string ProductId { get; set; }
            public string Language { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<List<ProductAssociationDto>>>
        {
            private readonly DataContext _context;

            // REFACTOR: Inject DataContext to interact with OFBiz database, ensuring compatibility with ProductAssoc and ProductAssocType entities
            public Handler(DataContext context)
            {
                _context = context;
            }

            public async Task<Result<List<ProductAssociationDto>>> Handle(Query request,
                CancellationToken cancellationToken)
            {
                // REFACTOR: Validate ProductId to prevent invalid queries, aligning with OFBiz productId validation
                if (string.IsNullOrEmpty(request.ProductId))
                {
                    // REFACTOR: Fix type mismatch by returning correct DTO type (ProductAssociationDto instead of ProductAssociationTypeDto)
                    // Ensures return type aligns with method signature and React component expectations
                    return Result<List<ProductAssociationDto>>.Failure("ProductId is required.");
                }

                var productAssociations = await _context.ProductAssocs
                    .Where(x => x.ProductId == request.ProductId)
                    .Join(
                        _context.ProductAssocTypes,
                        pa => pa.ProductAssocTypeId,
                        pat => pat.ProductAssocTypeId,
                        (pa, pat) => new { ProductAssoc = pa, ProductAssocType = pat }
                    )
                    .Join(
                        _context.Products,
                        pa => pa.ProductAssoc.ProductIdTo,
                        p => p.ProductId,
                        (pa, p) => new { pa.ProductAssoc, pa.ProductAssocType, ProductTo = p }
                    )
                    .Join(
                        _context.Products,
                        pa => pa.ProductAssoc.ProductId,
                        p => p.ProductId,
                        (pa, p) => new ProductAssociationDto
                        {
                            ProductId = pa.ProductAssoc.ProductId,
                            ProductIdTo = pa.ProductAssoc.ProductIdTo,
                            ProductNameTo = pa.ProductTo.ProductName,
                            ProductAssocTypeId = pa.ProductAssoc.ProductAssocTypeId,
                            ProductAssocTypeDescription = pa.ProductAssocType.Description,
                            FromDate = pa.ProductAssoc.FromDate,
                            ThruDate = pa.ProductAssoc.ThruDate,
                            Reason = pa.ProductAssoc.Reason,
                            Quantity = pa.ProductAssoc.Quantity,
                            SequenceNum = pa.ProductAssoc.SequenceNum
                        }
                    )
                    .OrderBy(x => x.SequenceNum) // REFACTOR: Sort by SequenceNum
                    .ToListAsync(cancellationToken);


                // REFACTOR: Handle empty results, aligning with OFBiz behavior for no associations
                if (!productAssociations.Any())
                {
                    return Result<List<ProductAssociationDto>>.Success(new List<ProductAssociationDto>());
                }

                // REFACTOR: Return success result, ensuring compatibility with React component expectations
                return Result<List<ProductAssociationDto>>.Success(productAssociations);
            }
        }
    }
}