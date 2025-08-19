using Application.Accounting.PaymentGroup;
using Application.Accounting.Payments;
using Application.Accounting.Services.Models;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Accounting;

public class PaymentGroupsController : BaseApiController
{
    [HttpPost("createPaymentGroup")]
    public async Task<IActionResult> CreatePaymentGroup([FromBody] CreatePaymentGroupDto paymentGroupDto)
    {
        var language = GetLanguage();
        var result = await Mediator.Send(new CreatePaymentGroup.Command { Params = paymentGroupDto });
        return Ok(result);
    }

    [HttpPost("createPaymentGroupPayment")]
    public async Task<IActionResult> CreatePaymentGroupPayment([FromBody] CreatePaymentGroupMemberDto paymentGroupDto)
    {
        var language = GetLanguage();
        var result = await Mediator.Send(new CreatePaymentGroupPayment.Command { Params = paymentGroupDto });
        return Ok(result);
    }

    [HttpPost("{paymentGroupId}/cancelCheckRun")]
    public async Task<IActionResult> CancelCheckRun(string paymentGroupId)
    {
        var result = await Mediator.Send(new CancelCheckRun.Command { PaymentGroupId = paymentGroupId });
        return Ok(result);
    }

    [HttpPost("expirePaymentGroupMember")]
    public async Task<IActionResult> ExpirePaymentGroupMember([FromBody] ExpirePaymentGroupMemberInput input)
    {
        var result = await Mediator.Send(new ExpirePaymentGroupMember.Command { Params = input });
        if (!result.IsSuccess)
            return BadRequest(result.Message);

        return Ok(result);
    }
    

    [HttpGet("{paymentGroupId}/getPaymentGroupMembers")]
    public async Task<IActionResult> GetPaymentGroupMembers(string paymentGroupId)
    {
        return HandleResult(await Mediator.Send(new GetPaymentGroupMembers.Query { PaymentGroupId = paymentGroupId }));
    }

    [HttpPut("updatePaymentGroupMember")]
    public async Task<IActionResult> UpdatePaymentGroupMember([FromBody] UpdatePaymentGroupMemberInput input)
    {
        var result = await Mediator.Send(new UpdatePaymentGroupMember.Command { Params = input });
        if (!result.IsSuccess)
            return BadRequest(result.Message);

        return Ok(result);
    }
}