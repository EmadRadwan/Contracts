using Application.Shipments.Transactions;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Accounting.Transactions;

public class AccountingTransactionTypesController : BaseApiController
{
    [HttpGet("getAcctgTransTypesList")]
    public async Task<IActionResult> GetAcctgTransTypesList()
    {
        return HandleResult(await Mediator.Send(new ListAccountingTransactionTypes.Query()));
    }
}