using API.Controllers.OData;
using Application.Shipments.Agreement;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace API.Controllers.Accounting;

public class AgreementRecordsController : BaseODataController<AgreementRecord>
{
    [HttpGet]
    [EnableQuery]
    public async Task<IActionResult> Get(ODataQueryOptions<AgreementRecord> options)
    {
        var language = GetLanguage(); // Access the language here if needed

        var query = await Mediator.Send(new ListAgreements.Query 
        { 
            Options = options,
            Language = language // Pass the language to the query
        });
        
        return await HandleODataQueryAsync(query, options);
    }
}