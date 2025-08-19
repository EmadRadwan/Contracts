using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Facilities.RejectionReasons;

public class List
{
    public class Query : IRequest<Result<List<RejectionReasonDto>>>
    {
    }


    public class Handler : IRequestHandler<Query, Result<List<RejectionReasonDto>>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<List<RejectionReasonDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var query = _context.RejectionReasons
                .Select(x => new RejectionReasonDto
                {
                    RejectionId = x.RejectionId,
                    Description = x.Description
                })
                .OrderBy(x => x.Description)
                .AsQueryable();


            var rejectionReasons = await query
                .ToListAsync();

            // Insert the empty option at the beginning
            rejectionReasons.Insert(0, new RejectionReasonDto
            {
                RejectionId = "", // Or your desired default ID
                Description = "" // Explicitly set to an empty string
            });

            return Result<List<RejectionReasonDto>>.Success(rejectionReasons);
        }
    }
}