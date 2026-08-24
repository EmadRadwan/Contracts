using Application.Core;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.CRM.SalesOpportunities;

/// <summary>
/// Lists all history entries for a specific Sales Opportunity.
/// Sorted by CreatedStamp (latest first). Supports multi-language stage descriptions.
/// </summary>
public class ListSalesOpportunityHistory
{
    public record Query : IRequest<Result<List<SalesOpportunityHistoryDto>>>
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

    public class Handler : IRequestHandler<Query, Result<List<SalesOpportunityHistoryDto>>>
    {
        private readonly DataContext _context;
        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<List<SalesOpportunityHistoryDto>>> Handle(Query request, CancellationToken ct)
        {
            var history = await _context.SalesOpportunityHistories
                .Where(h => h.SalesOpportunityId == request.SalesOpportunityId)
                .Include(h => h.OpportunityStage)   // Join with Stage for description
                .OrderByDescending(h => h.CreatedStamp)   // Latest first
                .ToListAsync(ct);

            // Resolve DisplayName for each ModifiedByUserLogin in one batch,
            // avoiding an N+1 query per history row. The column holds
            // AspNetUsers.Id, so this is a single lookup - previously it went
            // via USER_LOGIN, which only covers 10 of 24 users and left the
            // rest showing a raw id.
            var userIds = history
                .Where(h => !string.IsNullOrEmpty(h.ModifiedByUserLogin))
                .Select(h => h.ModifiedByUserLogin!)
                .Distinct()
                .ToList();

            var displayNameByUserId = new Dictionary<string, string>();

            if (userIds.Count > 0)
            {
                displayNameByUserId = await _context.Users
                    .Where(u => userIds.Contains(u.Id))
                    .ToDictionaryAsync(u => u.Id, u => u.DisplayName, ct);
            }

            var result = history.Select(h => new SalesOpportunityHistoryDto
            {
                SalesOpportunityHistoryId = h.SalesOpportunityHistoryId,
                SalesOpportunityId = h.SalesOpportunityId,
                Description = h.Description,
                NextStep = h.NextStep,
                EstimatedAmount = h.EstimatedAmount,
                EstimatedProbability = h.EstimatedProbability,
                CurrencyUomId = h.CurrencyUomId,
                EstimatedCloseDate = h.EstimatedCloseDate,
                OpportunityStageId = h.OpportunityStageId,
                OpportunityStageDescription = GetStageDescription(h.OpportunityStage, request.Language),
                ChangeNote = h.ChangeNote,
                ModifiedByUserLogin = h.ModifiedByUserLogin,
                ModifiedByDisplayName = h.ModifiedByUserLogin != null
                    && displayNameByUserId.TryGetValue(h.ModifiedByUserLogin, out var displayName)
                    ? displayName
                    : h.ModifiedByUserLogin,
                ModifiedTimestamp = h.ModifiedTimestamp,

                CreatedStamp = h.CreatedStamp,
                LastUpdatedStamp = h.LastUpdatedStamp,
                CreatedTxStamp = h.CreatedTxStamp,
                LastUpdatedTxStamp = h.LastUpdatedTxStamp
            }).ToList();

            return Result<List<SalesOpportunityHistoryDto>>.Success(result);
        }

        /// <summary>
        /// Returns Description or DescriptionArabic based on language
        /// </summary>
        private static string? GetStageDescription(Domain.SalesOpportunityStage? stage, string language)
        {
            if (stage == null) return null;
            return language.ToLower() == "ar"
                ? stage.DescriptionArabic ?? stage.Description
                : stage.Description;
        }
    }
}