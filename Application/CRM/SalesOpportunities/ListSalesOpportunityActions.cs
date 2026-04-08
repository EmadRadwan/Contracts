using Application.Core;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.CRM.SalesOpportunities;

/// <summary>
/// Lists all Sales Opportunity Actions for use in dropdowns or action selection.
/// Supports Arabic localization based on language parameter.
/// </summary>
public class ListSalesOpportunityActions
{
    public record Query : IRequest<Result<List<SalesOpportunityActionDto>>>
    {
        public string Language { get; set; } = "en"; // Default to English
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
            var actions = await _context.Enumerations
                .Where(e => e.EnumTypeId == "CRM_ACTION_TYPE") // Adjust EnumType if your table uses a different identifier
                .Select(e => new SalesOpportunityActionDto
                {
                    ActionId = e.EnumId,
                    Description = request.Language == "ar" 
                        ? e.DescriptionArabic 
                        : e.Description
                })
                .ToListAsync(ct);

            return Result<List<SalesOpportunityActionDto>>.Success(actions);
        }
    }
}

public class SalesOpportunityActionDto
{
    public string ActionId { get; set; } = null!;
    public string? Description { get; set; }
}