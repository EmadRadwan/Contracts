using Application.Catalog.ProductStores;
using Application.order.Orders;
using Application.Order.Orders;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Projects;

public class ApprovePOForCertificate
{
    public class Command : IRequest<Result<Unit>>
    {
        public string WorkEffortId { get; set; } = string.Empty;
    }

    public class Handler : IRequestHandler<Command, Result<Unit>>
    {
        private readonly DataContext _context;
        private readonly IOrderService _orderService;
        private readonly IProductStoreService _productStoreService;

        public Handler(
            DataContext context,
            IOrderService orderService,
            IProductStoreService productStoreService)
        {
            _context = context;
            _orderService = orderService;
            _productStoreService = productStoreService;
        }

        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            // 1. Find the certificate (WorkEffort) and its related PO
            var certificate = await _context.WorkEfforts
                .AsNoTracking()
                .FirstOrDefaultAsync(we =>
                    we.WorkEffortId == request.WorkEffortId &&
                    we.WorkEffortTypeId == "PROJECT_CERTIFICATE" &&
                    we.RelatedOrderId != null,
                    cancellationToken);

            if (certificate == null || string.IsNullOrEmpty(certificate.RelatedOrderId))
            {
                return Result<Unit>.Failure("No purchase order found for this certificate.");
            }

            var orderId = certificate.RelatedOrderId;

            // 2. Load the OrderHeader
            var orderHeader = await _context.OrderHeaders
                .AsNoTracking()
                .FirstOrDefaultAsync(oh => oh.OrderId == orderId, cancellationToken);

            if (orderHeader == null)
            {
                return Result<Unit>.Failure("Purchase order not found in database.");
            }

            // 3. Load OrderItems
            var orderItems = await _context.OrderItems
                .Where(oi => oi.OrderId == orderId)
                .Select(oi => new OrderItemDto2
                {
                    OrderId = oi.OrderId,
                    OrderItemSeqId = oi.OrderItemSeqId,
                    ProductId = oi.ProductId,
                    //ProductName = oi.ProductName,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    //SubTotal = oi.SubTotal,
                    UomId = oi.UomId,
                    //FacilityId = oi.FacilityId,
                    ItemDescription = oi.ItemDescription,
                    OrderItemTypeId = oi.OrderItemTypeId,
                    StatusId = oi.StatusId,
                    CreatedStamp = oi.CreatedStamp,
                    LastUpdatedStamp = oi.LastUpdatedStamp
                })
                .ToListAsync(cancellationToken);

            // 4. Load OrderAdjustments
            var orderAdjustments = await _context.OrderAdjustments
                .Where(oa => oa.OrderId == orderId)
                .Select(oa => new OrderAdjustmentDto2
                {
                    OrderAdjustmentId = oa.OrderAdjustmentId,
                    OrderAdjustmentTypeId = oa.OrderAdjustmentTypeId,
                    OrderId = oa.OrderId,
                    OrderItemSeqId = oa.OrderItemSeqId,
                    Amount = oa.Amount,
                    CorrespondingProductId = oa.CorrespondingProductId,
                    IsManual = oa.IsManual,
                    CreatedDate = oa.CreatedDate,
                    SourcePercentage = oa.SourcePercentage
                    // Add any other needed fields
                })
                .ToListAsync(cancellationToken);

            // 5. Build minimal DTO for approval
            var approveOrderDto = new OrderDto
            {
                OrderId = orderHeader.OrderId,
                FromPartyId = certificate.PartyIdContractor ?? certificate.PartyIdSupplier,
                GrandTotal = orderHeader.GrandTotal,
                OrderDate = orderHeader.OrderDate,
                CurrencyUomId = await _productStoreService.GetProductStoreDefaultCurrencyId(),
                StatusId = orderHeader.StatusId,
                OrderItems = orderItems,
                OrderAdjustments = orderAdjustments
            };

            // 6. Approve the PO
            await _orderService.UpdateOrApprovePurchaseOrder(approveOrderDto, "APPROVE");

            // Optional: Save any side effects if needed (usually not required since service does it)
            var saveResult = await _context.SaveChangesAsync(cancellationToken);
            if (saveResult <= 0)
            {
                return Result<Unit>.Failure("Failed to persist approval changes.");
            }

            return Result<Unit>.Success(Unit.Value);
        }
    }
}