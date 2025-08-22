using Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Service;

public class VehicleAnnotations : BaseApiController
{
    


    /*[HttpPost("createVehicleModel", Name = "CreateVehicleModel")]
    public async Task<IActionResult> CreateVehicleModel(VehicleModelDto vehicleModelDto)
    {
        return HandleResult(await Mediator.Send(new CreateVehicleModel.Command { VehicleModelDto = vehicleModelDto }));
    }

    [HttpPut("updateVehicleModel", Name = "UpdateVehicleModel")]
    public async Task<IActionResult> UpdateVehicleModel(VehicleModelDto vehicleModelDto)
    {
        return HandleResult(await Mediator.Send(new UpdateVehicleModel.Command { VehicleModelDto = vehicleModelDto }));
    }*/
}