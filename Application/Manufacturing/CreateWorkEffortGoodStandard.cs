using Application.Interfaces;
using AutoMapper;
using Domain;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Manufacturing;

// Create WorkEffortGoodStandard
public class CreateWorkEffortGoodStandard
{
    public class Command : IRequest<Result<Unit>>
    {
        public Command(string workEffortId, string productId, string workEffortGoodStdTypeId, DateTime? fromDate,
            decimal? estimatedQuantity = null)
        {
            WorkEffortId = workEffortId;
            ProductId = productId;
            WorkEffortGoodStdTypeId = workEffortGoodStdTypeId;
            FromDate = fromDate;
            EstimatedQuantity = estimatedQuantity;
        }

        public string WorkEffortId { get; set; }
        public string ProductId { get; set; }
        public string WorkEffortGoodStdTypeId { get; set; }
        public DateTime? FromDate { get; set; }
        public decimal? EstimatedQuantity { get; set; }
    }

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            // Validate required fields
            // Business: Ensures critical fields for WorkEffortGoodStandard are provided
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
                // Business: Ensures both WorkEffort and Product exist before creating the association
                // Technical: Uses LINQ to replace OFBiz's implicit entity validation
                // REFACTOR: Added explicit checks for WorkEffort and Product to ensure data integrity
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

                // Check for existing association
                // Business: Prevents duplicate associations for the same composite key
                // Technical: Queries for any record with matching composite key
                // REFACTOR: Added check for existing WorkEffortGoodStandard to prevent duplicates
                var existingAssoc = await _context.WorkEffortGoodStandards
                    .AnyAsync(
                        w => w.WorkEffortId == request.WorkEffortId && w.ProductId == request.ProductId &&
                             w.WorkEffortGoodStdTypeId == request.WorkEffortGoodStdTypeId, cancellationToken);
                if (existingAssoc)
                {
                    _logger.LogWarning(
                        "WorkEffortGoodStandard already exists for WorkEffortId: {WorkEffortId}, ProductId: {ProductId}, WorkEffortGoodStdTypeId: {WorkEffortGoodStdTypeId}",
                        request.WorkEffortId, request.ProductId, request.WorkEffortGoodStdTypeId);
                    return Result<Unit>.Failure("A WorkEffortGoodStandard with the same details already exists");
                }

                // Begin transaction
                // Business: Ensures atomicity of WorkEffortGoodStandard creation
                // Technical: Uses EF Core transaction for database consistency
                using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    // Create the WorkEffortGoodStandard entity
                    // Business: Links a WorkEffort to a Product in a manufacturing routing
                    // Technical: Translates OFBiz's createWorkEffortGoodStandard service
                    // REFACTOR: Structured entity creation to match OFBiz auto-attributes
                    var workEffortGoodStandard = new WorkEffortGoodStandard
                    {
                        WorkEffortId = request.WorkEffortId,
                        ProductId = request.ProductId,
                        WorkEffortGoodStdTypeId = request.WorkEffortGoodStdTypeId,
                        FromDate = request.FromDate ?? DateTime.UtcNow,
                        EstimatedQuantity = request.EstimatedQuantity as double?
                    };

                    // Add the entity to the DbContext
                    // Business: Prepares the WorkEffortGoodStandard for persistence
                    // Technical: Stages the record for insertion
                    _context.WorkEffortGoodStandards.Add(workEffortGoodStandard);

                    // Save changes
                    // Business: Persists the WorkEffortGoodStandard to the database
                    // Technical: Executes the database insert asynchronously
                    var result = await _context.SaveChangesAsync(cancellationToken) > 0;
                    if (!result)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        _logger.LogError("Failed to create WorkEffortGoodStandard");
                        return Result<Unit>.Failure("Failed to create WorkEffortGoodStandard");
                    }

                    // Commit transaction
                    // Business: Finalizes the creation of the WorkEffortGoodStandard
                    // Technical: Ensures the transaction is committed
                    await transaction.CommitAsync(cancellationToken);

                    // Log success
                    // Business: Confirms successful creation
                    // Technical: Adds logging for traceability
                    _logger.LogInformation(
                        "WorkEffortGoodStandard created successfully for WorkEffortId: {WorkEffortId}, ProductId: {ProductId}",
                        request.WorkEffortId, request.ProductId);

                    return Result<Unit>.Success(Unit.Value);
                }
                catch (Exception ex)
                {
                    // Roll back transaction on error
                    // Business: Ensures database consistency on failure
                    // Technical: Handles errors with async rollback
                    await transaction.RollbackAsync(cancellationToken);
                    _logger.LogError(ex, "Failed to create WorkEffortGoodStandard: {Message}", ex.Message);
                    return Result<Unit>.Failure($"Failed to create WorkEffortGoodStandard: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                // Log unhandled exceptions
                // Business: Captures errors for debugging
                // Technical: Ensures comprehensive error logging
                _logger.LogError(ex,
                    "Error in createWorkEffortGoodStandard for WorkEffortId: {WorkEffortId}, ProductId: {ProductId}",
                    request.WorkEffortId, request.ProductId);
                return Result<Unit>.Failure($"Error in createWorkEffortGoodStandard: {ex.Message}");
            }
        }
    }
}