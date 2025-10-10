using API.Controllers.OData;
using Application.Accounting.Transactions;
using Application.Shipments.Transactions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace API.Controllers.Accounting.Transactions;

public class AccountingTransactionEntryRecordsController : BaseODataController<AccountingTransactionEntryRecord>
{
    // REFACTOR: Updated Get method to include Language parameter in the Query.
    // Purpose: Passes the language from the request to the handler to support Arabic field selection.
    // Context: Aligns with the provided example where GetLanguage() is used to set the Language property.
    [HttpGet]
    [EnableQuery]
    public async Task<IActionResult> Get(ODataQueryOptions<AccountingTransactionEntryRecord> options)
    {
        var language = GetLanguage();
        var query = await Mediator.Send(new ListAccountingTransactionEntries.Query 
        { 
            Options = options, 
            Language = language 
        });
        return await HandleODataQueryAsync(query, options);
    }
}