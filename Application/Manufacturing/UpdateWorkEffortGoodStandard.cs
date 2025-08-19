using Application.Interfaces;
using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Manufacturing;

public class UpdateWorkEffortGoodStandard
{
    public class Command : IRequest<Result<Unit>>
    {
        public Command(string workEffortId, string productId, string workEffortGoodStdTypeId, DateTime? fromDate,
            DateTime? thruDate, decimal? estimatedQuantity)
        {
            WorkEffortId = workEffortId;
            ProductId = productId;
            WorkEffortGoodStdTypeId = workEffortGoodStdTypeId;
            FromDate = fromDate;
            ThruDate = thruDate;
            EstimatedQuantity = estimatedQuantity;
        }

        public string WorkEffortId { get; set; }
        public string ProductId { get; set; }
        public string WorkEffortGoodStdTypeId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ThruDate { get; set; }
        public decimal? EstimatedQuantity { get; set; }
    }

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            // Validate required fields
            // Business: Ensures critical fields for WorkEffortGoodStandard update are provided
            // Technical: Translates OFBiz's <type-validate> for required fields
            RuleFor(x => x.WorkEffortId)
                .NotEmpty()
                .WithMessage("WorkEffortRequiredFieldMissingWorkEffortId");
            RuleFor(x => x.ProductId)
                .NotEmpty()
                .WithMessage("WorkEffortRequiredFieldMissingProductId");
            RuleFor(x => x.WorkEffortGoodStdTypeId)
                .NotEmpty()
                .WithMessage("WorkEffortRequiredFieldMissingWorkEffortGoodStdTypeId");
            RuleFor(x => x.FromDate)
                .NotEmpty()
                .WithMessage("WorkEffortRequiredFieldMissingFromDate");
        }
    }

    public class Handler : IRequestHandler<Command, Result<Unit>>
    {
        private readonly DataContext _context;
        private readonly ILogger<Handler> _logger;
        private readonly IMapper _mapper;
        private readonly IUserAccessor _userAccessor;

        public Handler(DataContext context, IMapper mapper, IUserAccessor userAccessor, ILogger<Handler> logger)
        {
            _context = context;
            _mapper = mapper;
            _userAccessor = userAccessor;
            _logger = logger;
        }

        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            try
            {
                // Verify existence of WorkEffort and Product
                // Business: Ensures both WorkEffort and Product exist before updating
                // Technical: Uses LINQ for explicit validation
                // REFACTOR: Added explicit checks for WorkEffort and Product
                var workEffortExists =
                    await _context.WorkEfforts.AnyAsync(we => we.WorkEffortId == request.WorkEffortId,
                        cancellationToken);
                var productExists =
                    await _context.Products.AnyAsync(p => p.ProductId == request.ProductId, cancellationToken);
                if (!workEffortExists || !productExists)
                {
                    _logger.LogError("Invalid WorkEffortId or ProductId: {WorkEffortId}, {ProductId}",
                        request.WorkEffortId, request.ProductId);
                    return Result<Unit>.Failure("Invalid WorkEffortId or ProductId");
                }

                // Find the existing WorkEffortGoodStandard
                // Business: Ensures the association exists before updating
                // Technical: Queries using composite key
                // REFACTOR: Uses composite key to uniquely identify the record
                var workEffortGoodStandard = await _context.WorkEffortGoodStandards
                    .FirstOrDefaultAsync(w => w.WorkEffortId == request.WorkEffortId &&
                                              w.ProductId == request.ProductId &&
                                              w.WorkEffortGoodStdTypeId == request.WorkEffortGoodStdTypeId,
                        cancellationToken);
                if (workEffortGoodStandard == null)
                {
                    _logger.LogWarning(
                        "WorkEffortGoodStandard not found for WorkEffortId: {WorkEffortId}, ProductId: {ProductId}, WorkEffortGoodStdTypeId: {WorkEffortGoodStdTypeId}",
                        request.WorkEffortId, request.ProductId, request.WorkEffortGoodStdTypeId);
                    return Result<Unit>.Failure("WorkEffortGoodStandard not found");
                }

                // Begin transaction
                // Business: Ensures atomicity of update
                // Technical: Uses EF Core transaction
                using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    // Update the WorkEffortGoodStandard entity
                    // Business: Updates relevant fields
                    // Technical: Modifies entity properties
                    // REFACTOR: Structured update to match OFBiz auto-attributes
                    workEffortGoodStandard.ThruDate = request.ThruDate;
                    workEffortGoodStandard.EstimatedQuantity = (double?)request.EstimatedQuantity;

                    // Save changes
                    // Business: Persists the updated record
                    // Technical: Executes the update asynchronously
                    var result = await _context.SaveChangesAsync(cancellationToken) > 0;
                    if (!result)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        _logger.LogError("Failed to update WorkEffortGoodStandard");
                        return Result<Unit>.Failure("Failed to update WorkEffortGoodStandard");
                    }

                    // Commit transaction
                    // Business: Finalizes the update
                    // Technical: Ensures transaction is committed
                    await transaction.CommitAsync(cancellationToken);

                    // Log success
                    // Business: Confirms successful update
                    // Technical: Adds logging for traceability
                    _logger.LogInformation(
                        "WorkEffortGoodStandard updated successfully for WorkEffortId: {WorkEffortId}, ProductId: {ProductId}",
                        request.WorkEffortId, request.ProductId);

                    return Result<Unit>.Success(Unit.Value);
                }
                catch (Exception ex)
                {
                    // Roll back transaction on error
                    // Business: Ensures database consistency
                    // Technical: Handles errors with async rollback
                    await transaction.RollbackAsync(cancellationToken);
                    _logger.LogError(ex, "Failed to update WorkEffortGoodStandard: {Message}", ex.Message);
                    return Result<Unit>.Failure($"Failed to update WorkEffortGoodStandard: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                // Log unhandled exceptions
                // Business: Captures errors for debugging
                // Technical: Ensures comprehensive error logging
                _logger.LogError(ex,
                    "Error in updateWorkEffortGoodStandard for WorkEffortId: {WorkEffortId}, ProductId: {ProductId}",
                    request.WorkEffortId, request.ProductId);
                return Result<Unit>.Failure($"Error in updateWorkEffortGoodStandard: {ex.Message}");
            }
        }
    }
}