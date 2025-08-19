using API.Controllers.OData;
using Application.Shipments.Transactions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace API.Controllers.Accounting.Transactions;

public class AccountingTransactionRecordsController : BaseODataController<AccountingTransactionRecord>
{
    [HttpGet]
    [EnableQuery]
    public async Task<IActionResult> Get(ODataQueryOptions<AccountingTransactionRecord> options)
    {
        var query = await Mediator.Send(new ListAccountingTransactions.Query { Options = options });
        return await HandleODataQueryAsync(query, options);
    }
}