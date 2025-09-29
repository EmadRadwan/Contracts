using Application.Interfaces;
using Application.Order.Orders;
using Application.Shipments;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Persistence;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Facilities
{
    public class ReceiveInventoryProducts
    {
        public class Command : IRequest<Result<ReceiveInventoryResult>>
        {
            public ReceiveInventoryItemsDto ReceivedItems { get; set; }
        }

        public class Handler : IRequestHandler<Command, Result<ReceiveInventoryResult>>
        {
            private readonly DataContext _context;
            private readonly ILogger<Handler> _logger;
            private readonly IShipmentService _shipmentService;

            public Handler(DataContext context, IShipmentService shipmentService, ILogger<Handler> logger)
            {
                _context = context;
                _logger = logger;
                _shipmentService = shipmentService;
            }

            public async Task<Result<ReceiveInventoryResult>> Handle(Command request, CancellationToken cancellationToken)
            {
                // REFACTOR: Implemented conditional transaction management to detect and reuse existing outer transactions;
                // this prevents InvalidOperationException from nested BeginTransactionAsync calls when invoked via MediatR within another handler's transaction.
                // Improves code by supporting both standalone and nested execution, ensuring atomicity without conflicts.
                IDbContextTransaction? transaction = null;
                bool ownsTransaction = false;
                try
                {
                    if (_context.Database.CurrentTransaction == null)
                    {
                        transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                        ownsTransaction = true;
                        _logger.LogDebug("Started new transaction for ReceiveInventoryProducts.");
                    }
                    else
                    {
                        _logger.LogDebug("Using existing transaction for ReceiveInventoryProducts.");
                    }

                    var results = new ReceiveInventoryResult();
                    foreach (var item in request.ReceivedItems.OrderItems)
                    {
                        await _shipmentService.ReceiveInventoryProduct("NON_SERIAL_INV_ITEM", (decimal)item.QuantityAccepted, (decimal)item.QuantityRejected, null, null, null, item.OrderId, item.OrderItemSeqId, item.ProductId, item.FacilityId, null, null, item.Color, item.UomId);
                    }

                    var result = await _context.SaveChangesAsync(cancellationToken) > 0;

                    // REFACTOR: Commit only if this handler owns the transaction; skips commit in nested scenarios to defer to outer handler.
                    // This avoids premature commits and leverages EF Core's savepoint rollback for inner errors.
                    if (ownsTransaction)
                    {
                        await transaction.CommitAsync(cancellationToken);
                        _logger.LogDebug("Committed transaction for ReceiveInventoryProducts.");
                    }

                    var receiveInventoryResult = new ReceiveInventoryResult()
                        .SetSuccess(true)
                        .SetMessage("Operation completed successfully.")
                        .SetData(null);

                    return Result<ReceiveInventoryResult>.Success(receiveInventoryResult);
                }
                catch (Exception ex)
                {
                    // REFACTOR: Rollback only if this handler owns the transaction; allows outer transaction to handle nested failures.
                    // Enhances error resilience in chained handler calls.
                    if (ownsTransaction)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        _logger.LogDebug("Rolled back transaction for ReceiveInventoryProducts.");
                    }
                    _logger.LogError(ex, "An error occurred while receiving products. Stack Trace: {StackTrace}", ex.StackTrace);
                    return Result<ReceiveInventoryResult>.Failure("An error occurred while receiving products.");
                }
                finally
                {
                    // REFACTOR: Dispose transaction only if owned to prevent interference with outer transactions.
                    if (ownsTransaction && transaction != null)
                    {
                        await transaction.DisposeAsync();
                    }
                }
            }
        }
    }
}