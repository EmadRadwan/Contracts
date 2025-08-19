using MediatR;
using Persistence;
using System.Threading;
using System.Threading.Tasks;
using Application.Catalog.Products.Services.Cost;
using Application.Manufacturing;
using Microsoft.EntityFrameworkCore;

namespace Application.WorkEfforts
{
    public class GetProductionRunMaterials
    {
        public class Query : IRequest<Result<List<WorkEffortGoodStandardDto>>>
        {
            public string? WorkEffortId { get; set; }
            public string Language { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<List<WorkEffortGoodStandardDto>>>
        {
            private readonly DataContext _context;
            private readonly ICostService _costService;

            public Handler(DataContext context, ICostService costService)
            {
                _context = context;
                _costService = costService;
            }

            public async Task<Result<List<WorkEffortGoodStandardDto>>> Handle(Query request, CancellationToken cancellationToken)
            {
                var language = request.Language;
                var productionRunTaskIds = await _context.WorkEfforts
                    .Where(we => we.WorkEffortParentId == request.WorkEffortId)
                    .Select(we => we.WorkEffortId)
                    .ToListAsync(cancellationToken);

                var query = await (from wegs in _context.WorkEffortGoodStandards
                    join p in _context.Products on wegs.ProductId equals p.ProductId
                    join m in _context.Uoms on p.QuantityUomId equals m.UomId
                    where productionRunTaskIds.Contains(wegs.WorkEffortId)
                    select new WorkEffortGoodStandardDto
                    {
                        WorkEffortId = wegs.WorkEffortId,
                        ProductId = wegs.ProductId,
                        ProductName = p.ProductName,
                        EstimatedQuantity = wegs.EstimatedQuantity,
                        ProductQuantityUom = language == "ar" ? m.DescriptionArabic : m.Description,
                    }).ToListAsync(cancellationToken);

                return Result<List<WorkEffortGoodStandardDto>>.Success(query);
            }
        }
    }
}