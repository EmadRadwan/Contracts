using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Order.Orders.Returns
{
    public class GetOrderSummary
    {
        public class Query : IRequest<Result<OrderAmountsDto>>
        {
            public string OrderId { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<OrderAmountsDto>>
        {
            private readonly DataContext _context;

            public Handler(DataContext context)
            {
                _context = context;
            }

            public async Task<Result<OrderAmountsDto>> Handle(Query request, CancellationToken cancellationToken)
            {
                // 1. Validate OrderId
                if (string.IsNullOrEmpty(request.OrderId))
                    return Result<OrderAmountsDto>.Failure("OrderId is required.");

                // 2. Retrieve OrderHeader
                var order = await _context.OrderHeaders
                    .FirstOrDefaultAsync(o => o.OrderId == request.OrderId, cancellationToken);

                if (order == null)
                    return Result<OrderAmountsDto>.Failure("Order not found.");

                // 3. Instantiate OrderReadHelper class
                var orh = new OrderReadHelper(request.OrderId)
                {
                    Context = _context
                };
                orh.InitializeOrder();

                // 4. Calculate amounts
                var orderTotal = await orh.GetOrderGrandTotal();
                var amountCredited = await orh.GetReturnedCreditTotalWithBillingAccountBd();
                var amountRefunded = orh.GetReturnedRefundTotalWithBillingAccountBd();

                // 5. Create DTO
                var orderAmountsDto = new OrderAmountsDto
                {
                    OrderId = request.OrderId,
                    OrderTotal = orderTotal,
                    AmountAlreadyCredited = amountCredited,
                    AmountAlreadyRefunded = amountRefunded
                };

                // 6. Return result
                return Result<OrderAmountsDto>.Success(orderAmountsDto);
            }
        }
    }
}