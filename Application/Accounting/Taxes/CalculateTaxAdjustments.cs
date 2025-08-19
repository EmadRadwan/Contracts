using Application.Accounting.Services;
using Application.Order.Orders;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Accounting.Taxes;

public class CalculateTaxAdjustments
{
    public class Command : IRequest<Result<OrderAdjustmentDto2[]>>
    {
        public OrderItemsToBeTaxedDto OrderItems { get; set; }
    }


    public class Handler : IRequestHandler<Command, Result<OrderAdjustmentDto2[]>>
    {
        private readonly ILogger<CalculateTaxAdjustments> _logger;
        private readonly ITaxService _taxService;

        public Handler(ITaxService taxService, ILogger<CalculateTaxAdjustments> logger)
        {
            _logger = logger;
            _taxService = taxService;
        }

        public async Task<Result<OrderAdjustmentDto2[]>> Handle(Command request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Calculate Tax Adjustments For Sales Order Started. {Transaction} for {OrderId}",
                    "calculate tax adjustments for sales order", request.OrderItems.OrderItems.First().OrderId);

                var orderAdjustments = await _taxService.CalculateTaxAdjustments(request.OrderItems.OrderItems);
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