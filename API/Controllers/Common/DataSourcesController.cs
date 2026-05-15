using Application.Common.DataSources;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class DataSourcesController : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> List()
    {
        return HandleResult(await Mediator.Send(new ListDataSources.Query{Language = GetLanguage()}));
    }
}
