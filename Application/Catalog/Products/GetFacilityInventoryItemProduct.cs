using AutoMapper;
using MediatR;
using Persistence;

namespace Application.Catalog.Products;

public class GetFacilityInventoryItemProduct
{
    public class Query : IRequest<Result<ProductLovDto>>
    {
        public FacilityInventoryItemParams? Params { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<ProductLovDto>>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public Handler(DataContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task<Result<ProductLovDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var orderItemProduct = (from prd in _context.Products
                    join inv in _context.InventoryItems on prd.ProductId equals inv.ProductId
                    join fac in _context.Facilities on inv.FacilityId equals fac.FacilityId
                    where fac.FacilityId == request.Params.FacilityId &&
                          inv.InventoryItemId == request.Params.InventoryItemId
                    select new ProductLovDto
                    {
                        ProductId = prd.ProductId,
                        ProductName = prd.ProductName,
                        FacilityName = fac.FacilityName,
                        InventoryItem = inv.InventoryItemId
                    }
                ).FirstOrDefault();


            return Result<ProductLovDto>.Success(orderItemProduct);
        }
    }
}