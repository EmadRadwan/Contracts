using Application.CRM.SalesOpportunities;
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
            SortDescending = sortDesc
        }));
    }

    /// <summary>
    /// Get all opportunity stages for pipeline/board view.
    /// </summary>
    [HttpGet("stages")]
    public async Task<IActionResult> GetStages()
    {
        return HandleResult(await Mediator.Send(new ListOpportunityStages.Query()));
    }

    [HttpGet("actions")]
    public async Task<IActionResult> GetActions()
    {
        var Language = GetLanguage();
        return HandleResult(await Mediator.Send(new ListSalesOpportunityActions.Query { Language = Language }));
    }

    [HttpGet("cancellation-reasons")]
    public async Task<IActionResult> GetCancellationReasons()
    {
        var Language = GetLanguage();
        return HandleResult(await Mediator.Send(new ListSalesOpportunityCancellationReasons.Query { Language = Language }));
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
            OpportunityStageId = request.StageId,
            OpportunityName = request.OpportunityName // Required field
        };

        return HandleResult(await Mediator.Send(new UpdateSalesOpportunity.Command
        {
            Opportunity = opportunity
        }));
    }
}

public class UpdateStageRequest
{
    public string StageId { get; set; } = null!;
    public string OpportunityName { get; set; } = null!;
}
