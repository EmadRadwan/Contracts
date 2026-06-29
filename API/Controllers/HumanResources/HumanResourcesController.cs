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

    [HttpPost("approveEmployeeAdvance/{id}")]
    public async Task<IActionResult> ApproveEmployeeAdvance(string id)
    {
        return HandleResults(await Mediator.Send(new ApproveEmployeeAdvance.Command { AdvanceId = id, Language = GetLanguage() }));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEmployeeAdvance(string id, [FromQuery] bool dropPayment = false)
    {
        return HandleResults(await Mediator.Send(new DeleteEmployeeAdvance.Command { AdvanceId = id, DropPayment = dropPayment }));
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

    [HttpGet("getEmployeeAdvancesByDateRange")]
    public async Task<IActionResult> GetEmployeeAdvancesByDateRange([FromQuery] DateOnly fromDate, [FromQuery] DateOnly toDate)
    {
        var language = GetLanguage();
        return HandleResults(await Mediator.Send(new ListEmployeeAdvancesByDateRange.Query
        {
            FromDate = fromDate,
            ToDate = toDate,
            Language = language
        }));
    }

    [HttpGet("listPayrollAdvances")]
    public async Task<IActionResult> ListPayrollAdvances([FromQuery] DateTime invoiceDate, [FromQuery] string organizationPartyId)
    {
        return HandleResults(await Mediator.Send(new ListPayrollAdvances.Query
        {
            InvoiceDate = invoiceDate,
            OrganizationPartyId = organizationPartyId,
            Language = GetLanguage()
        }));
    }
}
