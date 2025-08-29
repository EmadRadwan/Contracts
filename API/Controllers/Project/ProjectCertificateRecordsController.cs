using API.Controllers.OData;
using Application.Projects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace API.Controllers.Project;

public class ProjectCertificateRecordsController : BaseODataController<ProjectCertificateRecord>
{
    [HttpGet]
    [EnableQuery]
    public async Task<IActionResult> Get(ODataQueryOptions<ProjectCertificateRecord> options)
    {
        var language = GetLanguage();
        var query = await Mediator.Send(new ListProjectCertificates.Query { Options = options, Language = language });
        return await HandleODataQueryAsync(query, options);
    }
}