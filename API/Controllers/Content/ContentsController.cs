using Application.Content;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class ContentsController : BaseApiController
{
    

   


    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteVehicleContent(string id)
    {
        return HandleResult(await Mediator.Send(new DeleteVehicleContent.Command { Id = id }));
    }
}