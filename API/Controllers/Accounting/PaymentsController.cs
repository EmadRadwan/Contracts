using Application.Accounting.FinAccounts;
using Application.Accounting.Payments;
using Application.Order.Orders;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Accounting;

public class PaymentsController : BaseApiController
{
    [HttpGet("{orderId}/getPaymentsForOrder")]
    public async Task<IActionResult> GetPaymentsForOrder(string orderId)
    {
        return HandleResult(await Mediator.Send(new ListPaymentsForOrder.Query { OrderId = orderId }));
    }

    [HttpGet("getPaymentsMethods")]
    public async Task<IActionResult> GetPaymentsMethods()
    {
        return HandleResult(await Mediator.Send(new ListPaymentMethods.Query()));
    }

    [HttpGet("{partyId}/getPaymentsMethodsByPartyId", Name = "GetPaymentMethodsByPartyId")]
    public async Task<IActionResult> GetPaymentMethodsByPartyId(string partyId)
    {
        return HandleResult(await Mediator.Send(new ListPaymentMethodsByPartyId.Query { PartyId = partyId }));
    }

    [HttpGet("getPaymentApplicationsLov", Name = "GetPaymentApplicationsLov")]
    public async Task<IActionResult> GetPaymentApplicationsLov([FromQuery] PaymentsLovParam param)
    {
        return HandleResult(await Mediator.Send(new GetPaymentApplicationsLov.Query { Params = param }));
    }

    [HttpGet("getPaymentsTypesIncoming")]
    public async Task<IActionResult> GetPaymentsTypesIncoming()
    {
        return HandleResult(await Mediator.Send(new ListIncomingPaymentTypes.Query()));
    }

    [HttpGet("getPaymentsTypesOutgoing")]
    public async Task<IActionResult> GetPaymentsTypesOutgoing()
    {
        return HandleResult(await Mediator.Send(new ListOutgoingPaymentTypes.Query()));
    }

    [HttpGet("getPaymentsIncoming")]
    public async Task<IActionResult> GetPaymentsIncoming()
    {
        return HandleResult(await Mediator.Send(new ListIncomingPayments.Query()));
    }

    [HttpGet("getPaymentsOutgoing")]
    public async Task<IActionResult> GetPaymentsOutgoing()
    {
        return HandleResult(await Mediator.Send(new ListOutgoingPayments.Query()));
    }

    [HttpGet("getPaymentsTypes")]
    public async Task<IActionResult> GetPaymentsTypes()
    {
        return HandleResult(await Mediator.Send(new ListAllPaymentTypes.Query {Language = GetLanguage()}));
    }

    [HttpPut("updateOrApproveSalesOrderPayments", Name = "UpdateOrApproveSalesOrderPayments")]
    public async Task<IActionResult> UpdateOrApproveSalesOrderPayments(PaymentsDto paymentsDto)
    {
        return HandleResult(await Mediator.Send(new UpdateOrApproveSalesOrderPayments.Command
            { PaymentsDto = paymentsDto }));
    }


    [HttpPut("completeSalesOrderPayments", Name = "CompleteSalesOrderPayments")]
    public async Task<IActionResult> CompleteSalesOrderPayments(PaymentsDto paymentDto)
    {
        return HandleResult(await Mediator.Send(new CompleteSalesOrderPayments.Command
            { PaymentsDto = paymentDto }));
    }

    [HttpPost("createPaymentAndFinAccountTrans")]
    public async Task<IActionResult> CreatePaymentAndFinAccountTrans(CreatePaymentAndFinAccountTransRequest request)
    {
        return HandleResults(await Mediator.Send(new CreatePaymentAndFinAccountTrans.Command { request = request }));
    }
    
    [HttpPost("duplicate/{originalPaymentId}")]
    public async Task<IActionResult> DuplicatePayment(string originalPaymentId)
    {
        return HandleResults(await Mediator.Send(new DuplicatePayment.Command 
        { 
            OriginalPaymentId = originalPaymentId 
        }));
    }

    [HttpPut("updatePayment", Name = "UpdatePayment")]
    public async Task<IActionResult> UpdatePayment(PaymentDto paymentDto)
    {
        return HandleResults(await Mediator.Send(new UpdatePayment.Command
            { PaymentDto = paymentDto }));
    }

