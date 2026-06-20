using Application.Projects;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Project;

[Route("api/[controller]")]
public class SalesCommissionController : BaseApiController
{
    [HttpGet("defaults/{salesRequestId}")]
    public async Task<IActionResult> GetDefaults(string salesRequestId)
    {
        return HandleResult(await Mediator.Send(
            new GetSalesCommissionDefaults.Query { SalesRequestId = salesRequestId }));
    }

    [HttpGet("bySalesRequest/{salesRequestId}")]
    public async Task<IActionResult> GetBySalesRequest(string salesRequestId)
    {
        return HandleResult(await Mediator.Send(
            new GetSalesCommissionBySalesRequest.Query { SalesRequestId = salesRequestId }));
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] SalesCommissionDto dto)
    {
        return HandleResult(await Mediator.Send(new CreateSalesCommission.Command { Dto = dto }));
    }

    [HttpPost("approve/{id}")]
    public async Task<IActionResult> Approve(string id)
    {
        return HandleResult(await Mediator.Send(new ApproveSalesCommission.Command { SalesCommissionId = id }));
    }
}
