using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;
using Serilog;

namespace Application.Order.Orders;

public class GetItemBackOrderedQuantity
{
    public class Query : IRequest<Result<int>>
    {
        public string OrderId { get; set; }
    }

    public class QueryValidator : AbstractValidator<Query>
    {
        public QueryValidator()
        {
            // REFACTOR: Updated error message to reflect OrderId instead of ItemId for clarity
            RuleFor(x => x.OrderId).NotEmpty().WithMessage("Order ID is required.");
        }
    }

    public class Handler : IRequestHandler<Query, Result<int>>
    {
        private readonly DataContext _context;
        private readonly ILogger<GetItemBackOrderedQuantity.Handler> _logger;

        public Handler(DataContext context, ILogger<GetItemBackOrderedQuantity.Handler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Result<int>> Handle(Query request, CancellationToken cancellationToken)
        {
            // REFACTOR: Use try-catch to handle errors from database queries and OrderReadHelper, ensuring robust error handling
            try
            {
                _logger.LogInformation("Fetching back-ordered quantities for order {OrderId}", request.OrderId);

                // REFACTOR: Query OrderItems from DataContext to get all items for the given order, improving data retrieval efficiency
                var orderItems = await _context.OrderItems
                    .Where(oi => oi.OrderId == request.OrderId)
                    .ToListAsync(cancellationToken);

                // REFACTOR: Check if order exists and has items, preventing unnecessary processing
                if (orderItems == null || !orderItems.Any())
                {
                    _logger.LogWarning("No items found for order {OrderId}", request.OrderId);
                    return Result<int>.Success(0); // Return 0 if no items found
                }

                var totalBackOrderedQuantity = 0;
                var orh = new OrderReadHelper(request.OrderId)
                {
                    Context = _context
                };
                orh.InitializeOrder();

                // REFACTOR: Iterate over order items to sum back-ordered quantities, ensuring all items are checked
                foreach (var item in orderItems)
                {
                    var backOrderedQuantity = await orh.GetItemBackOrderedQuantity(item);

                    // REFACTOR: Validate back-ordered quantity to prevent invalid data from affecting the total
                    if (backOrderedQuantity < 0)
                    {
                        _logger.LogWarning(
                            "Invalid back-ordered quantity {Quantity} for item {ItemId} in order {OrderId}",
                            backOrderedQuantity, item.OrderId, request.OrderId);
                        return Result<int>.Failure($"Invalid back-ordered quantity for item {item.OrderId}.");
                    }

                    totalBackOrderedQuantity += (int)backOrderedQuantity;
                }

                // REFACTOR: Use Serilog for structured logging, aligning with provided example for consistency
                Log.ForContext("OrderId", request.OrderId)
                    .Information(
                        "Successfully retrieved total back-ordered quantity {TotalQuantity} for order {OrderId}",
                        totalBackOrderedQuantity, request.OrderId);

                return Result<int>.Success(totalBackOrderedQuantity);
            }
            catch (Exception ex)
            {
                // REFACTOR: Updated error message to reference OrderId, improving error context
                _logger.LogError(ex,
                    "An unexpected error occurred while fetching back-ordered quantity for order {OrderId}.",
                    request.OrderId);
                return Result<int>.Failure("An unexpected error occurred while fetching back-ordered quantity.");
            }
        }
    }
}