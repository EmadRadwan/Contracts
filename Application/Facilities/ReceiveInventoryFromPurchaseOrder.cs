using Application.Order.Orders;
using Application.Shipments;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Facilities;

public class ReceiveInventoryFromPurchaseOrder
{
    public class Command : IRequest<Result<ReceiveInventoryResult>>
    {
        public string OrderId { get; set; }
        public string FacilityId { get; set; } // Optional: Default facility if not derived
    }

    public class Handler : IRequestHandler<Command, Result<ReceiveInventoryResult>>
    {
        private readonly DataContext _context;
        private readonly ILogger<Handler> _logger;
        private readonly IMediator _mediator;
        private readonly IShipmentService _shipmentService;

        public Handler(DataContext context, IMediator mediator, ILogger<Handler> logger,
            IShipmentService shipmentService)
        {
            _context = context;
            _mediator = mediator;
            _logger = logger;
            _shipmentService = shipmentService;
        }

        public async Task<Result<ReceiveInventoryResult>> Handle(Command request, CancellationToken cancellationToken)
        {
            // REFACTOR: Replaced explicit transaction with conditional enlistment to existing transaction or new one;
            // this prevents nested transaction errors when called within an outer transaction (e.g., from another handler),
            // allowing seamless integration while maintaining atomicity. Improves robustness by checking CurrentTransaction.
            IDbContextTransaction? transaction = null;
            var ownsTransaction = false;
            try
            {
                if (_context.Database.CurrentTransaction == null)
                {
                    transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                    ownsTransaction = true;
                    _logger.LogDebug("Started new transaction for ReceiveInventoryFromPurchaseOrder.");
                }
                else
                {
                    _logger.LogDebug("Using existing transaction for ReceiveInventoryFromPurchaseOrder.");
                }

                // REFACTOR: Added validation for purchase order to ensure it exists and is of type PURCHASE_ORDER;
                // this aligns with ListPurchaseOrderItemsForReceive logic and prevents processing invalid orders.
                var purchaseOrder = await _context.OrderHeaders
                    .FirstOrDefaultAsync(po => po.OrderId == request.OrderId, cancellationToken);
                if (purchaseOrder == null)
                {
                    _logger.LogWarning("Purchase Order with ID {PurchaseOrderId} not found.", request.OrderId);
                    return Result<ReceiveInventoryResult>.Failure(
                        $"Purchase Order with ID {request.OrderId} not found.");
                }

                if (!string.Equals(purchaseOrder.OrderTypeId, "PURCHASE_ORDER", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("OrderTypeId for Purchase Order ID {PurchaseOrderId} is not 'PURCHASE_ORDER'.",
                        request.OrderId);
                    return Result<ReceiveInventoryResult>.Failure(
                        $"OrderTypeId for Purchase Order ID {request.OrderId} is invalid.");
                }

                // REFACTOR: Integrated shipment check and creation logic from ListPurchaseOrderItemsForReceive and QuickReceivePurchaseOrder;
                // this ensures a shipment exists before attempting to receive inventory, addressing the failure due to missing shipment artifacts.
                // Improves code by consolidating the workflow in the backend, bypassing frontend dependencies.
                var orderShipments = await _context.OrderShipments
                    .Where(os => os.OrderId == request.OrderId)
                    .ToListAsync(cancellationToken);

                if (!orderShipments.Any())
                {
                    var quickReceiveResult =
                        await _shipmentService.QuickReceivePurchaseOrder(request.OrderId, request.FacilityId);
                    if (quickReceiveResult == null)
                    {
                        _logger.LogWarning("Failed to create shipment for OrderId: {OrderId}", request.OrderId);
                        if (ownsTransaction) await transaction.RollbackAsync(cancellationToken);
                        return Result<ReceiveInventoryResult>.Failure("Failed to create shipment for purchase order.");
                    }

                    // REFACTOR: Added SaveChanges to persist shipment-related entities before proceeding to inventory receipt;
                    // this ensures the shipment is available for ReceiveInventoryProducts, maintaining transaction integrity.
                    var shipmentSaveResult = await _context.SaveChangesAsync(cancellationToken);
                    if (shipmentSaveResult <= 0)
                    {
                        _logger.LogWarning("Failed to persist shipment for OrderId: {OrderId}", request.OrderId);
                        if (ownsTransaction) await transaction.RollbackAsync(cancellationToken);
                        return Result<ReceiveInventoryResult>.Failure("Failed to persist shipment for purchase order.");
                    }
                }

                // REFACTOR: Extracted order item fetching into a separate method for clarity and reusability.
                // This improves maintainability by isolating data retrieval logic and makes it easier to test.
                var orderItems = await GetOrderItemsForPurchaseOrder(request.OrderId, cancellationToken);
                if (!orderItems.Any())
                {
                    _logger.LogWarning("No order items found for OrderId: {OrderId}", request.OrderId);
                    if (ownsTransaction) await transaction.RollbackAsync(cancellationToken);
                    return Result<ReceiveInventoryResult>.Failure("No order items found for the provided OrderId.");
                }

                // REFACTOR: Moved DTO construction to a separate method to encapsulate mapping logic.
                // This reduces code duplication and makes the handler more focused on orchestration.
                var receivedItemsDto = await ConstructReceiveInventoryItemsDto(request.OrderId, request.FacilityId,
                    orderItems, cancellationToken);

                // REFACTOR: Reuse existing ReceiveInventoryProducts logic to avoid duplicating transaction and error-handling code.
                // This ensures consistency with existing inventory receiving functionality. Note: Inner handler will detect existing transaction and skip its own.
                var result =
                    await _mediator.Send(new ReceiveInventoryProducts.Command { ReceivedItems = receivedItemsDto },
                        cancellationToken);
                if (!result.IsSuccess)
                {
                    _logger.LogError("Failed to receive inventory for OrderId: {OrderId}. Error: {Error}",
                        request.OrderId, result.Error);
                    if (ownsTransaction) await transaction.RollbackAsync(cancellationToken);
                    return result;
                }

                // REFACTOR: Removed final SaveChanges as ReceiveInventoryProducts handles its own persistence within the shared transaction;
                // this avoids redundant saves and leverages EF Core's automatic savepoint rollback on errors for safety.

                // REFACTOR: Commit only if this handler owns the transaction; defers to outer transaction otherwise.
                // Improves integration in nested scenarios without premature commits.
                if (ownsTransaction)
                {
                    await transaction.CommitAsync(cancellationToken);
                    _logger.LogDebug("Committed transaction for ReceiveInventoryFromPurchaseOrder.");
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "An error occurred while processing ReceiveInventoryFromPurchaseOrder for OrderId: {OrderId}.",
                    request.OrderId);
                if (ownsTransaction) await transaction.RollbackAsync(cancellationToken);
                return Result<ReceiveInventoryResult>.Failure(
                    $"An error occurred while receiving inventory: {ex.Message}");
            }
            finally
            {
                // REFACTOR: Dispose transaction only if owned to avoid interfering with outer transactions.
                if (ownsTransaction && transaction != null) await transaction.DisposeAsync();
            }
        }

        private async Task<List<OrderItem>> GetOrderItemsForPurchaseOrder(string orderId,
            CancellationToken cancellationToken)
        {
            // REFACTOR: Used Include to eagerly load Product data, reducing database queries.
            // This improves performance by fetching all necessary data in a single query.
            return await _context.OrderItems
                .Where(oi => oi.OrderId == orderId)
                .Include(oi => oi.Product)
                .ToListAsync(cancellationToken);
        }

        private async Task<ReceiveInventoryItemsDto> ConstructReceiveInventoryItemsDto(
            string orderId, string? facilityId, List<OrderItem> orderItems, CancellationToken cancellationToken)
        {
            var items = new List<ReceiveInventoryItemDto>();
            foreach (var orderItem in orderItems)
            {
                // REFACTOR: Simplified DTO mapping with default values to reduce boilerplate code.
                // Defaults like QuantityAccepted = Quantity ensure full acceptance unless overridden.
                var itemDto = new ReceiveInventoryItemDto
                {
                    OrderId = orderItem.OrderId,
                    OrderItemSeqId = orderItem.OrderItemSeqId,
                    ProductId = orderItem.ProductId,
                    FacilityId = facilityId,
                    Quantity = orderItem.Quantity,
                    QuantityAccepted = orderItem.Quantity,
                    QuantityRejected = 0,
                    UnitPrice = orderItem.UnitPrice,
                    RejectionReasonId = null
                };
                items.Add(itemDto);
            }

            return new ReceiveInventoryItemsDto { OrderItems = items };
        }
    }
}