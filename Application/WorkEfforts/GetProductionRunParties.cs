using MediatR;
using Persistence;
using System.Threading;
using System.Threading.Tasks;
using Application.Catalog.Products.Services.Cost;
using Application.Manufacturing;
using Microsoft.EntityFrameworkCore;

namespace Application.WorkEfforts
{
    public class GetProductionRunParties
    {
        public class Query : IRequest<Result<List<WorkEffortPartyAssignmentDto>>>
        {
            public string? WorkEffortId { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<List<WorkEffortPartyAssignmentDto>>>
        {
            private readonly DataContext _context;

            public Handler(DataContext context)
            {
                _context = context;
            }

            public async Task<Result<List<WorkEffortPartyAssignmentDto>>> Handle(Query request, CancellationToken cancellationToken)
            {
                var parentWorkEffortId = request.WorkEffortId;

                var query = await (from wepa in _context.WorkEffortPartyAssignments
                    join pr in _context.PartyRoles on wepa.RoleTypeId equals pr.RoleTypeId
                    join rt in _context.RoleTypes on pr.RoleTypeId equals rt.RoleTypeId
                    join p in _context.Parties on wepa.PartyId equals p.PartyId
                    join s in _context.StatusItems on wepa.StatusId equals s.StatusId
                    join we in _context.WorkEfforts on wepa.WorkEffortId equals we.WorkEffortId
                    where we.WorkEffortParentId == parentWorkEffortId
                    select new WorkEffortPartyAssignmentDto
                    {
                        WorkEffortId = wepa.WorkEffortId,
                        PartyId = wepa.PartyId,
                        PartyName = p.Description,
                        RoleTypeId = wepa.RoleTypeId,
                        RoleTypeDescription = rt.Description,
                        FromDate = wepa.FromDate,
                        ThruDate = wepa.ThruDate,
                        StatusId = wepa.StatusId,
                        StatusDescription = s.Description,
                        StatusDateTime = wepa.StatusDateTime,
                    }).Distinct().ToListAsync(cancellationToken);

                return Result<List<WorkEffortPartyAssignmentDto>>.Success(query);
            }
        }
    }
}