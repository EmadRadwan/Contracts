using Application.Catalog.Products.Services.Inventory;
using Application.Facilities;
using Application.Facilities.Facilities;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Facility;

public class FacilitiesController : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> GetFacilities([FromQuery] FacilityParams param)
    {
        var lang = GetLanguage();
        return HandleResult(await Mediator.Send(new ListFacilities.Query { Params = param, Language = lang }));
    }
    
    [HttpGet("getRowMaterialFacilities")]
    public async Task<IActionResult> GetRowMaterialFacilities([FromQuery] FacilityParams param)
    {
        var lang = GetLanguage();
        return HandleResult(await Mediator.Send(new ListRowMaterialFacilities.Query { Params = param, Language = lang }));
    }
    
    [HttpGet("getFinishedProductFacilities")]
    public async Task<IActionResult> GetFinishedProductFacilities([FromQuery] FacilityParams param)
    {
        var lang = GetLanguage();
        return HandleResult(await Mediator.Send(new ListFinishedProductFacilities.Query { Params = param, Language = lang }));
    }


    [HttpPut]
    public async Task<IActionResult> UpdateFacility(Domain.Facility facility)
    {
        return HandleResult(await Mediator.Send(new UpdateFacility.Command { Facility = facility }));
    }

    [HttpPost]
    public async Task<IActionResult> CreateFacility(Domain.Facility facility)
    {
        return HandleResult(await Mediator.Send(new CreateFacility.Command { Facility = facility }));
    }


    [HttpGet("findOrdersToPickMove")]
    public async Task<IActionResult> FindOrdersToPickMove(
        [FromQuery] string facilityId,
        [FromQuery] string? shipmentMethodTypeId,
        [FromQuery] string? isRushOrder,
        [FromQuery] long? maxNumberOfOrders,
        [FromQuery] string? groupByNoOfOrderItems,
        [FromQuery] string? groupByWarehouseArea,
        [FromQuery] string? groupByShippingMethod,
        [FromQuery] string? orderId)
    {
        return HandleResult(await Mediator.Send(new FindOrdersToPickMove.Query
        {
            FacilityId = facilityId,
            ShipmentMethodTypeId = shipmentMethodTypeId,
            IsRushOrder = isRushOrder,
            MaxNumberOfOrders = maxNumberOfOrders,
            GroupByNoOfOrderItems = groupByNoOfOrderItems,
            GroupByWarehouseArea = groupByWarehouseArea,
            GroupByShippingMethod = groupByShippingMethod,
            OrderId = orderId,
            OrderHeaderList = null // Optional parameter, not provided via query
        }));
    }
    
    [HttpPost("createPicklist")]
    public async Task<IActionResult> CreatePicklist([FromBody] CreatePicklistRequest request)
    {
        return HandleResult(await Mediator.Send(new CreatePicklistCommand
        {
            FacilityId = request.FacilityId,
            OrderReadyToPickInfoList = request.OrderReadyToPickInfoList,
            OrderNeedsStockMoveInfoList = request.OrderNeedsStockMoveInfoList
        }));
    }
    
    [HttpGet("getPicklistDisplayInfo")]
    public async Task<IActionResult> GetPicklistDisplayInfo(
        [FromQuery] string facilityId,
        [FromQuery] int? viewIndex,
        [FromQuery] int? viewSize)
    {
        return HandleResult(await Mediator.Send(new GetPicklistDisplayInfoQuery
        {
            FacilityId = facilityId,
            ViewIndex = viewIndex,
            ViewSize = viewSize
        }));
    }

    [HttpGet("findStockMovesNeeded")]
    public async Task<IActionResult> FindStockMovesNeeded([FromQuery] string facilityId)
    {
        // Send the MediatR command
        return HandleResult(await Mediator.Send(new FindStockMovesNeededCommand
        {
            FacilityId = facilityId
        }));
    }
    
    [HttpPost("updatePicklist")]
    public async Task<IActionResult> UpdatePicklist([FromBody] UpdatePicklistCommand command)
    {
        return HandleResult(await Mediator.Send(command));
    }

    [HttpGet("loadPackingData")]
    public async Task<IActionResult> LoadPackingData(
        [FromQuery] string facilityId,
        [FromQuery] string? shipmentId,
        [FromQuery] string? orderId,
        [FromQuery] string? shipGroupSeqId,
        [FromQuery] string? picklistBinId)
    {
        var command = new LoadPackingDataCommand
        {
            FacilityId = facilityId,
            ShipmentId = shipmentId,
            OrderId = orderId,
            ShipGroupSeqId = shipGroupSeqId,
            PicklistBinId = picklistBinId
        };
        return HandleResult(await Mediator.Send(command));
    }
    
    [HttpPost("createPhysicalInventoryAndVariance")]
    public async Task<IActionResult> CreatePhysicalInventoryAndVariance([FromBody] PhysicalInventoryVarianceDto dto)
    {
        return HandleResult(await Mediator.Send(new CreatePhysicalInventoryAndVariance.Command { Dto = dto }));
    }
    
    [HttpGet("getValidOrderPicklistBins")]
    public async Task<IActionResult> GetValidOrderPicklistBins([FromQuery] string facilityId)
    {
        return HandleResult(await Mediator.Send(new GetValidOrderPicklistBinsCommand
        {
            FacilityId = facilityId
        }));
    }

}