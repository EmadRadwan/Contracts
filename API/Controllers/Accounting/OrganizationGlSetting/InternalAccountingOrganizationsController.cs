using Application.Accounting.OrganizationGlSettings;
using Application.Accounting.Services.Models;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Accounting.OrganizationGlSetting;

public class InternalAccountingOrganizationsController : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> ListInternalAccountingOrganizationsLov()
    {
        return HandleResult(await Mediator.Send(new ListInternalAccountingOrganizationsLov.Query()));
    }
    
    [HttpGet("diagram/{acctgTransId}")]
    public async Task<IActionResult> GetGlAccountDiagram(string acctgTransId)
    {
        var query = new GetGlAccountDiagramQuery { AcctgTransId = acctgTransId };
        var result = await Mediator.Send(query);
        return Ok(result);
    }
}