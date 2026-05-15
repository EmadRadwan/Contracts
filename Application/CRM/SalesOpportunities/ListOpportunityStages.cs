using Application.Core;
using FluentValidation.Resources;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.CRM.SalesOpportunities;

/// <summary>
/// Lists all Sales Opportunity Stages for pipeline/board view.
/// </summary>
public class ListOpportunityStages
{
    public record Query : IRequest<Result<List<OpportunityStageDto>>>
    {
        public string Language {get; set;}
    };

    public class Handler : IRequestHandler<Query, Result<List<OpportunityStageDto>>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<List<OpportunityStageDto>>> Handle(Query request, CancellationToken ct)
        {
            var stages = await _context.SalesOpportunityStages
                .OrderBy(s => s.SequenceNum)
                .Select(s => new OpportunityStageDto
                {
                    OpportunityStageId = s.OpportunityStageId,
                    Description = request.Language == "ar" ? s.DescriptionArabic : s.Description,
                    DefaultProbability = s.DefaultProbability,
                    SequenceNum = s.SequenceNum
                })
                .ToListAsync(ct);

            return Result<List<OpportunityStageDto>>.Success(stages);
        }
    }
}

public class OpportunityStageDto
{
    public string OpportunityStageId { get; set; } = null!;
    public string? Description { get; set; }
    public decimal? DefaultProbability { get; set; }
    public int? SequenceNum { get; set; }
}
