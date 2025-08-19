using API.Controllers.OData;
using Application.Parties.Parties;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace API.Controllers.Party;

public class PartyRecordsController : BaseODataController<PartyRecord>
{
    [HttpGet]
    [EnableQuery]
    public async Task<IActionResult> Get(ODataQueryOptions<PartyRecord> options)
    {
        var query = await Mediator.Send(new PartiesList.Query { Options = options });
        return await HandleODataQueryAsync(query, options);
    }
}