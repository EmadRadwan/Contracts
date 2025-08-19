using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Persistence;

namespace Application.Order.Orders.Returns
{
    public class GetReturnableItems
    {
        public class Query : IRequest<ReturnableItemsResult>
        {
            public string OrderId { get; set; }
        }

        public class Handler : IRequestHandler<Query, ReturnableItemsResult>
        {
            private readonly DataContext _context;
            private readonly IReturnService _returnService;

            public Handler(DataContext context, IReturnService returnService)
            {
                _context = context;
                _returnService = returnService;
            }

            public async Task<ReturnableItemsResult> Handle(Query request, CancellationToken cancellationToken)
            {
                // Validate the OrderId
                if (string.IsNullOrEmpty(request.OrderId))
                {
                    return new ReturnableItemsResult
                    {
                        Success = false,
                        Message = "OrderId is required."
                    };
                }

                // Fetch OrderHeader to verify its existence
                var orderHeader = await _context.OrderHeaders
                    .FindAsync(request.OrderId);

                if (orderHeader == null)
                {
                    return new ReturnableItemsResult
                    {
                        Success = false,
                        Message = "Order not found."
                    };
                }

                // Call the GetReturnableItems service and return its result
                return await _returnService.GetReturnableItems(request.OrderId);
            }
        }
    }
}