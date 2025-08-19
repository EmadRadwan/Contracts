using MediatR;
using Persistence;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Application.Shipments.Costs
{
    public class GetMaterialCostConfig
    {
        public class Query : IRequest<Result<List<MaterialCostConfigDto>>>
        {
            public string? ProductId { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<List<MaterialCostConfigDto>>>
        {
            private readonly DataContext _context;

            public Handler(DataContext context)
            {
                _context = context;
            }

            public async Task<Result<List<MaterialCostConfigDto>>> Handle(Query request,
                CancellationToken cancellationToken)
            {
                var fromDate = DateTime.UtcNow;

                // Step 1: Get the components (BOM) from ProductAssocs, linking to UOM via Products
                var components = await _context.ProductAssocs
                    .Where(c => c.ProductId == request.ProductId &&
                                c.ProductAssocTypeId == "MANUF_COMPONENT" &&
                                c.FromDate <= fromDate &&
                                (c.ThruDate == null || c.ThruDate >= fromDate))
                    .Join(_context.Products,
                        pa => pa.ProductIdTo,
                        p => p.ProductId,
                        (pa, p) => new { pa, p.QuantityUomId, p.ProductName })
                    .Join(_context.Uoms,
                        pp => pp.QuantityUomId,
                        u => u.UomId,
                        (pp, u) => new { pp.pa, UomDescription = u.Description, pp.ProductName })
                    .ToListAsync(cancellationToken);

                if (!components.Any())
                {
                    return Result<List<MaterialCostConfigDto>>.Success(new List<MaterialCostConfigDto>());
                }

                var materialCostConfigs = new List<MaterialCostConfigDto>();

                // Step 2: For each component, get the cost estimate from CostComponents and join with CostComponentTypes
                foreach (var component in components)
                {
                    var costComponentsTemp = await _context.CostComponents
                        .Where(cc =>
                            cc.ProductId == component.pa.ProductIdTo &&
                            cc.CostComponentTypeId.StartsWith("EST_STD_MAT") &&
                            cc.FromDate <= DateTime.UtcNow &&
                            (cc.ThruDate == null || cc.ThruDate >= DateTime.UtcNow))
                        .Join(_context.CostComponentTypes,
                            cc => cc.CostComponentTypeId,
                            cct => cct.CostComponentTypeId,
                            (cc, cct) => new { cc, cct.Description })
                        .ToListAsync(cancellationToken);

                    foreach (var costComponent in costComponentsTemp)
                    {
                        materialCostConfigs.Add(new MaterialCostConfigDto
                        {
                            ProductId = component.pa.ProductIdTo,
                            ProductName = component.ProductName,
                            Quantity = (decimal)component.pa.Quantity,
                            UomId = component.UomDescription,
                            EstimatedUnitCost = (decimal)costComponent.cc.Cost,
                            CostComponentTypeId = costComponent.cc.CostComponentTypeId,
                            CostComponentTypeDescription = costComponent.Description,
                            CostUomId = costComponent.cc.CostUomId,
                            FromDate = (DateTime)costComponent.cc.FromDate,
                            ThruDate = costComponent.cc.ThruDate
                        });
                    }
                }

                return Result<List<MaterialCostConfigDto>>.Success(materialCostConfigs);
            }
        }
    }
    
}
