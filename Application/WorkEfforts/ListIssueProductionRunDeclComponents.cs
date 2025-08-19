using MediatR;
using Persistence;
using System.Threading;
using System.Threading.Tasks;
using Application.Catalog.Products.Services.Cost;
using Application.Manufacturing;
using Microsoft.EntityFrameworkCore;

namespace Application.WorkEfforts
{
    public class ListIssueProductionRunDeclComponents
    {
        public class Query : IRequest<Result<List<WorkEffortGoodStandardDto>>>
        {
            public string? WorkEffortId { get; set; }
            public string Language { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<List<WorkEffortGoodStandardDto>>>
        {
            private readonly DataContext _context;
            private readonly IProductionRunService _productionRunService;

            public Handler(DataContext context, IProductionRunService productionRunService) {
                _context = context;
                _productionRunService = productionRunService;
            }

            public async Task<Result<List<WorkEffortGoodStandardDto>>> Handle(Query request, CancellationToken cancellationToken)
            {
                var query = await _productionRunService.ListIssueProductionRunDeclComponents(request.WorkEffortId, request.Language);

                return Result<List<WorkEffortGoodStandardDto>>.Success(query);
            }
        }
    }
}