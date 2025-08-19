using Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Service;

public class VehiclesController : BaseApiController
{
    [HttpPost("createVehicle", Name = "CreateVehicle")]
    public async Task<IActionResult> CreateVehicle(VehicleDto vehicleDto)
    {
        return HandleResult(await Mediator.Send(new CreateVehicle.Command { VehicleDto = vehicleDto }));
    }

    [HttpPut("updateVehicle", Name = "UpdateVehicle")]
    public async Task<IActionResult> UpdateVehicle(VehicleDto vehicleDto)
    {
        return HandleResult(await Mediator.Send(new UpdateVehicle.Command { VehicleDto = vehicleDto }));
    }

    [HttpGet("getVehiclesLov", Name = "GetVehiclesLov")]
    public async Task<IActionResult> GetVehiclesLov([FromQuery] VehicleLovParams param)
    {
        return HandleResult(await Mediator.Send(new GetVehiclesLov.Query { Params = param }));
    }
}