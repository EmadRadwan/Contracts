using Application.CRM.Leads;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.CRM;

/// <summary>
/// API Controller for CRM Leads (People).
///
/// KEY CONCEPT:
/// A Lead is a PERSON - someone you know.
/// Leads are linked to Sales Opportunities via SalesOpportunityRole.
/// </summary>
public class LeadsController : BaseApiController
{
    /// <summary>
    /// Get all leads with optional filtering.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetLeads(
        [FromQuery] string? search,
        [FromQuery] string? dataSourceId,
        [FromQuery] string? sortBy,
        [FromQuery] bool sortDesc = false)
    {
        return HandleResult(await Mediator.Send(new ListLeads.Query
        {
            SearchTerm = search,
            DataSourceId = dataSourceId,
            SortBy = sortBy,
            SortDescending = sortDesc
        }));
    }

    /// <summary>
    /// Get leads for LOV/picker (lightweight).
    /// </summary>
    [HttpGet("lov")]
    public async Task<IActionResult> GetLeadsLov(
        [FromQuery] string? search,
        [FromQuery] int take = 20)
    {
        return HandleResult(await Mediator.Send(new ListLeadsLov.Query
        {
            SearchTerm = search,
            Take = take
        }));
    }

    /// <summary>
    /// Create a new lead.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateLead([FromBody] LeadDto lead)
    {
        return HandleResult(await Mediator.Send(new CreateLead.Command
        {
            Lead = lead
        }));
    }

    /// <summary>
    /// Update an existing lead.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateLead(string id, [FromBody] LeadDto lead)
    {
        lead.PartyId = id;
        return HandleResult(await Mediator.Send(new UpdateLead.Command
        {
            Lead = lead
        }));
    }
}
