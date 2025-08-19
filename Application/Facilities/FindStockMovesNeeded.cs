using MediatR;
using Microsoft.Extensions.Logging;
using Persistence;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Facilities;

public class FindStockMovesNeededCommand : IRequest<Result<FindStockMovesNeededResult>>
{
    /// <summary>
    /// Facility or warehouse ID (required).
    /// </summary>
    public string FacilityId { get; set; }
}

/// <summary>
/// Handler that processes findStockMovesNeeded logic using the domain/service method
/// replicating the OFBiz minilang steps.
/// </summary>
public class FindStockMovesNeededCommandHandler 
    : IRequestHandler<FindStockMovesNeededCommand, Result<FindStockMovesNeededResult>>
{
    private readonly IPickListService _pickListService;
    private readonly ILogger<FindStockMovesNeededCommandHandler> _logger;
    private readonly DataContext _context;

    public FindStockMovesNeededCommandHandler(
        IPickListService PickListService,
        ILogger<FindStockMovesNeededCommandHandler> logger,
        DataContext context)
    {
        _logger = logger;
        _pickListService = PickListService;
        _context = context;
    }

    public async Task<Result<FindStockMovesNeededResult>> Handle(
        FindStockMovesNeededCommand request,
        CancellationToken cancellationToken)
    {
        // Begin transaction (consistent with your pattern)
        await using var transaction = await _context.Database
            .BeginTransactionAsync(cancellationToken);

        try
        {
            // 1) Validate facilityId
            if (string.IsNullOrWhiteSpace(request.FacilityId))
            {
                return Result<FindStockMovesNeededResult>
                    .Failure("FacilityId is required.");
            }

            // 2) Call the domain/service method which implements the OFBiz logic 
            // inlined with EF queries
            var serviceResult = await _pickListService
                .FindStockMovesNeeded(request.FacilityId);

            // If your logic changes data, 
            // you might do: await _context.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            // Return success
            return Result<FindStockMovesNeededResult>.Success(serviceResult);
        }
        catch (Exception ex)
        {
            // Roll back on error
            await transaction.RollbackAsync(cancellationToken);

            _logger.LogError(ex, 
                "Error while finding stock moves needed for facility {FacilityId}", 
                request.FacilityId);

            return Result<FindStockMovesNeededResult>.Failure(
                $"Exception: {ex.Message}");
        }
    }
}
