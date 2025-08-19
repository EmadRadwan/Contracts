using Application.Accounting.Services;
using Application.Order.Orders;



using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Shipments.Taxes;

public class CalculateTaxAdjustmentsForSalesOrder
{
    public class Command : IRequest<Result<OrderAdjustmentDto2[]>>
    {
        public OrderItemsAndAdjustmentsDto OrderItemsAndAdjustments { get; set; }
    }


    public class Handler : IRequestHandler<Command, Result<OrderAdjustmentDto2[]>>
    {
        private readonly ILogger<CalculateTaxAdjustmentsForSalesOrder> _logger;
        private readonly ITaxService _taxService;

        public Handler(ITaxService taxService, ILogger<CalculateTaxAdjustmentsForSalesOrder> logger)
        {
            _logger = logger;
            _taxService = taxService;
        }

        public async Task<Result<OrderAdjustmentDto2[]>> Handle(Command request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Calculate Tax Adjustments For Sales Order Started. {Transaction} for {OrderId}",
                    "calculate tax adjustments for sales order",
                    request.OrderItemsAndAdjustments.OrderItems.First().OrderId);

                var orderAdjustments =
                    await _taxService.CalculateTaxAdjustmentsForSalesOrder(request.OrderItemsAndAdjustments);
                return Result<OrderAdjustmentDto2[]>.Success(orderAdjustments);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(
                    "An error occurred while calculate tax adjustments for sales order {Transaction}. Stack Trace: {StackTrace}",
                    "calculate tax adjustments for sales order", ex.StackTrace);
                return Result<OrderAdjustmentDto2[]>.Failure(
                    "An error occurred while calculate tax adjustments for sales order");
            }
        }
    }
}