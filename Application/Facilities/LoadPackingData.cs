using MediatR;
using Microsoft.Extensions.Logging;
using Persistence; // or wherever your DbContext is
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Facilities;
public class LoadPackingDataCommand : IRequest<Result<LoadPackingDataResult>>
{
    // Mandatory
    public string FacilityId { get; set; }

    // Optional
    public string ShipmentId { get; set; }
    public string OrderId { get; set; }
    public string ShipGroupSeqId { get; set; }
    public string PicklistBinId { get; set; }
}
/// <summary>
/// Handler that processes LoadPackingDataCommand by calling the domain logic
/// which replicates the original Groovy script (without sessions),
/// and then manages DB transactions.
/// </summary>
public class LoadPackingDataCommandHandler : IRequestHandler<LoadPackingDataCommand, Result<LoadPackingDataResult>>
{
    private readonly IPickListService _pickListService;
    private readonly ILogger<LoadPackingDataCommandHandler> _logger;
    private readonly DataContext _context;

    public LoadPackingDataCommandHandler(
        IPickListService pickListService,
        ILogger<LoadPackingDataCommandHandler> logger,
        DataContext context)
    {
        _pickListService = pickListService;
        _logger = logger;
        _context = context;
    }

    public async Task<Result<LoadPackingDataResult>> Handle(LoadPackingDataCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // 1) Convert the command to your domain/service input
            var input = new LoadPackingDataInput
            {
                FacilityId = request.FacilityId,
                ShipmentId = request.ShipmentId,
                OrderId = request.OrderId,
                ShipGroupSeqId = request.ShipGroupSeqId,
                PicklistBinId = request.PicklistBinId
            };

            // 2) Call domain/service logic
            var resultDto = await _pickListService.LoadPackingData(input);

           
            // 3) Return success
            return Result<LoadPackingDataResult>.Success(resultDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading packing data for facility {FacilityId}", request.FacilityId);
            return Result<LoadPackingDataResult>.Failure($"Exception: {ex.Message}");
        }
    }
}
