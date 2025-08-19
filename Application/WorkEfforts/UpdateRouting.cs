using Domain;
using MediatR;
using Persistence;
using Serilog;

namespace Application.WorkEfforts;

public class UpdateRoutingDto
{
    public string WorkEffortId { get; set; }
    public string WorkEffortTypeId { get; set; }
    public string WorkEffortName { get; set; }
    public string? Description { get; set; }
    public decimal? QuantityToProduce { get; set; }
    public string? CurrentStatusId { get; set; }
}

public class UpdateRouting
{
    public class Command : IRequest<Result<string>>
    {
        public UpdateRoutingDto UpdateRoutingDto { get; set; }
    }

    // REFACTOR: Implement handler to process UpdateRouting command, using new UpdateWorkEffort signature
    public class Handler : IRequestHandler<Command, Result<string>>
    {
        private readonly IWorkEffortService _workEfforts;
        private readonly DataContext _context;
        private readonly Serilog.ILogger _loggerForTransaction;

        // REFACTOR: Inject IWorkEffortService and DataContext, initialize Serilog logger
        public Handler(IWorkEffortService workEfforts, DataContext context)
        {
            _workEfforts = workEfforts;
            _context = context;
            _loggerForTransaction = Log.ForContext("Transaction", "update work effort");
        }

        public async Task<Result<string>> Handle(Command request, CancellationToken cancellationToken)
        {
            // REFACTOR: Use transaction to ensure atomicity, consistent with CreateRouting
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                _loggerForTransaction.Information("UpdateRouting.cs starting");

                // REFACTOR: Construct dictionary from DTO, aligning with EditRouting.tsx and UpdateWorkEffort
                var parameters = new Dictionary<string, object>();
                if (request.UpdateRoutingDto.WorkEffortName != null)
                    parameters["WorkEffortName"] = request.UpdateRoutingDto.WorkEffortName;
                if (request.UpdateRoutingDto.Description != null)
                    parameters["Description"] = request.UpdateRoutingDto.Description;
                if (request.UpdateRoutingDto.QuantityToProduce.HasValue)
                    parameters["QuantityToProduce"] = request.UpdateRoutingDto.QuantityToProduce.Value;
                if (request.UpdateRoutingDto.CurrentStatusId != null)
                    parameters["CurrentStatusId"] = request.UpdateRoutingDto.CurrentStatusId;

                // REFACTOR: Call UpdateWorkEffort with workEffortId and parameters
                var workEffort = await _workEfforts.UpdateWorkEffort(
                    request.UpdateRoutingDto.WorkEffortId,
                    parameters
                );

                // REFACTOR: Save changes to persist entities, ensuring transaction integrity
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                _loggerForTransaction.Information("UpdateRouting.cs end - Success");
                return Result<string>.Success(workEffort.WorkEffortId);
            }
            catch (Exception ex)
            {
                // REFACTOR: Roll back transaction and log error, mirroring CreateRouting style
                await transaction.RollbackAsync(cancellationToken);
                _loggerForTransaction.Information("UpdateRouting.cs error: {Error}", ex.Message);
                return Result<string>.Failure($"Error updating work effort: {ex.Message}");
            }
        }
    }
}