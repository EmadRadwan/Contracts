using Application.Accounting.OrganizationGlSettings;
using Microsoft.AspNetCore.Mvc;
using Application.Accounting.FinAccounts;

namespace API.Controllers.Accounting;

public class FinAccountController : BaseApiController
{
    [HttpPost("getTransListAndTotals")]
    public async Task<IActionResult> GetFinAccountTransListAndTotals([FromBody] FinAccountTransationParamsDto paramsDto)
    {
        return HandleResult(await Mediator.Send(new GetFinAccountTransListAndTotals.Command { Params = paramsDto }));
    }

    [HttpPost("createFinAccountTrans")]
    public async Task<IActionResult> CreateFinAccountTrans([FromBody] CreateFinAccountTransRequest finAccountTrans)
    {
        return HandleResult(await Mediator.Send(new CreateFinAccountTrans.Command { FinAccountTrans = finAccountTrans }));
    }

    [HttpGet("listFinAccountTransTypes")]

    public async Task<IActionResult> ListFinAccountTransTypes()
    {
        return HandleResult(await Mediator.Send(new ListFinAccountTransTypes.Query()));
    }

    [HttpGet("listFinAccountTransStatus")]

    public async Task<IActionResult> ListFinAccountTransStatus()
    {
        return HandleResult(await Mediator.Send(new ListFinAccountTransStatus.Query()));
    }

    [HttpGet("listFinAccountDepositsWidthrawls")]
    public async Task<IActionResult> ListFinAccountDepositsWidthrawals(
        [FromQuery] string? paymentMethodTypeId,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? thruDate,
        [FromQuery] string? partyIdFrom,
        [FromQuery] bool? checkFinAccountTransNull,
        [FromQuery] List<string> partyIdSetFromFinAccountRole)
    {
        return HandleResult(await Mediator.Send(new GetDepositWithdrawPayments.Query
        {
            PaymentMethodTypeId = paymentMethodTypeId,
            FromDate = fromDate,
            ThruDate = thruDate,
            PartyIdFrom = partyIdFrom,
            CheckFinAccountTransNull = checkFinAccountTransNull.GetValueOrDefault(false),
            PartyIdSetFromFinAccountRole = partyIdSetFromFinAccountRole
        }));
    }

    [HttpGet("getFinAccountsForParty")]
    public async Task<ActionResult<List<FinAccountDto>>> GetFinAccounts([FromQuery] string partyId)
    {
        if (string.IsNullOrEmpty(partyId))
        {
            return BadRequest("PartyId is required");
        }

        var query = new GetFinAccountsQuery { PartyId = partyId };
        var result = await Mediator.Send(query);

        return Ok(result);
    }
}