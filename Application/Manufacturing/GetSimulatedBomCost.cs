using MediatR;
using Persistence;
using System.Threading;
using System.Threading.Tasks;
using Application.Catalog.Products.Services.Cost;
using Microsoft.EntityFrameworkCore;

namespace Application.Manufacturing
{
    public class GetSimulatedBomCost
    {
        public class Query : IRequest<Result<List<BOMSimulationDto>>>
        {
            public string? ProductId { get; set; }
            public decimal QuantityToProduce { get; set; }
            public string? CurrencyUomId { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<List<BOMSimulationDto>>>
        {
            private readonly DataContext _context;
            private readonly ICostService _costService;

            public Handler(DataContext context, ICostService costService)
            {
                _context = context;
                _costService = costService;
            }

            public async Task<Result<List<BOMSimulationDto>>> Handle(Query request, CancellationToken cancellationToken)
            {
                // Call _costService to simulate BOM cost
                var simulationResult = await _costService.SimulateBomCost(request.ProductId, request.QuantityToProduce, request.CurrencyUomId);

                // Return the simulation result
                return Result<List<BOMSimulationDto>>.Success(simulationResult);
            }
        }
    }
}