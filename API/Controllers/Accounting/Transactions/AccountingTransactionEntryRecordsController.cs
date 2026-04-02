using API.Controllers.OData;
using Application.Accounting.Transactions;
using Application.Shipments.Transactions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace API.Controllers.Accounting.Transactions;

public class AccountingTransactionEntryRecordsController : BaseODataController<AccountingTransactionEntryRecord>
{
    [HttpGet]
    [EnableQuery]
    public async Task<IActionResult> Get(ODataQueryOptions<AccountingTransactionEntryRecord> options, [FromQuery] string companyId)
    {
        var language = GetLanguage();
        var query = await Mediator.Send(new ListAccountingTransactionEntries.Query 
        { 
            Options = options,
            Language = language,
            CompanyId = companyId // Pass companyId to the query
        });
        return await HandleODataQueryAsync(query, options);
    }

    [HttpGet("getAcctTransEntriesByDateRange")]
    public async Task<ActionResult<AccountingTransactionEntriesResponse>> GetByDateRange([FromQuery] string companyId, [FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
    {
        return await Mediator.Send(new ListAccountingTransactionEntriesByDateRange.Query
        {
            CompanyId = companyId,
            FromDate = fromDate,
            ToDate = toDate,
            Language = GetLanguage()
        });
    }
}