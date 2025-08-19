using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.OData.Query;
using Persistence;

namespace Application.Manufacturing;

public class ListBOMProductsQuery
{
    public class Query : IRequest<IQueryable<BillOfMaterialRecord>>
    {
        public ODataQueryOptions<BillOfMaterialRecord> Options { get; set; }
    }

    public class Handler : IRequestHandler<Query, IQueryable<BillOfMaterialRecord>>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public Handler(DataContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task<IQueryable<BillOfMaterialRecord>> Handle(Query request, CancellationToken cancellationToken)
        {
            var query = from p in _context.Products
                join uom in _context.Uoms on p.QuantityUomId equals uom.UomId
                where _context.ProductAssocs.Any(pa => pa.ProductId == p.ProductId &&
                                                    pa.ProductAssocTypeId == "MANUF_COMPONENT" &&
                                                    pa.ProductAssocType != null)  // Ensure ProductAssocType exists
                select new BillOfMaterialRecord
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    ProductDescription = p.Description,
                    ProductAssocTypeDescription = p.ProductAssocProducts.FirstOrDefault(a => a.ProductAssocTypeId == "MANUF_COMPONENT").ProductAssocType.Description,
                    UomDescription = uom.Description
                };

            return query;

        }
    }
}