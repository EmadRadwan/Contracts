using API.Controllers.OData;
using Application.Catalog.ProductStores;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace API.Controllers.Catalog;

public class ProductStoreRecordsController : BaseODataController<ProductStoreRecord>
{
    [HttpGet]
    [EnableQuery]
    public async Task<IActionResult> Get(ODataQueryOptions<ProductStoreRecord> options)
    {
        var query = await Mediator.Send(new ListProductStores.Query { Options = options });
        return await HandleODataQueryAsync(query, options);
    }
}