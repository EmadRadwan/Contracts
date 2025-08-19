using API.Controllers.OData;
using Application.Shipments.Transactions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace API.Controllers.Accounting.Transactions;

public class AccountingTransactionEntryRecordsController : BaseODataController<AccountingTransactionEntryRecord>
{
    [HttpGet]
    [EnableQuery]
    public async Task<IActionResult> Get(ODataQueryOptions<AccountingTransactionEntryRecord> options)
    {
        var query = await Mediator.Send(new ListAccountingTransactionEntries.Query { Options = options });
        return await HandleODataQueryAsync(query, options);
    }
}