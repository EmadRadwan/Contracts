using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Manufacturing;

public class GetRoutingTasksLov
{
    // Parameters for filtering and pagination
    // Business: Allows filtering routing tasks by search term and pagination
    // Technical: Mirrors ProductLovParams for consistent query parameter handling
    public class RoutingTaskLovParams
    {
        public string? SearchTerm { get; set; }
        public int Skip { get; set; } = 0;
        public int PageSize { get; set; } = 10;
    }

    // DTO for routing task list of values
    // Business: Represents a WorkEffort (routing task) for dropdown selection
    // Technical: Matches WorkEffort entity fields relevant to the dropdown
    public class RoutingTaskLovDto
    {
        public string WorkEffortIdTo { get; set; }
        public string WorkEffortName { get; set; }
    }

    // Envelope for paginated response
    // Business: Wraps the list of routing tasks with total count for frontend
    // Technical: Mirrors FinishedProductsEnvelope for consistent API response
    public class RoutingTasksEnvelope
    {
        public List<RoutingTaskLovDto> RoutingTasks { get; set; }
        public int RoutingTaskCount { get; set; }
    }

    // Query to retrieve routing tasks
    public class Query : IRequest<Result<RoutingTasksEnvelope>>
    {
        public RoutingTaskLovParams? Params { get; set; }
    }

    // Handler for retrieving routing tasks
    public class Handler : IRequestHandler<Query, Result<RoutingTasksEnvelope>>
    {
        private readonly DataContext _context;
        private readonly ILogger<Handler> _logger;

        public Handler(DataContext context, ILogger<Handler> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Handles the query to retrieve routing tasks for association
        // Business: Fetches WorkEffort records of type ROUTING_TASK for dropdown selection
        // Technical: Translates OFBiz-style entity queries into a LINQ-based MediatR handler
        // REFACTOR: Optimized LINQ query for performance and type safety, replacing OFBiz entity queries
        public async Task<Result<RoutingTasksEnvelope>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Wrap logic in try-catch for error handling
            // Business: Ensures errors during data retrieval are captured
            // Technical: Implements try-catch for robustness, consistent with GetFinishedProductsLov
            try
            {
                // Initialize parameters
                // Business: Provides default values for pagination and filtering
                // Technical: Ensures null safety for optional parameters
                var taskParams = request.Params ?? new RoutingTaskLovParams();

                // Build LINQ query to fetch routing tasks
                // Business: Filters WorkEfforts by type ROUTING_TASK and optional search term
                // Technical: Joins with WorkEffortType to ensure type correctness
                // REFACTOR: Removed language-specific checks to simplify query and use WorkEffortName directly
                var query = from we in _context.WorkEfforts
                            join wet in _context.WorkEffortTypes on we.WorkEffortTypeId equals wet.WorkEffortTypeId
                            where wet.WorkEffortTypeId == "ROU_TASK"
                                  && (taskParams.SearchTerm == null || we.WorkEffortName.Contains(taskParams.SearchTerm))
                            select new RoutingTaskLovDto
                            {
                                WorkEffortIdTo = we.WorkEffortId,
                                WorkEffortName = we.WorkEffortName
                            };

                // Ensure distinct results
                // Business: Prevents duplicate tasks in the dropdown
                // Technical: Uses Distinct to align with GetFinishedProductsLov
                var distinctQuery = query.Distinct().AsQueryable();

                // Execute paginated query
                // Business: Retrieves a subset of tasks for dropdown display
                // Technical: Applies Skip and Take for pagination
                // REFACTOR: Optimized pagination with async LINQ operations
                var routingTasks = await distinctQuery
                    .Skip(taskParams.Skip)
                    .Take(taskParams.PageSize)
                    .ToListAsync(cancellationToken);

                // Get total count for pagination
                // Business: Provides total count for frontend pagination controls
                // Technical: Uses CountAsync for efficient counting
                var routingTaskCount = await distinctQuery.CountAsync(cancellationToken);

                // Create response envelope
                // Business: Wraps results for frontend consumption
                // Technical: Structures response like FinishedProductsEnvelope
                var envelope = new RoutingTasksEnvelope
                {
                    RoutingTasks = routingTasks,
                    RoutingTaskCount = routingTaskCount
                };

                // Return success result
                // Business: Provides the list of tasks for dropdown population
                // Technical: Returns Result.Success with the envelope
                return Result<RoutingTasksEnvelope>.Success(envelope);
            }
            catch (Exception ex)
            {
                // Log and handle exceptions
                // Business: Captures errors for debugging
                // Technical: Implements try-catch as per guidelines
                _logger.LogError(ex, "Error retrieving routing tasks");
                return Result<RoutingTasksEnvelope>.Failure($"Error retrieving routing tasks: {ex.Message}");
            }
        }
    }
}