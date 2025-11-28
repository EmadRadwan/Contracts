using Application.Core;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Projects;

public class IssueMaterialsForCertificate
{
    public class Command : IRequest<Results<IssueMaterialsForCertificateResult>>
    {
        public string WorkEffortId { get; set; }
    }


    public class Handler : IRequestHandler<Command, Results<IssueMaterialsForCertificateResult>>
    {
        private readonly DataContext _context;
        private readonly ILogger<Handler> _logger;
        private readonly IProjectService _projectService;
        private readonly IMediator _mediator;


        public Handler(IProjectService projectService, DataContext context, ILogger<Handler> logger, IMediator mediator)
        {
            _projectService = projectService;
            _context = context;
            _logger = logger;
            _mediator = mediator;
        }

        public async Task<Results<IssueMaterialsForCertificateResult>> Handle(Command request,
            CancellationToken cancellationToken)
        {
            //  Added transaction management to ensure atomicity of issuance operations;
            // this prevents partial issuances in case of errors, improving data consistency for certificate material issuance.
            await using var transaction = _context.Database.CurrentTransaction == null
                ? await _context.Database.BeginTransactionAsync(cancellationToken)
                : null;

            var ownsTransaction = transaction != null;
            
            try
            {
                var result = await _projectService.IssueMaterialsForCertificate(request.WorkEffortId);

                if (!result.IsSuccess)
                {
                    if (ownsTransaction) await transaction!.RollbackAsync(cancellationToken);
                    return Results<IssueMaterialsForCertificateResult>.Failure(result.ErrorMessage, result.ErrorCode);
                }

                var workEffort = await _context.WorkEfforts
                    .FirstOrDefaultAsync(
                        we => we.WorkEffortId == request.WorkEffortId && we.WorkEffortTypeId == "PROJECT_CERTIFICATE",
                        cancellationToken);

                if (workEffort != null)
                {
                    workEffort.CurrentStatusId = "WEPR_APPROVED";
                    workEffort.LastStatusUpdate = DateTime.UtcNow; // Update timestamp for auditability
                    _context.WorkEfforts.Update(workEffort);
                    _logger.LogInformation("Updated WorkEffort {WorkEffortId} status to WEPR_APPROVED",
                        request.WorkEffortId);
                }
                else
                {
                    _logger.LogWarning("No PROJECT_CERTIFICATE found for WorkEffortId: {WorkEffortId}",
                        request.WorkEffortId);
                    // Note: Not failing the transaction as certificate update is secondary to material issuance
                }

                // REFACTOR: Moved SaveChanges after WorkEffort update to persist both material issuance and certificate status
                // within the same transaction; this ensures data consistency and avoids partial updates.
                var saveResult = await _context.SaveChangesAsync(cancellationToken);
                if (saveResult > 0)
                {
                    _logger.LogDebug("Persisted WorkEffort status update for WorkEffortId: {WorkEffortId}",
                        request.WorkEffortId);
                }

                if (ownsTransaction)
                    await transaction!.CommitAsync(cancellationToken);

                return Results<IssueMaterialsForCertificateResult>.Success(result.Value);
            }
            catch (Exception ex)
            {
                if (ownsTransaction && transaction != null)
                    await transaction.RollbackAsync(cancellationToken);

                _logger.LogError(ex, "Error issuing materials for certificate WorkEffortId: {WorkEffortId}",
                    request.WorkEffortId);
                return Results<IssueMaterialsForCertificateResult>.Failure(
                    ex.Message ?? "An unexpected error occurred while issuing materials for certificate.");
            }
        }
    }
}