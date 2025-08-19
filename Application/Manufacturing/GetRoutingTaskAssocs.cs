using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Manufacturing
{
    public class GetRoutingTaskAssocs
    {
        public class WorkEffortAssocDto
        {
            public string WorkEffortIdFrom { get; set; } // Source WorkEffort ID
            public WorkEffortToDto WorkEffortIdTo { get; set; } // Target WorkEffort ID
            public string WorkEffortToName { get; set; } // Name of the target WorkEffort
            public string WorkEffortAssocTypeId { get; set; } // Type of association
            public string WorkEffortAssocTypeDescription { get; set; } // Description of the association type
            public DateTime? FromDate { get; set; } // Start date of the association
            public int? SequenceNum { get; set; } // Sequence number for ordering
        }
        
        public class WorkEffortToDto
        {
            public string WorkEffortIdTo { get; set; } // Target WorkEffort ID
            public string WorkEffortName { get; set; } // Name of the target WorkEffort
        }

        // Query to retrieve WorkEffortAssoc records for a given WorkEffortId
        public class Query : IRequest<Result<List<WorkEffortAssocDto>>>
        {
            public string WorkEffortId { get; set; } // ID of the WorkEffort to find associations for
        }

        // Handler for retrieving WorkEffortAssoc records
        public class Handler : IRequestHandler<Query, Result<List<WorkEffortAssocDto>>>
        {
            private readonly DataContext _context;
            private readonly ILogger<Handler> _logger;

            public Handler(DataContext context, ILogger<Handler> logger)
            {
                _context = context;
                _logger = logger;
            }

            // Handles the query to retrieve routing task associations
            // Business: Retrieves all WorkEffortAssoc records linked to a given WorkEffortId (as source or target)
            // Technical: Translates OFBiz-style entity queries into a LINQ-based MediatR handler
            public async Task<Result<List<WorkEffortAssocDto>>> Handle(Query request, CancellationToken cancellationToken)
            {
                try
                {
                    // REFACTOR: Added join with WorkEffort to fetch WorkEffortName for WorkEffortIdTo
                    // Purpose: Includes WorkEffortToName in the DTO to support FormComboBoxVirtualRoutingTasks
                    // Benefit: Provides the necessary name field for frontend form binding
                    var query = (from wea in _context.WorkEffortAssocs
                                 join wat in _context.WorkEffortAssocTypes
                                     on wea.WorkEffortAssocTypeId equals wat.WorkEffortAssocTypeId
                                 join we in _context.WorkEfforts
                                     on wea.WorkEffortIdTo equals we.WorkEffortId
                                 where wea.WorkEffortIdFrom == request.WorkEffortId || wea.WorkEffortIdTo == request.WorkEffortId
                                 orderby wea.SequenceNum ascending, wea.FromDate descending
                                 select new WorkEffortAssocDto
                                 {
                                     WorkEffortIdFrom = wea.WorkEffortIdFrom,
                                     
                                     WorkEffortIdTo = new WorkEffortToDto
                                     {
                                         WorkEffortIdTo = wea.WorkEffortIdTo,
                                         WorkEffortName = we.WorkEffortName
                                     },
                                     WorkEffortToName = we.WorkEffortName,
                                     WorkEffortAssocTypeId = wea.WorkEffortAssocTypeId,
                                     WorkEffortAssocTypeDescription = wat.Description,
                                     FromDate = wea.FromDate,
                                     SequenceNum = wea.SequenceNum
                                 }).ToListAsync(cancellationToken);

                    // Execute the query
                    // Business: Retrieves the list of associations for the frontend
                    // Technical: Awaits the async LINQ query to fetch results
                    var result = await query;

                    // REFACTOR: Updated to return Success with empty list when no records are found
                    // Business: Allows frontend to display an empty grid when no associations exist
                    // Technical: Simplifies client-side handling by returning an empty list
                    if (result == null || !result.Any())
                    {
                        _logger.LogInformation("No WorkEffortAssoc records found for WorkEffortId {WorkEffortId}", request.WorkEffortId);
                        return Result<List<WorkEffortAssocDto>>.Success(new List<WorkEffortAssocDto>());
                    }

                    // Return the successful result
                    // Business: Provides the list of associations to the frontend for display
                    // Technical: Returns Result.Success with the DTO list
                    return Result<List<WorkEffortAssocDto>>.Success(result);
                }
                catch (Exception ex)
                {
                    // Log and handle any exceptions
                    // Business: Captures errors during data retrieval for debugging
                    // Technical: Implements try-catch as per guidelines
                    _logger.LogError(ex, "Error retrieving WorkEffortAssoc records for WorkEffortId: {WorkEffortId}", request.WorkEffortId);
                    return Result<List<WorkEffortAssocDto>>.Failure($"Error retrieving routing task associations: {ex.Message}");
                }
            }
        }
    }
}