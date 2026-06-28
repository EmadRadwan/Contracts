using Application.Core;
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

                var user = await _context.Users
                    .FirstOrDefaultAsync(x => x.UserName == _userAccessor.GetUsername(), ct);
                var userLogin = user != null
                    ? await _context.UserLogins.FirstOrDefaultAsync(x => x.PartyId == user.PartyId, ct)
                    : null;

                // Check if stage changed (important for history)
                var stageChanged = opportunity.OpportunityStageId != dto.OpportunityStageId;

                // Only update these fields if they are explicitly provided
                if (dto.WorkEffortId != null)
                    opportunity.WorkEffortId = dto.WorkEffortId;
                if (dto.ProductId != null)
                    opportunity.ProductId = dto.ProductId;

                opportunity.LastUpdatedStamp = stamp;

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

                    if (newStage.OpportunityStageId == "SOSTG_CLOSED_WON")
                    {
                        var apartment = await _context.Products
                        .FirstOrDefaultAsync(p => p.ProductId == opportunity.ProductId, ct);

                        if (apartment != null)
                            apartment.ApartmentStatusId = "APARTMENT_RESERVED";
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
                        ModifiedByUserLogin = userLogin?.UserLoginId,
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

                        var roleTypeId = lead.RoleTypeId ?? "LEAD_CONTACT";
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
