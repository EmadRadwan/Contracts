using MediatR;
using Persistence;
using System.Threading;
using System.Threading.Tasks;
using Application.Catalog.Products.Services.Cost;
using Application.Manufacturing;
using Microsoft.EntityFrameworkCore;

namespace Application.WorkEfforts
{
    public class GetProductionRunComponentsForReturn
    {
        public class Query : IRequest<Result<List<ProductionRunComponentDto>>>
        {
            public string? WorkEffortId { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<List<ProductionRunComponentDto>>>
        {
            private readonly DataContext _context;
            private readonly IProductionRunService _productionRunService;

            public Handler(DataContext context, IProductionRunService productionRunService) {
                _context = context;
                _productionRunService = productionRunService;
            }

            public async Task<Result<List<ProductionRunComponentDto>>> Handle(Query request, CancellationToken cancellationToken)
            {
                var query = await _productionRunService.GetProductionRunComponentsForReturn(request.WorkEffortId);

                return Result<List<ProductionRunComponentDto>>.Success(query);
            }
        }
    }
}