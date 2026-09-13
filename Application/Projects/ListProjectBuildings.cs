using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Projects;

// Distinct building numbers of a project's apartment inventory. Feeds the "exclude buildings"
// picker in the project-report export dialog (the management-fee base is a % of agreed revenue
// for all buildings except a chosen few, e.g. A1/A2).
public class ListProjectBuildings
{
    public class Query : IRequest<List<string>>
    {
        public string ProjectId { get; set; } = null!;
    }

    public class Handler : IRequestHandler<Query, List<string>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context) => _context = context;

        public async Task<List<string>> Handle(Query request, CancellationToken ct)
        {
            return await _context.Products.AsNoTracking()
                .Where(p => p.ProjectId == request.ProjectId
                            && p.ProductTypeId == "APARTMENT"
                            && p.BuildingNumber != null
                            && p.BuildingNumber != "")
                .Select(p => p.BuildingNumber!)
                .Distinct()
                .OrderBy(b => b)
                .ToListAsync(ct);
        }
    }
}
