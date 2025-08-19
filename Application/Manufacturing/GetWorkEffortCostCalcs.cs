using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Manufacturing
{
    public class GetWorkEffortCostCalcs
    {
        // REFACTOR: DTO for WorkEffortCostCalc, including descriptions for display
        // Purpose: Matches OFBiz grid fields and frontend requirements
        public class WorkEffortCostCalcDto
        {
            public string WorkEffortId { get; set; }
            public string CostComponentCalcId { get; set; }
            public string CostComponentTypeId { get; set; }
            public string CostComponentTypeDescription { get; set; }
            public string CostComponentCalcDescription { get; set; }
            public string FromDate { get; set; }
            public string ThruDate { get; set; }
        }

        // REFACTOR: Query to retrieve WorkEffortCostCalc records for a given WorkEffortId
        // Business: Retrieves cost calculations associated with a WorkEffort
        public class Query : IRequest<Result<List<WorkEffortCostCalcDto>>>
        {
            public string WorkEffortId { get; set; }
        }

        // REFACTOR: Handler for retrieving WorkEffortCostCalc records
        public class Handler : IRequestHandler<Query, Result<List<WorkEffortCostCalcDto>>>
        {
            private readonly DataContext _context;
            private readonly ILogger<Handler> _logger;

            public Handler(DataContext context, ILogger<Handler> logger)
            {
                _context = context;
                _logger = logger;
            }

            // REFACTOR: Query WorkEffortCostCalc with joins to CostComponentType and CostComponentCalc
            // Business: Retrieves records with descriptions for display in the grid
            // Technical: Uses LINQ to join entities, matching OFBiz grid's display-entity
            public async Task<Result<List<WorkEffortCostCalcDto>>> Handle(Query request, CancellationToken cancellationToken)
            {
                try
                {
                    // REFACTOR: Join with CostComponentType and CostComponentCalc for descriptions
                    // Purpose: Provides descriptive fields for frontend grid
                    // Benefit: Aligns with OFBiz grid's <display-entity> for CostComponentType and CostComponentCalc
                    var query = (from wec in _context.WorkEffortCostCalcs
                                 join cct in _context.CostComponentTypes
                                     on wec.CostComponentTypeId equals cct.CostComponentTypeId
                                 join ccc in _context.CostComponentCalcs
                                     on wec.CostComponentCalcId equals ccc.CostComponentCalcId
                                 where wec.WorkEffortId == request.WorkEffortId
                                 orderby wec.FromDate descending
                                 select new WorkEffortCostCalcDto
                                 {
                                     WorkEffortId = wec.WorkEffortId,
                                     CostComponentCalcId = wec.CostComponentCalcId,
                                     CostComponentTypeId = wec.CostComponentTypeId,
                                     CostComponentTypeDescription = cct.Description,
                                     CostComponentCalcDescription = ccc.Description,
                                     FromDate = wec.FromDate.ToString("o"),
                                     ThruDate = wec.ThruDate != null ? wec.ThruDate.Value.ToString("o") : null
                                 }).ToListAsync(cancellationToken);

                    // REFACTOR: Execute the query
                    // Business: Fetches cost calculations for the frontend
                    // Technical: Awaits async LINQ query
                    var result = await query;

                    // REFACTOR: Return empty list for no records
                    // Business: Allows frontend to display empty grid
                    // Technical: Simplifies client-side handling, consistent with GetRoutingTaskAssocs
                    if (result == null || !result.Any())
                    {
                        _logger.LogInformation("No WorkEffortCostCalc records found for WorkEffortId {WorkEffortId}", request.WorkEffortId);
                        return Result<List<WorkEffortCostCalcDto>>.Success(new List<WorkEffortCostCalcDto>());
                    }

                    // REFACTOR: Return successful result
                    // Business: Provides cost calculations to frontend
                    // Technical: Returns Result.Success with DTO list
                    return Result<List<WorkEffortCostCalcDto>>.Success(result);
                }
                catch (Exception ex)
                {
                    // REFACTOR: Log and handle exceptions
                    // Business: Captures errors for debugging
                    // Technical: Implements try-catch per guidelines
                    _logger.LogError(ex, "Error retrieving WorkEffortCostCalc records for WorkEffortId: {WorkEffortId}", request.WorkEffortId);
                    return Result<List<WorkEffortCostCalcDto>>.Failure($"Error retrieving work effort cost calculations: {ex.Message}");
                }
            }
        }
    }
}