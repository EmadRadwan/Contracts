using Application.Catalog.Products;
using Application.Order.Orders;
using AutoMapper;
using MediatR;
using Persistence;

namespace Application.Order.JobOrders;

public class ListJobOrderItems
{
    public class Query : IRequest<Result<List<OrderItemDto>>>
    {
        public string OrderId { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<List<OrderItemDto>>>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public Handler(DataContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task<Result<List<OrderItemDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var orderItems = (from itm in _context.OrderItems
                join prd in _context.Products on itm.ProductId equals prd.ProductId
                where itm.OrderId == request.OrderId
                select new OrderItemDto
                {
                    OrderId = itm.OrderId,
                    OrderItemSeqId = itm.OrderItemSeqId,
                    ProductId = new ProductLovDto
                        { ProductId = prd.ProductId, ProductName = prd.ProductName, ProductTypeId = prd.ProductTypeId },
                    ProductTypeId = prd.ProductTypeId,
                    ProductName = itm.IsPromo == "Y" ? prd.ProductName + " (Promo)" : prd.ProductName,
                    IsPromo = itm.IsPromo,
                    Quantity = itm.Quantity,
                    UnitPrice = itm.UnitPrice,
                    UnitListPrice = itm.UnitListPrice,
                    IsProductDeleted = false
                }).ToList();

            return Result<List<OrderItemDto>>.Success(orderItems);
        }
    }
}