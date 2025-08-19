using Application.Facilities;
using MediatR;
using Microsoft.Extensions.Logging;
using Persistence;


namespace Application.Facilities;

/// <summary>
/// The command defining the input data for creating a picklist.
/// </summary>
public class CreatePicklistCommand : IRequest<Result<CreatePicklistFromOrdersResult>>
{
    /// <summary>
    /// The facility or warehouse ID (required).
    /// </summary>
    public string FacilityId { get; set; }

    /// <summary>
    /// Orders known to be ready for picking.
    /// </summary>
    public List<string> OrderReadyToPickInfoList { get; set; }

    /// <summary>
    /// Orders needing a stock move before picking.
    /// </summary>
    public List<string> OrderNeedsStockMoveInfoList { get; set; }
}

/// <summary>
/// The handler that processes CreatePicklistCommand by combining
/// the provided order IDs and calling a domain/service method.
/// </summary>
public class CreatePicklistCommandHandler : IRequestHandler<CreatePicklistCommand, Result<CreatePicklistFromOrdersResult>>
{
    private readonly IPickListService _pickListService;
    private readonly ILogger<CreatePicklistCommandHandler> _logger;
    private readonly DataContext _context;


    public CreatePicklistCommandHandler(IPickListService pickListService, ILogger<CreatePicklistCommandHandler> logger, DataContext context)
    {
        _pickListService = pickListService;
        _logger = logger;
        _context = context;
    }

    public async Task<Result<CreatePicklistFromOrdersResult>> Handle(CreatePicklistCommand request, CancellationToken cancellationToken)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            // 1) Validate facility ID
            if (string.IsNullOrWhiteSpace(request.FacilityId))
            {
                return Result<CreatePicklistFromOrdersResult>.Failure("FacilityId is required.");
            }

            // 2) Initialize the lists to avoid null reference
            var readyList = request.OrderReadyToPickInfoList ?? new List<string>();
            var needsMoveList = request.OrderNeedsStockMoveInfoList ?? new List<string>();

            // 3) Combine the two lists of order IDs
            // Decide if you truly want to pick stock-move orders or skip them:
            var combinedOrderIds = new HashSet<string>(readyList);
            foreach (var moveId in needsMoveList)
            {
                combinedOrderIds.Add(moveId);
            }

            if (combinedOrderIds.Count == 0)
            {
                return Result<CreatePicklistFromOrdersResult>.Failure("No order IDs found in the command.");
            }

            _logger.LogInformation("Combining {Count} order IDs from ready and stock-move lists.",
                combinedOrderIds.Count);

            // 4) Build the input for the domain service
            var createPicklistInput = new CreatePicklistFromOrdersInput
            {
                FacilityId = request.FacilityId,
                OrderIdList = combinedOrderIds.ToList()
            };

            // 5) Call the domain/service method to create the picklist
            var serviceResult = await _pickListService.CreatePicklistFromOrders(createPicklistInput);

            await _context.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            
            if (serviceResult.HasError)
            {
                return Result<CreatePicklistFromOrdersResult>.Failure(serviceResult.ErrorMessage);
            }

            // 6) Return success
            return Result<CreatePicklistFromOrdersResult>.Success(serviceResult);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);

            _logger.LogError(ex, "Error while creating picklist.");
            return Result<CreatePicklistFromOrdersResult>.Failure($"Exception: {ex.Message}");
        }
    }
}