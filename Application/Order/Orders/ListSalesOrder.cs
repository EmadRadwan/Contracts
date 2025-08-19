using Application.Catalog.Products;
using AutoMapper;
using MediatR;
using Persistence;

namespace Application.Order.Orders;

public class ListSalesOrder
{
    public class Query : IRequest<Result<OrderDto2>>
    {
        public string OrderId { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<OrderDto2>>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public Handler(DataContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task<Result<OrderDto2>> Handle(Query request, CancellationToken cancellationToken)
        {
            var query = from ord in _context.OrderHeaders
                join orole in _context.OrderRoles on ord.OrderId equals orole.OrderId
                join sts in _context.StatusItems on ord.StatusId equals sts.StatusId
                join pty in _context.Parties on orole.PartyId equals pty.PartyId
                join opp in _context.OrderPaymentPreferences on ord.OrderId equals opp.OrderId
                where ord.OrderId == request.OrderId && orole.RoleTypeId == "PLACING_CUSTOMER"
                select new OrderDto2
                {
                    OrderId = ord.OrderId,
                    PaymentMethodTypeId = opp.PaymentMethodTypeId,
                    FromPartyId = new OrderPartyDto
                    {
                        FromPartyId = pty.PartyId,
                        FromPartyName = pty.Description
                    },
                    //OrderDate = ord.OrderDate,
                    StatusDescription = sts.Description,
                    AllowSubmit = false
                };


            var results = query.ToList();

            var orderItems = (from itm in _context.OrderItems
                join prd in _context.Products on itm.ProductId equals prd.ProductId
                join prc in _context.ProductPrices on prd.ProductId equals prc.ProductId
                join inv in _context.InventoryItems on prd.ProductId equals inv.ProductId
                join fac in _context.Facilities on inv.FacilityId equals fac.FacilityId
                where itm.OrderId == request.OrderId
                select new OrderItemDto
                {
                    OrderId = itm.OrderId,
                    OrderItemSeqId = itm.OrderItemSeqId,
                    ProductId = new ProductLovDto
                    {
                        ProductId = itm.ProductId,
                        ProductName = prd.ProductName,
                        FacilityName = fac.FacilityName,
                        InventoryItem = inv.InventoryItemId,
                        QuantityOnHandTotal = inv.QuantityOnHandTotal,
                        AvailableToPromiseTotal = inv.AvailableToPromiseTotal,
                        Price = prc.PriceWithTax
                    },
                    ProductName = prd.ProductName,
                    Quantity = itm.Quantity,
                    IsProductDeleted = false
                }).ToList();

            var orderAdjustments = (from ord in _context.OrderHeaders
                join qoti in _context.OrderItems on ord.OrderId equals qoti.OrderId
                join qadj in _context.OrderAdjustments on new { qoti.OrderId, qoti.OrderItemSeqId } equals
                    new { qadj.OrderId, qadj.OrderItemSeqId }
                join oadjt in _context.OrderAdjustmentTypes on qadj.OrderAdjustmentTypeId equals oadjt
                    .OrderAdjustmentTypeId
                join prd in _context.Products on qadj.CorrespondingProductId equals prd.ProductId
                join prc in _context.ProductPrices on prd.ProductId equals prc.ProductId
                join inv in _context.InventoryItems on prd.ProductId equals inv.ProductId
                join fac in _context.Facilities on inv.FacilityId equals fac.FacilityId
                where ord.OrderId == request.OrderId
                select new OrderAdjustmentDto
                {
                    OrderAdjustmentId = qadj.OrderAdjustmentId,
                    OrderId = ord.OrderId,
                    OrderItemSeqId = qoti.OrderItemSeqId,
                    OrderAdjustmentTypeId = qadj.OrderAdjustmentTypeId,
                    OrderAdjustmentTypeDescription = oadjt.Description,
                    CorrespondingProductId = new ProductLovDto
                    {
                        ProductId = qadj.CorrespondingProductId,
                        ProductName = prd.ProductName,
                        FacilityName = fac.FacilityName,
                        InventoryItem = inv.InventoryItemId,
                        QuantityOnHandTotal = inv.QuantityOnHandTotal,
                        AvailableToPromiseTotal = inv.AvailableToPromiseTotal,
                        Price = prc.PriceWithTax
                    },
                    CorrespondingProductName = prd.ProductName,
                    Amount = qadj.Amount,
                    IsAdjustmentDeleted = false
                }).ToList();

            var orderAdjustments2 = (from ord in _context.OrderHeaders
                join qadj in _context.OrderAdjustments on ord.OrderId equals qadj.OrderId
                join oadjt in _context.OrderAdjustmentTypes on qadj.OrderAdjustmentTypeId equals oadjt
                    .OrderAdjustmentTypeId
                where ord.OrderId == request.OrderId && qadj.OrderItemSeqId == "_NA_"
                select new OrderAdjustmentDto
                {
                    OrderAdjustmentId = qadj.OrderAdjustmentId,
                    OrderId = ord.OrderId,
                    OrderItemSeqId = "_NA_",
                    OrderAdjustmentTypeId = qadj.OrderAdjustmentTypeId,
                    OrderAdjustmentTypeDescription = oadjt.Description,
                    CorrespondingProductId = new ProductLovDto
                    {
                        ProductId = qadj.CorrespondingProductId,
                        ProductName = null,
                        Price = null
                    },
                    CorrespondingProductName = null,
                    Amount = qadj.Amount,
                    IsAdjustmentDeleted = false
                }).ToList();


            var orderToReturn = new OrderDto2();

            if (results.Any())
            {
                orderToReturn = results[0];
                if (orderItems.Any()) orderToReturn.OrderItems = orderItems;
                if (orderAdjustments.Any() || orderAdjustments2.Any())
                    orderToReturn.OrderAdjustments = orderAdjustments.Union(orderAdjustments2).ToList();
            }

            return Result<OrderDto2>.Success(orderToReturn);
        }
    }
}