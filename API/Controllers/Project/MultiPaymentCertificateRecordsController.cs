using API.Controllers.OData;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Application.MultiPaymentCertificates;
using Application.Projects;

namespace API.Controllers.Project
{
    public class MultiPaymentCertificateRecordsController : BaseODataController<MultiPaymentCertificateRecord>
    {
        [HttpGet]
        [EnableQuery]
        public async Task<IActionResult> Get(ODataQueryOptions<MultiPaymentCertificateRecord> options)
        {
            var language = GetLanguage();
            var query = await Mediator.Send(new ListMultiPaymentCertificates.Query { Options = options, Language = language });
            return await HandleODataQueryAsync(query, options);
        }
    }
}