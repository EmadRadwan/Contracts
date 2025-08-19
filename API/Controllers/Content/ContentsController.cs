using Application.Content;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class ContentsController : BaseApiController
{
    [HttpGet("{vehicleId}/getVehicleContents")]
    public async Task<IActionResult> GetVehicleContents(string vehicleId)
    {
        return HandleResult(await Mediator.Send(new GetVehicleContents.Query { VehicleId = vehicleId }));
    }

    [HttpPost("{vehicleId}")]
    public async Task<IActionResult> AddVehicleContent(string vehicleId, [FromForm] IFormFile file)
    {
        var command = new AddVehicleContent.Command
        {
            VehicleId = vehicleId,
            File = file
        };

        return HandleResult(await Mediator.Send(command));
    }


    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteVehicleContent(string id)
    {
        return HandleResult(await Mediator.Send(new DeleteVehicleContent.Command { Id = id }));
    }
}