using Application.order.Orders;
using Application.Order.Orders;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Order;

public class OrdersController : BaseApiController
{
    [HttpGet("{orderId}/getSalesOrder")]
    public async Task<IActionResult> GetSalesOrder(string orderId)
    {
        return HandleResult(await Mediator.Send(new ListSalesOrder.Query { OrderId = orderId }));
    }

    [HttpGet("{partyId}/getSalesOrdersByPartyId")]
    public async Task<IActionResult> GetSalesOrdersByPartyId(string partyId)
    {
        return HandleResult(await Mediator.Send(new ListSalesOrdersByPartyId.Query { PartyId = partyId }));
    }


    [HttpGet("{orderId}/getSalesOrderItems")]
    public async Task<IActionResult> GetSalesOrderItems(string orderId)
    {
        var language = GetLanguage();
        return HandleResult(await Mediator.Send(new ListSalesOrderItems.Query { OrderId = orderId, Language = language }));
    }

    [HttpGet("{orderId}/getPurchaseOrderItems")]
    public async Task<IActionResult> GetPurchaseOrderItems(string orderId)
    {
        var language = GetLanguage();
        return HandleResult(await Mediator.Send(new ListPurchaseOrderItems.Query { OrderId = orderId, Language = language }));
    }
    
    [HttpPost("listPurchaseOrderItemsForReceive")]
    public async Task<IActionResult> ListPurchaseOrderItemsForReceive([FromBody] ReceiveInventoryRequestDto receiveInventoryRequestDto)
    {
        var language = GetLanguage();
        return HandleResult(await Mediator.Send(new ListPurchaseOrderItemsForReceive.Query 
        { 
            ReceiveInventoryRequestDto = receiveInventoryRequestDto,
            Language = language
        }));
    }
    
    [HttpPost("quickReceivePurchaseOrder")]
    public async Task<IActionResult> QuickReceivePurchaseOrder([FromBody] ReceiveInventoryRequestDto receiveInventoryRequestDto)
    {
        return HandleResult(await Mediator.Send(new QuickReceivePurchaseOrder.Query { ReceiveInventoryRequestDto = receiveInventoryRequestDto }));
    }

    [HttpGet("{orderId}/getSalesOrderAdjustments")]
    public async Task<IActionResult> GetSalesOrderAdjustments(string orderId)
    {
        return HandleResult(await Mediator.Send(new ListSalesOrderAdjustments.Query { OrderId = orderId }));
    }

    [HttpGet("{orderItemId}/getSalesOrderItemProduct")]
    public async Task<IActionResult> GetSalesOrderItemProduct(string orderItemId)
    {
        return HandleResult(await Mediator.Send(new GetSalesOrderItemProduct.Query { OrderItemId = orderItemId }));
    }

    [HttpGet("{orderId}/getSalesOrderPaymentPreference")]
    public async Task<IActionResult> GetSalesOrderPaymentPreference(string orderId)
    {
        return HandleResult(await Mediator.Send(new GetSalesOrderPaymentPreference.Query { OrderId = orderId }));
    }

    [HttpGet("{orderId}/getOrderTerms")]
    public async Task<IActionResult> GetOrderTerms(string orderId)
    {
        var language = GetLanguage();
        return HandleResult(await Mediator.Send(new GetOrderTerms.Query { OrderId = orderId, Langauge = language }));
    }


    [HttpPost("createSalesOrder", Name = "CreateSalesOrder")]
    public async Task<IActionResult> CreateSalesOrder(OrderDto orderDto)
    {
        return HandleResult(await Mediator.Send(new CreateSalesOrder.Command { OrderDto = orderDto }));
    }
    
    // Endpoint in the controller
    [HttpGet("getBackOrderedQuantity/{orderId}", Name = "GetItemBackOrderedQuantity")]
    public async Task<IActionResult> GetItemBackOrderedQuantity(string OrderId)
    {
        // REFACTOR: Use Mediator pattern to decouple controller from handler logic, consistent with provided example
        return HandleResult(await Mediator.Send(new GetItemBackOrderedQuantity.Query { OrderId = OrderId }));
    }


