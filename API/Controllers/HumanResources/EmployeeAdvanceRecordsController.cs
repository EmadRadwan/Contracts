using API.Controllers.OData;
using Application.HumanResources;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace API.Controllers.HumanResources;

public class EmployeeAdvanceRecordsController : BaseODataController<EmployeeAdvanceRecord>
{
    [HttpGet]
    [EnableQuery]
    public async Task<IActionResult> Get(ODataQueryOptions<EmployeeAdvanceRecord> options)
    {
        var language = GetLanguage();

        var query = await Mediator.Send(new ListEmployeeAdvancesQuery.Query
        {
            Options = options,
            Language = language
        });

        return await HandleODataQueryAsync(query, options);
    }
}