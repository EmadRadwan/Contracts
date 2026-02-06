// In HandleODataQueryAsync or controller
var total = queryWithFilterOnly.Count();           // sync ok since in-memory
var paged = options.ApplyTo(query) as IQueryable<T>;

return Ok(new
{
    value = paged,                  // paged items
    ["@odata.count"] = total       // ← manually add it!
});