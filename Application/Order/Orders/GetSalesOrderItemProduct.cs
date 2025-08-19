using Application.Catalog.Products;
using AutoMapper;
using MediatR;
using Persistence;

namespace Application.Order.Orders;

public class GetSalesOrderItemProduct
{
    public class Query : IRequest<Result<ProductLovDto>>
    {
        public string OrderItemId { get; set; }
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
            var orderItemProduct = (from itm in _context.OrderItems
                    join prd in _context.Products on itm.ProductId equals prd.ProductId
                    join prc in _context.ProductPrices on prd.ProductId equals prc.ProductId
                    join inv in _context.InventoryItems on itm.ProductId equals inv.ProductId
                    join fac in _context.Facilities on inv.FacilityId equals fac.FacilityId
                    where itm.OrderId + itm.OrderItemSeqId == request.OrderItemId
                    select new ProductLovDto
                    {
                        ProductId = itm.ProductId,
                        ProductName = prd.ProductName,
                        FacilityName = fac.FacilityName,
                        InventoryItem = inv.InventoryItemId,
                        QuantityOnHandTotal = inv.QuantityOnHandTotal,
                        AvailableToPromiseTotal = inv.AvailableToPromiseTotal,
                        Price = prc.Price
                    }
                ).FirstOrDefault();


            return Result<ProductLovDto>.Success(orderItemProduct);
        }
    }
}