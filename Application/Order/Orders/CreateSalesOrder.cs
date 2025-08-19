using API.Middleware;
using Application.order.Orders;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Persistence;
using Serilog;

namespace Application.Order.Orders;

public class CreateSalesOrder
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
        private readonly ILogger<CreateSalesOrder.Handler> _logger;
        private readonly IOrderService _orderService;

        public Handler(DataContext context, IOrderService orderService, ILogger<CreateSalesOrder.Handler> logger)
        {
            _context = context;
            _logger = logger;
            _orderService = orderService;
        }

        public async Task<Result<OrderDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            var loggerForTransaction = Log.ForContext("Transaction", "create sales order");

            try
            {
                var newSalesOrder = await _orderService.CreateSalesOrder(request.OrderDto);
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                var orderToReturn = new OrderDto
                {
                    OrderId = newSalesOrder.OrderId,
                    FromPartyId = request.OrderDto.FromPartyId,
                    StatusDescription = newSalesOrder.StatusId == "ORDER_APPROVED" ? "Approved" : "Created",
                    CustomerRemarks = request.OrderDto.CustomerRemarks,
                    InternalRemarks = request.OrderDto.InternalRemarks,
                    UseUpToFromBillingAccount = request.OrderDto.UseUpToFromBillingAccount,
                    BillingAccountId = request.OrderDto.BillingAccountId,
                    PaymentMethodId = request.OrderDto.PaymentMethodId,
                    PaymentMethodTypeId = request.OrderDto.PaymentMethodTypeId,
                    CurrencyUomId = request.OrderDto.CurrencyUomId,
                    AgreementId = request.OrderDto.AgreementId,
                };
                
                _logger.LogInformation("Starting to create sales order {orderToReturn}", orderToReturn);
                // Use Serilog for transaction-specific logs
                loggerForTransaction.Information("Successfully created sales order with ID {OrderId}", orderToReturn.OrderId);


                return Result<OrderDto>.Success(orderToReturn);
            }
            catch (ProductNotFoundException ex)
            {
                _logger.LogError(ex, "Product not found when creating sales order.");
                await transaction.RollbackAsync(cancellationToken);
                return Result<OrderDto>.Failure("Product not found.");
            }
            catch (InsufficientInventoryException ex)
            {
                _logger.LogError(ex, "Insufficient inventory when creating sales order.");
                await transaction.RollbackAsync(cancellationToken);
                return Result<OrderDto>.Failure("Insufficient inventory to fulfill order.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred.");
                await transaction.RollbackAsync(cancellationToken);
                return Result<OrderDto>.Failure("An unexpected error occurred while creating the sales order.");
            }
        }
    }
}