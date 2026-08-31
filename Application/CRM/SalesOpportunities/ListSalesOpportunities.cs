using Application.Core;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.CRM.SalesOpportunities;

/// <summary>
/// Lists Sales Opportunities with filtering by pipeline stage, owner, date range, etc.
/// Supports both list view and board (Kanban) view.
/// </summary>
public class ListSalesOpportunities
{
    public record Query : IRequest<Result<List<SalesOpportunityDto>>>
    {
        public string? OpportunityStageId { get; init; }
        public string? OwnerPartyId { get; init; }
        public DateTime? EstimatedCloseDateFrom { get; init; }
        public DateTime? EstimatedCloseDateTo { get; init; }
        public string? SearchTerm { get; init; }
        public string? SortBy { get; init; } = "createdStamp";
        public bool SortDescending { get; init; } = true;

        public string Language {get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<List<SalesOpportunityDto>>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<List<SalesOpportunityDto>>> Handle(Query request, CancellationToken ct)
        {
            var query = _context.SalesOpportunities
                .Include(o => o.OpportunityStage)
                .Include(o => o.SalesOpportunityRoles)
                    .ThenInclude(r => r.Party)
                        .ThenInclude(p => p.Person)
                .Include(o => o.SalesOpportunityRoles)
                    .ThenInclude(r => r.Party)
                        .ThenInclude(p => p.PartyGroup)
                .Include(o => o.SalesOpportunityRoles)
                    .ThenInclude(r => r.RoleType)
                // NEW: Include Project (WorkEffort)
                .Include(o => o.WorkEffort)
                // NEW: Include Product (Unit/Apartment)
                .Include(o => o.Product)
                .AsQueryable();

            // Filter by stage
            if (!string.IsNullOrEmpty(request.OpportunityStageId))
            {
                query = query.Where(o => o.OpportunityStageId == request.OpportunityStageId);
            }

            // Filter by owner
            if (!string.IsNullOrEmpty(request.OwnerPartyId))
            {
                query = query.Where(o => o.SalesOpportunityRoles
                    .Any(r => r.PartyId == request.OwnerPartyId && r.RoleTypeId == "OWNER"));
            }

            // Filter by estimated close date range
            if (request.EstimatedCloseDateFrom.HasValue)
            {
                query = query.Where(o => o.EstimatedCloseDate >= request.EstimatedCloseDateFrom.Value);
            }
            if (request.EstimatedCloseDateTo.HasValue)
            {
                query = query.Where(o => o.EstimatedCloseDate <= request.EstimatedCloseDateTo.Value);
            }

            // Search by name or description
            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                var term = request.SearchTerm.ToLower();
                query = query.Where(o =>
                    (o.OpportunityName != null && o.OpportunityName.ToLower().Contains(term)) ||
                    (o.Description != null && o.Description.ToLower().Contains(term)));
            }

            // Apply sorting
            query = request.SortBy?.ToLower() switch
            {
                "name" => request.SortDescending
                    ? query.OrderByDescending(o => o.OpportunityName)
                    : query.OrderBy(o => o.OpportunityName),
                "value" or "amount" => request.SortDescending
                    ? query.OrderByDescending(o => o.EstimatedAmount)
                    : query.OrderBy(o => o.EstimatedAmount),
                "closedate" => request.SortDescending
                    ? query.OrderByDescending(o => o.EstimatedCloseDate)
                    : query.OrderBy(o => o.EstimatedCloseDate),
                "stage" => request.SortDescending
                    ? query.OrderByDescending(o => o.OpportunityStage!.SequenceNum)
                    : query.OrderBy(o => o.OpportunityStage!.SequenceNum),
                _ => request.SortDescending
                    ? query.OrderByDescending(o => o.CreatedStamp)
                    : query.OrderBy(o => o.CreatedStamp)
            };

            var opportunities = await query.ToListAsync(ct);

            var result = opportunities.Select(o =>
            {
                var ownerRole = o.SalesOpportunityRoles.FirstOrDefault(r => r.RoleTypeId == "OWNER");
                var brokerRole = o.SalesOpportunityRoles.FirstOrDefault(r => r.RoleTypeId == "BROKER");
                var leads = o.SalesOpportunityRoles
                    .Where(r => r.RoleTypeId != "OWNER" && r.RoleTypeId != "BROKER") // Exclude OWNER and BROKER roles
                    .Select(r => new SalesOpportunityLeadDto
                    {
                        PartyId = r.PartyId,
                        PartyName = GetPartyName(r.Party),
                        RoleTypeId = r.RoleTypeId,
                        RoleDescription = r.RoleType?.Description,
                        DataSourceId = r.Party?.DataSourceId
                    })
                    .ToList();

                return new SalesOpportunityDto
                {
                    SalesOpportunityId = o.SalesOpportunityId,
                    OpportunityName = o.OpportunityName,
                    Description = o.Description,
                    EstimatedAmount = o.EstimatedAmount,
                    CurrencyUomId = o.CurrencyUomId,
                    EstimatedProbability = o.EstimatedProbability,
                    OpportunityStageId = o.OpportunityStageId,
                    OpportunityStageName = request.Language == "ar" ? o.OpportunityStage?.DescriptionArabic :  o.OpportunityStage?.Description,
                    StageSequenceNum = o.OpportunityStage?.SequenceNum,
                    OwnerPartyId = ownerRole?.PartyId,
                    OwnerName = ownerRole != null ? GetPartyName(ownerRole.Party) : null,
                    BrokerPartyId = brokerRole?.PartyId,
                    BrokerName = brokerRole != null ? GetPartyName(brokerRole.Party) : null,
                    EstimatedCloseDate = o.EstimatedCloseDate,
                    CreatedStamp = o.CreatedStamp,
                    NextStep = o.NextStep,
                    NextStepDate = o.NextStepDate,
                    DataSourceId = o.DataSourceId,
                    MarketingCampaignId = o.MarketingCampaignId,
                    TypeEnumId = o.TypeEnumId,
                    WorkEffortId = o.WorkEffortId,
                    ProductId = o.ProductId,
                    WorkEffortName = o.WorkEffort?.WorkEffortName ?? o.WorkEffort?.ProjectName,
                    ProductName = o.Product?.ProductName ?? o.Product?.ProductName,
                    IsWon = o.IsWon,
                    IsClosed = o.IsClosed,
                    Leads = leads
                };
            }).ToList();

            return Result<List<SalesOpportunityDto>>.Success(result);
        }

        /// <summary>
        /// The display name for a party on a board card.
        ///
        /// Party.Description holds the whole name in one column - that is the
        /// convention across the app - so it is preferred over reassembling the
        /// PERSON parts. Rebuilding from the parts got this wrong in both
        /// directions: it dropped MIDDLE_NAME (16 parties carry one, so an
        /// imported lead showed only its first name), and where FIRST_NAME
        /// already held the whole name it appended LAST_NAME again and repeated
        /// a fragment.
        /// </summary>
        private static string GetPartyName(Domain.Party? party)
        {
            if (party == null) return "";

            if (!string.IsNullOrWhiteSpace(party.Description))
                return party.Description.Trim();

            // Fallbacks for parties with no description. All three name parts,
            // not just two.
            if (party.Person != null)
            {
                var name = string.Join(" ", new[]
                {
                    party.Person.FirstName,
                    party.Person.MiddleName,
                    party.Person.LastName
                }.Where(part => !string.IsNullOrWhiteSpace(part)));

                if (!string.IsNullOrWhiteSpace(name)) return name;
            }

            if (!string.IsNullOrWhiteSpace(party.PartyGroup?.GroupName))
                return party.PartyGroup.GroupName.Trim();

            return party.PartyId;
        }
    }
}
