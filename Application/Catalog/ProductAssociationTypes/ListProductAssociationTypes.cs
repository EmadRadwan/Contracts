using MediatR;
using Microsoft.AspNetCore.Mvc;
using Persistence;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Application.Catalog.ProductAssociationTypes
{
    // DTO to match OFBiz ProductAssocType entity
    public class ProductAssociationTypeDto
    {
        public string ProductAssociationTypeId { get; set; }
        public string Description { get; set; }
    }

    public class ListProductAssociationTypes
    {
        public class Query : IRequest<Result<List<ProductAssociationTypeDto>>>
        {
            public string Language { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<List<ProductAssociationTypeDto>>>
        {
            private readonly DataContext _context;

            // REFACTOR: Inject DataContext to interact with OFBiz database, ensuring compatibility with entity schema
            public Handler(DataContext context)
            {
                _context = context;
            }

            public async Task<Result<List<ProductAssociationTypeDto>>> Handle(Query request, CancellationToken cancellationToken)
            {
                // REFACTOR: Define allowed association types to filter relevant ProductAssocType records, aligning with OFBiz usage
                var allowedTypes = new List<string> { "MANUF_COMPONENT", "PRODUCT_VARIANT" };

                // REFACTOR: Query ProductAssocType entity, selecting ID and localized description based on language
                // Use LINQ to map to DTO, matching OFBiz ProductAssocType fields
                var productAssocTypes = await _context.ProductAssocTypes
                    .Where(x => allowedTypes.Contains(x.ProductAssocTypeId))
                    .Select(x => new ProductAssociationTypeDto
                    {
                        ProductAssociationTypeId = x.ProductAssocTypeId,
                        Description = request.Language == "ar" ? x.Description : x.Description
                    })
                    .ToListAsync(cancellationToken);

                // REFACTOR: Return success result, ensuring compatibility with MediatR and React component expectations
                return Result<List<ProductAssociationTypeDto>>.Success(productAssocTypes);
            }
        }
    }
}