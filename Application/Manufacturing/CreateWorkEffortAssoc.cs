using Application.Interfaces;
using AutoMapper;
using Domain;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Manufacturing
{
    public class CreateWorkEffortAssoc
    {
        public class WorkEffortAssocDto
        {
            public string WorkEffortIdFrom { get; set; } // Source WorkEffort ID (required)
            public string WorkEffortIdTo { get; set; } // Target WorkEffort ID (required)
            public string WorkEffortAssocTypeId { get; set; } // Type of association (required)
            public DateTime? FromDate { get; set; } // Start date of the association (optional)
            public int? SequenceNum { get; set; } // Sequence number for ordering (optional)
        }

        public class Command : IRequest<Result<Unit>>
        {
            public Command(string workEffortIdFrom, string workEffortIdTo, string workEffortAssocTypeId, DateTime? fromDate, int? sequenceNum = null)
            {
                WorkEffortIdFrom = workEffortIdFrom;
                WorkEffortIdTo = workEffortIdTo;
                WorkEffortAssocTypeId = workEffortAssocTypeId;
                FromDate = fromDate;
                SequenceNum = sequenceNum;
            }

            public string WorkEffortIdFrom { get; set; } // Source WorkEffort ID (required)
            public string WorkEffortIdTo { get; set; } // Target WorkEffort ID (required)
            public string WorkEffortAssocTypeId { get; set; } // Type of association (required)
            public DateTime? FromDate { get; set; } // Start date of the association (optional)
            public int? SequenceNum { get; set; } // Sequence number for ordering (optional)
        }

        // Validator for the command
        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                // Validate required fields
                // Business: Ensures critical fields for WorkEffortAssoc are provided
                // Technical: Translates OFBiz's <type-validate> for required fields
                RuleFor(x => x.WorkEffortAssocTypeId)
                    .NotEmpty()
                    .WithMessage("WorkEffortRequiredFieldMissingWorkEffortAssocTypeId");
                RuleFor(x => x.WorkEffortIdFrom)
                    .NotEmpty()
                    .WithMessage("WorkEffortRequiredFieldMissingWorkEffortIdFrom");
                RuleFor(x => x.WorkEffortIdTo)
                    .NotEmpty()
                    .WithMessage("WorkEffortRequiredFieldMissingWorkEffortIdTo");
            }
        }

        // Handler for creating a WorkEffortAssoc
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

            // Handles the command to create a WorkEffortAssoc
            // Business: Links two WorkEffort entities (e.g., tasks or routing tasks) in a project or manufacturing context
            // Technical: Translates OFBiz's createWorkEffortAssoc service into a MediatR handler
            public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
            {
                try
                {
                    // Verify existence of source and target WorkEfforts
                    // Business: Ensures both WorkEfforts exist before creating the association
                    // Technical: Uses LINQ to replace OFBiz's implicit entity validation
                    // REFACTOR: Replaced implicit OFBiz entity checks with explicit LINQ queries for EF compatibility
                    var sourceExists = await _context.WorkEfforts.AnyAsync(we => we.WorkEffortId == request.WorkEffortIdFrom, cancellationToken);
                    var targetExists = await _context.WorkEfforts.AnyAsync(we => we.WorkEffortId == request.WorkEffortIdTo, cancellationToken);
                    if (!sourceExists || !targetExists)
                    {
                        _logger.LogError("One or both WorkEffort IDs do not exist: {WorkEffortIdFrom}, {WorkEffortIdTo}", request.WorkEffortIdFrom, request.WorkEffortIdTo);
                        return Result<Unit>.Failure("Invalid WorkEffortIdFrom or WorkEffortIdTo");
                    }

                    // REFACTOR: Updated check for existing WorkEffortAssoc record to remove ThruDate
                    // Business: Prevents duplicate associations for the same composite key
                    // Technical: Queries for any record with matching composite key
                    var existingAssoc = await _context.WorkEffortAssocs
                        .AnyAsync(wea =>
                            wea.WorkEffortIdFrom == request.WorkEffortIdFrom &&
                            wea.WorkEffortIdTo == request.WorkEffortIdTo &&
                            wea.WorkEffortAssocTypeId == request.WorkEffortAssocTypeId,
                            cancellationToken);

                    if (existingAssoc)
                    {
                        _logger.LogWarning("WorkEffortAssoc already exists for WorkEffortIdFrom: {WorkEffortIdFrom}, WorkEffortIdTo: {WorkEffortIdTo}, WorkEffortAssocTypeId: {WorkEffortAssocTypeId}",
                            request.WorkEffortIdFrom, request.WorkEffortIdTo, request.WorkEffortAssocTypeId);
                        return Result<Unit>.Failure("A WorkEffortAssoc with the same details already exists");
                    }

                    // Begin transaction
                    // Business: Ensures atomicity of WorkEffortAssoc creation
                    // Technical: Uses EF Core transaction for database consistency
                    using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                    try
                    {
                        // Create the WorkEffortAssoc entity
                        // Business: Links two WorkEffort entities in the database
                        // Technical: Translates the MiniLang createWorkEffortAssoc method as an EF Add operation
                        // REFACTOR: Removed ThruDate from entity creation
                        var workEffortAssoc = new WorkEffortAssoc
                        {
                            WorkEffortIdFrom = request.WorkEffortIdFrom,
                            WorkEffortIdTo = request.WorkEffortIdTo,
                            WorkEffortAssocTypeId = request.WorkEffortAssocTypeId,
                            FromDate = request.FromDate ?? DateTime.UtcNow,
                            SequenceNum = request.SequenceNum
                        };

                        // Add the entity to the DbContext
                        // Business: Prepares the WorkEffortAssoc for persistence
                        // Technical: Uses EF Core to stage the record for insertion
                        _context.WorkEffortAssocs.Add(workEffortAssoc);

                        // Save changes
                        // Business: Persists the WorkEffortAssoc to the database
                        // Technical: Executes the database insert asynchronously
                        var result = await _context.SaveChangesAsync(cancellationToken) > 0;

                        if (!result)
                        {
                            await transaction.RollbackAsync(cancellationToken);
                            _logger.LogError("Failed to create WorkEffortAssoc");
                            return Result<Unit>.Failure("Failed to create WorkEffortAssoc");
                        }

                        // Commit transaction
                        // Business: Finalizes the creation of the WorkEffortAssoc
                        // Technical: Ensures the transaction is committed
                        await transaction.CommitAsync(cancellationToken);

                        // Log success message
                        // Business: Informs that the association was created successfully
                        // Technical: Adds logging to confirm successful creation
                        _logger.LogInformation(
                            "WorkEffortAssoc created successfully for {WorkEffortIdFrom} to {WorkEffortIdTo}",
                            request.WorkEffortIdFrom, request.WorkEffortIdTo);

                        // Return success result
                        // Business: Confirms successful creation without returning data
                        // Technical: Returns Unit to indicate completion
                        return Result<Unit>.Success(Unit.Value);
                    }
                    catch (Exception ex)
                    {
                        // Roll back transaction on error
                        // Business: Ensures database consistency on failure
                        // Technical: Translates OFBiz error handling with async rollback
                        await transaction.RollbackAsync(cancellationToken);
                        _logger.LogError(ex, "Failed to create WorkEffortAssoc: {Message}", ex.Message);
                        return Result<Unit>.Failure($"Failed to create WorkEffortAssoc: {ex.Message}");
                    }
                }
                catch (Exception ex)
                {
                    // Log any unhandled exceptions
                    // Business: Captures errors during association creation for debugging
                    // Technical: Implements try-catch as per guidelines
                    _logger.LogError(ex, "Error in createWorkEffortAssoc for WorkEffortIdFrom: {WorkEffortIdFrom}, WorkEffortIdTo: {WorkEffortIdTo}", request.WorkEffortIdFrom, request.WorkEffortIdTo);
                    return Result<Unit>.Failure($"Error in createWorkEffortAssoc: {ex.Message}");
                }
            }
        }
    }
}