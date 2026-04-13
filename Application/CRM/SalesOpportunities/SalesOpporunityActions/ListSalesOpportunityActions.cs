using Application.Core;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.CRM.SalesOpportunities;

/// <summary>
/// Lists all actions for a specific Sales Opportunity.
/// Sorted by CreatedStamp (latest first). Supports multi-language descriptions.
/// </summary>
public class ListSalesOpportunityActions
{
    public record Query : IRequest<Result<List<SalesOpportunityActionDto>>>
    {
        public string SalesOpportunityId { get; init; } = null!;
        public string Language { get; init; } = "en";   // Default to English
    }

    public class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(x => x.SalesOpportunityId)
                .NotEmpty()
                .WithMessage("Sales Opportunity ID is required.")
                .NotNull()
                .WithMessage("Sales Opportunity ID is required.");
        }
    }

    public class Handler : IRequestHandler<Query, Result<List<SalesOpportunityActionDto>>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<List<SalesOpportunityActionDto>>> Handle(Query request, CancellationToken ct)
        {
            var actions = await _context.SalesOpportunityActions
                .Where(a => a.SalesOpportunityId == request.SalesOpportunityId)
                .Include(a => a.ActionType)           // Join with Enumeration for Action Type
                .Include(a => a.CancelReason)         // Join with Enumeration for Cancel Reason
                .Include(a => a.MeetingType)         // Join with Enumeration for Cancel Reason
                .Include(a => a.MeetingLocation)         // Join with Enumeration for Cancel Reason
                .OrderByDescending(a => a.CreatedStamp)   // Latest first
                .ToListAsync(ct);

            var result = actions.Select(a => new SalesOpportunityActionDto
            {
                SalesOpportunityActionId = a.SalesOpportunityActionId,
                SalesOpportunityId = a.SalesOpportunityId,

                ActionTypeId = a.ActionTypeId,
                ActionTypeDescription = GetDescription(a.ActionType, request.Language),

                IsAnswered = a.IsAnswered,
                ActionDate = a.ActionDate,
                CancelReasonId = a.CancelReasonId,
                CancelReasonDescription = GetDescription(a.CancelReason, request.Language),

                MeetingTypeId = a.MeetingTypeId,
                MeetingTypeDescription = GetDescription(a.MeetingType, request.Language),

                MeetingLocationId = a.MeetingLocationId,
                MeetingLocationDescription = GetDescription(a.MeetingLocation, request.Language),

                Note = a.Note,
                Comment = a.Comment,

                CreatedByUserLogin = a.CreatedByUserLogin,
                CreatedStamp = a.CreatedStamp,
                LastUpdatedStamp = a.LastUpdatedStamp,
                CreatedTxStamp = a.CreatedTxStamp,
                LastUpdatedTxStamp = a.LastUpdatedTxStamp
            }).ToList();

            return Result<List<SalesOpportunityActionDto>>.Success(result);
        }

        /// <summary>
        /// Returns Description or DescriptionArabic based on language
        /// </summary>
        private static string? GetDescription(Domain.Enumeration? enumeration, string language)
        {
            if (enumeration == null) return null;

            return language.ToLower() == "ar"
                ? enumeration.DescriptionArabic ?? enumeration.Description
                : enumeration.Description;
        }
    }
}