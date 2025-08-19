using API.Controllers.OData;
using Application.Catalog.Products;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace API.Controllers.Catalog;

public class ProductRecordsController : BaseODataController<ProductRecord>
{
    [HttpGet]
    [EnableQuery]
    public async Task<IActionResult> Get(ODataQueryOptions<ProductRecord> options)
    {
        var language = GetLanguage();
        var query = await Mediator.Send(new ListProductsQuery.Query { Options = options, Language = language });
        return await HandleODataQueryAsync(query, options);
    }
}