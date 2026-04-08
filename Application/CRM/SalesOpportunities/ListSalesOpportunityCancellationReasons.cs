using Application.Core;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.CRM.SalesOpportunities;

/// <summary>
/// Lists all Sales Opportunity Cancellation Reasons.
/// Supports Arabic localization based on language parameter.
/// </summary>
public class ListSalesOpportunityCancellationReasons
{
    public record Query : IRequest<Result<List<SalesOpportunityCancellationReasonDto>>>
    {
        public string Language { get; set; } = "en"; // Default to English
    }

    public class Handler : IRequestHandler<Query, Result<List<SalesOpportunityCancellationReasonDto>>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<List<SalesOpportunityCancellationReasonDto>>> Handle(Query request, CancellationToken ct)
        {
            var reasons = await _context.Enumerations
                .Where(e => e.EnumTypeId == "CRM_CANCELLATION_REASON")
                .Select(e => new SalesOpportunityCancellationReasonDto
                {
                    ReasonId = e.EnumId,
                    Description = request.Language == "ar" 
                        ? e.DescriptionArabic 
                        : e.Description
                })
                .ToListAsync(ct);

            return Result<List<SalesOpportunityCancellationReasonDto>>.Success(reasons);
        }
    }
}

public class SalesOpportunityCancellationReasonDto
{
    public string ReasonId { get; set; } = null!;
    public string? Description { get; set; }
}