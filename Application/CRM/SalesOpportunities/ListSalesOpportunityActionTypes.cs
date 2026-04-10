using Application.Core;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.CRM.SalesOpportunities;

/// <summary>
/// Lists all Sales Opportunity Actions for use in dropdowns or action selection.
/// Supports Arabic localization based on language parameter.
/// </summary>
public class ListSalesOpportunityActionTypes
{
    public record Query : IRequest<Result<List<SalesOpportunityActionTypesDto>>>
    {
        public string Language { get; set; } = "en"; // Default to English
    }

    public class Handler : IRequestHandler<Query, Result<List<SalesOpportunityActionTypesDto>>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<List<SalesOpportunityActionTypesDto>>> Handle(Query request, CancellationToken ct)
        {
            var actions = await _context.Enumerations
                .Where(e => e.EnumTypeId == "CRM_ACTION_TYPE") // Adjust EnumType if your table uses a different identifier
                .Select(e => new SalesOpportunityActionTypesDto
                {
                    ActionId = e.EnumId,
                    Description = request.Language == "ar" 
                        ? e.DescriptionArabic 
                        : e.Description
                })
                .ToListAsync(ct);

            return Result<List<SalesOpportunityActionTypesDto>>.Success(actions);
        }
    }
}

public class SalesOpportunityActionTypesDto
{
    public string ActionId { get; set; } = null!;
    public string? Description { get; set; }
}