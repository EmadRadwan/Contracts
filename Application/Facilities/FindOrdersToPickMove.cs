using Domain;
using MediatR;
using Microsoft.Extensions.Logging;
using Application.Interfaces;


namespace Application.Facilities;

public class FindOrdersToPickMove
{
    public class Query : IRequest<Result<List<PickMoveInfoGroupDto>>>
    {
        public string FacilityId { get; set; }
        public string? ShipmentMethodTypeId { get; set; }
        public string? IsRushOrder { get; set; }
        public long? MaxNumberOfOrders { get; set; }
        public List<OrderHeader>? OrderHeaderList { get; set; }
        public string? GroupByNoOfOrderItems { get; set; }
        public string? GroupByWarehouseArea { get; set; }
        public string? GroupByShippingMethod { get; set; }
        public string? OrderId { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<List<PickMoveInfoGroupDto>>>
    {
        private readonly ILogger<Handler> _logger;
        private readonly IPickListService _pickListService;

        public Handler(IPickListService pickListService, ILogger<Handler> logger)
        {
            _pickListService = pickListService;
            _logger = logger;
        }

        public async Task<Result<List<PickMoveInfoGroupDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _pickListService.FindOrdersToPickMove(
                    request.FacilityId,
                    request.ShipmentMethodTypeId,
                    request.IsRushOrder,
                    request.MaxNumberOfOrders,
                    request.OrderHeaderList,
                    request.GroupByNoOfOrderItems,
                    request.GroupByWarehouseArea,
                    request.GroupByShippingMethod,
                    request.OrderId);

                return Result<List<PickMoveInfoGroupDto>>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing FindOrdersToPickMove");
                return Result<List<PickMoveInfoGroupDto>>.Failure(
                    "An error occurred while finding orders to pick or move.");
            }
        }
    }
}