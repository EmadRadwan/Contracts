using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Facilities;

public class PackOrder
{
    public class Command : IRequest<Result<PackOrderResult>>
    {
        public string OrderId { get; set; }
        public string ShipGroupSeqId { get; set; }
        public string FacilityId { get; set; }
        public string PicklistBinId { get; set; }
        public List<PackOrderItemDto> ItemsToPack { get; set; }
    }

    public class Handler : IRequestHandler<Command, Result<PackOrderResult>>
    {
        private readonly DataContext _context;
        private readonly ILogger<Handler> _logger;
        private readonly IPackService _packService;

        public Handler(DataContext context,
            IPackService packService,
            ILogger<Handler> logger)
        {
            _context = context;
            _packService = packService;
            _logger = logger;
        }

        public async Task<Result<PackOrderResult>> Handle(Command request, CancellationToken cancellationToken)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                // 1) Build the PackItemsInput DTO from the command
                var input = new PackItemsInput
                {
                    FacilityId = request.FacilityId,
                    OrderId = request.OrderId,
                    ShipGroupSeqId = string.IsNullOrEmpty(request.ShipGroupSeqId) ? "01" : request.ShipGroupSeqId,
                    PicklistBinId = request.PicklistBinId,
                    ItemsToPack = request.ItemsToPack.Select(item => new PackItemLineDto
                    {
                        OrderId = item.OrderId,
                        OrderItemSeqId = item.OrderItemSeqId,
                        ShipGroupSeqId = string.IsNullOrEmpty(item.ShipGroupSeqId) ? "01" : item.ShipGroupSeqId,
                        ProductId = item.ProductId,
                        InventoryItemId = item.InventoryItemId,
                        Quantity = item.Quantity,
                        Weight = (decimal)item.Weight,
                        PackageSeqId = (int)item.PackageSeqId
                    }).ToList()
                };


                // 2) Invoke the packing logic in one shot
                var packResult = await _packService.PackItems(input);

                // If packResult is unsuccessful, handle it
                if (!packResult.Success)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return Result<PackOrderResult>.Failure(
                        $"Failed to pack items: {packResult.Message}"
                    );
                }

                var affectedRecords = _context.ChangeTracker
                    .Entries()
                    .Where(e => e.State == EntityState.Added ||
                                e.State == EntityState.Modified ||
                                e.State == EntityState.Deleted)
                    .Select(e => new ChangeRecord
                    {
                        TableName = e.Entity.GetType().Name,
                        PKValues = string.Join(", ", e.Properties
                            .Where(p => p.Metadata.IsPrimaryKey())
                            .Select(p => $"{p.Metadata.Name}: {p.CurrentValue}")),
                        Operation = e.State.ToString()
                    })
                    .ToList();

                foreach (var record in affectedRecords)
                {
                    Console.WriteLine(record);
                }
                
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                // 4) Return the final PackOrderResult
                var result = new PackOrderResult()
                    .SetSuccess(true)
                    .SetMessage("Packing operation completed successfully.")
                    .SetData(null); // or pass additional data if needed

                return Result<PackOrderResult>.Success(result);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);

                _logger.LogError(ex,
                    "An error occurred during PackOrder. StackTrace: {StackTrace}",
                    ex.StackTrace);

                return Result<PackOrderResult>.Failure(
                    "An error occurred while packing items."
                );
            }
        }
    }
}