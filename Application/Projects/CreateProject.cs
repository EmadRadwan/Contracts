#nullable enable
using FluentValidation;
using MediatR;
using Persistence;
using Domain;
using Application.Interfaces;

namespace Application.Projects;

public class CreateProject
{
    public class Command : IRequest<Result<ProjectDto>>
    {
        public ProjectDto? ProjectDto { get; set; }
    }

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(x => x.ProjectDto!.ProjectName).NotEmpty().WithMessage("Project Name is required");
        }
    }

    public class Handler : IRequestHandler<Command, Result<ProjectDto>>
    {
        private readonly DataContext _context;
        private readonly IUserAccessor _userAccessor;

        public Handler(DataContext context, IUserAccessor userAccessor)
        {
            _context = context;
            _userAccessor = userAccessor;
        }

        public async Task<Result<ProjectDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            // REFACTOR: Use transaction for atomicity, mirroring CreateProduct's approach.
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var stamp = DateTime.UtcNow;
                // Create WorkEffort entity
                var project = new WorkEffort
                {
                    WorkEffortId = request.ProjectDto!.WorkEffortId,
                    ProjectNum = request.ProjectDto.ProjectNum,
                    ProjectName = request.ProjectDto.ProjectName,
                    PartyId = request.ProjectDto.PartyId,
                    WorkEffortTypeId = "PROJECT",
                    CurrentStatusId = request.ProjectDto.CurrentStatusId,
                    EstimatedStartDate = request.ProjectDto.EstimatedStartDate,
                    EstimatedCompletionDate = request.ProjectDto.EstimatedCompletionDate,
                    CreatedByUserLogin = _userAccessor.GetUsername(),
                    CreatedDate = stamp,
                    LastModifiedByUserLogin = _userAccessor.GetUsername(),
                    LastModifiedDate = stamp,
                    LastUpdatedStamp = stamp
                };

                _context.WorkEfforts.Add(project);
                var result = await _context.SaveChangesAsync(cancellationToken) > 0;
                if (!result)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return Result<ProjectDto>.Failure("Failed to create project");
                }
                

                await transaction.CommitAsync(cancellationToken);
                return Result<ProjectDto>.Success(request.ProjectDto!);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Result<ProjectDto>.Failure($"Failed to create project: {ex.Message}");
            }
        }
    }
}