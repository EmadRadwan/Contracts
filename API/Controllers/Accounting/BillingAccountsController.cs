using Application.Accounting.BillingAccounts;
using Application.Shipments.BillingAccounts;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Accounting;

public class BillingAccountsController : BaseApiController
{
    [HttpGet("{partyId}/getPartyBillingAccounts")]
    public async Task<IActionResult> GetPartyBillingAccounts(string partyId)
    {
        return HandleResult(await Mediator.Send(new ListPartyBillingAccounts.Query { PartyId = partyId }));
    }
    
    [HttpGet("{partyId}/makePartyBillingAccountList")]
    public async Task<IActionResult> MakePartyBillingAccountList(string partyId)
    {
        return HandleResult(await Mediator.Send(new MakePartyBillingAccountList.Query { PartyId = partyId }));
    }
    
    [HttpGet("{billingAccountId}/getBillingAccountBalance")]
    public async Task<IActionResult> GetBillingAccountBalance(string billingAccountId)
    {
        return HandleResult(await Mediator.Send(new GetBillingAccountBalance.Query { BillingAccountId = billingAccountId}));
    }
    
    [HttpGet("{billingAccountId}/listBillingAccountInvoices/{statusId?}")]
    public async Task<IActionResult> ListBillingAccountInvoices(string billingAccountId, string statusId = null)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(billingAccountId))
        {
            return BadRequest(new { Error = "BillingAccountId is required." });
        }

        // Create the request DTO
        var request = new ListBillingAccountInvoicesRequest
        {
            BillingAccountId = billingAccountId,
            StatusId = statusId
        };

        // Create the MediatR Query
        var query = new ListBillingAccountInvoices.Query(request);

        // Send the query to MediatR
        var result = await Mediator.Send(query);

        // Handle and return the result using the base controller method
        return HandleResult(result);
    }

    [HttpGet("{billingAccountId}/getBillingAccountPayments")]
    public async Task<IActionResult> GetBillingAccountPayments(string billingAccountId)
    {
        // Validate the required billingAccountId parameter
        if (string.IsNullOrWhiteSpace(billingAccountId))
        {
            return BadRequest(new { Error = "BillingAccountId is required." });
        }

        // Create the MediatR Query with the billingAccountId
        var query = new GetBillingAccountPayments.Query(billingAccountId);

        // Send the query to MediatR to invoke the corresponding handler
        var result = await Mediator.Send(query);

        // Use the base controller's HandleResult method to standardize the HTTP response
        return HandleResult(result);
    }
    
    [HttpGet("{billingAccountId}/getBillingAccountOrders")]
    public async Task<IActionResult> GetBillingAccountOrders(string billingAccountId)
    {
        // Validate the required billingAccountId parameter
        if (string.IsNullOrWhiteSpace(billingAccountId))
        {
            return BadRequest(new { Error = "BillingAccountId is required." });
        }

        // Create the MediatR Query with the billingAccountId
        var query = new GetBillingAccountOrders.Query(billingAccountId);

        // Send the query to MediatR to invoke the corresponding handler
        var result = await Mediator.Send(query);

        // Use the base controller's HandleResult method to standardize the HTTP response
        return HandleResult(result);
    }
    
    [HttpGet("getBillingAccountsLov", Name = "GetBillingAccountsLov")]
    public async Task<IActionResult> GetBillingAccountsLov([FromQuery] BillingAccountLovParams param)
    {
        return HandleResult(await Mediator.Send(new GetBillingAccountsLov.Query { Params = param }));
    }
    
    [HttpGet("getBillingAccountsForParty")]
    public async Task<ActionResult<List<BillingAccountDto>>> GetBillingAccounts([FromQuery] string partyId)
    {
        if (string.IsNullOrEmpty(partyId))
        {
            return BadRequest("PartyId is required");
        }

        var query = new GetBillingAccountsQuery { PartyId = partyId };
        var result = await Mediator.Send(query);

        return Ok(result);
    }
}