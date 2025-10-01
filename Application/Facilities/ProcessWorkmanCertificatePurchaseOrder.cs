using Application.Order.Orders;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Facilities
{
    public class ProcessWorkmanCertificatePurchaseOrder
    {
        public class Command : IRequest<Result<OrderStatusChangeResult>>
        {
            public string WorkEffortId { get; set; }
        }

        public class OrderStatusChangeResult
        {
            public string OrderId { get; set; }
            public string OrderStatusId { get; set; }
            public string InvoiceId { get; set; }
        }

        public class Handler : IRequestHandler<Command, Result<OrderStatusChangeResult>>
        {
            private readonly DataContext _context;
            private readonly ILogger<Handler> _logger;
            private readonly IMediator _mediator;
            private readonly IOrderHelperService _orderHelperService;

            public Handler(DataContext context, IMediator mediator, ILogger<Handler> logger, IOrderHelperService orderHelperService)
            {
                _context = context;
                _mediator = mediator;
                _logger = logger;
                _orderHelperService = orderHelperService;
            }

            public async Task<Result<OrderStatusChangeResult>> Handle(Command request, CancellationToken cancellationToken)
            {
                IDbContextTransaction? transaction = null;
                var ownsTransaction = false;

                try
                {
                    if (_context.Database.CurrentTransaction == null)
                    {
                        transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                        ownsTransaction = true;
                        _logger.LogDebug("Started new transaction for ProcessWorkEffortCertificate.");
                    }
                    else
                    {
                        _logger.LogDebug("Using existing transaction for ProcessWorkEffortCertificate.");
                    }

                    // REFACTOR: Added validation for WorkEffort to ensure it exists and is linked to a PURCHASE_ORDER;
                    // this aligns with OFBiz's WorkEffort-to-Order linkage (e.g., via WorkEffort.OrderId) and prevents processing invalid certificates.
                    var workEffort = await _context.WorkEfforts
                        .FirstOrDefaultAsync(we => we.WorkEffortId == request.WorkEffortId, cancellationToken);

                    if (workEffort == null)
                    {
                        _logger.LogWarning("WorkEffort with ID {WorkEffortId} not found.", request.WorkEffortId);
                        return Result<OrderStatusChangeResult>.Failure(
                            $"WorkEffort with ID {request.WorkEffortId} not found.");
                    }

                    var orderId = workEffort.RelatedOrderId; // Assumes WorkEffort has RelatedOrderId field
                    if (string.IsNullOrEmpty(orderId))
                    {
                        _logger.LogWarning("No OrderId associated with WorkEffort ID {WorkEffortId}.", request.WorkEffortId);
                        return Result<OrderStatusChangeResult>.Failure(
                            "No purchase order associated with the WorkEffort.");
                    }

                    var purchaseOrder = await _context.OrderHeaders
                        .FirstOrDefaultAsync(po => po.OrderId == orderId, cancellationToken);

                    if (purchaseOrder == null)
                    {
                        _logger.LogWarning("Purchase Order with ID {OrderId} not found.", orderId);
                        return Result<OrderStatusChangeResult>.Failure($"Purchase Order with ID {orderId} not found.");
                    }

                    if (!string.Equals(purchaseOrder.OrderTypeId, "PURCHASE_ORDER", StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.LogWarning("OrderTypeId for Order ID {OrderId} is not 'PURCHASE_ORDER'.", orderId);
                        return Result<OrderStatusChangeResult>.Failure(
                            $"OrderTypeId for Order ID {orderId} is invalid.");
                    }

                    // REFACTOR: Configured OrderStatusChanges call with service-specific parameters;
                    // sets ORDER_COMPLETED and ITEM_COMPLETED for service items to trigger direct invoicing, per OFBiz patterns.
                    await _orderHelperService.OrderStatusChanges(
                        orderId: orderId,
                        orderStatus: "ORDER_COMPLETED",
                        fromItemStatus: null,
                        toItemStatus: "ITEM_COMPLETED",
                        digitalItemStatus: "ITEM_COMPLETED"
                    );

                    // REFACTOR: Added logic to update WorkEfforts CURRENT_STATUS_ID to WEPR_APPROVED for projectCertificate
                    // where WorkEffortId matches the provided ID. This ties certificate approval to successful order processing,
                    // ensuring atomicity within the transaction. Improves traceability by logging the update.
                    workEffort.CurrentStatusId = "WEPR_APPROVED";
                    workEffort.LastStatusUpdate = DateTime.UtcNow; // Update timestamp for auditability
                    _context.WorkEfforts.Update(workEffort);
                    _logger.LogInformation("Updated WorkEffort {WorkEffortId} status to WEPR_APPROVED for OrderId: {OrderId}", request.WorkEffortId, orderId);

                    // REFACTOR: Added SaveChanges to persist WorkEffort update before committing the transaction;
                    // this ensures the certificate status is saved within the same transaction as order status changes.
                    var workEffortSaveResult = await _context.SaveChangesAsync(cancellationToken);
                    if (workEffortSaveResult > 0)
                    {
                        _logger.LogDebug("Persisted WorkEffort status update for WorkEffortId: {WorkEffortId}", request.WorkEffortId);
                    }

                    // REFACTOR: Persist changes only if this handler owns the transaction;
                    // this avoids redundant saves and ensures compatibility with outer transactions in the certificate workflow.
                    if (ownsTransaction)
                    {
                        await transaction.CommitAsync(cancellationToken);
                        _logger.LogDebug("Committed transaction for ProcessWorkEffortCertificate.");
                    }

                    return Result<OrderStatusChangeResult>.Success(new OrderStatusChangeResult
                    {
                        OrderId = orderId,
                        OrderStatusId = "ORDER_APPROVED",
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while processing ProcessWorkEffortCertificate for WorkEffortId: {WorkEffortId}.", request.WorkEffortId);
                    if (ownsTransaction) await transaction.RollbackAsync(cancellationToken);
                    return Result<OrderStatusChangeResult>.Failure(
                        $"An error occurred while processing certificate: {ex.Message}");
                }
                finally
                {
                    // REFACTOR: Dispose transaction only if owned to avoid interfering with outer transactions;
                    // ensures robust integration with certificate approval workflows.
                    if (ownsTransaction && transaction != null)
                        await transaction.DisposeAsync();
                }
            }
        }
    }
}