#nullable enable
using Application.Core;
using FluentValidation;
using MediatR;
using Persistence;
using Domain;
using Application.Interfaces;
using System;

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
        private readonly IUtilityService _utilityService;

        public Handler(DataContext context, IUserAccessor userAccessor, IUtilityService utilityService)
        {
            _context = context;
            _userAccessor = userAccessor;
            _utilityService = utilityService;
        }

        public async Task<Result<ProjectDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var newProjectSerial = await _utilityService.GetNextSequence("WorkEffort");
                var stamp = DateTime.UtcNow;

                // REFACTOR: Added Facility creation with PROJECT_FACILITY type and linked it to the project
                // Purpose: Create a new Facility entity for the project with FacilityTypeId = 'PROJECT_FACILITY'
                // and set its FacilityId in the WorkEffort entity to establish the relationship
                var facilityId = Guid.NewGuid().ToString();
                var facility = new Facility
                {
                    FacilityId = facilityId,
                    FacilityTypeId = "PROJECT_FACILITY",
                    FacilityName = request.ProjectDto!.ProjectName,
                    OwnerPartyId = "Company",
                    CreatedStamp = stamp,
                    LastUpdatedStamp = stamp
                };
                _context.Facilities.Add(facility);

                // Create WorkEffort entity
                var project = new WorkEffort
                {
                    WorkEffortId = newProjectSerial,
                    ProjectName = request.ProjectDto!.ProjectName,
                    WorkEffortTypeId = "PROJECT",
                    CurrentStatusId = request.ProjectDto.CurrentStatusId,
                    GlAccountId = request.ProjectDto.GlAccountId,
                    EstimatedStartDate = request.ProjectDto.EstimatedStartDate,
                    EstimatedCompletionDate = request.ProjectDto.EstimatedCompletionDate,
                    CreatedDate = stamp,
                    LastUpdatedStamp = stamp,
                    FacilityId = facilityId
                };
                _context.WorkEfforts.Add(project);

                var result = await _context.SaveChangesAsync(cancellationToken) > 0;
                if (!result)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return Result<ProjectDto>.Failure("Failed to create project and facility");
                }

                await transaction.CommitAsync(cancellationToken);
                return Result<ProjectDto>.Success(request.ProjectDto!);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Result<ProjectDto>.Failure($"Failed to create project and facility: {ex.Message}");
            }
        }
    }
}