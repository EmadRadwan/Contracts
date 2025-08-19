using Application.Catalog.Products.Services.Inventory;
using Application.Facilities;
using Application.Facilities.FacilityInventories;
using Application.Facilities.InventoryTransfer;
using Application.Facilities.PhysicalInventories;
using Application.Order.Orders;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Facility;

public class FacilityInventoriesController : BaseApiController
{
    [HttpPost("receiveInventoryProducts", Name = "ReceiveInventoryItems")]
    public async Task<IActionResult> ReceiveInventoryProducts(ReceiveInventoryItemsDto ReceivedItems)
    {
        return HandleResult(await Mediator.Send(new ReceiveInventoryProducts.Command
            { ReceivedItems = ReceivedItems }));
    }

    [HttpPost("createInventoryTransfer", Name = "CreateInventoryTransfer")]
    public async Task<IActionResult> CreateInventoryTransfer(InventoryTransferDto inventoryTransferDto)
    {
        return HandleResult(await Mediator.Send(new CreateInventoryTransfer.Command
            { InventoryTransferDto = inventoryTransferDto }));
    }

    [HttpGet("getInventoryAvailableByFacility")]
    public async Task<IActionResult> GetInventoryAvailableByFacility([FromQuery] string facilityId,
        [FromQuery] string productId)
    {
        return HandleResult(await Mediator.Send(new GetInventoryAvailableByFacility.Query
        {
            FacilityId = facilityId,
            ProductId = productId
        }));
    }
    
    [HttpGet("lots")]
    public async Task<IActionResult> GetInventoryItemLots([FromQuery] string productId, [FromQuery] string workEffortId)
    {
        if (string.IsNullOrWhiteSpace(productId) || string.IsNullOrWhiteSpace(workEffortId))
        {
            return BadRequest("ProductId and WorkEffortId are required.");
        }

        var query = new GetInventoryItemLots.Query { ProductId = productId, WorkEffortId = workEffortId };
        var result = await Mediator.Send(query);

        return HandleResult(result);
    }
    
    [HttpPut("packOrder", Name = "PackOrder")]
    public async Task<IActionResult> PackOrder([FromBody] PackOrderDto packOrderDto)
    {
        // Model binding and validation errors will automatically return 400 due to [ApiController]
        var result = await Mediator.Send(new PackOrder.Command
        {
            OrderId = packOrderDto.OrderId,
            ShipGroupSeqId = packOrderDto.ShipGroupSeqId,
            FacilityId = packOrderDto.FacilityId,
            PicklistBinId = packOrderDto.PicklistBinId,
            ItemsToPack = packOrderDto.ItemsToPack
        });

        return HandleResult(result);
    }
    
    [HttpPost("createInventoryItem", Name = "CreateInventoryItem")]
    public async Task<IActionResult> CreateInventoryItem([FromBody] CreateInventoryItem.CreateInventoryItemDto inventoryItem)
    {
        return HandleResult(await Mediator.Send(new CreateInventoryItem.Command { InventoryItemDto = inventoryItem }));
    }
    
    [HttpPost("updateInventoryItem", Name = "UpdateInventoryItem")]
    public async Task<IActionResult> UpdateInventoryItem([FromBody] UpdateInventoryItem.UpdateInventoryItemDto inventoryItem)
    {
        // REFACTOR: Dispatched the MediatR command with the DTO, using HandleResult
        // to standardize response formatting (e.g., 200, 400, 401).
        // Why: Mirrors CreateInventoryItem’s controller logic for consistency and reusability.
        return HandleResult(await Mediator.Send(new UpdateInventoryItem.Command { InventoryItemDto = inventoryItem }));
    }
}