using Application.Interfaces;
using Application.Order.Orders;
using Application.Shipments;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Facilities;

public class ReceiveInventoryProducts
{
    public class Command : IRequest<Result<ReceiveInventoryResult>>
    {
        public ReceiveInventoryItemsDto ReceivedItems { get; set; }
    }

    public class Handler : IRequestHandler<Command, Result<ReceiveInventoryResult>>
    {
        private readonly DataContext _context;
        private readonly ILogger _logger;
        private readonly IShipmentService _shipmentService;

        public Handler(DataContext context,
            IShipmentService shipmentService, ILogger<Handler> logger)
        {
            _context = context;
            _logger = logger;
            _shipmentService = shipmentService;
        }

        public async Task<Result<ReceiveInventoryResult>> Handle(Command request, CancellationToken cancellationToken)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var results = new ReceiveInventoryResult();

                foreach (var item in request.ReceivedItems.OrderItems)
                {
                    await _shipmentService.ReceiveInventoryProduct("NON_SERIAL_INV_ITEM",
                        (decimal)item.QuantityAccepted,
                        (decimal)item.QuantityRejected,
                        null,
                        null,
                        null,
                        item.OrderId,
                        item.OrderItemSeqId,
                        item.ProductId,
                        item.FacilityId,
                        null,
                        null,
                        item.Color
                    );
                }

                var result = await _context.SaveChangesAsync(cancellationToken) > 0;


                await transaction.CommitAsync(cancellationToken);


                var receiveInventoryResult = new ReceiveInventoryResult()
                    .SetSuccess(true)
                    .SetMessage("Operation completed successfully.")
                    .SetData(null);

                return Result<ReceiveInventoryResult>.Success(receiveInventoryResult);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);

                _logger.LogError(ex, "An error occurred while receiving products. Stack Trace: {StackTrace}",
                    ex.StackTrace);
                return Result<ReceiveInventoryResult>.Failure("An error occurred while receiving products.");
            }
        }
    }
}