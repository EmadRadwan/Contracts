using MediatR;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Facilities;

/// <summary>
/// Query for getting picklist display info with pagination.
/// </summary>
public class GetPicklistDisplayInfoQuery : IRequest<Result<GetPicklistDisplayInfoResult>>
{
    /// <summary>
    /// Facility or warehouse ID (required)
    /// </summary>
    public string FacilityId { get; set; }

    /// <summary>
    /// Which page of results (optional). Defaults in service if null.
    /// </summary>
    public int? ViewIndex { get; set; }

    /// <summary>
    /// How many items per page (optional). Defaults in service if null.
    /// </summary>
    public int? ViewSize { get; set; }
}

/// <summary>
/// Handler that processes the GetPicklistDisplayInfoQuery by calling the domain/service method
/// which replicates the getPicklistDisplayInfo + getPicklistSingleInfoInline logic.
/// </summary>
public class
    GetPicklistDisplayInfoQueryHandler : IRequestHandler<GetPicklistDisplayInfoQuery,
        Result<GetPicklistDisplayInfoResult>>
{
    private readonly IPickListService _pickListService;
    private readonly ILogger<GetPicklistDisplayInfoQueryHandler> _logger;
    private readonly DataContext _context;

    public GetPicklistDisplayInfoQueryHandler(
        IPickListService pickListService,
        ILogger<GetPicklistDisplayInfoQueryHandler> logger,
        DataContext context)
    {
        _pickListService = pickListService;
        _logger = logger;
        _context = context;
    }

    public async Task<Result<GetPicklistDisplayInfoResult>> Handle(GetPicklistDisplayInfoQuery request,
        CancellationToken cancellationToken)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            // 1) Validate facility
            if (string.IsNullOrWhiteSpace(request.FacilityId))
            {
                return Result<GetPicklistDisplayInfoResult>.Failure("FacilityId is required.");
            }

            // 2) Call the domain/service method
            var displayInfo = await _pickListService.GetPicklistDisplayInfo(
                request.FacilityId,
                request.ViewIndex,
                request.ViewSize
            );

            // (Optional) If your logic modifies data or if you want consistent reads,
            // you can save changes. Usually for a read, there's no save step:
            // await _context.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            // 3) Return success
            return Result<GetPicklistDisplayInfoResult>.Success(displayInfo);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);

            _logger.LogError(ex, "Error retrieving picklist display info for facility {FacilityId}.",
                request.FacilityId);
            return Result<GetPicklistDisplayInfoResult>.Failure($"Exception: {ex.Message}");
        }
    }
}