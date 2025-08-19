using Application.order.Orders;
using Application.Shipments;
using MediatR;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Order.Orders;

public class QuickReceivePurchaseOrder
{
    public class Query : IRequest<Result<Application.Shipments.OperationResult>>
    {
        public ReceiveInventoryRequestDto ReceiveInventoryRequestDto { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<Application.Shipments.OperationResult>>
    {
        private readonly ILogger<Handler> _logger;
        private readonly IShipmentService _shipmentService;
        private readonly DataContext _context;


        public Handler(DataContext context, ILogger<Handler> logger, IShipmentService shipmentService)
        {
            _logger = logger;
            _context = context;
            _shipmentService = shipmentService;
        }

        public async Task<Result<Application.Shipments.OperationResult>> Handle(Query request,
            CancellationToken cancellationToken)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            
            try
            {
                // 1. Extract Parameters
                string? facilityId = request.ReceiveInventoryRequestDto.FacilityId;
                string purchaseOrderId = request.ReceiveInventoryRequestDto.PurchaseOrderId;

                var responseDto = await _shipmentService.QuickReceivePurchaseOrder(purchaseOrderId, facilityId);
                
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                
                // Return Success Result
                return Result<Application.Shipments.OperationResult>.Success(responseDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing QuickReceivePurchaseOrder.");
                await transaction.RollbackAsync(cancellationToken);
                return Result<Application.Shipments.OperationResult>.Failure("An unexpected error occurred.");
            }
        }
    }
}