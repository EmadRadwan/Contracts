using Application.Core;
using Application.Projects;
using MediatR;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.ProjectCertificates;

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

        public Handler(IProjectService projectService, DataContext context, ILogger<Handler> logger)
        {
            _projectService = projectService;
            _context = context;
            _logger = logger;
        }

        public async Task<Results<IssueMaterialsForCertificateResult>> Handle(Command request,
            CancellationToken cancellationToken)
        {
            // REFACTOR: Added transaction management to ensure atomicity of issuance operations;
            // this prevents partial issuances in case of errors, improving data consistency for certificate material issuance.
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var result = await _projectService.IssueMaterialsForCertificate(request.WorkEffortId);

                if (!result.IsSuccess)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return Results<IssueMaterialsForCertificateResult>.Failure(result.ErrorMessage, result.ErrorCode);
                }

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return Results<IssueMaterialsForCertificateResult>.Success(result.Value);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Error issuing materials for certificate WorkEffortId: {WorkEffortId}",
                    request.WorkEffortId);
                return Results<IssueMaterialsForCertificateResult>.Failure(
                    ex.Message ?? "An unexpected error occurred while issuing materials for certificate.");
            }
        }
    }
}