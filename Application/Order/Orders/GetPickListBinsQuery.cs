using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence; 

namespace Application.Order.Orders
{
    // Query definition with no parameters.
    public class GetPickListBinsQuery : IRequest<Result<List<PickListBinDto>>>
    {
    }

    public class GetPickListBinsQueryHandler : IRequestHandler<GetPickListBinsQuery, Result<List<PickListBinDto>>>
    {
        private readonly DataContext _context;
        private readonly ILogger<GetPickListBinsQueryHandler> _logger;

        public GetPickListBinsQueryHandler(DataContext context, ILogger<GetPickListBinsQueryHandler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Result<List<PickListBinDto>>> Handle(GetPickListBinsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var pickListBins = await _context.PicklistBins
                    .Select(bin => new PickListBinDto
                    {
                        PICKLIST_BIN_ID = bin.PicklistBinId,
                        PICKLIST_ID = bin.PicklistId,
                        PRIMARY_ORDER_ID = bin.PrimaryOrderId,
                    })
                    .ToListAsync(cancellationToken);

                return Result<List<PickListBinDto>>.Success(pickListBins);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pick list bins.");
                return Result<List<PickListBinDto>>.Failure($"Exception: {ex.Message}");
            }
        }
    }
}
