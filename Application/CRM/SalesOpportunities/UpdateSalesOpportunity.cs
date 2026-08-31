using Application.Core;
using Application.CRM.Leads.Assignment;
using Application.Interfaces;
using Domain;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.CRM.SalesOpportunities;

/// <summary>
/// Updates an existing Sales Opportunity.
/// Tracks changes in SalesOpportunityHistory for audit trail.
/// </summary>
public class UpdateSalesOpportunity
{
    public record Command : IRequest<Result<SalesOpportunityDto>>
    {
        public SalesOpportunityDto Opportunity { get; init; } = null!;
    }

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(x => x.Opportunity.SalesOpportunityId)
                .NotEmpty().WithMessage("Opportunity ID is required");
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

                var opportunity = await _context.SalesOpportunities
                    .Include(o => o.OpportunityStage)
                    .Include(o => o.SalesOpportunityRoles)
                    .FirstOrDefaultAsync(o => o.SalesOpportunityId == dto.SalesOpportunityId, ct);

                if (opportunity == null)
                    return Result<SalesOpportunityDto>.Failure("Opportunity not found");

                // AspNetUsers.Id straight off the JWT - no USER_LOGIN hop.
                var actingUserId = _userAccessor.GetUserId();

                // Check if stage changed (important for history)
                var stageChanged = opportunity.OpportunityStageId != dto.OpportunityStageId;

                // Only update these fields if they are explicitly provided.
                // The stage-only PATCH endpoint reuses this command with a sparse DTO,
                // so every assignment must stay null-guarded.
                if (dto.WorkEffortId != null)
                    opportunity.WorkEffortId = dto.WorkEffortId;
                if (dto.ProductId != null)
                {
                    // Moving an already-won opportunity onto a different unit would
                    // otherwise slip past the check below, which only runs on a stage
                    // change. Guard the swap itself.
                    if (dto.ProductId != opportunity.ProductId
                        && opportunity.OpportunityStageId == UnitReservationGuard.ClosedWonStageId)
                    {
                        var swapConflict = await UnitReservationGuard.CheckAsync(
                            _context, dto.ProductId, opportunity.SalesOpportunityId, ct);

                        if (swapConflict != null)
                        {
                            await transaction.RollbackAsync(ct);
                            return Result<SalesOpportunityDto>.Failure(swapConflict);
                        }
                    }

                    opportunity.ProductId = dto.ProductId;
                }
                if (dto.OpportunityName != null)
                    opportunity.OpportunityName = dto.OpportunityName;
                if (dto.Description != null)
                    opportunity.Description = dto.Description;
                if (dto.EstimatedAmount.HasValue)
                    opportunity.EstimatedAmount = dto.EstimatedAmount;
                if (dto.CurrencyUomId != null)
                    opportunity.CurrencyUomId = dto.CurrencyUomId;
                if (dto.EstimatedProbability.HasValue)
                    opportunity.EstimatedProbability = dto.EstimatedProbability;
                if (dto.EstimatedCloseDate.HasValue)
                    opportunity.EstimatedCloseDate = dto.EstimatedCloseDate;
                if (dto.NextStep != null)
                    opportunity.NextStep = dto.NextStep;
                if (dto.NextStepDate.HasValue)
                    opportunity.NextStepDate = dto.NextStepDate;
                if (dto.DataSourceId != null)
                    opportunity.DataSourceId = dto.DataSourceId;
                if (dto.MarketingCampaignId != null)
                    opportunity.MarketingCampaignId = dto.MarketingCampaignId;
                if (dto.TypeEnumId != null)
                    opportunity.TypeEnumId = dto.TypeEnumId;

                opportunity.LastUpdatedStamp = stamp;
                opportunity.LastUpdatedTxStamp = stamp;

                // Update stage if changed
                if (stageChanged && !string.IsNullOrEmpty(dto.OpportunityStageId))
                {
                    var newStage = await _context.SalesOpportunityStages
                        .FirstOrDefaultAsync(s => s.OpportunityStageId == dto.OpportunityStageId, ct);

                    if (newStage == null)
                        return Result<SalesOpportunityDto>.Failure($"Stage '{dto.OpportunityStageId}' not found");

                    opportunity.OpportunityStageId = dto.OpportunityStageId;

                    var historyId = await _utilityService.GetNextSequence("SalesOpportunityHistory");
                    var changeNote = stageChanged
                        ? $"Stage changed to {newStage.Description}"
                        : "Opportunity updated";

                    if (newStage.OpportunityStageId == UnitReservationGuard.ClosedWonStageId)
                    {
                        // A unit can only be won once - two leads winning the same
                        // apartment is a conflict with a real customer, not a
                        // reporting nuisance.
                        var conflict = await UnitReservationGuard.CheckAsync(
                            _context, opportunity.ProductId, opportunity.SalesOpportunityId, ct);

                        if (conflict != null)
                        {
                            await transaction.RollbackAsync(ct);
                            return Result<SalesOpportunityDto>.Failure(conflict);
                        }

                        var apartment = await _context.Products
                        .FirstOrDefaultAsync(p => p.ProductId == opportunity.ProductId, ct);

                        if (apartment != null)
                            apartment.ApartmentStatusId = UnitReservationGuard.ReservedStatusId;

                        opportunity.IsWon = true;
                        opportunity.IsClosed = true;
                    }
                    else if (newStage.OpportunityStageId == "SOSTG_CLOSED_LOST")
                    {
                        opportunity.IsWon = false;
                        opportunity.IsClosed = true;
                    }
                    else
                    {
                        // Moved back into an open stage - reopen the opportunity.
                        opportunity.IsWon = false;
                        opportunity.IsClosed = false;
                    }

                    _context.SalesOpportunityHistories.Add(new SalesOpportunityHistory
                    {
                        SalesOpportunityHistoryId = historyId,
                        SalesOpportunityId = opportunity.SalesOpportunityId,
                        Description = opportunity.Description,
                        EstimatedAmount = opportunity.EstimatedAmount,
                        EstimatedProbability = opportunity.EstimatedProbability,
                        CurrencyUomId = opportunity.CurrencyUomId,
                        OpportunityStageId = opportunity.OpportunityStageId,
                        EstimatedCloseDate = opportunity.EstimatedCloseDate,
                        ChangeNote = changeNote,
                        ModifiedByUserLogin = actingUserId,
                        ModifiedTimestamp = stamp,
                        CreatedStamp = stamp,
                        LastUpdatedStamp = stamp
                    });

                    // Update probability from stage default if not explicitly set
                    if (!dto.EstimatedProbability.HasValue && newStage.DefaultProbability.HasValue)
                    {
                        opportunity.EstimatedProbability = newStage.DefaultProbability.Value;
                    }
                }

