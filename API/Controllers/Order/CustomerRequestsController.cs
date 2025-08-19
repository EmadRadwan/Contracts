using Application.Order.CustomerRequests;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Request;

public class CustomerRequestsController : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult> GetCustomerRequests([FromQuery] CustomerRequestParams param)
    {
        return HandlePagedResult(await Mediator.Send(new ListCustomerRequests.Query { Params = param }));
    }


    [HttpGet("{customerRequestId}/getCustomerRequest")]
    public async Task<IActionResult> GetCustomerRequest(string customerRequestId)
    {
        return HandleResult(
            await Mediator.Send(new ListCustomerRequest.Query { CustomerRequestId = customerRequestId }));
    }

    [HttpPost("createCustomerRequest", Name = "CreateCustomerRequest")]
    public async Task<IActionResult> CreateCustomerRequest(CustRequestDto custRequestDto)
    {
        return HandleResult(await Mediator.Send(new CreateCustomerRequest.Command { CustRequestDto = custRequestDto }));
    }

    [HttpPut("updateCustomerRequest", Name = "UpdateCustomerRequest")]
    public async Task<IActionResult> UpdateCustomerRequest(CustRequestDto custRequestDto)
    {
        return HandleResult(await Mediator.Send(new UpdateCustomerRequest.Command { CustRequestDto = custRequestDto }));
    }
}