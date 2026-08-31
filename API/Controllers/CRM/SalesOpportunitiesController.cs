using Application.CRM.SalesOpportunities;
using FluentValidation.Resources;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.CRM;

/// <summary>
/// API Controller for Sales Opportunities (Leads/Deals).
///
/// KEY CONCEPT:
/// A Sales Opportunity (Lead) is NOT a person - it's a business opportunity.
/// Leads (People) are linked to opportunities via the Leads array.
/// </summary>
public class SalesOpportunitiesController : BaseApiController
{
    /// <summary>
    /// Get all sales opportunities with optional filtering.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetOpportunities(
        [FromQuery] string? stageId,
        [FromQuery] string? ownerPartyId,
        [FromQuery] DateTime? closeDateFrom,
        [FromQuery] DateTime? closeDateTo,
        [FromQuery] string? search,
        [FromQuery] string? sortBy,
        [FromQuery] bool sortDesc = true)
    {
        return HandleResult(await Mediator.Send(new ListSalesOpportunities.Query
        {
            OpportunityStageId = stageId,
            OwnerPartyId = ownerPartyId,
            EstimatedCloseDateFrom = closeDateFrom,
            EstimatedCloseDateTo = closeDateTo,
            SearchTerm = search,
            SortBy = sortBy,
            SortDescending = sortDesc,
            Language = GetLanguage()
        }));
    }

    /// <summary>
    /// Open opportunities already linked to the given leads. Advisory only - a lead
    /// may belong to several opportunities; this lets the UI say so before a second
    /// one is created by mistake.
    /// </summary>
    [HttpGet("open-by-leads")]
    public async Task<IActionResult> GetOpenOpportunitiesByLeads(
        [FromQuery] List<string> leadPartyIds,
        [FromQuery] string? excludeOpportunityId)
    {
        return HandleResult(await Mediator.Send(new ListOpenOpportunitiesByLead.Query
        {
            LeadPartyIds = leadPartyIds ?? new List<string>(),
            ExcludeOpportunityId = excludeOpportunityId,
            Language = GetLanguage()
        }));
    }

    /// <summary>
    /// Get all opportunity stages for pipeline/board view.
    /// </summary>
    [HttpGet("stages")]
    public async Task<IActionResult> GetStages()
    {
        return HandleResult(await Mediator.Send(new ListOpportunityStages.Query { Language = GetLanguage() }));
    }

    [HttpGet("actions")]
    public async Task<IActionResult> GetActions()
    {
        var Language = GetLanguage();
        return HandleResult(await Mediator.Send(new ListSalesOpportunityActionTypes.Query { Language = Language }));
    }

    [HttpGet("cancellation-reasons")]
    public async Task<IActionResult> GetCancellationReasons()
    {
        var Language = GetLanguage();
        return HandleResult(await Mediator.Send(new ListSalesOpportunityCancellationReasons.Query { Language = Language }));
    }

    [HttpGet("meeting-types")]
    public async Task<IActionResult> GetMeetingTypes()
    {
        var Language = GetLanguage();
        return HandleResult(await Mediator.Send(new ListSalesOpportunityMeetingTypes.Query { Language = Language }));
    }

    [HttpGet("meeting-locations")]
    public async Task<IActionResult> GetMeetingLocations()
    {
        var Language = GetLanguage();
        return HandleResult(await Mediator.Send(new ListSalesOpportunityMeetingLocations.Query { Language = Language }));
    }

    /// <summary>
    /// Create a new sales opportunity.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateOpportunity([FromBody] SalesOpportunityDto opportunity)
    {
        return HandleResult(await Mediator.Send(new CreateSalesOpportunity.Command
        {
            Opportunity = opportunity
        }));
    }

    /// <summary>
    /// Create a new action for a sales opportunity.
    /// </summary>
    [HttpPost("{id}/actions")]
    public async Task<IActionResult> AddAction(string id, [FromBody] SalesOpportunityActionDto action)
    {

        return HandleResult(await Mediator.Send(new CreateSalesOpportunityAction.Command
        {
            Action = action
        }));
    }

    /// <summary>
    /// Create a new action for a sales opportunity.
    /// </summary>
    [HttpGet("{id}/actions")]
    public async Task<IActionResult> ListSalesOpportinutyAction(string id)
    {

        return HandleResult(await Mediator.Send(new ListSalesOpportunityActions.Query
        {
            SalesOpportunityId = id,
            Language = GetLanguage()
        }));
    }

    /// <summary>
    /// Update an existing sales opportunity.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateOpportunity(string id, [FromBody] SalesOpportunityDto opportunity)
    {
        opportunity.SalesOpportunityId = id;
        return HandleResult(await Mediator.Send(new UpdateSalesOpportunity.Command
        {
            Opportunity = opportunity
        }));
    }

    /// <summary>
    /// Update only the stage of an opportunity (for drag-and-drop on board).
    /// </summary>
    [HttpPatch("{id}/stage")]
    public async Task<IActionResult> UpdateOpportunityStage(string id, [FromBody] UpdateStageRequest request)
    {
        var opportunity = new SalesOpportunityDto
        {
            SalesOpportunityId = id,
            OpportunityStageId = request.StageId
        };

        return HandleResult(await Mediator.Send(new UpdateSalesOpportunity.Command
        {
            Opportunity = opportunity
        }));
    }

    /// <summary>
    /// Get all history entries for a sales opportunity.
    /// </summary>
    [HttpGet("{id}/history")]
    public async Task<IActionResult> ListSalesOpportunityHistory(string id)
    {
        return HandleResult(await Mediator.Send(new ListSalesOpportunityHistory.Query
        {
            SalesOpportunityId = id,
            Language = GetLanguage()
        }));
    }
}

public class UpdateStageRequest
{
    public string StageId { get; set; } = null!;
}
