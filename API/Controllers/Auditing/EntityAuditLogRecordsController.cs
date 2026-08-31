using API.Controllers.OData;
using Application.Auditing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace API.Controllers.Auditing;

// The audit trail spans every module, so unlike most endpoints here a leak is not scoped to
// one feature - it exposes what everybody did. Gated on the same Admin role the UI uses.
[Authorize(Roles = "Admin")]
public class EntityAuditLogRecordsController : BaseODataController<EntityAuditLogRecord>
{
    [HttpGet]
    [EnableQuery]
    public async Task<IActionResult> Get(ODataQueryOptions<EntityAuditLogRecord> options)
    {
        var query = await Mediator.Send(new ListEntityAuditLogs.Query { Options = options });
        return await HandleODataQueryAsync(query, options);
    }
}
