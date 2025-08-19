using Application.Catalog.ProductStores;
using Application.Errors;
using Application.Order.Orders;
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Core;

public interface IRepositoryService
{
    Task<List<ShipmentReceipt>> GetShipmentReceiptsById(string shipmentId);
    Task<List<InventoryItem>> GetInventoryItemsByShipmentId(string shipmentId);

}

public class RepositoryService : IRepositoryService
{
    private readonly DataContext _context;
    private readonly ILogger<UtilityService> _logger;


    public RepositoryService(DataContext context, ILogger<UtilityService> logger)
    {
        _context = context;
        _logger = logger;
    }
    

    public async Task<List<ShipmentReceipt>> GetShipmentReceiptsById(string shipmentId)
    {
        try
        {
            // Validate input
            if (string.IsNullOrEmpty(shipmentId))
            {
                _logger.LogWarning("Invalid shipmentId provided.");
                return new List<ShipmentReceipt>();
            }

            // Get local shipment receipts from change tracker
            var localShipmentReceipts = _context.ChangeTracker.Entries<ShipmentReceipt>()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
                .Select(e => e.Entity)
                .Where(sr => sr.ShipmentId == shipmentId)
                .ToList();

            // Get database shipment receipts
            var dbShipmentReceipts = await _context.Set<ShipmentReceipt>()
                .AsNoTracking()
                .Where(sr => sr.ShipmentId == shipmentId)
                .ToListAsync();

            // Combine and deduplicate by ReceiptId, prioritizing local changes
            var shipmentReceipts = localShipmentReceipts
                .Concat(dbShipmentReceipts.Where(db => !localShipmentReceipts.Any(local => local.ReceiptId == db.ReceiptId)))
                .GroupBy(sr => sr.ReceiptId)
                .Select(g =>
                {
                    var receipt = g.First();
                    return receipt;
                })
                .ToList();

            return shipmentReceipts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving shipment receipts for shipmentId: {ShipmentId}", shipmentId);
            throw;
        }
    }
    
    public async Task<List<InventoryItem>> GetInventoryItemsByShipmentId(string shipmentId)
{
    try
    {
        // Validate input
        if (string.IsNullOrEmpty(shipmentId))
        {
            _logger.LogWarning("Invalid shipmentId provided.");
            return new List<InventoryItem>();
        }

        // Get local inventory items from change tracker, joined with shipment receipts
        var localInventoryItems = _context.ChangeTracker.Entries<InventoryItem>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
            .Select(e => e.Entity)
            .Join(_context.ChangeTracker.Entries<ShipmentReceipt>()
                    .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
                    .Select(e => e.Entity)
                    .Where(sr => sr.ShipmentId == shipmentId),
                ii => ii.InventoryItemId,
                sr => sr.InventoryItemId,
                (ii, sr) => ii)
            .ToList();

        // Get database inventory items, joined with shipment receipts
        var dbInventoryItems = await _context.Set<InventoryItem>()
            .AsNoTracking()
            .Join(_context.Set<ShipmentReceipt>()
                    .AsNoTracking()
                    .Where(sr => sr.ShipmentId == shipmentId),
                ii => ii.InventoryItemId,
                sr => sr.InventoryItemId,
                (ii, sr) => ii)
            .ToListAsync();

        // Combine and deduplicate by InventoryItemId, prioritizing local changes
        var inventoryItems = localInventoryItems
            .Concat(dbInventoryItems.Where(db => !localInventoryItems.Any(local => local.InventoryItemId == db.InventoryItemId)))
            .GroupBy(ii => ii.InventoryItemId)
            .Select(g =>
            {
                var item = g.First();
                if (g.Count() > 1)
                {
                    _logger.LogWarning("Duplicate InventoryItemId {InventoryItemId} found for ShipmentId {ShipmentId}. Using first instance.", item.InventoryItemId, shipmentId);
                }
                return item;
            })
            .ToList();

        return inventoryItems;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error retrieving inventory items for shipmentId: {ShipmentId}", shipmentId);
        throw;
    }
}
   
}