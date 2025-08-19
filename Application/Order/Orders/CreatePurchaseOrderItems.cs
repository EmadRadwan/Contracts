using Application.Interfaces;
using Domain;
using FluentValidation;
using MediatR;
using Persistence;

namespace Application.Order.Orders;

public class CreatePurchaseOrderItems
{
    public class Command : IRequest<Result<OrderItemsDto>>
    {
        public OrderItemsDto OrderItemsDto { get; set; }
    }

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(x => x.OrderItemsDto).SetValidator(new OrderItemValidator());
        }
    }

    public class Handler : IRequestHandler<Command, Result<OrderItemsDto>>
    {
        private readonly DataContext _context;
        private readonly IUserAccessor _userAccessor;

        public Handler(DataContext context, IUserAccessor userAccessor)
        {
            _userAccessor = userAccessor;
            _context = context;
        }

        public async Task<Result<OrderItemsDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            var stamp = DateTime.UtcNow;


            foreach (var item in request.OrderItemsDto.OrderItems)
            {
                if (item.IsProductDeleted) continue;

                var newItem = new OrderItem
                {
                    OrderId = request.OrderItemsDto.OrderId,
                    OrderItemSeqId = item.OrderItemSeqId,
                    ProductId = item.ProductId,
                    ItemDescription = item.ProductName,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    OrderItemTypeId = "PRODUCT_ORDER_ITEM",
                    StatusId = "ITEM_CREATED",
                    CreatedStamp = stamp,
                    LastUpdatedStamp = stamp
                };
                _context.OrderItems.Add(newItem);

                var orderItemCreatedStatus = new OrderStatus
                {
                    OrderStatusId = Guid.NewGuid().ToString(),
                    StatusId = "ITEM_CREATED",
                    OrderId = request.OrderItemsDto.OrderId,
                    OrderItemSeqId = item.OrderItemSeqId,
                    StatusDatetime = stamp,
                    LastUpdatedStamp = stamp,
                    CreatedStamp = stamp
                };
                _context.OrderStatuses.Add(orderItemCreatedStatus);
            }


            var result = await _context.SaveChangesAsync() > 0;

            if (!result)
            {
                transaction.Rollback();
                return Result<OrderItemsDto>.Failure("Failed to create Purchase Order Items");
            }

            transaction.Commit();


            var orderToReturn = new OrderItemsDto
            {
                OrderId = request.OrderItemsDto.OrderId,
                StatusDescription = "Created"
            };

            return Result<OrderItemsDto>.Success(orderToReturn);
        }
    }
}