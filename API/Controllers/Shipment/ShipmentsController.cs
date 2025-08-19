using Application.Facilities;
using Application.Order.Orders;
using Application.Shipments;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Shipment;

public class ShipmentsController : BaseApiController
{
    [HttpGet("{orderId}/getShipmentReceipts")]
    public async Task<IActionResult> GetShipmentReceipts(string orderId)
    {
        return HandleResult(await Mediator.Send(new GetShipmentReceipts.Query { OrderId = orderId }));
    }
    
    /*[HttpGet("findOrdersToPickMove")]
    public async Task<IActionResult> FindOrdersToPickMove(
        [FromQuery] string facilityId,
        [FromQuery] string? shipmentMethodTypeId,
        [FromQuery] string? isRushOrder,
        [FromQuery] long? maxNumberOfOrders,
        [FromQuery] List<OrderHeader>? orderHeaderList,
        [FromQuery] string? groupByNoOfOrderItems,
        [FromQuery] string? groupByWarehouseArea,
        [FromQuery] string? groupByShippingMethod,
        [FromQuery] string? orderId)
    {
        var query = new FindOrdersToPickMove.Query
        {
            FacilityId = facilityId,
            ShipmentMethodTypeId = shipmentMethodTypeId,
            IsRushOrder = isRushOrder,
            MaxNumberOfOrders = maxNumberOfOrders,
            OrderHeaderList = orderHeaderList,
            GroupByNoOfOrderItems = groupByNoOfOrderItems,
            GroupByWarehouseArea = groupByWarehouseArea,
            GroupByShippingMethod = groupByShippingMethod,
            OrderId = orderId
        };

        return HandleResult(await Mediator.Send(query));
    }*/
}