    [HttpPut("updateOrApproveSalesOrderItems", Name = "UpdateOrApproveSalesOrderItems")]
    public async Task<IActionResult> UpdateOrApproveSalesOrderItems(OrderItemsDto orderItemsDto)
    {
        return HandleResult(await Mediator.Send(new UpdateOrApproveSalesOrderItems.Command
            { OrderItemsDto = orderItemsDto }));
    }

    [HttpPut("updateOrApproveSalesOrderAdjustments", Name = "UpdateOrApproveSalesOrderAdjustments")]
    public async Task<IActionResult> UpdateOrApproveSalesOrderAdjustments(OrderAdjustmentsDto orderAdjustmentsDto)
    {
        return HandleResult(await Mediator.Send(new UpdateOrApproveSalesOrderAdjustments.Command
            { OrderAdjustmentsDto = orderAdjustmentsDto }));
    }


    [HttpGet("{orderId}/getPurchaseOrder")]
    public async Task<IActionResult> GetPurchaseOrder(string orderId)
    {
        return HandleResult(await Mediator.Send(new ListPurchaseOrder.Query { OrderId = orderId }));
    }

    [HttpGet("getApprovedPurchaseOrders")]
    public async Task<IActionResult> GetApprovedPurchaseOrders([FromQuery] string? partyId)
    {
        return HandleResult(await Mediator.Send(new ListApprovedPurchaseOrders.Query { PartyId = partyId }));
    }


    [HttpPut("updateOrApproveSalesOrder", Name = "UpdateOrApproveSalesOrder")]
    public async Task<IActionResult> UpdateOrApproveSalesOrder(OrderDto orderDto)
    {
        return HandleResult(await Mediator.Send(new UpdateOrApproveSalesOrder.Command { OrderDto = orderDto }));
    }


    [HttpPost("createPurchaseOrder", Name = "CreatePurchaseOrder")]
    public async Task<IActionResult> CreatePurchaseOrder(OrderDto orderDto)
    {
        return HandleResult(await Mediator.Send(new CreatePurchaseOrder.Command { OrderDto = orderDto }));
    }


    [HttpPost("createPurchaseOrderItems", Name = "CreatePurchaseOrderItems")]
    public async Task<IActionResult> CreatePurchaseOrderItems(OrderItemsDto orderItemsDto)
    {
        return HandleResult(await Mediator.Send(new CreatePurchaseOrderItems.Command
            { OrderItemsDto = orderItemsDto }));
    }

    [HttpPut("updateOrApprovePurchaseOrder", Name = "UpdateOrApprovePurchaseOrder")]
    public async Task<IActionResult> UpdateOrApprovePurchaseOrder(OrderDto orderDto)
    {
        return HandleResult(await Mediator.Send(new UpdateOrApprovePurchaseOrder.Command { OrderDto = orderDto }));
    }

    [HttpPut("updateOrApprovePurchaseOrderItems", Name = "UpdateOrApprovePurchaseOrderItems")]
    public async Task<IActionResult> UpdateOrApprovePurchaseOrderItems(OrderItemsDto orderItemsDto)
    {
        return HandleResult(await Mediator.Send(new UpdateOrApprovePurchaseOrderItems.Command
            { OrderItemsDto = orderItemsDto }));
    }

    [HttpPut("quickShipSalesOrder", Name = "QuickShipSalesOrder")]
    public async Task<IActionResult> QuickShipSalesOrder(OrderDto orderDto)
    {
        return HandleResult(await Mediator.Send(new QuickShipSalesOrder.Command { OrderDto = orderDto }));
    }
    
    [HttpGet("getSalesOrdersApproved")]
    public async Task<IActionResult> GetApprovedSalesOrders([FromQuery] string? partyId)
    {
        return HandleResult(await Mediator.Send(new ListApprovedSalesOrders.Query { PartyId = partyId }));
    }
    
    [HttpGet("picklistBins")]
    public async Task<IActionResult> GetPickListBins()
    {
        return HandleResult(await Mediator.Send(new GetPickListBinsQuery()));
    }

}