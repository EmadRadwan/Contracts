using Application.Catalog.ProductStores;
using Application.Core;
using Application.Facilities.PhysicalInventories;
using Application.Order.Orders;
using Domain;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Facilities;

public interface IFacilityService
{
    Task RemoveSalesOrderItemInventoryReservation(OrderItemDto2 orderItem);
    Task AdjustInventoryItemForDeletedOrUpdatedOrderItem(OrderItemDto2 orderItem);

    Task ReserveInventoryForSalesOrderItem(OrderItemDto2 orderItem);

    Task IssueInventoryForSalesOrderItem(OrderItemDto2 orderItem);

    Task<InventoryItemDetail> GetInventoryItemDetail(OrderItemDto2 orderItem);
    Task<InventoryItem> GetInventoryItem(string inventoryItemId);
    Task<string> GetFacilityOwnerPartyId(string facilityId);
    IQueryable<ProductInventoryItem> GetInventoryAvailableByFacility(string facilityId, string productId);

    Task<ProductFacility> CreateProductFacility(string productId, string facilityId, decimal? minimumStock = 0,
        decimal? reorderQuantity = 1, int? daysToShip = 1, string replenishMethodEnumId = null,
        decimal? lastInventoryCount = 0, string requirementMethodEnumId = null);
}

//todo: Check CreateInventoryItem for PurchaseOrder. it should include DateTimeReceived and ExpireDate
// to be added in InventoryItem Form

public class FacilityService : IFacilityService
{
    private readonly DataContext _context;
    private readonly IProductStoreService _productStoreService;
    private readonly IUtilityService _utilityService;

    public FacilityService(DataContext context, IUtilityService utilityService,
        IProductStoreService productStoreService)
    {
        _context = context;
        _utilityService = utilityService;
        _productStoreService = productStoreService;
    }
    
    public async Task<ProductFacility> CreateProductFacility(string productId, string facilityId, decimal? minimumStock = 0, decimal? reorderQuantity = 1, int? daysToShip = 1, string replenishMethodEnumId = null, decimal? lastInventoryCount = 0, string requirementMethodEnumId = null)
    {
        try
        {
            var productFacility = new ProductFacility
            {
                ProductId = productId,
                FacilityId = facilityId,
                MinimumStock = minimumStock ?? 0m,
                ReorderQuantity = reorderQuantity ?? 1m,
                DaysToShip = daysToShip ?? 1,
                ReplenishMethodEnumId = replenishMethodEnumId,
                LastInventoryCount = lastInventoryCount ?? 0m,
                RequirementMethodEnumId = requirementMethodEnumId,
                CreatedStamp = DateTime.UtcNow,
                LastUpdatedStamp = DateTime.UtcNow
            };

            _context.ProductFacilities.Add(productFacility);
            await _context.SaveChangesAsync();

            return productFacility;
        }
        catch (Exception ex)
        {
            // Handle exceptions as necessary
            throw new Exception($"Error creating ProductFacility: {ex.Message}");
        }
    }



    public async Task RemoveSalesOrderItemInventoryReservation(OrderItemDto2 orderItem)
    {
        var orderItemShipGrpInvRes = await _context.OrderItemShipGrpInvRes.SingleOrDefaultAsync(x =>
            x.OrderId == orderItem.OrderId
            && x.OrderItemSeqId == orderItem.OrderItemSeqId);

        if (orderItemShipGrpInvRes != null)
            _context.OrderItemShipGrpInvRes.RemoveRange(orderItemShipGrpInvRes);
    }

    public async Task AdjustInventoryItemForDeletedOrUpdatedOrderItem(OrderItemDto2 orderItem)
    {
        // retrieve inventory item details for order item
        var inventoryItemDetailsForDeletedOrderItem = await GetInventoryItemDetails(orderItem);
        // update inventory item for each inventory item detail
        var stamp = DateTime.UtcNow;
        foreach (var inventoryItemDetail in inventoryItemDetailsForDeletedOrderItem)
        {
            var inventoryItem = await GetInventoryItem(inventoryItemDetail.InventoryItemId);
            inventoryItem.AvailableToPromiseTotal -= inventoryItemDetail.AvailableToPromiseDiff * -1;
            inventoryItem.LastUpdatedStamp = stamp;
        }

        // remove inventory item details for order item
        _context.InventoryItemDetails.RemoveRange(inventoryItemDetailsForDeletedOrderItem);
    }


