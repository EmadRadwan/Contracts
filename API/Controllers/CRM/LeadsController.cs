using Application.CRM.Leads;
using Application.CRM.Leads.Assignment;
using Microsoft.AspNetCore.Authorization;
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
    [Authorize(Roles = LeadAssignmentConstants.CreateSecurityRole)]
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
    [Authorize(Roles = LeadAssignmentConstants.EditSecurityRole)]
    public async Task<IActionResult> UpdateLead(string id, [FromBody] LeadDto lead)
    {
        lead.PartyId = id;
        return HandleResult(await Mediator.Send(new UpdateLead.Command
        {
            Lead = lead
        }));
    }

    /// <summary>
    /// Assign a lead to a sales rep, or reassign it to a different one.
    /// </summary>
    [HttpPost("{id}/assign")]
    [Authorize(Roles = LeadAssignmentConstants.AssignSecurityRole)]
    public async Task<IActionResult> AssignLead(string id, [FromBody] AssignLeadRequest request)
    {
        return HandleResult(await Mediator.Send(new AssignLead.Command
        {
            LeadPartyId = id,
            OwnerPartyId = request.OwnerPartyId,
            Comments = request.Comments
        }));
    }

    /// <summary>
    /// Assign many leads to one sales rep in a single call.
    /// </summary>
    [HttpPost("bulk-assign")]
    [Authorize(Roles = LeadAssignmentConstants.AssignSecurityRole)]
    public async Task<IActionResult> BulkAssignLeads([FromBody] BulkAssignLeadsRequest request)
    {
        return HandleResult(await Mediator.Send(new BulkAssignLeads.Command
        {
            LeadPartyIds = request.LeadPartyIds,
            OwnerPartyId = request.OwnerPartyId,
            Comments = request.Comments
        }));
    }

    /// <summary>
    /// Full ownership history for a lead, newest first.
    /// </summary>
    [HttpGet("{id}/assignment-history")]
    [Authorize(Roles = LeadAssignmentConstants.AssignSecurityRole)]
    public async Task<IActionResult> GetLeadAssignmentHistory(string id)
    {
        return HandleResult(await Mediator.Send(new ListLeadAssignmentHistory.Query
        {
            LeadPartyId = id
        }));
    }

    /// <summary>
    /// Remove the current owner from a lead, returning it to the unassigned pool.
    /// </summary>
    [HttpDelete("{id}/assign")]
    [Authorize(Roles = LeadAssignmentConstants.AssignSecurityRole)]
    public async Task<IActionResult> UnassignLead(string id)
    {
        return HandleResult(await Mediator.Send(new UnassignLead.Command
        {
            LeadPartyId = id
        }));
    }
}

public class AssignLeadRequest
{
    public string OwnerPartyId { get; set; } = null!;
    public string? Comments { get; set; }
}

public class BulkAssignLeadsRequest
{
    public List<string> LeadPartyIds { get; set; } = new();
    public string OwnerPartyId { get; set; } = null!;
    public string? Comments { get; set; }
}
