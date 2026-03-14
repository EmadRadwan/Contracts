using Application.Core;
using Application.Interfaces;
using Domain;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.CRM.SalesOpportunities;

/// <summary>
/// Creates a Sales Opportunity (Lead/Deal) - the proper CRM Lead entity.
///
/// KEY CONCEPT:
/// - A Lead is NOT a person. A Lead is a business opportunity (potential sale).
/// - People (Leads) are linked to business opportunities via SalesOpportunityRole.
/// - One business opportunity can involve multiple Leads.
/// - One Lead can be involved in multiple business opportunities.
/// </summary>
public class CreateSalesOpportunity
{
    public record Command : IRequest<Result<SalesOpportunityDto>>
    {
        public SalesOpportunityDto Opportunity { get; init; } = null!;
    }

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(x => x.Opportunity.OpportunityName)
                .NotEmpty().WithMessage("Opportunity name is required");

            RuleFor(x => x.Opportunity.OpportunityStageId)
                .NotEmpty().WithMessage("Stage is required");
        }
    }

    public class Handler : IRequestHandler<Command, Result<SalesOpportunityDto>>
    {
        private readonly DataContext _context;
        private readonly IUserAccessor _userAccessor;
        private readonly IUtilityService _utilityService;

        public Handler(DataContext context, IUserAccessor userAccessor, IUtilityService utilityService)
        {
            _context = context;
            _userAccessor = userAccessor;
            _utilityService = utilityService;
        }

        public async Task<Result<SalesOpportunityDto>> Handle(Command request, CancellationToken ct)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(ct);

            try
            {
                var stamp = DateTime.UtcNow;
                var dto = request.Opportunity;

                // Get current user
                var user = await _context.Users
                    .FirstOrDefaultAsync(x => x.UserName == _userAccessor.GetUsername(), ct);

                var userLogin = user != null
                    ? await _context.UserLogins.FirstOrDefaultAsync(x => x.PartyId == user.PartyId, ct)
                    : null;

                // Validate stage exists
                var stage = await _context.SalesOpportunityStages
                    .FirstOrDefaultAsync(x => x.OpportunityStageId == dto.OpportunityStageId, ct);

                if (stage == null)
                    return Result<SalesOpportunityDto>.Failure($"Stage '{dto.OpportunityStageId}' not found");

                // Generate ID
                var opportunityId = await _utilityService.GetNextSequence("SalesOpportunity");

                // Create the Sales Opportunity
                var opportunity = new SalesOpportunity
                {
                    SalesOpportunityId = opportunityId,
                    OpportunityName = dto.OpportunityName,
                    Description = dto.Description,
                    EstimatedAmount = dto.EstimatedAmount ?? 0,
                    EstimatedProbability = dto.EstimatedProbability ?? stage.DefaultProbability ?? 0,
                    CurrencyUomId = dto.CurrencyUomId ?? "USD",
                    OpportunityStageId = dto.OpportunityStageId,
                    EstimatedCloseDate = dto.EstimatedCloseDate,
                    NextStep = dto.NextStep,
                    NextStepDate = dto.NextStepDate,
                    DataSourceId = dto.DataSourceId,
                    MarketingCampaignId = dto.MarketingCampaignId,
                    TypeEnumId = dto.TypeEnumId,
                    CreatedByUserLogin = userLogin?.UserLoginId,
                    CreatedStamp = stamp,
                    LastUpdatedStamp = stamp
                };

                _context.SalesOpportunities.Add(opportunity);

                // Link Owner as a SalesOpportunityRole (OWNER role)
                if (!string.IsNullOrEmpty(dto.OwnerPartyId))
                {
                    await EnsurePartyRoleExists(dto.OwnerPartyId, "OWNER", stamp, ct);

                    _context.SalesOpportunityRoles.Add(new SalesOpportunityRole
                    {
                        SalesOpportunity = opportunity,
                        PartyId = dto.OwnerPartyId,
                        RoleTypeId = "OWNER",
                        CreatedStamp = stamp,
                        LastUpdatedStamp = stamp
                    });
                }

                // Link Leads via SalesOpportunityRole
                foreach (var lead in dto.Leads)
                {
                    if (string.IsNullOrEmpty(lead.PartyId))
                        continue;

                    var roleTypeId = lead.RoleTypeId ?? "LEAD_CONTACT";

                    // Ensure the party has the required role
                    await EnsurePartyRoleExists(lead.PartyId, roleTypeId, stamp, ct);

                    _context.SalesOpportunityRoles.Add(new SalesOpportunityRole
                    {
                        SalesOpportunity = opportunity,
                        PartyId = lead.PartyId,
                        RoleTypeId = roleTypeId,
                        CreatedStamp = stamp,
                        LastUpdatedStamp = stamp
                    });
                }

                // Create initial history entry
                var historyId = await _utilityService.GetNextSequence("SalesOpportunityHistory");
                _context.SalesOpportunityHistories.Add(new SalesOpportunityHistory
                {
                    SalesOpportunityHistoryId = historyId,
                    SalesOpportunity = opportunity,
                    Description = dto.Description,
                    EstimatedAmount = dto.EstimatedAmount ?? 0,
                    EstimatedProbability = dto.EstimatedProbability ?? stage.DefaultProbability ?? 0,
                    CurrencyUomId = dto.CurrencyUomId ?? "USD",
                    OpportunityStageId = dto.OpportunityStageId,
                    EstimatedCloseDate = dto.EstimatedCloseDate,
                    ChangeNote = "Opportunity created",
                    ModifiedByUserLogin = userLogin?.UserLoginId,
                    ModifiedTimestamp = stamp,
                    CreatedStamp = stamp,
                    LastUpdatedStamp = stamp
                });

                var saved = await _context.SaveChangesAsync(ct) > 0;
                if (!saved)
                {
                    await transaction.RollbackAsync(ct);
                    return Result<SalesOpportunityDto>.Failure("Failed to create Sales Opportunity");
                }

                await transaction.CommitAsync(ct);

                // Return the created opportunity
                var result = new SalesOpportunityDto
                {
                    SalesOpportunityId = opportunity.SalesOpportunityId,
                    OpportunityName = opportunity.OpportunityName,
                    Description = opportunity.Description,
                    EstimatedAmount = opportunity.EstimatedAmount,
                    CurrencyUomId = opportunity.CurrencyUomId,
                    EstimatedProbability = opportunity.EstimatedProbability,
                    OpportunityStageId = opportunity.OpportunityStageId,
                    OpportunityStageName = stage.Description,
                    StageSequenceNum = stage.SequenceNum,
                    OwnerPartyId = dto.OwnerPartyId,
                    EstimatedCloseDate = opportunity.EstimatedCloseDate,
                    CreatedStamp = opportunity.CreatedStamp,
                    NextStep = opportunity.NextStep,
                    NextStepDate = opportunity.NextStepDate,
                    DataSourceId = opportunity.DataSourceId,
                    MarketingCampaignId = opportunity.MarketingCampaignId,
                    TypeEnumId = opportunity.TypeEnumId,
                    Leads = dto.Leads
                };

                return Result<SalesOpportunityDto>.Success(result);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(ct);
                return Result<SalesOpportunityDto>.Failure($"Error creating opportunity: {ex.Message}");
            }
        }

        /// <summary>
        /// Ensures the party has the specified role type. Creates it if missing.
        /// </summary>
        private async Task EnsurePartyRoleExists(string partyId, string roleTypeId, DateTime stamp, CancellationToken ct)
        {
            var exists = await _context.PartyRoles
                .AnyAsync(pr => pr.PartyId == partyId && pr.RoleTypeId == roleTypeId, ct);

            if (!exists)
            {
                var party = await _context.Parties.FindAsync(new object[] { partyId }, ct);
                var roleType = await _context.RoleTypes.FindAsync(new object[] { roleTypeId }, ct);

                if (party != null && roleType != null)
                {
                    _context.PartyRoles.Add(new PartyRole
                    {
                        Party = party,
                        RoleType = roleType,
                        CreatedStamp = stamp,
                        LastUpdatedStamp = stamp
                    });
                }
            }
        }
    }
}
