using Application.Core;
using Application.Interfaces;
using Domain;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.CRM.SalesOpportunities;

/// <summary>
/// Creates a new Action on a Sales Opportunity (Follow-up, Meeting, Cancellation, etc.)
/// </summary>
public class CreateSalesOpportunityAction
{
    public record Command : IRequest<Result<SalesOpportunityActionDto>>
    {
        public SalesOpportunityActionDto Action { get; init; } = null!;
    }

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(x => x.Action.SalesOpportunityId)
                .NotEmpty().WithMessage("Sales Opportunity ID is required");

            RuleFor(x => x.Action.ActionTypeId)
                .NotEmpty().WithMessage("Action Type is required");
        }

        private bool NeedsComment(string? actionTypeId)
        {
            if (string.IsNullOrEmpty(actionTypeId)) return true;
            return true; // Most actions require comment
        }
    }

    public class Handler : IRequestHandler<Command, Result<SalesOpportunityActionDto>>
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

        public async Task<Result<SalesOpportunityActionDto>> Handle(Command request, CancellationToken ct)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(ct);

            try
            {
                var stamp = DateTime.UtcNow;
                var dto = request.Action;

                // AspNetUsers.Id straight off the JWT - no USER_LOGIN hop.
                var actingUserId = _userAccessor.GetUserId();

                // Validate Sales Opportunity exists
                var opportunity = await _context.SalesOpportunities
                    .FirstOrDefaultAsync(x => x.SalesOpportunityId == dto.SalesOpportunityId, ct);

                if (opportunity == null)
                    return Result<SalesOpportunityActionDto>.Failure($"Sales Opportunity '{dto.SalesOpportunityId}' not found");

                // Validate Action Type exists in enumerations
                if (string.IsNullOrEmpty(dto.ActionTypeId))
                    return Result<SalesOpportunityActionDto>.Failure("Action Type is required");

                var actionTypeExists = await _context.Enumerations
                    .AnyAsync(e => e.EnumTypeId == "CRM_ACTION_TYPE" && e.EnumId == dto.ActionTypeId, ct);

                if (!actionTypeExists)
                    return Result<SalesOpportunityActionDto>.Failure($"Invalid Action Type: '{dto.ActionTypeId}'");

                // Validate Cancel Reason when action is a cancellation type
                if (IsCancellationAction(dto.ActionTypeId))
                {
                    if (string.IsNullOrEmpty(dto.CancelReasonId))
                        return Result<SalesOpportunityActionDto>.Failure("Cancel Reason is required for cancellation actions");

                    var reasonExists = await _context.Enumerations
                        .AnyAsync(e => e.EnumTypeId == "CRM_CANCELLATION_REASON" && e.EnumId == dto.CancelReasonId, ct);

                    if (!reasonExists)
                        return Result<SalesOpportunityActionDto>.Failure($"Invalid Cancel Reason: '{dto.CancelReasonId}'");
                }

                if (IsMeetingAction(dto.ActionTypeId))
                {
                    if (string.IsNullOrEmpty(dto.MeetingTypeId))
                        return Result<SalesOpportunityActionDto>.Failure("Meeting Type is required for meeting actions");

                    var meetingTypeExists = await _context.Enumerations
                        .AnyAsync(e => e.EnumTypeId == "CRM_MEETING_TYPE" && e.EnumId == dto.MeetingTypeId, ct);

                    if (!meetingTypeExists)
                        return Result<SalesOpportunityActionDto>.Failure($"Invalid Cancel Reason: '{dto.MeetingTypeId}'");
                }

                if (IsUnitRelatedAction(dto.ActionTypeId))
                {
                    if (string.IsNullOrEmpty(dto.ProductId))
                        return Result<SalesOpportunityActionDto>.Failure("Unit (Product) is required for Reservation and Done Deal actions");

                    if (string.IsNullOrEmpty(dto.WorkEffortId))
                        return Result<SalesOpportunityActionDto>.Failure("Project (Work Effort) is required for Reservation and Done Deal actions");

                    bool projectChanged = opportunity.WorkEffortId != dto.WorkEffortId;
                    bool unitChanged = opportunity.ProductId != dto.ProductId;

                    if (projectChanged || unitChanged)
                    {
                        opportunity.WorkEffortId = dto.WorkEffortId;
                        opportunity.ProductId = dto.ProductId;
                        opportunity.LastUpdatedStamp = stamp;
                        opportunity.LastUpdatedTxStamp = stamp;
                    }
                }

                // Validate Action Date for types that require it
                if (RequiresActionDate(dto.ActionTypeId) && !dto.ActionDate.HasValue)
                {
                    return Result<SalesOpportunityActionDto>.Failure("Action Date is required for this action type");
                }

                if (dto.ActionTypeId == "DONE_DEAL")
                {

                    var doneDealStage = await _context.SalesOpportunityStages
                        .FirstOrDefaultAsync(s => s.OpportunityStageId == "SOSTG_CLOSED_WON", ct);

                    if (doneDealStage == null)
                        return Result<SalesOpportunityActionDto>.Failure("Stage 'SOSTG_CLOSED_WON' not found");

                    var apartment = await _context.Products
                        .FirstOrDefaultAsync(p => p.ProductId == dto.ProductId, ct);

                    if (apartment != null)
                    {
                        apartment.ApartmentStatusId = "APARTMENT_RESERVED";
                    }

                    opportunity.IsWon = true;
                    opportunity.IsClosed = true;
                    opportunity.OpportunityStageId = doneDealStage.OpportunityStageId;
                    opportunity.EstimatedProbability = doneDealStage.DefaultProbability ?? opportunity.EstimatedProbability;
                    opportunity.LastUpdatedStamp = stamp;
                    opportunity.LastUpdatedTxStamp = stamp;

                    await AddStageHistoryAsync(
                        opportunity,
                        $"Stage changed to {doneDealStage.Description}",
                        actingUserId,
                        stamp);
                }
                else if (IsCancellationAction(dto.ActionTypeId))
                {
                    // A cancellation/loss must close the opportunity, otherwise it sits
                    // in its original pipeline column forever.
                    var lostStage = await _context.SalesOpportunityStages
                        .FirstOrDefaultAsync(s => s.OpportunityStageId == "SOSTG_CLOSED_LOST", ct);

                    if (lostStage == null)
                        return Result<SalesOpportunityActionDto>.Failure("Stage 'SOSTG_CLOSED_LOST' not found");

                    var cancelReasonDescription = await _context.Enumerations
                        .Where(e => e.EnumTypeId == "CRM_CANCELLATION_REASON" && e.EnumId == dto.CancelReasonId)
                        .Select(e => e.Description)
                        .FirstOrDefaultAsync(ct);

                    opportunity.IsWon = false;
                    opportunity.IsClosed = true;
                    opportunity.OpportunityStageId = lostStage.OpportunityStageId;
                    opportunity.EstimatedProbability = lostStage.DefaultProbability ?? 0;
                    opportunity.LastUpdatedStamp = stamp;
                    opportunity.LastUpdatedTxStamp = stamp;

                    var changeNote = string.IsNullOrEmpty(cancelReasonDescription)
                        ? $"Stage changed to {lostStage.Description}"
                        : $"Stage changed to {lostStage.Description} - {cancelReasonDescription}";

                    await AddStageHistoryAsync(
                        opportunity,
                        changeNote,
                        actingUserId,
                        stamp);
                }

                // Generate next sequence ID
                var actionId = await _utilityService.GetNextSequence("SalesOpportunityAction");

                // Create the entity
                var action = new SalesOpportunityAction
                {
                    SalesOpportunityActionId = actionId,
                    SalesOpportunityId = dto.SalesOpportunityId,
                    ActionTypeId = dto.ActionTypeId,
                    IsAnswered = dto.IsAnswered,
                    ActionDate = dto.ActionDate,
                    CancelReasonId = dto.CancelReasonId,
                    Comment = dto.Comment,
                    MeetingTypeId = dto.MeetingTypeId,
                    MeetingLocationId = dto.MeetingLocationId,
                    Note = dto.Note,


                    // Audit fields (following your existing pattern)
                    CreatedByUserLogin = actingUserId,
                    CreatedStamp = stamp,
                    LastUpdatedStamp = stamp,
                    CreatedTxStamp = stamp,
                    LastUpdatedTxStamp = stamp
                };

                _context.SalesOpportunityActions.Add(action);

                var saved = await _context.SaveChangesAsync(ct) > 0;

                if (!saved)
                {
                    await transaction.RollbackAsync(ct);
                    return Result<SalesOpportunityActionDto>.Failure("Failed to save Sales Opportunity Action");
                }

                await transaction.CommitAsync(ct);

                // Return DTO
                var resultDto = new SalesOpportunityActionDto
                {
                    SalesOpportunityActionId = action.SalesOpportunityActionId,
                    SalesOpportunityId = action.SalesOpportunityId,
                    ActionTypeId = action.ActionTypeId,
                    IsAnswered = action.IsAnswered,
                    ActionDate = action.ActionDate,
                    CancelReasonId = action.CancelReasonId,
                    Comment = action.Comment,
                    CreatedByUserLogin = action.CreatedByUserLogin,
                    CreatedStamp = action.CreatedStamp,
                    LastUpdatedStamp = action.LastUpdatedStamp
                };

                return Result<SalesOpportunityActionDto>.Success(resultDto);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(ct);
                return Result<SalesOpportunityActionDto>.Failure($"Error creating Sales Opportunity Action: {ex.Message}");
            }
        }

        #region Business Rule Helpers

        /// <summary>
        /// Writes a SalesOpportunityHistory snapshot for a stage transition driven by an action.
        /// </summary>
        private async Task AddStageHistoryAsync(
            SalesOpportunity opportunity,
            string changeNote,
            string? modifiedByUserLogin,
            DateTime stamp)
        {
            var historyId = await _utilityService.GetNextSequence("SalesOpportunityHistory");

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
                ModifiedByUserLogin = modifiedByUserLogin,
                ModifiedTimestamp = stamp,
                CreatedStamp = stamp,
                LastUpdatedStamp = stamp
            });
        }

        private bool IsCancellationAction(string? actionTypeId)
        {
            if (string.IsNullOrEmpty(actionTypeId)) return false;

            var cancellationActions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "LOST_DEAL", "CANCELLATION", "NOT_INTERESTED", "UNREACHABLE", "WRONG_NUMBER"
            };

            return cancellationActions.Contains(actionTypeId);
        }

        private bool IsMeetingAction(string? actionTypeId)
        {
            if (string.IsNullOrEmpty(actionTypeId)) return false;

            var meetingActions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "MEETING", "SITE_VISIT"
            };

            return meetingActions.Contains(actionTypeId);
        }

        private bool IsUnitRelatedAction(string? actionTypeId)
        {
            if (string.IsNullOrEmpty(actionTypeId)) return false;

            var unitRelatedActions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "RESERVATION", "DONE_DEAL"
            };

            return unitRelatedActions.Contains(actionTypeId);
        }

        private bool RequiresActionDate(string? actionTypeId)
        {
            if (string.IsNullOrEmpty(actionTypeId)) return false;

            var dateRequiredActions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "NO_ANSWER", "FOLLOW_UP", "FOLLOW_UP_AFTER_MEETING",
                "SET_MEETING", "INTERESTED", "FRESH_STAGE", "MEETING"
            };

            return dateRequiredActions.Contains(actionTypeId);
        }

        #endregion
    }
}