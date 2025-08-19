using AutoMapper;
using MediatR;
using Persistence;

namespace Application.Order.Orders;

public class ListSalesOrderAdjustments
{
    public class Query : IRequest<Result<List<OrderAdjustmentDto2>>>
    {
        public string OrderId { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<List<OrderAdjustmentDto2>>>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public Handler(DataContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task<Result<List<OrderAdjustmentDto2>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var orderAdjustments = (from ord in _context.OrderHeaders
                join qoti in _context.OrderItems on ord.OrderId equals qoti.OrderId
                join qadj in _context.OrderAdjustments on new { qoti.OrderId, qoti.OrderItemSeqId } equals
                    new { qadj.OrderId, qadj.OrderItemSeqId }
                join oadjt in _context.OrderAdjustmentTypes on qadj.OrderAdjustmentTypeId equals oadjt
                    .OrderAdjustmentTypeId
                join prd in _context.Products on qadj.CorrespondingProductId equals prd.ProductId
                where ord.OrderId == request.OrderId
                select new OrderAdjustmentDto2
                {
                    OrderAdjustmentId = qadj.OrderAdjustmentId,
                    OrderId = ord.OrderId,
                    OrderItemSeqId = qoti.OrderItemSeqId,
                    OrderAdjustmentTypeId = qadj.OrderAdjustmentTypeId,
                    OrderAdjustmentTypeDescription = oadjt.Description,
                    CorrespondingProductId = qadj.CorrespondingProductId,
                    CorrespondingProductName = prd.ProductName,
                    Amount = qadj.Amount,
                    IsAdjustmentDeleted = false,
                    IsManual = qadj.IsManual,
                    SourcePercentage = qadj.SourcePercentage,
                    ProductPromoId = qadj.ProductPromoId,
                    Description = qadj.Description,
                    TaxAuthGeoId = qadj.TaxAuthGeoId,
                    TaxAuthPartyId = qadj.TaxAuthPartyId,
                    TaxAuthorityRateSeqId = qadj.TaxAuthorityRateSeqId,
                    OverrideGlAccountId = qadj.OverrideGlAccountId
                }).ToList();

            var orderAdjustments2 = (from ord in _context.OrderHeaders
                join qadj in _context.OrderAdjustments on ord.OrderId equals qadj.OrderId
                join oadjt in _context.OrderAdjustmentTypes on qadj.OrderAdjustmentTypeId equals oadjt
                    .OrderAdjustmentTypeId
                where ord.OrderId == request.OrderId && qadj.OrderItemSeqId == "_NA_"
                select new OrderAdjustmentDto2
                {
                    OrderAdjustmentId = qadj.OrderAdjustmentId,
                    OrderId = ord.OrderId,
                    OrderItemSeqId = "_NA_",
                    OrderAdjustmentTypeId = qadj.OrderAdjustmentTypeId,
                    OrderAdjustmentTypeDescription = oadjt.Description,
                    CorrespondingProductId = qadj.CorrespondingProductId,
                    CorrespondingProductName = null,
                    Amount = qadj.Amount,
                    IsAdjustmentDeleted = false,
                    IsManual = qadj.IsManual,
                    SourcePercentage = qadj.SourcePercentage,
                    ProductPromoId = qadj.ProductPromoId,
                    Description = qadj.Description,
                    TaxAuthGeoId = qadj.TaxAuthGeoId,
                    TaxAuthPartyId = qadj.TaxAuthPartyId,
                    TaxAuthorityRateSeqId = qadj.TaxAuthorityRateSeqId,
                    OverrideGlAccountId = qadj.OverrideGlAccountId
                }).ToList();


            var orderToReturn = orderAdjustments.Union(orderAdjustments2).ToList();

            return Result<List<OrderAdjustmentDto2>>.Success(orderToReturn);
        }
    }
}