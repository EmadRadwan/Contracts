using Application.Shipments.OrganizationGlSettings;
using Microsoft.AspNetCore.Mvc;
using Application.Accounting.OrganizationGlSettings;

namespace API.Controllers.Accounting;

public class CustomTimePeriodsController : BaseApiController
{
    [HttpGet("listCustomTimePeriodsLov")]
    public async Task<IActionResult> ListCustomTimePeriodsLov()
    {
        return HandleResult(await Mediator.Send(new ListCustomTimePeriodsLov.Query()));
    }

    [HttpGet("listCustomTimePeriodTypesLov")]
    public async Task<IActionResult> ListCustomTimePeriodTypesLov()
    {
        return HandleResult(await Mediator.Send(new ListCustomTimePeriodTypesLov.Query()));
    }

    [HttpPost("{customTimePeriodId}/closeTimePeriod")]
    public async Task<IActionResult> CloseTimePeriod(string customTimePeriodId)
    {
        return HandleResult(await Mediator.Send(new CloseFinancialTimePeriod.Command {CustomTimePeriodId = customTimePeriodId}));
    }
}