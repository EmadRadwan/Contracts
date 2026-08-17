#nullable enable
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Application.Projects;

public class UpdateProject
{
    public class Command : IRequest<Result<ProjectDto>>
    {
        public ProjectDto? ProjectDto { get; set; }
    }

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(x => x.ProjectDto!.ProjectNum).NotEmpty().WithMessage("Project Number is required");
            RuleFor(x => x.ProjectDto!.ProjectName).NotEmpty().WithMessage("Project Name is required");
        }
    }

    public class Handler : IRequestHandler<Command, Result<ProjectDto>>
    {
        private readonly DataContext _context;
        private readonly IUserAccessor _userAccessor;
        private readonly ILogger<Handler> _logger;

        public Handler(DataContext context, IUserAccessor userAccessor, ILogger<Handler> logger)
        {
            _context = context;
            _userAccessor = userAccessor;
            _logger = logger;
        }

        public async Task<Result<ProjectDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            var project = await _context.WorkEfforts
                .FirstOrDefaultAsync(x => x.WorkEffortId == request.ProjectDto.WorkEffortId, cancellationToken);
            if (project == null)
            {
                _logger.LogWarning($"Project with ID {request.ProjectDto.WorkEffortId} not found.");
                return Result<ProjectDto>.Failure($"Project with ID {request.ProjectDto.WorkEffortId} not found.");
            }
            

            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var stamp = DateTime.UtcNow;
                project.ProjectName = request.ProjectDto.ProjectName ?? project.ProjectName;
                project.GlAccountId = request.ProjectDto.GlAccountId ?? project.GlAccountId;
                project.OperatingExpenseGlAccountId = request.ProjectDto.OperatingExpenseGlAccountId ?? project.OperatingExpenseGlAccountId;
                project.IsCompanyProject = request.ProjectDto.IsCompanyProject ?? project.IsCompanyProject;
                project.CurrentStatusId = request.ProjectDto.CurrentStatusId ?? project.CurrentStatusId;
                project.EstimatedStartDate = request.ProjectDto.EstimatedStartDate ?? project.EstimatedStartDate;
                project.EstimatedCompletionDate = request.ProjectDto.EstimatedCompletionDate ?? project.EstimatedCompletionDate;
                project.LastUpdatedStamp = stamp;

                var result = await _context.SaveChangesAsync(cancellationToken) > 0;
                if (!result)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    _logger.LogWarning($"Failed to update project {project.WorkEffortId}.");
                    return Result<ProjectDto>.Failure("Failed to update project.");
                }

                await transaction.CommitAsync(cancellationToken);
                return Result<ProjectDto>.Success(request.ProjectDto);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, $"Failed to update project {request.ProjectDto.WorkEffortId}.");
                return Result<ProjectDto>.Failure($"Failed to update project: {ex.Message}");
            }
        }
    }
}