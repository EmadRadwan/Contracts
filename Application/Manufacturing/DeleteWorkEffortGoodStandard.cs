using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Manufacturing;

public class DeleteWorkEffortGoodStandard
{
    public class Command : IRequest<Result<Unit>>
    {
        public string WorkEffortId { get; set; }
        public string ProductId { get; set; }
        public string WorkEffortGoodStdTypeId { get; set; }
        public DateTime FromDate { get; set; }
    }

    public class Handler : IRequestHandler<Command, Result<Unit>>
    {
        private readonly DataContext _context;
        private readonly ILogger<Handler> _logger;

        public Handler(DataContext context, ILogger<Handler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            try
            {
                // Find the WorkEffortGoodStandard record
                // Business: Locates the specific association using composite key
                // Technical: Queries with composite key
                // REFACTOR: Uses composite key including FromDate for precise deletion
                var workEffortGoodStandard = await _context.WorkEffortGoodStandards
                    .FirstOrDefaultAsync(w => w.WorkEffortId == request.WorkEffortId &&
                                              w.ProductId == request.ProductId &&
                                              w.WorkEffortGoodStdTypeId == request.WorkEffortGoodStdTypeId &&
                                              w.FromDate == request.FromDate,
                        cancellationToken);
                if (workEffortGoodStandard == null)
                {
                    _logger.LogWarning(
                        "No WorkEffortGoodStandard found for WorkEffortId: {WorkEffortId}, ProductId: {ProductId}, WorkEffortGoodStdTypeId: {WorkEffortGoodStdTypeId}, FromDate: {FromDate}",
                        request.WorkEffortId, request.ProductId, request.WorkEffortGoodStdTypeId, request.FromDate);
                    return Result<Unit>.Failure("No WorkEffortGoodStandard found to delete");
                }

                // Remove the record
                // Business: Permanently deletes the association
                // Technical: Uses EF Core Remove for hard delete
                // REFACTOR: Implemented hard delete as per OFBiz service
                _context.WorkEffortGoodStandards.Remove(workEffortGoodStandard);

                // Save changes
                // Business: Persists the deletion
                // Technical: Executes the delete operation
                await _context.SaveChangesAsync(cancellationToken);

                // Log success
                // Business: Confirms successful deletion
                // Technical: Adds logging for traceability
                _logger.LogInformation(
                    "WorkEffortGoodStandard deleted successfully for WorkEffortId: {WorkEffortId}, ProductId: {ProductId}",
                    request.WorkEffortId, request.ProductId);

                return Result<Unit>.Success(Unit.Value);
            }
            catch (Exception ex)
            {
                // Log and handle exceptions
                // Business: Captures errors during deletion
                // Technical: Consistent error handling
                _logger.LogError(ex,
                    "Error deleting WorkEffortGoodStandard for WorkEffortId: {WorkEffortId}, ProductId: {ProductId}, WorkEffortGoodStdTypeId: {WorkEffortGoodStdTypeId}",
                    request.WorkEffortId, request.ProductId, request.WorkEffortGoodStdTypeId);
                return Result<Unit>.Failure($"Error deleting WorkEffortGoodStandard: {ex.Message}");
            }
        }
    }
}