                // Update owner if changed
                if (!string.IsNullOrEmpty(dto.OwnerPartyId))
                {
                    var existingOwnerRole = opportunity.SalesOpportunityRoles
                        .FirstOrDefault(r => r.RoleTypeId == "OWNER");

                    if (existingOwnerRole == null || existingOwnerRole.PartyId != dto.OwnerPartyId)
                    {
                        // Remove old owner
                        if (existingOwnerRole != null)
                        {
                            _context.SalesOpportunityRoles.Remove(existingOwnerRole);
                        }

                        // Add new owner
                        await EnsurePartyRoleExists(dto.OwnerPartyId, "OWNER", stamp, ct);
                        _context.SalesOpportunityRoles.Add(new SalesOpportunityRole
                        {
                            SalesOpportunityId = opportunity.SalesOpportunityId,
                            PartyId = dto.OwnerPartyId,
                            RoleTypeId = "OWNER",
                            CreatedStamp = stamp,
                            LastUpdatedStamp = stamp
                        });
                    }
                }

                if (!string.IsNullOrEmpty(dto.BrokerPartyId))
                {
                    var existingOwnerRole = opportunity.SalesOpportunityRoles
                        .FirstOrDefault(r => r.RoleTypeId == "BROKER");

                    if (existingOwnerRole == null || existingOwnerRole.PartyId != dto.BrokerPartyId)
                    {
                        // Remove old broker
                        if (existingOwnerRole != null)
                        {
                            _context.SalesOpportunityRoles.Remove(existingOwnerRole);
                        }

                        // Add new broker
                        await EnsurePartyRoleExists(dto.BrokerPartyId, "BROKER", stamp, ct);
                        _context.SalesOpportunityRoles.Add(new SalesOpportunityRole
                        {
                            SalesOpportunityId = opportunity.SalesOpportunityId,
                            PartyId = dto.BrokerPartyId,
                            RoleTypeId = "BROKER",
                            CreatedStamp = stamp,
                            LastUpdatedStamp = stamp
                        });
                    }
                }

                // Update leads
                if (dto.Leads.Any())
                {
                    // Remove existing non-owner roles
                    var existingLeadRoles = opportunity.SalesOpportunityRoles
                        .Where(r => r.RoleTypeId != "OWNER")
                        .ToList();

                    foreach (var role in existingLeadRoles)
                    {
                        _context.SalesOpportunityRoles.Remove(role);
                    }

                    // Add new leads
                    foreach (var lead in dto.Leads)
                    {
                        if (string.IsNullOrEmpty(lead.PartyId))
                            continue;

                        var roleTypeId = lead.RoleTypeId ?? LeadAssignmentConstants.LeadRoleTypeId;
                        await EnsurePartyRoleExists(lead.PartyId, roleTypeId, stamp, ct);

                        _context.SalesOpportunityRoles.Add(new SalesOpportunityRole
                        {
                            SalesOpportunityId = opportunity.SalesOpportunityId,
                            PartyId = lead.PartyId,
                            RoleTypeId = roleTypeId,
                            CreatedStamp = stamp,
                            LastUpdatedStamp = stamp
                        });
                    }
                }

                var saved = await _context.SaveChangesAsync(ct) > 0;
                if (!saved)
                {
                    await transaction.RollbackAsync(ct);
                    return Result<SalesOpportunityDto>.Failure("Failed to update Sales Opportunity");
                }

                await transaction.CommitAsync(ct);

                // Reload to get updated stage name
                var stage = await _context.SalesOpportunityStages
                    .FirstOrDefaultAsync(s => s.OpportunityStageId == opportunity.OpportunityStageId, ct);

                var result = new SalesOpportunityDto
                {
                    SalesOpportunityId = opportunity.SalesOpportunityId,
                    OpportunityStageId = opportunity.OpportunityStageId,
                    OpportunityStageName = stage?.Description,
                    OwnerPartyId = dto.OwnerPartyId,
                    Leads = dto.Leads
                };

                return Result<SalesOpportunityDto>.Success(result);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(ct);
                return Result<SalesOpportunityDto>.Failure($"Error updating opportunity: {ex.Message}");
            }
        }

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
