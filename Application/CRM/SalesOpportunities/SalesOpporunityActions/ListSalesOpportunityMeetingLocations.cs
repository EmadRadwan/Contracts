using Application.Core;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.CRM.SalesOpportunities;

/// <summary>
/// Lists all Sales Opportunity Meeting Locations.
/// Supports Arabic localization based on language parameter.
/// </summary>
public class ListSalesOpportunityMeetingLocations
{
    public record Query : IRequest<Result<List<SalesOpportunityMeetingLocationDto>>>
    {
        public string Language { get; set; } = "en"; // Default to English
    }

    public class Handler : IRequestHandler<Query, Result<List<SalesOpportunityMeetingLocationDto>>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<List<SalesOpportunityMeetingLocationDto>>> Handle(Query request, CancellationToken ct)
        {
            var reasons = await _context.Enumerations
                .Where(e => e.EnumTypeId == "CRM_MEETING_LOC")
                .Select(e => new SalesOpportunityMeetingLocationDto
                {
                    MeetingLocationId = e.EnumId,
                    Description = request.Language == "ar" 
                        ? e.DescriptionArabic 
                        : e.Description
                })
                .ToListAsync(ct);

            return Result<List<SalesOpportunityMeetingLocationDto>>.Success(reasons);
        }
    }
}

public class SalesOpportunityMeetingLocationDto
{
    public string MeetingLocationId { get; set; } = null!;
    public string? Description { get; set; }
}