    public async Task ReserveInventoryForSalesOrderItem(OrderItemDto2 orderItem)
    {
        // the function determines how many inventory items are needed to fulfill the order item
        var stamp = DateTime.UtcNow;

        var inventoryItemsForOrderItemProduct = await GetInventoryItems(orderItem);

        List<InventoryItem> inventoryItems;

        var itemQuantity = orderItem.Quantity;

        foreach (var invItem in inventoryItemsForOrderItemProduct)
            if (invItem.AvailableToPromiseTotal >= itemQuantity)
            {
                var quantityToUpdate = itemQuantity;
                await UpdateInventoryItemAndCreateDetail(orderItem, invItem, quantityToUpdate, stamp);
                break;
            }
            else
            {
                var quantityToUpdate = invItem.AvailableToPromiseTotal;
                await UpdateInventoryItemAndCreateDetail(orderItem, invItem, quantityToUpdate, stamp);
                itemQuantity -= quantityToUpdate;
            }
    }

    public async Task<InventoryItemDetail> GetInventoryItemDetail(OrderItemDto2 orderItem)
    {
        var oItem = await _context.InventoryItemDetails.SingleOrDefaultAsync(s =>
            s.OrderId == orderItem.OrderId && s.OrderItemSeqId == orderItem.OrderItemSeqId);
        return oItem;
    }


    public async Task<InventoryItem> GetInventoryItem(string inventoryItemId)
    {
        var inventoryItem =
            await _context.InventoryItems.SingleOrDefaultAsync(x => x.InventoryItemId == inventoryItemId);
        return inventoryItem;
    }

    public async Task IssueInventoryForSalesOrderItem(OrderItemDto2 orderItem)
    {
        var inventoryItemDetails = await GetInventoryItemDetails(orderItem);

        foreach (var inventoryItemDetail in inventoryItemDetails)
        {
            var inventoryItem = await GetInventoryItem(inventoryItemDetail.InventoryItemId);
            await UpdateInventoryItemAndCreateDetailForIssuedItem(orderItem, inventoryItem, inventoryItemDetail);
        }
    }

    public async Task<string> GetFacilityOwnerPartyId(string facilityId)
    {
        var facility = await _context.Facilities.SingleOrDefaultAsync(x => x.FacilityId == facilityId);
        return facility?.OwnerPartyId;
    }

    private async Task UpdateInventoryItemAndCreateDetail(OrderItemDto2 orderItem, InventoryItem invItem,
        decimal? quantityToUpdate, DateTime stamp)
    {
        invItem.AvailableToPromiseTotal -= quantityToUpdate;
        invItem.LastUpdatedStamp = stamp;
        CreateInventoryItemReservation(orderItem, quantityToUpdate, invItem.InventoryItemId);

        await CreateInventoryItemDetailForOrderItem(orderItem, quantityToUpdate, invItem.InventoryItemId);

        await SetLastInventoryCount(invItem, orderItem.Quantity);
    }

    private async Task CreateInventoryItemDetailForOrderItem(OrderItemDto2 orderItem, decimal? quantity,
        string inventoryItemId)
    {
        var stamp = DateTime.UtcNow;
        var newInventoryItemDetailSequence = await _utilityService.GetNextSequence("InventoryItemDetail");
        var inventoryItemDetailForCreatedOrder = new InventoryItemDetail
        {
            InventoryItemId = inventoryItemId,
            InventoryItemDetailSeqId = newInventoryItemDetailSequence,
            EffectiveDate = stamp,
            QuantityOnHandDiff = 0,
            AvailableToPromiseDiff = quantity * -1,
            AccountingQuantityDiff = 0,
            OrderId = orderItem.OrderId,
            OrderItemSeqId = orderItem.OrderItemSeqId,
            ShipGroupSeqId = "01",
            LastUpdatedStamp = stamp,
            CreatedStamp = stamp
        };
        _context.InventoryItemDetails.Add(inventoryItemDetailForCreatedOrder);
    }


    private async Task<List<InventoryItemDetail>> GetInventoryItemDetails(OrderItemDto2 orderItem)
    {
        var iiDetails = await _context.InventoryItemDetails
            .Where(s => s.OrderId == orderItem.OrderId && s.OrderItemSeqId == orderItem.OrderItemSeqId)
            .ToListAsync();
        return iiDetails;
    }

    public async Task<List<InventoryItem>> GetInventoryItems(OrderItemDto2 orderItem)
    {
        var inventoryItemsQuery = _context.InventoryItems
            .Where(x => x.ProductId == orderItem.ProductId && x.AvailableToPromiseTotal > 0)
            .AsQueryable();

        List<InventoryItem> inventoryItems;

        if (await _productStoreService.GetProductStoreReserveOrderEnumId() == "INVRO_FIFO_REC")
            inventoryItems = inventoryItemsQuery.OrderBy(x => x.CreatedStamp).ToList();
        else
            inventoryItems = inventoryItemsQuery.OrderByDescending(x => x.CreatedStamp).ToList();


        return inventoryItems;
    }


    private async Task UpdateInventoryItemForPurchaseOrder(string inventoryItemId, OrderItemDto2 orderItem)
    {
        var inventoryItem = await GetInventoryItem(inventoryItemId);
        inventoryItem.QuantityOnHandTotal += orderItem.Quantity;
        inventoryItem.AvailableToPromiseTotal += orderItem.Quantity;
        inventoryItem.AccountingQuantityTotal += orderItem.Quantity;
    }

