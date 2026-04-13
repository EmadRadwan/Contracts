using Application.Core;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.CRM.SalesOpportunities;

/// <summary>
/// Lists all Sales Opportunity Meeting Types.
/// Supports Arabic localization based on language parameter.
/// </summary>
public class ListSalesOpportunityMeetingTypes
{
    public record Query : IRequest<Result<List<SalesOpportunityMeetingTypeDto>>>
    {
        public string Language { get; set; } = "en"; // Default to English
    }

    public class Handler : IRequestHandler<Query, Result<List<SalesOpportunityMeetingTypeDto>>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<List<SalesOpportunityMeetingTypeDto>>> Handle(Query request, CancellationToken ct)
        {
            var reasons = await _context.Enumerations
                .Where(e => e.EnumTypeId == "CRM_MEETING_TYPE")
                .Select(e => new SalesOpportunityMeetingTypeDto
                {
                    MeetingTypeId = e.EnumId,
                    Description = request.Language == "ar" 
                        ? e.DescriptionArabic 
                        : e.Description
                })
                .ToListAsync(ct);

            return Result<List<SalesOpportunityMeetingTypeDto>>.Success(reasons);
        }
    }
}

public class SalesOpportunityMeetingTypeDto
{
    public string MeetingTypeId { get; set; } = null!;
    public string? Description { get; set; }
}