using API.Controllers.OData;
using Application.Accounting.Transactions;
using Application.Shipments.Transactions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace API.Controllers.Accounting.Transactions;

public class AccountingTransactionRecordsController : BaseODataController<AccountingTransactionRecord>
{
    [HttpGet]
    [EnableQuery]
    public async Task<IActionResult> Get(ODataQueryOptions<AccountingTransactionRecord> options, [FromQuery] string companyId)
    {
        // Log the received companyId for debugging
        Console.WriteLine($"Received companyId in controller: {companyId}");
        var query = await Mediator.Send(new ListAccountingTransactions.Query 
        { 
            Options = options,
            CompanyId = companyId
        });
        return await HandleODataQueryAsync(query, options);
    }
}