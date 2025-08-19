using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Facilities
{
    // Command definition that encapsulates the facilityId parameter.
    public class GetValidOrderPicklistBinsCommand : IRequest<Result<List<OrderPicklistBinDto>>>
    {
        public string FacilityId { get; set; }
    }

    /// <summary>
    /// Handler that processes GetValidOrderPicklistBinsCommand by calling
    /// IPickListService.GetValidOrderPicklistBins and returning the result.
    /// </summary>
    public class GetValidOrderPicklistBinsCommandHandler : IRequestHandler<GetValidOrderPicklistBinsCommand, Result<List<OrderPicklistBinDto>>>
    {
        private readonly IPickListService _pickListService;
        private readonly ILogger<GetValidOrderPicklistBinsCommandHandler> _logger;

        public GetValidOrderPicklistBinsCommandHandler(
            IPickListService pickListService,
            ILogger<GetValidOrderPicklistBinsCommandHandler> logger)
        {
            _pickListService = pickListService;
            _logger = logger;
        }

        public async Task<Result<List<OrderPicklistBinDto>>> Handle(GetValidOrderPicklistBinsCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Validate facilityId (the service method also validates it, but this is an extra safeguard).
                if (string.IsNullOrEmpty(request.FacilityId))
                {
                    throw new ArgumentException("facilityId is required.", nameof(request.FacilityId));
                }

                // Call the service method that queries the valid picklist bins.
                var bins = await _pickListService.GetValidOrderPicklistBins(request.FacilityId);

                // Return the bins wrapped in a successful Result.
                return Result<List<OrderPicklistBinDto>>.Success(bins);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving valid picklist bins for facility {FacilityId}", request.FacilityId);
                return Result<List<OrderPicklistBinDto>>.Failure($"Exception: {ex.Message}");
            }
        }
    }
}
