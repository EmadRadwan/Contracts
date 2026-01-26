using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Parties.Parties;

public class ListEmplPositionTypes
{
    public class Query : IRequest<Result<List<EmplPositionTypeDto>>>
    {
        // You can add optional filters later, e.g.:
        // public string? ParentTypeId { get; set; }
        // public bool? ActiveOnly { get; set; } = true;
    }

    public class Handler : IRequestHandler<Query, Result<List<EmplPositionTypeDto>>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<List<EmplPositionTypeDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var positionTypes = await _context.EmplPositionTypes
                .Where(x => x.EmplPositionTypeId != "_NA_")    
                .OrderBy(x => x.Description ?? x.EmplPositionTypeId)
                .Select(x => new EmplPositionTypeDto
                {
                    EmplPositionTypeId = x.EmplPositionTypeId,
                    Description = x.Description
                })
                .ToListAsync(cancellationToken);


            return Result<List<EmplPositionTypeDto>>.Success(positionTypes);
        }
    }
}

public class EmplPositionTypeDto
{
    public string EmplPositionTypeId { get; set; } = null!;
    public string? Description { get; set; }
}