    [HttpPost("receiveOfflinePayment", Name = "ReceiveOfflinePayment")]
    public async Task<IActionResult> ReceiveOfflinePayment(ReceiveOfflinePaymentInput receiveOfflinePaymentInput)
    {
        return HandleResult(await Mediator.Send(new ReceiveOfflinePayment.Command
            { ReceiveOfflinePaymentInput = receiveOfflinePaymentInput }));
    }

    [HttpPut("setPaymentStatusToReceived", Name = "SetPaymentStatusToReceived")]
    public async Task<IActionResult> SetPaymentStatusToReceived(PaymentChangeStatusDto paymentChangeStatusDto)
    {
        return HandleResults(await Mediator.Send(new SetPaymentStatusToReceived.Command
            { PaymentChangeStatusDto = paymentChangeStatusDto }));
    }

    [HttpGet("{invoiceId}/getPaymentsApplicationsForInvoice")]
    public async Task<IActionResult> GetPaymentsApplicationsForInvoice(string invoiceId)
    {
        return HandleResult(await Mediator.Send(new ListPaymentsApplicationsForInvoice.Query
            { InvoiceId = invoiceId }));
    }

    [HttpGet("{paymentId}/getPaymentsApplicationsForPayment")]
    public async Task<IActionResult> GetPaymentsApplicationsForPayment(string paymentId)
    {
        return HandleResult(await Mediator.Send(new ListPaymentsApplicationsForPayment.Query
            { PaymentId = paymentId }));
    }

    [HttpPost("CalculatePaymentTotals")]
    public async Task<ActionResult<IEnumerable<PaymentTotalsDto>>> CalculateTotals([FromBody] List<string> paymentIds)
    {
        var result = await Mediator.Send(new CalculatePaymentTotals.Query { PaymentIds = paymentIds });
        return Ok(result);
    }

    [HttpPost("createPaymentAndApplicationForBillingAccount")]
    public async Task<ActionResult> CreatePaymentAndApplicationForBillingAccount(
        [FromBody] CreatePaymentAndApplicationCommand command)
    {
        var result = await Mediator.Send(command);
        if (!result.IsSuccess)
        {
            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }
    
    [HttpPost("createPaymentApplication")]
    public async Task<IActionResult> CreatePaymentApplication([FromBody] PaymentApplicationParam param)
    {
        var result = await Mediator.Send(new CreatePaymentApplication.Command { Param = param });
        if (!result.IsSuccess)
        {
            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }
    
    [HttpGet("daily")]
    public async Task<ActionResult<PaymentsDailyResponse>> GetDailyPayments(
        [FromQuery] string paymentType,
        CancellationToken ct = default)
    {
        var language = GetLanguage();
        var query = new ListPaymentsDaily.Query
        {
            PaymentType = paymentType,
            Language = language
        };

        var result = await Mediator.Send(query, ct);
        return Ok(result);
    }
    
    // Controller addition
    [HttpGet("by-date-range")]
    public async Task<ActionResult<PaymentsDailyResponse>> GetPaymentsByDateRange(
        [FromQuery] string paymentType,
        [FromQuery] string fromDate,
        [FromQuery] string toDate,
        CancellationToken ct = default)
    {
        var language = GetLanguage();
        var query = new ListPaymentsByDateRange.Query
        {
            PaymentType = paymentType,
            FromDate = DateTime.ParseExact(fromDate, "yyyy-MM-dd", null),
            ToDate = DateTime.ParseExact(toDate, "yyyy-MM-dd", null).AddDays(1).AddTicks(-1), // end of day
            Language = language
        };

        var result = await Mediator.Send(query, ct);
        return Ok(result);
    }
    
    [HttpDelete("{paymentId}")]
    public async Task<ActionResult> DeletePayment(string paymentId, CancellationToken ct = default)
    {
        var command = new DeletePayment.Command
        {
            PaymentId = paymentId
        };

        var result = await Mediator.Send(command, ct);

        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.Error });
        }

        // 204 No Content is standard for successful DELETE
        return NoContent();
    }
}