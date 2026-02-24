using Application.HumanResources;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.HumanResources;

[ApiController]
[Route("api/[controller]")]
public class HumanResourcesController : BaseApiController
{
    [HttpPost("createEmployeeAdvance")]
    public async Task<IActionResult> CreateEmployeeAdvance(EmployeeAdvanceDto advanceDto)
    {
        return HandleResults(await Mediator.Send(new CreateEmployeeAdvance.Command { AdvanceDto = advanceDto, Language = GetLanguage() }));
    }

    [HttpPut("updateEmployeeAdvance")]
    public async Task<IActionResult> UpdateEmployeeAdvance(EmployeeAdvanceDto advanceDto)
    {
        return HandleResults(await Mediator.Send(new UpdateEmployeeAdvance.Command { AdvanceDto = advanceDto, Language = GetLanguage() }));
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetAdvanceWithSchedules(
        string id,
        [FromQuery] string language = "en")
    {
        var query = new GetEmployeeAdvanceWithSchedulesQuery.Query
        {
            AdvanceId = id,
            Language = language
        };

        var result = await Mediator.Send(query);
        
        if (!result.IsSuccess)
        {
            return NotFound(result.ErrorCode);
        }

        return Ok(result.Value);
    }
}
