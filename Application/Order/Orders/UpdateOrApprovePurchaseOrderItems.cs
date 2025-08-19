using Application.Interfaces;
using Domain;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Order.Orders;

public class UpdateOrApprovePurchaseOrderItems
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

            var updateMode = request.OrderItemsDto.ModificationType == "UPDATE" ? "UPDATE" : "APPROVE";

            var user = await _context.Users.FirstOrDefaultAsync(x =>
                x.UserName == _userAccessor.GetUsername());

            var stamp = DateTime.UtcNow;

            foreach (var updatedItem in request.OrderItemsDto.OrderItems)
            {
                var savedItem = _context.OrderItems.ToList()
                    .FirstOrDefault(item => item.OrderId == updatedItem.OrderId &&
                                            item.OrderItemSeqId == updatedItem.OrderItemSeqId);

                if (savedItem != null)
                {
                    var orderStatus = _context.OrderStatuses.SingleOrDefault(x => x.OrderId
                        == updatedItem.OrderId && x.OrderItemSeqId == updatedItem.OrderItemSeqId);


                    if (updatedItem.IsProductDeleted)
                    {
                        _context.OrderStatuses.Remove(orderStatus);

                        _context.OrderItems.Remove(savedItem);
                    }
                    else
                    {
                        // Since many parameters may have been changes, so undo the previous changes and then apply as a new record

                        orderStatus.LastUpdatedStamp = stamp;

                        // add order item approve status for each item if we are in Approve mode
                        if (updateMode == "APPROVE")
                        {
                            var orderItemCreatedStatus = new OrderStatus
                            {
                                OrderStatusId = Guid.NewGuid().ToString(),
                                StatusId = "ITEM_APPROVED",
                                OrderId = updatedItem.OrderId,
                                OrderItemSeqId = updatedItem.OrderItemSeqId,
                                StatusDatetime = stamp,
                                LastUpdatedStamp = stamp,
                                CreatedStamp = stamp
                            };
                            _context.OrderStatuses.Add(orderItemCreatedStatus);
                        }

                        savedItem.Quantity = updatedItem.Quantity;
                        savedItem.UnitPrice = updatedItem.UnitPrice;
                        savedItem.UnitListPrice = updatedItem.UnitListPrice;
                        savedItem.LastUpdatedStamp = stamp;
                    }
                }
                // new order item
                else
                {
                    var newItem = new OrderItem
                    {
                        OrderId = updatedItem.OrderId,
                        StatusId = updateMode == "Updated" ? "ITEM_CREATED" : "ITEM_APPROVED",
                        OrderItemSeqId = updatedItem.OrderItemSeqId,
                        ProductId = updatedItem.ProductId,
                        ItemDescription = updatedItem.ProductName,
                        Quantity = updatedItem.Quantity,
                        UnitPrice = updatedItem.UnitPrice,
                        UnitListPrice = updatedItem.UnitListPrice
                    };
                    _context.OrderItems.Add(newItem);

                    var orderItemCreatedStatus = new OrderStatus
                    {
                        OrderStatusId = Guid.NewGuid().ToString(),
                        StatusId = updateMode == "Updated" ? "ITEM_CREATED" : "ITEM_APPROVED",
                        OrderId = updatedItem.OrderId,
                        OrderItemSeqId = updatedItem.OrderItemSeqId,
                        StatusDatetime = stamp,
                        LastUpdatedStamp = stamp,
                        CreatedStamp = stamp
                    };
                    _context.OrderStatuses.Add(orderItemCreatedStatus);
                }
            }


            var result = await _context.SaveChangesAsync() > 0;

            if (!result)
            {
                transaction.Rollback();
                return Result<OrderItemsDto>.Failure($"Failed to {updateMode} Sales Order Items");
            }

            transaction.Commit();

            var orderItemsToReturn = new OrderItemsDto
            {
                OrderId = request.OrderItemsDto.OrderId,
                StatusDescription = updateMode == "Update" ? "Created" : "Approved"
            };


            return Result<OrderItemsDto>.Success(orderItemsToReturn);
        }
    }
}