using Application.Catalog.Products.Services.Inventory;
using Application.Core;
using MediatR;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Catalog.Products.Services.Inventory
{
    public class CreatePhysicalInventoryAndVariance
    {
        public class Command : IRequest<Result<string>>
        {
            public PhysicalInventoryVarianceDto Dto { get; set; }
        }

        public class Handler : IRequestHandler<Command, Result<string>>
        {
            private readonly IInventoryService _inventoryService;
            private readonly ILogger<Handler> _logger;
            private readonly DataContext _context;


            public Handler(IInventoryService inventoryService, ILogger<Handler> logger, DataContext context)
            {
                _inventoryService = inventoryService;
                _logger = logger;
                _context = context;
            }

            public async Task<Result<string>> Handle(Command request, CancellationToken cancellationToken)
            {
                await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

                try
                {
                    await _inventoryService.CreatePhysicalInventoryAndVariance(request.Dto);
                    var result = await _context.SaveChangesAsync(cancellationToken) > 0;
                    await transaction.CommitAsync(cancellationToken);

                    if (!result)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return Result<string>.Failure("Failed to create Physical Inventory");
                    }

                    return Result<string>.Success("create Physical Inventory");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Exception occurred while creating Physical Inventory and Variance.");
                    return Result<string>.Failure("An unexpected error occurred.");
                }
            }
        }
    }
}