    private OrderItemShipGrpInvRes CreateInventoryItemReservation(OrderItemDto2 orderItem, decimal? quantity,
        string inventoryItemId)
    {
        var stamp = DateTime.UtcNow;
        var orderItemShipGrpInvRes = new OrderItemShipGrpInvRes
        {
            OrderId = orderItem.OrderId,
            ShipGroupSeqId = "01",
            OrderItemSeqId = orderItem.OrderItemSeqId,
            InventoryItemId = inventoryItemId,
            Quantity = quantity,
            QuantityNotAvailable = null,
            ReservedDatetime = stamp,
            CreatedDatetime = stamp,
            LastUpdatedStamp = stamp,
            CreatedStamp = stamp
        };
        _context.OrderItemShipGrpInvRes.Add(orderItemShipGrpInvRes);
        return orderItemShipGrpInvRes;
    }


    private async Task SetLastInventoryCount(InventoryItem inventoryItem, decimal? quantity)
    {
        // select productFacility records for the inventory item
        var productFacility = await _context.ProductFacilities
            .Where(x => x.ProductId == inventoryItem.ProductId && x.FacilityId == inventoryItem.FacilityId)
            .FirstOrDefaultAsync();

        // if productFacility record exists, update LastInventoryCount
        if (productFacility != null) productFacility.LastInventoryCount -= quantity;
    }

    private async Task UpdateInventoryItemAndCreateDetailForIssuedItem(OrderItemDto2 orderItem,
        InventoryItem inventoryItem, InventoryItemDetail inventoryItemDetail)

    {
        var timestamp = DateTime.UtcNow;

        // Update the InventoryItem
        inventoryItem.QuantityOnHandTotal += inventoryItemDetail.AvailableToPromiseDiff;
        inventoryItem.AccountingQuantityTotal += inventoryItemDetail.AvailableToPromiseDiff;
        inventoryItem.LastUpdatedStamp = timestamp;

        // Save changes to the InventoryItem
        _context.InventoryItems.Update(inventoryItem);

        // Create InventoryItemDetail for the issued item
        var newInventoryItemDetailSequence = await _utilityService.GetNextSequence("InventoryItemDetail");
        var newInventoryItemDetail = new InventoryItemDetail
        {
            InventoryItemId = inventoryItem.InventoryItemId,
            InventoryItemDetailSeqId = newInventoryItemDetailSequence,
            EffectiveDate = timestamp,
            QuantityOnHandDiff = inventoryItemDetail.AvailableToPromiseDiff,
            AvailableToPromiseDiff = 0,
            AccountingQuantityDiff = 0,
            OrderId = orderItem.OrderId,
            OrderItemSeqId = orderItem.OrderItemSeqId,
            ShipGroupSeqId = "01",
            LastUpdatedStamp = timestamp,
            CreatedStamp = timestamp
        };

        // Add the new InventoryItemDetail
        _context.InventoryItemDetails.Add(newInventoryItemDetail);
    }
    
    public IQueryable<ProductInventoryItem> GetInventoryAvailableByFacility(string facilityId, string productId)
    {
        var inventoryItemsQuery = from ii in _context.InventoryItems
            join pr in _context.Products on ii.ProductId equals pr.ProductId
            where ii.FacilityId == facilityId && ii.ProductId == productId
            select new ProductInventoryItem
            {
                ProductId = pr.ProductId,
                ProductName = pr.ProductName,
                InternalName = pr.InternalName,
                FacilityId = ii.FacilityId,
                InventoryItemTypeId = ii.InventoryItemTypeId,
                ProductFacilityId = ii.FacilityId,
                InventoryComments = ii.Comments,
                QuantityOnHandTotal = ii.QuantityOnHandTotal,
                AvailableToPromiseTotal = ii.AvailableToPromiseTotal
            };

        var filteredItems = from item in inventoryItemsQuery
            where string.IsNullOrEmpty(item.InventoryItemTypeId) ||
                  item.InventoryItemTypeId == "SERIALIZED_INV_ITEM" ||
                  item.InventoryItemTypeId == "INV_AVAILABLE" ||
                  item.InventoryItemTypeId == "INV_NS_RETURNED"
            select item;

        // Use the filtered IQueryable and return it for further processing with OData queries
        return filteredItems;
    }

    public IQueryable<ProductInventoryItem> GetPhysicalInventory(string facilityId, string productId)
    {
        productId = productId?.Trim();

        var query = GetInventoryAvailableByFacility(facilityId, productId)
            .Where(ii => ii.InventoryItemTypeId == "NON_SERIAL_INV_ITEM");

        // Returning as IQueryable to allow OData query options to be applied
        return query;
    }


}