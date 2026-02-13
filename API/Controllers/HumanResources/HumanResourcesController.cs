using Application.HumanResources;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.HumanResources;

[ApiController]
[Route("api/[controller]")]
public class HumanResourcesController : BaseApiController
{
    [HttpPost("createEmployeeAdvance")]
    public async Task<IActionResult> CreateEmployeeAdvance(EmployeeAdvanceRecord advanceDto)
    {
        return HandleResult(await Mediator.Send(new CreateEmployeeAdvance.Command { AdvanceDto = advanceDto, Language = GetLanguage() }));
    }

    [HttpPut("updateEmployeeAdvance")]
    public async Task<IActionResult> UpdateEmployeeAdvance(EmployeeAdvanceRecord advanceDto)
    {
        return HandleResult(await Mediator.Send(new UpdateEmployeeAdvance.Command { AdvanceDto = advanceDto, Language = GetLanguage() }));
    }
}
