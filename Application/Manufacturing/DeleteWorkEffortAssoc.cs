using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Manufacturing
{
    public class DeleteWorkEffortAssoc
    {
        // Command to delete a specific WorkEffortAssoc
        public class Command : IRequest<Result<Unit>>
        {
            public string WorkEffortIdFrom { get; set; } // Source WorkEffort ID
            public string WorkEffortIdTo { get; set; } // Target WorkEffort ID
            public string WorkEffortAssocTypeId { get; set; } // Type of association
        }

        // Handler for deleting a WorkEffortAssoc record
        public class Handler : IRequestHandler<Command, Result<Unit>>
        {
            private readonly DataContext _context;
            private readonly ILogger<Handler> _logger;

            public Handler(DataContext context, ILogger<Handler> logger)
            {
                _context = context;
                _logger = logger;
            }

            // Handles the command to delete a WorkEffortAssoc record
            // Business: Permanently removes the association from the database
            // Technical: Uses EF Core to perform a hard delete
            public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
            {
                try
                {
                    // Find the WorkEffortAssoc record
                    // Business: Locates the specific association using composite key
                    // Technical: Uses EF Core to query with composite key
                    // REFACTOR: Removed ThruDate check to support hard delete
                    var workEffortAssoc = await _context.WorkEffortAssocs
                        .FirstOrDefaultAsync(wea =>
                            wea.WorkEffortIdFrom == request.WorkEffortIdFrom &&
                            wea.WorkEffortIdTo == request.WorkEffortIdTo &&
                            wea.WorkEffortAssocTypeId == request.WorkEffortAssocTypeId,
                            cancellationToken);

                    // Check if record exists
                    // Business: Ensures we don't attempt to delete a non-existent record
                    // Technical: Aligns with previous null handling for consistency
                    // REFACTOR: Updated error message to reflect hard delete
                    if (workEffortAssoc == null)
                    {
                        _logger.LogWarning("No WorkEffortAssoc found for WorkEffortIdFrom: {WorkEffortIdFrom}, WorkEffortIdTo: {WorkEffortIdTo}, WorkEffortAssocTypeId: {WorkEffortAssocTypeId}",
                            request.WorkEffortIdFrom, request.WorkEffortIdTo, request.WorkEffortAssocTypeId);
                        return Result<Unit>.Failure("No routing task association found to delete");
                    }

                    // Remove the record
                    // Business: Permanently deletes the association
                    // Technical: Uses EF Core Remove for hard delete
                    // REFACTOR: Replaced soft delete (ThruDate update) with hard delete
                    _context.WorkEffortAssocs.Remove(workEffortAssoc);

                    // Save changes to the database
                    // Business: Persists the deletion
                    // Technical: Executes the delete operation asynchronously
                    await _context.SaveChangesAsync(cancellationToken);

                    // Return success
                    // Business: Confirms successful deletion to the caller
                    // Technical: Returns Unit to indicate successful command execution
                    return Result<Unit>.Success(Unit.Value);
                }
                catch (Exception ex)
                {
                    // Log and handle any exceptions
                    // Business: Captures errors during deletion for debugging
                    // Technical: Consistent error handling with previous implementation
                    _logger.LogError(ex, "Error deleting WorkEffortAssoc for WorkEffortIdFrom: {WorkEffortIdFrom}, WorkEffortIdTo: {WorkEffortIdTo}, WorkEffortAssocTypeId: {WorkEffortAssocTypeId}",
                        request.WorkEffortIdFrom, request.WorkEffortIdTo, request.WorkEffortAssocTypeId);
                    return Result<Unit>.Failure($"Error deleting routing task association: {ex.Message}");
                }
            }
        }
    }
}