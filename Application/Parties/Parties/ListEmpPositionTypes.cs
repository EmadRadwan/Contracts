using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Parties.Parties;

public class ListEmplPositionTypes
{
    public class Query : IRequest<Result<List<EmplPositionType>>>
    {
        // You can add optional filters later, e.g.:
        // public string? ParentTypeId { get; set; }
        // public bool? ActiveOnly { get; set; } = true;
    }

    public class Handler : IRequestHandler<Query, Result<List<EmplPositionType>>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<List<EmplPositionType>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var query = _context.EmplPositionTypes
                .AsQueryable()
                .OrderBy(x => x.Description ?? x.EmplPositionTypeId);  // sort by description, fallback to ID

            // Optional: add filters here in the future
            // if (!string.IsNullOrEmpty(request.ParentTypeId))
            //     query = query.Where(x => x.ParentTypeId == request.ParentTypeId);
            //
            // if (request.ActiveOnly == true)
            //     query = query.Where(x => x.StatusId != "EMPL_POS_INACTIVE"); // example

            var positionTypes = await query
                .ToListAsync(cancellationToken);

            return Result<List<EmplPositionType>>.Success(positionTypes);
        }
    }
}