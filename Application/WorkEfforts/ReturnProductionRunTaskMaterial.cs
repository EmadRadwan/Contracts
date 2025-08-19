using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Manufacturing;
using Microsoft.EntityFrameworkCore;
using Persistence;
using MediatR;


namespace Application.WorkEfforts
{
    public class ReturnMaterialsCommand : MediatR.IRequest<Result<Unit>>
    {
        public string ProductionRunId { get; set; }
        public List<ReturnItemDto> Items { get; set; }
    }

    public class ReturnItemDto
    {
        public string ProductId { get; set; }
        public string WorkEffortId { get; set; }
        public DateTime? FromDate { get; set; }
        public string? LotId { get; set; } // Made optional
        public decimal Quantity { get; set; }
    }

    public class Handler : MediatR.IRequestHandler<ReturnMaterialsCommand, Result<Unit>>
    {
        private readonly DataContext _context;
        private readonly IProductionRunService _productionRunService;

        public Handler(DataContext context, IProductionRunService productionRunService)
        {
            _context = context;
            _productionRunService = productionRunService;
        }

        public async Task<Result<Unit>> Handle(ReturnMaterialsCommand request, CancellationToken cancellationToken)
        {
            var errors = new List<string>();

            // Validate production run exists
            var productionRunExists = await _context.WorkEfforts
                .AnyAsync(we => we.WorkEffortId == request.ProductionRunId,
                    cancellationToken);
            if (!productionRunExists)
            {
                return Result<Unit>.Failure($"Production run {request.ProductionRunId} not found.");
            }

            await using var tx = await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                foreach (var item in request.Items)
                {
                    // Validate inputs
                    if (string.IsNullOrWhiteSpace(item.ProductId) || string.IsNullOrWhiteSpace(item.WorkEffortId))
                    {
                        errors.Add($"Invalid item: ProductId and WorkEffortId are required for {item.ProductId}.");
                        continue;
                    }

                    if (item.Quantity <= 0)
                    {
                        errors.Add($"Invalid quantity for {item.ProductId}: Quantity must be greater than 0.");
                        continue;
                    }

                    // Call ProductionRunTaskReturnMaterial for each item
                    var serviceResult = await _productionRunService.ProductionRunTaskReturnMaterial(
                        workEffortId: item.WorkEffortId,
                        productId: item.ProductId,
                        quantity: item.Quantity,
                        lotId: item.LotId // Pass null if not provided
                    );

                    if (serviceResult.HasError)
                    {
                        errors.Add(
                            $"Failed to return {item.ProductId} for task {item.WorkEffortId}: {serviceResult.ErrorMessage}");
                    }
                }

                if (errors.Any())
                {
                    await tx.RollbackAsync(cancellationToken);
                    return Result<Unit>.Failure(string.Join("; ", errors));
                }

                await _context.SaveChangesAsync(cancellationToken);
                await tx.CommitAsync(cancellationToken);
                return Result<Unit>.Success(Unit.Value);
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync(cancellationToken);
                return Result<Unit>.Failure($"Error processing returns: {ex.Message}");
            }
        }
    }
}