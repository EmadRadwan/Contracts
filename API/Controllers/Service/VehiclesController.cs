using Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Service;

public class VehiclesController : BaseApiController
{
    

    [HttpPut("updateVehicle", Name = "UpdateVehicle")]
    public async Task<IActionResult> UpdateVehicle(VehicleDto vehicleDto)
    {
        return HandleResult(await Mediator.Send(new UpdateVehicle.Command { VehicleDto = vehicleDto }));
    }

}