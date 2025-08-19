using Application.Catalog.Products;
using Application.Core;
using Application.Manufacturing;
using MediatR;
using Microsoft.AspNetCore.OData.Query;
using Persistence;

namespace Application.WorkEfforts;

public class ListWorkEffortsWithReservations
{
    public class Query : IRequest<IQueryable<WorkEffortReservationRecord>>
    {
        public ODataQueryOptions<WorkEffortReservationRecord> Options { get; set; }
    }

    public class Handler : IRequestHandler<Query, IQueryable<WorkEffortReservationRecord>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<IQueryable<WorkEffortReservationRecord>> Handle(Query request,
            CancellationToken cancellationToken)
        {
            var query = from wia in _context.WorkEffortInventoryRes
                join weTask in _context.WorkEfforts
                    on wia.WorkEffortId equals weTask.WorkEffortId
                join weMain in _context.WorkEfforts
                    on weTask.WorkEffortParentId equals weMain.WorkEffortId
                where weMain.WorkEffortTypeId == "PROD_ORDER_HEADER"
                join si in _context.StatusItems
                    on weMain.CurrentStatusId equals si.StatusId into siGroup
                from si in siGroup.DefaultIfEmpty()
                join f in _context.Facilities
                    on weMain.FacilityId equals f.FacilityId into fGroup
                from f in fGroup.DefaultIfEmpty()
                group wia by new
                {
                    ReservationId = weTask.WorkEffortId, // First task ID
                    ProductionRunId = weMain.WorkEffortId, // Main production run ID
                    weMain.WorkEffortName,
                    weMain.Description,
                    weMain.FacilityId,
                    FacilityName = f.FacilityName,
                    weMain.QuantityToProduce,
                    weMain.QuantityProduced,
                    weMain.QuantityRejected,
                    weMain.EstimatedStartDate,
                    weMain.ActualStartDate,
                    weMain.EstimatedCompletionDate,
                    weMain.ActualCompletionDate,
                    weMain.CurrentStatusId,
                    CurrentStatusDescription = si.Description
                }
                into grouped
                select new WorkEffortReservationRecord
                {
                    ReservationWorkEffortId = grouped.Key.ReservationId,
                    ProductionRunWorkEffortId = grouped.Key.ProductionRunId,
                    ProductionRunName = grouped.Key.WorkEffortName,
                    Description = grouped.Key.Description,
                    FacilityId = grouped.Key.FacilityId,
                    FacilityName = grouped.Key.FacilityName,
                    QuantityToProduce = grouped.Key.QuantityToProduce,
                    QuantityProduced = grouped.Key.QuantityProduced,
                    QuantityRejected = grouped.Key.QuantityRejected,
                    EstimatedStartDate = grouped.Key.EstimatedStartDate,
                    ActualStartDate = grouped.Key.ActualStartDate,
                    EstimatedCompletionDate = grouped.Key.EstimatedCompletionDate,
                    ActualCompletionDate = grouped.Key.ActualCompletionDate,
                    CurrentStatusId = grouped.Key.CurrentStatusId,
                    CurrentStatusDescription = grouped.Key.CurrentStatusDescription,
                    TotalReservedQuantity = grouped.Sum(w => w.Quantity ?? 0) // ✅ Aggregated Total
                };

            return query;
        }
    }
}