using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

public class OrderService
{
    private readonly Persistence.DataContext _context;
    private readonly ILogger<OrderService> _logger;

    public OrderService(Persistence.DataContext context, ILogger<OrderService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SetUnitPriceAsLastPriceAsync(OrderItemDto2 orderItem)
    {
        // REFACTOR: Retain shared DbContext but ensure all queries are awaited sequentially.
        // Purpose: Prevents concurrent access issues within the same DbContext by awaiting all async operations.
        // Context: Assumes DbContext is scoped to a single request, managed by the caller.
        var nowTimestamp = DateTime.UtcNow;

        try
        {
            // REFACTOR: Combine OrderRole and OrderHeader queries into a single async join.
            // Purpose: Reduces database round-trips and ensures proper awaiting, minimizing connection contention.
            // Context: Fetches related data in one query to improve performance and avoid overlapping operations.
            var orderData = await (from or in _context.OrderRoles.AsQueryable()
                                  join oh in _context.OrderHeaders.AsQueryable() on or.OrderId equals oh.OrderId
                                  where or.OrderId == orderItem.OrderId && or.RoleTypeId == "BILL_FROM_VENDOR"
                                  select new { OrderRole = or, OrderHeader = oh })
                                 .FirstOrDefaultAsync();

            if (orderData == null)
            {
                _logger.LogWarning("No order role or header found for OrderId: {OrderId}", orderItem.OrderId);
                return;
            }

            var productSupplierId = orderData.OrderRole.PartyId;
            var orderCurrency = orderData.OrderHeader.CurrencyUom;

            // REFACTOR: Optimize SupplierProduct query by checking ChangeTracker and database in a single async operation.
            // Purpose: Avoids separate local and database queries, ensuring all operations are awaited and reducing complexity.
            // Context: Uses FirstOrDefault for in-memory check and FirstOrDefaultAsync for database, minimizing memory usage.
            var supplierProduct = _context.ChangeTracker.Entries<SupplierProduct>()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
                .Select(e => e.Entity)
                .AsQueryable()
                .Where(ps => ps.PartyId == productSupplierId &&
                             ps.AvailableThruDate == null &&
                             ps.CurrencyUomId == orderCurrency &&
                             ps.ProductId == orderItem.ProductId)
                .FirstOrDefault() ?? await _context.SupplierProducts
                .AsQueryable()
                .Where(ps => ps.PartyId == productSupplierId &&
                             ps.AvailableThruDate == null &&
                             ps.CurrencyUomId == orderCurrency &&
                             ps.ProductId == orderItem.ProductId)
                .FirstOrDefaultAsync();

            // REFACTOR: Simplify SupplierProduct creation or update logic with early exit.
            // Purpose: Eliminates redundant merge logic and ensures changes are tracked in the shared DbContext.
            // Context: Streamlines handling of new or existing SupplierProduct in a single path.
            if (supplierProduct == null)
            {
                var newSupplierProduct = new SupplierProduct
                {
                    ProductId = orderItem.ProductId,
                    PartyId = productSupplierId,
                    CurrencyUomId = orderCurrency,
                    LastPrice = orderItem.UnitPrice,
                    AvailableFromDate = nowTimestamp
                };
                _context.Add(newSupplierProduct);
                _logger.LogInformation("Added new SupplierProduct for OrderId {OrderId}", orderItem.OrderId);
            }
            else if (orderItem.UnitPrice != supplierProduct.LastPrice)
            {
                var newSupplierProduct = new SupplierProduct
                {
                    ProductId = supplierProduct.ProductId,
                    PartyId = supplierProduct.PartyId,
                    CurrencyUomId = supplierProduct.CurrencyUomId,
                    LastPrice = orderItem.UnitPrice,
                    AvailableFromDate = nowTimestamp
                };
                supplierProduct.AvailableThruDate = nowTimestamp;
                _context.Add(newSupplierProduct);
                _logger.LogInformation("Updated SupplierProduct for OrderId {OrderId}, ProductId {ProductId}", orderItem.OrderId, supplierProduct.ProductId);
            }

            // REFACTOR: Explicitly await SaveChangesAsync to commit changes.
            // Purpose: Ensures all changes are persisted in a single transaction, awaited properly to avoid connection reuse issues.
            // Context: Commits changes within the shared DbContext, assuming caller manages transaction if needed.
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // REFACTOR: Add exception handling to log and handle database errors.
            // Purpose: Prevents unhandled exceptions and provides context for debugging concurrency or connection issues.
            // Context: Captures OrderId for easier diagnosis of errors.
            _logger.LogError(ex, "Failed to update SupplierProduct for OrderId {OrderId}", orderItem.OrderId);
            throw;
        }
    }
}