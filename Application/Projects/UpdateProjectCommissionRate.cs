#nullable enable
using Application.Core;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Projects;

public class UpdateProjectCommissionRate
{
    public class Command : IRequest<Result<ProjectCommissionRateDto>>
    {
        public ProjectCommissionRateDto? Dto { get; set; }
    }

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(x => x.Dto!.ProjectCommissionRateId).NotEmpty().WithMessage("ID is required");
            RuleFor(x => x.Dto!.ProjectId).NotEmpty().WithMessage("Project is required");
            RuleFor(x => x.Dto!.SaleTypeId).NotEmpty().WithMessage("Sale Type is required");
        }
    }

    public class Handler : IRequestHandler<Command, Result<ProjectCommissionRateDto>>
    {
        private readonly DataContext _context;
        private readonly ILogger<Handler> _logger;

        public Handler(DataContext context, ILogger<Handler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Result<ProjectCommissionRateDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            var rate = await _context.ProjectCommissionRates
                .FirstOrDefaultAsync(x => x.ProjectCommissionRateId == request.Dto!.ProjectCommissionRateId, cancellationToken);

            if (rate == null)
            {
                _logger.LogWarning("Commission rate {Id} not found", request.Dto!.ProjectCommissionRateId);
                return Result<ProjectCommissionRateDto>.Failure($"Commission rate {request.Dto!.ProjectCommissionRateId} not found");
            }

            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                rate.ProjectId = request.Dto!.ProjectId;
                rate.SaleTypeId = request.Dto.SaleTypeId;
                rate.SalesRepPercent = request.Dto.SalesRepPercent;
                rate.ManagerPercent = request.Dto.ManagerPercent;
                rate.ExternalCompanyPercent = request.Dto.ExternalCompanyPercent;
                rate.ExternalSalesRepPercent = request.Dto.ExternalSalesRepPercent;
                rate.ExternalManagerPercent = request.Dto.ExternalManagerPercent;
                rate.LastUpdatedStamp = DateTime.UtcNow;

                var result = await _context.SaveChangesAsync(cancellationToken) > 0;
                if (!result)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return Result<ProjectCommissionRateDto>.Failure("Failed to update commission rate");
                }

                await transaction.CommitAsync(cancellationToken);
                return Result<ProjectCommissionRateDto>.Success(request.Dto);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Failed to update commission rate {Id}", request.Dto!.ProjectCommissionRateId);
                return Result<ProjectCommissionRateDto>.Failure($"Failed to update commission rate: {ex.Message}");
            }
        }
    }
}
