using Application.Order.Orders;
using AutoMapper;
using MediatR;
using Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Application.order.Orders;
using Application.Order.Orders.Returns;

namespace Application.Order.Orders.Returns;

public class ListReturnItems
{
    public class Query : IRequest<Result<ReturnItemsDto>>
    {
        public string ReturnId { get; set; }
        public string? OrderId { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<ReturnItemsDto>>
    {
        private readonly DataContext _context;
        private readonly IReturnService _returnService;

        public Handler(DataContext context, IReturnService returnService)
        {
            _context = context;
            _returnService = returnService;
        }

        public async Task<Result<ReturnItemsDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var context = new ReturnItemsDto();

            // 1. Check if returnId is provided
            if (string.IsNullOrEmpty(request.ReturnId))
                return Result<ReturnItemsDto>.Failure("ReturnId is required");

            context.ReturnId = request.ReturnId;

            // 2. Retrieve ReturnHeader from database
            var returnHeader = await _context.ReturnHeaders
                .FirstOrDefaultAsync(rh => rh.ReturnId == request.ReturnId, cancellationToken);

            if (returnHeader == null)
            {
                return Result<ReturnItemsDto>.Failure("ReturnHeader not found.");
            }

            context.ReturnHeader = returnHeader;
            context.ToPartyId = returnHeader.ToPartyId;

            // 3. Retrieve related ReturnItems for the given returnId
            context.ReturnItems = await _context.ReturnItems
                .Where(ri => ri.ReturnId == request.ReturnId)
                .ToListAsync(cancellationToken);

            // 4. Retrieve ReturnAdjustments not tied to return items
            context.ReturnAdjustments = await _context.ReturnAdjustments
                .Where(ra => ra.ReturnId == request.ReturnId && ra.ReturnItemSeqId == "_NA_")
                .OrderBy(ra => ra.ReturnItemSeqId)
                .ThenBy(ra => ra.ReturnAdjustmentTypeId)
                .ToListAsync(cancellationToken);

            // 5. Retrieve ReturnTypes
            context.ReturnTypes = await _context.ReturnTypes
                .OrderBy(rt => rt.SequenceId)
                .ToListAsync(cancellationToken);

            // 6. Retrieve Item Status for serialized inventory
            context.ItemStatus = await _context.StatusItems
                .Where(si => si.StatusTypeId == "INV_SERIALIZED_STTS")
                .OrderBy(si => si.StatusId)
                .ThenBy(si => si.Description)
                .ToListAsync(cancellationToken);

            // 7. Retrieve ReturnReasons
            context.ReturnReasons = await _context.ReturnReasons
                .OrderBy(rr => rr.SequenceId)
                .ToListAsync(cancellationToken);
            

            // 9. Return Item Type Map
            var returnItemTypeMap = await _context.ReturnItemTypeMaps
                .Where(rim => rim.ReturnHeaderTypeId == returnHeader.ReturnHeaderTypeId)
                .ToListAsync(cancellationToken);

            context.ReturnItemTypeMap =
                returnItemTypeMap.ToDictionary(value => value.ReturnItemMapKey, value => value.ReturnItemTypeId);

            // 10. Handle Order and Returnable Items
            if (!string.IsNullOrEmpty(request.OrderId))
            {
                var order = await _context.OrderHeaders
                    .FirstOrDefaultAsync(o => o.OrderId == request.OrderId, cancellationToken);

                if (order == null)
                {
                    return Result<ReturnItemsDto>.Failure("Order not found.");
                }

                var returnRes = await _returnService.GetReturnableItems(request.OrderId);
                context.ReturnableItems = returnRes.ReturnableItems;

                var orderReadHelper = new OrderReadHelper(order.OrderId);
                context.Orh = orderReadHelper;
                context.OrderHeaderAdjustments = await orderReadHelper.GetAvailableOrderHeaderAdjustments();

                var shipRes = await _returnService.GetOrderShippingAmount(request.OrderId);
                context.ShippingAmount = shipRes.ShippingAmount;
            }

            // 11. Determine RoleTypeId and PartyId
            string roleTypeId = "PLACING_CUSTOMER";
            string partyId = returnHeader.FromPartyId;

            if (returnHeader.ReturnHeaderTypeId == "VENDOR_RETURN")
            {
                roleTypeId = "BILL_FROM_VENDOR";
                partyId = returnHeader.ToPartyId;
            }

            // 12. Retrieve Party Orders using OrderHeaderItemAndRoles view's underlying tables
            context.PartyOrders = await (from ot in _context.OrderRoles
                    join oh in _context.OrderHeaders on ot.OrderId equals oh.OrderId
                    join oi in _context.OrderItems on oh.OrderId equals oi.OrderId
                    where ot.RoleTypeId == roleTypeId
                          && ot.PartyId == partyId
                          && oi.StatusId == "ITEM_COMPLETED"
                    orderby oh.OrderId
                    select new OrderHeaderItemAndRolesDto
                    {
                        OrderId = oh.OrderId,
                        OrderDate = (DateTime)oh.OrderDate,
                        PartyId = ot.PartyId,
                        RoleTypeId = ot.RoleTypeId,
                        OrderTypeId = oh.OrderTypeId,
                        StatusId = oh.StatusId,
                        ProductId = oi.ProductId,
                        Quantity = (decimal)oi.Quantity,
                        UnitPrice = (decimal)oi.UnitPrice,
                        ItemDescription = oi.ItemDescription,
                        OrderItemSeqId = oi.OrderItemSeqId
                    })
                .ToListAsync(cancellationToken);

            context.PartyId = partyId;

            // 13. Retrieve Return Shipments
            context.ReturnItemShipments = await _context.ReturnItemShipments
                .Where(ris => ris.ReturnId == request.ReturnId)
                .Distinct()
                .ToListAsync(cancellationToken);

            // 14. Create and map ReturnStatus DTO
            var ReturnItemsDto = new ReturnItemsDto
            {
                ReturnId = request.ReturnId,
                StatusId = returnHeader.StatusId,
                ReturnItemSeqId = context.ReturnItems.FirstOrDefault()?.ReturnItemSeqId,
                ReturnItems = context.ReturnItems, // Adding ReturnItems to DTO
                ReturnAdjustments = context.ReturnAdjustments, // Adding ReturnAdjustments to DTO
                ReturnTypes = context.ReturnTypes, // Adding ReturnTypes to DTO
                ItemStatus = context.ItemStatus, // Adding ItemStatus to DTO
                ReturnReasons = context.ReturnReasons, // Adding ReturnReasons to DTO
                ItemStts = context.ItemStts, // Adding ItemStts to DTO
                ReturnItemTypeMap = context.ReturnItemTypeMap, // Adding ReturnItemTypeMap to DTO
                PartyOrders = context.PartyOrders, // Adding PartyOrders to DTO
                ReturnItemShipments = context.ReturnItemShipments // Adding ReturnShipmentIds to DTO
            };

            // 15. Return Success
            return Result<ReturnItemsDto>.Success(ReturnItemsDto);
        }
    }
}