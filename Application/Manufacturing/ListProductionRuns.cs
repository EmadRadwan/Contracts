using Application.Catalog.Products;
using MediatR;
using Microsoft.AspNetCore.OData.Query;
using Persistence;

namespace Application.Manufacturing;

public class ListProductionRuns
{
    public class Query : IRequest<IQueryable<WorkEffortRecord>>
    {
        public ODataQueryOptions<WorkEffortRecord> Options { get; set; }
    }

    public class Handler : IRequestHandler<Query, IQueryable<WorkEffortRecord>>
    {
        private readonly DataContext _context;
        
        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<IQueryable<WorkEffortRecord>> Handle(Query request, CancellationToken cancellationToken)
        {
            var query = from we in _context.WorkEfforts
                        where we.WorkEffortTypeId == "PROD_ORDER_HEADER"
                        join wegs in _context.WorkEffortGoodStandards
                            on we.WorkEffortId equals wegs.WorkEffortId into wegsGroup
                        from wegs in wegsGroup.DefaultIfEmpty()
                        where wegs == null || wegs.WorkEffortGoodStdTypeId == "PRUN_PROD_DELIV"
                        join p in _context.Products
                            on wegs.ProductId equals p.ProductId into pGroup
                        from p in pGroup.DefaultIfEmpty()
                        join uom in _context.Uoms
                            on p.QuantityUomId equals uom.UomId into uomGroup
                        from uom in uomGroup.DefaultIfEmpty()
                        join si in _context.StatusItems
                            on we.CurrentStatusId equals si.StatusId into siGroup
                        from si in siGroup.DefaultIfEmpty()
                        join f in _context.Facilities
                            on we.FacilityId equals f.FacilityId into fGroup
                        from f in fGroup.DefaultIfEmpty()
                        select new WorkEffortRecord
                        {
                            WorkEffortId = we.WorkEffortId,
                            EstimatedStartDate = we.EstimatedStartDate,
                            ActualStartDate = we.ActualStartDate,
                            EstimatedCompletionDate = we.EstimatedCompletionDate,
                            ActualCompletionDate = we.ActualCompletionDate,
                            WorkEffortName = we.WorkEffortName,
                            Description = we.Description,
                            FacilityId = we.FacilityId,
                            FacilityName = f != null ? f.FacilityName : null,
                            QuantityToProduce = we.QuantityToProduce,
                            CurrentStatusId = we.CurrentStatusId,
                            ProductId = wegs != null ? new ProductLovDto
                            {
                                ProductId = wegs.ProductId,
                                ProductName = p.ProductName
                            } : null,
                            ProductName = p != null ? p.ProductName : null,
                            CurrentStatusDescription = si != null ? si.Description : null,
                            UomAndQuantity = uom != null ? $"{we.QuantityToProduce:F2} {uom.Description}" : null,
                            QuantityProduced = we.QuantityProduced,
                            QuantityRejected = we.QuantityRejected
                        };

            return query;
        }
    }
}
