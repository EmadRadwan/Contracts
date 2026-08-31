using Application.Core;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.CRM.SalesOpportunities;

/// <summary>
/// The open opportunities already attached to a set of leads.
///
/// A lead legitimately belongs to several opportunities - one buyer often pursues
/// more than one unit - so this does not block anything. It exists so the person
/// linking a lead can SEE the existing deals and decide whether they are creating
/// a genuine second one or duplicating by accident.
/// </summary>
public class ListOpenOpportunitiesByLead
{
    public class Query : IRequest<Result<List<LeadOpenOpportunityDto>>>
    {
        public List<string> LeadPartyIds { get; set; } = new();

        /// <summary>
        /// The opportunity being edited, so it does not warn about itself.
        /// </summary>
        public string? ExcludeOpportunityId { get; set; }

        public string? Language { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<List<LeadOpenOpportunityDto>>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<List<LeadOpenOpportunityDto>>> Handle(Query request, CancellationToken ct)
        {
            var leadIds = request.LeadPartyIds
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct()
                .ToList();

            if (leadIds.Count == 0)
                return Result<List<LeadOpenOpportunityDto>>.Success(new List<LeadOpenOpportunityDto>());

            var isArabic = request.Language == "ar";

            // Open is derived from the stage, not IsClosed: flag maintenance only
            // arrived in Aug 2026, so anything closed before then still reads as
            // open through the flag and would be reported here wrongly.
            var closedStages = new[] { "SOSTG_CLOSED_WON", "SOSTG_CLOSED_LOST" };

            var results = await _context.SalesOpportunityRoles
                .Where(r => r.RoleTypeId == "LEAD" && leadIds.Contains(r.PartyId))
                .Join(_context.SalesOpportunities,
                    r => r.SalesOpportunityId,
                    o => o.SalesOpportunityId,
                    (r, o) => new { r.PartyId, Opportunity = o })
                .Where(x => !closedStages.Contains(x.Opportunity.OpportunityStageId!)
                         && (request.ExcludeOpportunityId == null
                             || x.Opportunity.SalesOpportunityId != request.ExcludeOpportunityId))
                .Select(x => new LeadOpenOpportunityDto
                {
                    LeadPartyId = x.PartyId,
                    LeadName = _context.Parties
                        .Where(p => p.PartyId == x.PartyId)
                        .Select(p => p.Description)
                        .FirstOrDefault(),
                    SalesOpportunityId = x.Opportunity.SalesOpportunityId,
                    OpportunityName = x.Opportunity.OpportunityName,
                    OpportunityStageId = x.Opportunity.OpportunityStageId,
                    StageDescription = _context.SalesOpportunityStages
                        .Where(s => s.OpportunityStageId == x.Opportunity.OpportunityStageId)
                        .Select(s => isArabic ? (s.DescriptionArabic ?? s.Description) : s.Description)
                        .FirstOrDefault(),
                    ProductId = x.Opportunity.ProductId,
                    EstimatedAmount = x.Opportunity.EstimatedAmount,
                    EstimatedCloseDate = x.Opportunity.EstimatedCloseDate
                })
                .ToListAsync(ct);

            return Result<List<LeadOpenOpportunityDto>>.Success(results);
        }
    }
}

public class LeadOpenOpportunityDto
{
    public string? LeadPartyId { get; set; }
    public string? LeadName { get; set; }
    public string? SalesOpportunityId { get; set; }
    public string? OpportunityName { get; set; }
    public string? OpportunityStageId { get; set; }
    public string? StageDescription { get; set; }
    public string? ProductId { get; set; }
    public decimal? EstimatedAmount { get; set; }
    public DateTime? EstimatedCloseDate { get; set; }
}
