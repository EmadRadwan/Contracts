using Application.Interfaces;
using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Manufacturing
{
  public class UpdateWorkEffortAssoc
  {
    public class WorkEffortAssocDto
    {
      public string WorkEffortIdFrom { get; set; }
      public string WorkEffortIdTo { get; set; }
      public string WorkEffortAssocTypeId { get; set; }
      public DateTime? FromDate { get; set; }
      public DateTime? ThruDate { get; set; }
      public int? SequenceNum { get; set; }
    }

    public class Command : IRequest<Result<Unit>>
    {
      public Command(string workEffortIdFrom, string workEffortIdTo, string workEffortAssocTypeId, DateTime? fromDate, int? sequenceNum, DateTime? thruDate)
      {
        WorkEffortIdFrom = workEffortIdFrom;
        WorkEffortIdTo = workEffortIdTo;
        WorkEffortAssocTypeId = workEffortAssocTypeId;
        FromDate = fromDate;
        SequenceNum = sequenceNum;
        ThruDate = thruDate;
      }

      public string WorkEffortIdFrom { get; set; }
      public string WorkEffortIdTo { get; set; }
      public string WorkEffortAssocTypeId { get; set; }
      public DateTime? FromDate { get; set; }
      public int? SequenceNum { get; set; }
      public DateTime? ThruDate { get; set; }
    }

    // Validator for the command
    public class CommandValidator : AbstractValidator<Command>
    {
      public CommandValidator()
      {
        // Validate required fields
        // Business: Ensures critical fields for WorkEffortAssoc update are provided
        // Technical: Consistent with CreateWorkEffortAssoc validation
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

    // Handler for updating a WorkEffortAssoc
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
          // Verify existence of source and target WorkEfforts
          // Business: Ensures both WorkEfforts exist before updating the association
          // Technical: Uses LINQ for explicit validation
          var sourceExists = await _context.WorkEfforts.AnyAsync(we => we.WorkEffortId == request.WorkEffortIdFrom, cancellationToken);
          var targetExists = await _context.WorkEfforts.AnyAsync(we => we.WorkEffortId == request.WorkEffortIdTo, cancellationToken);
          if (!sourceExists || !targetExists)
          {
            _logger.LogError("One or both WorkEffort IDs do not exist: {WorkEffortIdFrom}, {WorkEffortIdTo}", request.WorkEffortIdFrom, request.WorkEffortIdTo);
            return Result<Unit>.Failure("Invalid WorkEffortIdFrom or WorkEffortIdTo");
          }

          // Find the existing WorkEffortAssoc
          // Business: Ensures the association exists before updating
          // Technical: Queries using composite key
          // REFACTOR: Uses composite key to uniquely identify the WorkEffortAssoc
          var workEffortAssoc = await _context.WorkEffortAssocs
            .FirstOrDefaultAsync(
              wea => wea.WorkEffortIdFrom == request.WorkEffortIdFrom &&
                     wea.WorkEffortIdTo == request.WorkEffortIdTo &&
                     wea.WorkEffortAssocTypeId == request.WorkEffortAssocTypeId,
              cancellationToken
            );

          if (workEffortAssoc == null)
          {
            _logger.LogWarning("WorkEffortAssoc not found for WorkEffortIdFrom: {WorkEffortIdFrom}, WorkEffortIdTo: {WorkEffortIdTo}, WorkEffortAssocTypeId: {WorkEffortAssocTypeId}",
              request.WorkEffortIdFrom, request.WorkEffortIdTo, request.WorkEffortAssocTypeId);
            return Result<Unit>.Failure("WorkEffortAssoc not found");
          }

          // Begin transaction
          // Business: Ensures atomicity of WorkEffortAssoc update
          // Technical: Uses EF Core transaction for database consistency
          using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
          try
          {
            // Update the WorkEffortAssoc entity
            // Business: Updates the relevant fields of the association
            // Technical: Modifies entity properties and saves changes
            workEffortAssoc.FromDate = request.FromDate ?? DateTime.UtcNow;
            workEffortAssoc.SequenceNum = request.SequenceNum;
            workEffortAssoc.ThruDate = request.ThruDate;

            // Save changes
            // Business: Persists the updated WorkEffortAssoc to the database
            // Technical: Executes the database update asynchronously
            var result = await _context.SaveChangesAsync(cancellationToken) > 0;
            if (!result)
            {
              await transaction.RollbackAsync(cancellationToken);
              _logger.LogError("Failed to update WorkEffortAssoc");
              return Result<Unit>.Failure("Failed to update WorkEffortAssoc");
            }

            // Commit transaction
            // Business: Finalizes the update of the WorkEffortAssoc
            // Technical: Ensures the transaction is committed
            await transaction.CommitAsync(cancellationToken);

            // Log success message
            // Business: Informs that the association was updated successfully
            // Technical: Adds logging to confirm successful update
            _logger.LogInformation(
              "WorkEffortAssoc updated successfully for {WorkEffortIdFrom} to {WorkEffortIdTo}",
              request.WorkEffortIdFrom, request.WorkEffortIdTo);

            // Return success result
            // Business: Confirms successful update without returning data
            // Technical: Returns Unit to indicate completion
            return Result<Unit>.Success(Unit.Value);
          }
          catch (Exception ex)
          {
            // Roll back transaction on error
            // Business: Ensures database consistency on failure
            // Technical: Translates error handling with async rollback
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Failed to update WorkEffortAssoc: {Message}", ex.Message);
            return Result<Unit>.Failure($"Failed to update WorkEffortAssoc: {ex.Message}");
          }
        }
        catch (Exception ex)
        {
          // Log any unhandled exceptions
          // Business: Captures errors during association update for debugging
          // Technical: Implements try-catch as per guidelines
          _logger.LogError(ex, "Error in updateWorkEffortAssoc for WorkEffortIdFrom: {WorkEffortIdFrom}, WorkEffortIdTo: {WorkEffortIdTo}",
            request.WorkEffortIdFrom, request.WorkEffortIdTo);
          return Result<Unit>.Failure($"Error in updateWorkEffortAssoc: {ex.Message}");
        }
      }
    }
  }
}