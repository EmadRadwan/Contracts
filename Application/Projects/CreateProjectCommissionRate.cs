#nullable enable
using Application.Core;
using Application.Interfaces;
using Domain;
using FluentValidation;
using MediatR;
using Persistence;

namespace Application.Projects;

public class CreateProjectCommissionRate
{
    public class Command : IRequest<Result<ProjectCommissionRateDto>>
    {
        public ProjectCommissionRateDto? Dto { get; set; }
    }

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(x => x.Dto!.ProjectId).NotEmpty().WithMessage("Project is required");
            RuleFor(x => x.Dto!.SaleTypeId).NotEmpty().WithMessage("Sale Type is required");
        }
    }

    public class Handler : IRequestHandler<Command, Result<ProjectCommissionRateDto>>
    {
        private readonly DataContext _context;
        private readonly IUtilityService _utilityService;

        public Handler(DataContext context, IUtilityService utilityService)
        {
            _context = context;
            _utilityService = utilityService;
        }

        public async Task<Result<ProjectCommissionRateDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var id = await _utilityService.GetNextSequence("ProjectCommissionRate");
                var stamp = DateTime.UtcNow;

                var rate = new ProjectCommissionRate
                {
                    ProjectCommissionRateId = id,
                    ProjectId = request.Dto!.ProjectId,
                    SaleTypeId = request.Dto.SaleTypeId,
                    SalesRepPercent = request.Dto.SalesRepPercent,
                    ManagerPercent = request.Dto.ManagerPercent,
                    ExternalCompanyPercent = request.Dto.ExternalCompanyPercent,
                    ExternalSalesRepPercent = request.Dto.ExternalSalesRepPercent,
                    ExternalManagerPercent = request.Dto.ExternalManagerPercent,
                    CreatedStamp = stamp,
                    LastUpdatedStamp = stamp
                };

                _context.ProjectCommissionRates.Add(rate);

                var result = await _context.SaveChangesAsync(cancellationToken) > 0;
                if (!result)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return Result<ProjectCommissionRateDto>.Failure("Failed to create commission rate");
                }

                await transaction.CommitAsync(cancellationToken);
                request.Dto.ProjectCommissionRateId = id;
                return Result<ProjectCommissionRateDto>.Success(request.Dto);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Result<ProjectCommissionRateDto>.Failure($"Failed to create commission rate: {ex.Message}");
            }
        }
    }
}
