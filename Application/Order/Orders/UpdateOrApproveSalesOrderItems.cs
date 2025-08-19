using Application.Interfaces;
using Domain;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Order.Orders;

public class UpdateOrApproveSalesOrderItems
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
            var transaction = _context.Database.BeginTransaction();

            var updateMode = request.OrderItemsDto.ModificationType == "UPDATE" ? "UPDATE" : "APPROVE";

            var user = await _context.Users.FirstOrDefaultAsync(x =>
                x.UserName == _userAccessor.GetUsername());

            var productStore = await _context.ProductStores.SingleOrDefaultAsync();

            var stamp = DateTime.UtcNow;

            var inventoryItemDetailSequenceRecord = await _context.SequenceValueItems
                .Where(x => x.SeqName == "InventoryItemDetail").SingleOrDefaultAsync();

            foreach (var updatedItem in request.OrderItemsDto.OrderItems)
            {
                var savedItem = _context.OrderItems.ToList()
                    .FirstOrDefault(item => item.OrderId == updatedItem.OrderId &&
                                            item.OrderItemSeqId == updatedItem.OrderItemSeqId);

                if (savedItem != null)
                {
                    var orderStatuses = _context.OrderStatuses.Where(x => x.OrderId
                        == updatedItem.OrderId && x.OrderItemSeqId == updatedItem.OrderItemSeqId);

                    var orderItemShipGrpInvReses = _context.OrderItemShipGrpInvRes
                        .Where(x => x.OrderId == updatedItem.OrderId
                                    && x.OrderItemSeqId == updatedItem.OrderItemSeqId).ToList();

                    var inventoryItemDetails = _context.InventoryItemDetails
                        .Where(x => x.OrderId == updatedItem.OrderId
                                    && x.OrderItemSeqId == updatedItem.OrderItemSeqId).ToList();


                    if (updatedItem.IsProductDeleted)
                    {
                        _context.OrderStatuses.RemoveRange(orderStatuses);
                        _context.OrderItemShipGrpInvRes.RemoveRange(orderItemShipGrpInvReses);
                        foreach (var itemDetail in inventoryItemDetails)
                        {
                            var inventoryItem = _context.InventoryItems.SingleOrDefault(
                                x => x.InventoryItemId == itemDetail.InventoryItemId);

                            inventoryItem.AvailableToPromiseTotal -= itemDetail.AvailableToPromiseDiff * -1;
                            inventoryItem.LastUpdatedStamp = stamp;
                        }

                        _context.InventoryItemDetails.RemoveRange(inventoryItemDetails);

                        _context.OrderItems.Remove(savedItem);
                    }
                    else
                    {
                        // Since many parameters may have been changes, so undo the previous changes and then apply as a new record
                        _context.OrderItemShipGrpInvRes.RemoveRange(orderItemShipGrpInvReses);

                        // reverse effect for inventoryItems and remove inventoryItemDetails
                        foreach (var inventoryItemDetail in inventoryItemDetails)
                        {
                            var inventoryItem = _context.InventoryItems.SingleOrDefault(
                                x => x.InventoryItemId == inventoryItemDetail.InventoryItemId);

                            inventoryItem.AvailableToPromiseTotal -= inventoryItemDetail.AvailableToPromiseDiff;
                            inventoryItem.LastUpdatedStamp = stamp;
                        }

                        _context.InventoryItemDetails.RemoveRange(inventoryItemDetails);

                        // loop through orderStatuses and update LastUpdatedStamp for each record with stamp
                        foreach (var orderStatus in orderStatuses)
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

                        var inventoryItemsQuery = _context.InventoryItems
                            .Where(x => x.ProductId == updatedItem.ProductId && x.AvailableToPromiseTotal > 0)
                            .AsQueryable();

                        List<InventoryItem> inventoryItems;

                        if (productStore.ReserveOrderEnumId == "INVRO_FIFO_REC")
                            inventoryItems = inventoryItemsQuery.OrderBy(x => x.CreatedStamp).ToList();
                        else
                            inventoryItems = inventoryItemsQuery.OrderByDescending(x => x.CreatedStamp).ToList();

                        var itemQuantity = updatedItem.Quantity;

                        foreach (var invItem in inventoryItems)
                            if (invItem.AvailableToPromiseTotal >= itemQuantity)
                            {
                                invItem.AvailableToPromiseTotal += itemQuantity * -1;
                                invItem.LastUpdatedStamp = stamp;
                                var orderItemShipGrpInvRes = new OrderItemShipGrpInvRes
                                {
                                    OrderId = updatedItem.OrderId,
                                    ShipGroupSeqId = "01",
                                    OrderItemSeqId = updatedItem.OrderItemSeqId,
                                    InventoryItemId = invItem.InventoryItemId,
                                    Quantity = itemQuantity,
                                    QuantityNotAvailable = null,
                                    ReservedDatetime = stamp,
                                    CreatedDatetime = stamp,
                                    LastUpdatedStamp = stamp,
                                    CreatedStamp = stamp
                                };
                                _context.OrderItemShipGrpInvRes.Add(orderItemShipGrpInvRes);

                                var newInventoryItemDetailSequence = inventoryItemDetailSequenceRecord.SeqId + 1;
                                inventoryItemDetailSequenceRecord.SeqId = newInventoryItemDetailSequence;
                                var inventoryItemDetail = new InventoryItemDetail
                                {
                                    InventoryItemId = invItem.InventoryItemId,
                                    InventoryItemDetailSeqId = newInventoryItemDetailSequence.ToString(),
                                    EffectiveDate = stamp,
                                    QuantityOnHandDiff = 0,
                                    AvailableToPromiseDiff = itemQuantity * -1,
                                    AccountingQuantityDiff = 0,
                                    OrderId = updatedItem.OrderId,
                                    OrderItemSeqId = updatedItem.OrderItemSeqId,
                                    ShipGroupSeqId = "01",
                                    LastUpdatedStamp = stamp,
                                    CreatedStamp = stamp
                                };
                                _context.InventoryItemDetails.Add(inventoryItemDetail);

                                break;
                            }
                            else
                            {
                                itemQuantity -= invItem.AvailableToPromiseTotal;

                                invItem.LastUpdatedStamp = stamp;
                                var orderItemShipGrpInvRes = new OrderItemShipGrpInvRes
                                {
                                    OrderId = updatedItem.OrderId,
                                    ShipGroupSeqId = "01",
                                    OrderItemSeqId = updatedItem.OrderItemSeqId,
                                    InventoryItemId = invItem.InventoryItemId,
                                    Quantity = invItem.AvailableToPromiseTotal,
                                    QuantityNotAvailable = null,
                                    ReservedDatetime = stamp,
                                    CreatedDatetime = stamp,
                                    LastUpdatedStamp = stamp,
                                    CreatedStamp = stamp
                                };
                                _context.OrderItemShipGrpInvRes.Add(orderItemShipGrpInvRes);


                                var newInventoryItemDetailSequence = inventoryItemDetailSequenceRecord.SeqId + 1;
                                inventoryItemDetailSequenceRecord.SeqId = newInventoryItemDetailSequence;
                                var inventoryItemDetail = new InventoryItemDetail
                                {
                                    InventoryItemId = invItem.InventoryItemId,
                                    InventoryItemDetailSeqId = newInventoryItemDetailSequence.ToString(),
                                    EffectiveDate = stamp,
                                    QuantityOnHandDiff = 0,
                                    AvailableToPromiseDiff = invItem.AvailableToPromiseTotal * -1,
                                    AccountingQuantityDiff = 0,
                                    OrderId = updatedItem.OrderId,
                                    OrderItemSeqId = updatedItem.OrderItemSeqId,
                                    ShipGroupSeqId = "01",
                                    LastUpdatedStamp = stamp,
                                    CreatedStamp = stamp
                                };
                                _context.InventoryItemDetails.Add(inventoryItemDetail);

                                invItem.AvailableToPromiseTotal = 0;
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

                    var inventoryItemsQuery = _context.InventoryItems
                        .Where(x => x.ProductId == updatedItem.ProductId && x.AvailableToPromiseTotal > 0)
                        .AsQueryable();

                    List<InventoryItem> inventoryItems;

                    if (productStore.ReserveOrderEnumId == "INVRO_FIFO_REC")
                        inventoryItems = inventoryItemsQuery.OrderBy(x => x.CreatedStamp).ToList();
                    else
                        inventoryItems = inventoryItemsQuery.OrderByDescending(x => x.CreatedStamp).ToList();

                    var itemQuantity = updatedItem.Quantity;

                    foreach (var invItem in inventoryItems)
                        if (invItem.AvailableToPromiseTotal >= itemQuantity)
                        {
                            invItem.AvailableToPromiseTotal += itemQuantity * -1;
                            invItem.LastUpdatedStamp = stamp;
                            var orderItemShipGrpInvRes = new OrderItemShipGrpInvRes
                            {
                                OrderId = updatedItem.OrderId,
                                ShipGroupSeqId = "01",
                                OrderItemSeqId = updatedItem.OrderItemSeqId,
                                InventoryItemId = invItem.InventoryItemId,
                                Quantity = itemQuantity,
                                QuantityNotAvailable = null,
                                ReservedDatetime = stamp,
                                CreatedDatetime = stamp,
                                LastUpdatedStamp = stamp,
                                CreatedStamp = stamp
                            };
                            _context.OrderItemShipGrpInvRes.Add(orderItemShipGrpInvRes);

                            var newInventoryItemDetailSequence = inventoryItemDetailSequenceRecord.SeqId + 1;
                            inventoryItemDetailSequenceRecord.SeqId = newInventoryItemDetailSequence;
                            var inventoryItemDetail = new InventoryItemDetail
                            {
                                InventoryItemId = invItem.InventoryItemId,
                                InventoryItemDetailSeqId = newInventoryItemDetailSequence.ToString(),
                                EffectiveDate = stamp,
                                QuantityOnHandDiff = 0,
                                AvailableToPromiseDiff = itemQuantity * -1,
                                AccountingQuantityDiff = 0,
                                OrderId = updatedItem.OrderId,
                                OrderItemSeqId = updatedItem.OrderItemSeqId,
                                ShipGroupSeqId = "01",
                                LastUpdatedStamp = stamp,
                                CreatedStamp = stamp
                            };
                            _context.InventoryItemDetails.Add(inventoryItemDetail);

                            break;
                        }
                        else
                        {
                            itemQuantity -= invItem.AvailableToPromiseTotal;

                            invItem.LastUpdatedStamp = stamp;
                            var orderItemShipGrpInvRes = new OrderItemShipGrpInvRes
                            {
                                OrderId = updatedItem.OrderId,
                                ShipGroupSeqId = "01",
                                OrderItemSeqId = updatedItem.OrderItemSeqId,
                                InventoryItemId = invItem.InventoryItemId,
                                Quantity = invItem.AvailableToPromiseTotal,
                                QuantityNotAvailable = null,
                                ReservedDatetime = stamp,
                                CreatedDatetime = stamp,
                                LastUpdatedStamp = stamp,
                                CreatedStamp = stamp
                            };
                            _context.OrderItemShipGrpInvRes.Add(orderItemShipGrpInvRes);


                            var newInventoryItemDetailSequence = inventoryItemDetailSequenceRecord.SeqId + 1;
                            inventoryItemDetailSequenceRecord.SeqId = newInventoryItemDetailSequence;
                            var inventoryItemDetail = new InventoryItemDetail
                            {
                                InventoryItemId = invItem.InventoryItemId,
                                InventoryItemDetailSeqId = newInventoryItemDetailSequence.ToString(),
                                EffectiveDate = stamp,
                                QuantityOnHandDiff = 0,
                                AvailableToPromiseDiff = invItem.AvailableToPromiseTotal * -1,
                                AccountingQuantityDiff = 0,
                                OrderId = updatedItem.OrderId,
                                OrderItemSeqId = updatedItem.OrderItemSeqId,
                                ShipGroupSeqId = "01",
                                LastUpdatedStamp = stamp,
                                CreatedStamp = stamp
                            };
                            _context.InventoryItemDetails.Add(inventoryItemDetail);

                            invItem.AvailableToPromiseTotal = 0;
                        }
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