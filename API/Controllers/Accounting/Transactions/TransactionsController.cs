using Application.Accounting.Services.Models;
using Application.Accounting.Transactions;


using Application.Shipments.Transactions;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Accounting.Transactions;

public class TransactionsController : BaseApiController
{
    [HttpGet("{invoiceId}/{acctgTransTypeId}/getInvoiceTransactions")]
    public async Task<IActionResult> GetInvoiceTransactions(string invoiceId, string acctgTransTypeId)
    {
        return HandleResult(await Mediator.Send(new GetInvoiceTransactionEntries.Query 
        { 
            InvoiceId = invoiceId, 
            AcctgTransTypeId = acctgTransTypeId 
        }));
    }


    [HttpGet("{paymentId}/getPaymentTransactions")]
    public async Task<IActionResult> GetPaymentTransactions(string paymentId)
    {
        return HandleResult(await Mediator.Send(new GetPaymentTransactionEntries.Query { PaymentId = paymentId }));
    }

    [HttpGet("{acctgTransId}/getGeneralTransactions")]
    public async Task<IActionResult> GetGeneralTransactions(string acctgTransId)
    {
        return HandleResult(await Mediator.Send(new GetGeneralTransactionEntries.Query
            { AcctgTransId = acctgTransId }));
    }

    [HttpPost("quickCreateAcctgTrans", Name = "QuickCreateAcctgTrans")]
    public async Task<IActionResult> QuickCreateAcctgTrans(CreateQuickAcctgTransAndEntriesParams CreateQuickAcctgTransAndEntriesParams)
    {
        return HandleResult(await Mediator.Send(new QuickCreateAcctgTrans.Command
            { CreateQuickAcctgTransAndEntriesParams = CreateQuickAcctgTransAndEntriesParams }));
    }
    
    [HttpPost("createAcctgTrans", Name = "CreateAcctgTrans")]
    public async Task<IActionResult> CreateAcctgTrans(CreateAcctgTransParams createAcctgTransParams)
    {
        var command = new CreateAcctgTrans.Command 
        { 
            CreateAcctgTransParams = createAcctgTransParams 
        };

        var result = await Mediator.Send(command);

        return HandleResult(result);
    }
    
    [HttpPost("createAcctgTransEntry", Name = "CreateAcctgTransEntry")]
    public async Task<IActionResult> CreateAcctgTransEntry(AcctgTransEntry acctgTransEntry)
    {
        var command = new CreateAcctgTransEntry.Command 
        { 
            AcctgTransEntry = acctgTransEntry 
        };

        var result = await Mediator.Send(command);

        return HandleResult(result);
    }
    
    [HttpPut("updateAcctgTrans", Name = "UpdateAcctgTrans")]
    public async Task<IActionResult> UpdateAcctgTrans(AcctgTran acctgTransParams)
    {
        var command = new UpdateAcctgTrans.Command
        {
            AcctgTransParams = acctgTransParams
        };

        var result = await Mediator.Send(command);

        return HandleResult(result);
    }
    
    /// <summary>
    /// Completes the AcctgTransEntries for the given AcctgTrans record.
    /// </summary>
    /// <param name="acctgTransId">The AcctgTrans identifier.</param>
    /// <returns>The AcctgTransId if successful; otherwise an error response.</returns>
    [HttpPost("{acctgTransId}/complete")]
    public async Task<IActionResult> CompleteAcctgTransEntries(string acctgTransId)
    {
        var command = new CompleteAcctgTransEntries.Command
        {
            AcctgTransId = acctgTransId
        };

        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
        {
            // You can customize error handling based on result.Error.
            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }
    
    [HttpPut("updateAcctgTransEntry", Name = "UpdateAcctgTransEntry")]
    public async Task<IActionResult> UpdateAcctgTransEntry(AcctgTransEntry acctgTransEntry)
    {
        var command = new UpdateAcctgTransEntry.Command 
        { 
            AcctgTransEntry = acctgTransEntry 
        };

        var result = await Mediator.Send(command);

        return HandleResult(result);
    }
    
    [HttpDelete("deleteAcctgTransEntry", Name = "DeleteAcctgTransEntry")]
    public async Task<IActionResult> DeleteAcctgTransEntry([FromQuery] string acctgTransId, [FromQuery] string acctgTransEntrySeqId)
    {
        var command = new DeleteAcctgTransEntry.Command
        {
            AcctgTransId = acctgTransId,
            AcctgTransEntrySeqId = acctgTransEntrySeqId
        };

        var result = await Mediator.Send(command);

        return HandleResult(result);
    }
    
    [HttpPost("postAcctgTrans", Name = "PostAcctgTrans")]
    public async Task<IActionResult> PostAcctgTrans([FromBody] PostAcctgTrans.Command command)
    {
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }
}