using Application.Accounting.Services;



using Application.Interfaces;
using Application.order.Orders;
using Application.Shipments;
using Domain;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Order.Orders;

public class QuickShipSalesOrder
{
    public class Command : IRequest<Result<OrderDto>>
    {
        public OrderDto OrderDto { get; set; }
    }

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(x => x.OrderDto).SetValidator(new OrderValidator());
        }
    }

    public class Handler : IRequestHandler<Command, Result<OrderDto>>
    {
        private readonly DataContext _context;
        private readonly ILogger _logger;
        private readonly IShipmentHelperService _shipmentHelperService;
        private readonly IOrderHelperService _orderHelperService;
        private readonly IInvoiceHelperService _invoiceHelperService;
        private readonly IUserAccessor _userAccessor;

        public Handler(DataContext context, IUserAccessor userAccessor,
            IShipmentHelperService shipmentHelperService, ILogger<Handler> logger, IOrderHelperService orderHelperService, IInvoiceHelperService invoiceHelperService)
        {
            _userAccessor = userAccessor;
            _context = context;
            _shipmentHelperService = shipmentHelperService;
            _orderHelperService = orderHelperService;
            _invoiceHelperService = invoiceHelperService;
            _logger = logger;
        }

        public async Task<Result<OrderDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                _logger.LogInformation("Order Complete Started. {Transaction} for {OrderId}",
                    "complete sales order", request.OrderDto.OrderId);

                var order = await _shipmentHelperService.QuickShipEntireOrder(request.OrderDto.OrderId, null, null, null);
                
                
                _logger.LogInformation("Order Complete successfully. {Transaction} for {OrderId}",
                    "complete sales order", request.OrderDto.OrderId);
                
                string orderId = request.OrderDto.OrderId; // Replace with the actual Order ID
                string currentOrderStatus = "ORDER_APPROVED"; // Current order status
                string newOrderStatus = "ORDER_COMPLETED"; // The new status to move to
                string fromItemStatus = "ITEM_APPROVED"; // Status for order items
                string toItemStatus = "ITEM_COMPLETED"; // The status for order items after the change
                string digitalItemStatus = "DIGITAL_ITEM_COMPLETED"; // Optional: For digital items

                await _orderHelperService.OrderStatusChanges(orderId, newOrderStatus, fromItemStatus, toItemStatus, digitalItemStatus);
                
                string paymentId = null;
                string invoiceId = null;

                    // Example: Your business logic that creates or modifies PaymentApplication records here

                // Later, use the change tracker to find the PaymentApplication entity entry.
                var paymentAppEntry = _context.ChangeTracker
                    .Entries<PaymentApplication>()
                    .FirstOrDefault(e => e.State == EntityState.Added || 
                                         e.State == EntityState.Modified || 
                                         e.State == EntityState.Deleted);

                if (paymentAppEntry != null)
                {
                    // Retrieve the InvoiceId and PaymentId from the entity entry.
                    invoiceId = paymentAppEntry.Property("InvoiceId").CurrentValue?.ToString();
                    paymentId = paymentAppEntry.Property("PaymentId").CurrentValue?.ToString();
                }

                var result = await _context.SaveChangesAsync(cancellationToken) > 0;
                
                await transaction.CommitAsync(cancellationToken);

                var orderToReturn = new OrderDto
                {
                    OrderId = request.OrderDto.OrderId,
                    FromPartyId = request.OrderDto.FromPartyId,
                    CurrentMileage = request.OrderDto.CurrentMileage,
                    CustomerRemarks = request.OrderDto.CustomerRemarks,
                    InternalRemarks = request.OrderDto.InternalRemarks,
                    StatusDescription = "Completed",
                    InvoiceId = invoiceId,
                    PaymentId = paymentId
                };

                return Result<OrderDto>.Success(orderToReturn);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                foreach (var entry in ex.Entries)
                {
                    var entityType = entry.Entity.GetType().Name;
                    var keyValues = string.Join(", ", entry.Properties
                        .Where(p => p.Metadata.IsPrimaryKey())
                        .Select(p => $"{p.Metadata.Name}: {p.CurrentValue}"));

                    _logger.LogError("Concurrency conflict detected on entity {EntityType} with keys {KeyValues}.", entityType, keyValues);
                }

                // Optionally, rethrow or handle accordingly
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while completing the sales order. Stack Trace: {StackTrace}",
                    ex.StackTrace);
                return Result<OrderDto>.Failure("An error occurred while completing the sales order.");
            }
        }
    }
}