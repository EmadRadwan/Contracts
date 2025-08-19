using API.Middleware;
using Application.Accounting.Services;
using Application.Accounting.Services.Models;
using Application.Catalog.Products.Services.Cost;
using Application.Catalog.ProductStores;
using Application.Core;
using Application.Facilities;
using Application.Facilities.InventoryTransfer;
using Application.Manufacturing;
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Catalog.Products.Services.Inventory;

public interface IInventoryService
{
    Task<string> CreateInventoryItem(CreateInventoryItemParam param);
    Task<InventoryTransfer> CreateInventoryTransfer(InventoryTransferDto inventoryTransferDto);
    Task<string> PrepareInventoryTransfer(string inventoryItemId, decimal transferQuantity);

    Task CreateInventoryItemDetail(CreateInventoryItemDetailParam param);

    Task<OperationResult> BalanceInventoryItems(string inventoryItemId, string priorityOrderId = null,
        string priorityOrderItemSeqId = null);


    Lot CreateLot(string lotId = null, DateTime? creationDate = null, decimal? quantity = 1,
        DateTime? expirationDate = null);


    Task<InventoryTotals> GetProductInventoryAvailable(string facilityId, string productId,
        string statusId = "");

    Task CancelOrderItemShipGrpInvRes(string orderId, string orderItemSeqId, string inventoryItemId,
        string shipGroupSeqId, decimal? cancelQuantity = null);


    Task<UpdateInventoryItemResult> UpdateInventoryItem(
        string inventoryItemId,
        decimal? unitCost = null,
        string lotId = null,
        string statusId = null,
        string ownerPartyId = null,
        string productId = null,
        string facilityId = null);

    Task<Result<string>> CreatePhysicalInventoryAndVariance(PhysicalInventoryVarianceDto dto);

    Task<decimal> ReserveProductInventory(
        decimal quantity,
        string productId,
        string orderId,
        string orderItemSeqId,
        string shipGroupSeqId,
        string reserveOrderEnumId,
        string productFeatureId = null,
        string facilityId = null,
        DateTime? reservedDatetime = null,
        string requireInventory = "N",
        string containerId = null,
        string lotId = null,
        string? sequenceId = null,
        int? priority = 1,
        string reserveReasonEnumId = null);
}

public class InventoryService : IInventoryService
{
    private readonly DataContext _context;
    private readonly ICostService _costService;
    private readonly IFacilityService _facilityService;
    private readonly ILogger _logger;
    private readonly IProductStoreService _productStoreService;
    private readonly IUtilityService _utilityService;
    private readonly Lazy<IGeneralLedgerService> _generalLedgerService;


    public InventoryService(DataContext context, IUtilityService utilityService,
        ILogger<InventoryService> logger, IFacilityService facilityService,
        ICostService costService, IProductStoreService productStoreService,
        Lazy<IGeneralLedgerService> generalLedgerService)
    {
        _context = context;
        _logger = logger;
        _utilityService = utilityService;
        _facilityService = facilityService;
        _productStoreService = productStoreService;
        _costService = costService;
        _generalLedgerService = generalLedgerService;
    }

    public async Task<string> CreateInventoryItem(CreateInventoryItemParam param)
    {
        try
        {
            // 1. Fetch Product
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.ProductId == param.ProductId);
            if (product == null)
            {
                // You could also throw a custom exception type here
                throw new Exception("Product not found.");
            }

            // 2. Validate LotId based on product configuration
            if (product.LotIdFilledIn == "Mandatory" && string.IsNullOrEmpty(param.LotId))
            {
                throw new Exception("ProductLotIdMandatory");
            }

            if (product.LotIdFilledIn == "Forbidden" && !string.IsNullOrEmpty(param.LotId))
            {
                throw new Exception("ProductLotIdForbidden");
            }

            // 3. If isReturned == "N" and lotId is not empty, create or verify the Lot
            if (param.IsReturned == "N" && !string.IsNullOrEmpty(param.LotId))
            {
                var existingLot = await _context.Lots
                    .FirstOrDefaultAsync(x => x.LotId == param.LotId);

                if (existingLot == null)
                {
                    var newLot = new Lot
                    {
                        LotId = param.LotId
                        // Additional fields if needed
                    };
                    _context.Lots.Add(newLot);
                }
            }

            // 4. Get sequence ID for InventoryItem
            var inventoryItemId = await _utilityService.GetNextSequence("InventoryItem");
            var stamp = DateTime.Now;

            // 5. Build out the InventoryItem
            var inventoryItem = new InventoryItem
            {
                InventoryItemId = inventoryItemId,
                InventoryItemTypeId = param.InventoryItemTypeId ?? "NON_SERIAL_INV_ITEM",
                FacilityId = param.FacilityId,
                LotId = param.LotId,
                LocationSeqId = param.LocationSeqId,
                ProductId = param.ProductId,
                CurrencyUomId = param.CurrencyUomId,
                PartyId = param.SupplierId,
                DatetimeReceived = stamp,
                DatetimeManufactured = param.DateTimeManufactured,
                UnitCost = param.UnitCost,
                CreatedStamp = stamp,
                LastUpdatedStamp = stamp
            };

            // 6. Check and set default values
            var inventoryItemTobeSaved = await InventoryItemCheckSetDefaultValues(inventoryItem, param);

            // 7. Save InventoryItem
            _context.InventoryItems.Add(inventoryItemTobeSaved);

            // 8. (Optional) Additional logic to manage QOH/ATP
            param.InventoryItemId = inventoryItemId;

            if (inventoryItemTobeSaved.AvailableToPromiseTotal.HasValue ||
                inventoryItemTobeSaved.QuantityOnHandTotal.HasValue)
            {
                await CreateInventoryItemCheckSetAtpQoh(param);
            }

            // 9. Return newly created ID
            return inventoryItemTobeSaved.InventoryItemId;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error creating InventoryItem: {ex.Message}", ex);
        }
    }


    public async Task CreateInventoryItemDetail(CreateInventoryItemDetailParam param)
    {
        var stamp = DateTime.UtcNow;

        if (string.IsNullOrEmpty(param.InventoryItemId))
            throw new ArgumentException("InventoryItemId must not be null or empty.", nameof(param.InventoryItemId));

        var newInventoryItemDetailSequence = await _utilityService.GetNextSequence("InventoryItemDetail");

        // Create a new InventoryItemDetail entity
        var newEntity = new InventoryItemDetail
        {
            InventoryItemId = param.InventoryItemId,
            InventoryItemDetailSeqId = newInventoryItemDetailSequence
        };

        try
        {
            if (!string.IsNullOrEmpty(param.OrderId) && !string.IsNullOrEmpty(param.OrderItemSeqId))
            {
                newEntity.OrderId = param.OrderId;
                newEntity.OrderItemSeqId = param.OrderItemSeqId;

                var orderShipments = await _utilityService.FindLocalOrDatabaseListAsync<OrderShipment>(
                    query => query.Where(os =>
                        os.OrderId == param.OrderId && os.OrderItemSeqId == param.OrderItemSeqId),
                    param.OrderId // Pass relevant parameters as needed
                );

                // Extract the ShipmentId from the retrieved entities
                var shipmentId = orderShipments.Select(os => os.ShipmentId).FirstOrDefault();


                newEntity.ShipmentId = shipmentId;

                // Fetch InventoryItem using the new method
                var inventoryItem =
                    await _utilityService.FindLocalOrDatabaseAsync<InventoryItem>(param.InventoryItemId);
                newEntity.UnitCost = inventoryItem?.UnitCost;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while retrieving related data: {ex.Message}");
            throw;
        }

        try
        {
            if (!string.IsNullOrEmpty(param.ItemIssuanceId))
            {
                var itemIssuance = await _utilityService.FindLocalOrDatabaseAsync<ItemIssuance>(param.ItemIssuanceId);
                if (itemIssuance != null)
                    newEntity.EffectiveDate = itemIssuance.IssuedDateTime;

                newEntity.ItemIssuanceId = param.ItemIssuanceId;
            }
            else
            {
                newEntity.EffectiveDate = stamp;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while retrieving item issuance data: {ex.Message}");
            throw;
        }

        // missing fields based on use case
        newEntity.WorkEffortId = param.WorkEffortId;

        // Set defaults if certain fields are empty
        newEntity.AvailableToPromiseDiff = param.AvailableToPromiseDiff ?? 0;
        newEntity.QuantityOnHandDiff = param.QuantityOnHandDiff ?? 0;
        newEntity.AccountingQuantityDiff = param.AccountingQuantityDiff ?? 0;
        newEntity.CreatedStamp = stamp;
        newEntity.LastUpdatedStamp = stamp;

        try
        {
            // Add the new entity to the DbContext and save changes
            _context.InventoryItemDetails.Add(newEntity);

            await UpdateInventoryItemFromDetail(param.InventoryItemId);

            if (newEntity.AvailableToPromiseDiff != 0)
                await SetLastInventoryCount(param.InventoryItemId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while saving the InventoryItemDetail: {ex.Message}");
            throw;
        }
    }

    public async Task<InventoryTransfer> CreateInventoryTransfer(InventoryTransferDto inventoryTransferDto)
    {
        var stamp = DateTime.Now;
        //grab  the next sequence for inventory transfer
        var inventoryTransferId = await _utilityService.GetNextSequence("InventoryTransfer");
        var inventoryTransfer = new InventoryTransfer
        {
            InventoryTransferId = inventoryTransferId,
            StatusId = inventoryTransferDto.StatusId,
            FacilityId = inventoryTransferDto.FacilityId,
            LocationSeqId = inventoryTransferDto.LocationSeqId,
            ContainerId = inventoryTransferDto.ContainerId,
            FacilityIdTo = inventoryTransferDto.FacilityIdTo,
            LocationSeqIdTo = inventoryTransferDto.LocationSeqIdTo,
            ContainerIdTo = inventoryTransferDto.ContainerIdTo,
            ItemIssuanceId = inventoryTransferDto.ItemIssuanceId,
            SendDate = inventoryTransferDto.SendDate,
            ReceiveDate = inventoryTransferDto.ReceiveDate,
            Comments = inventoryTransferDto.Comments,
            CreatedStamp = stamp,
            LastUpdatedStamp = stamp
        };

        var newInventoryItemId = await PrepareInventoryTransfer(inventoryTransferDto.InventoryItemId,
            inventoryTransferDto.TransferQuantity);
        inventoryTransfer.InventoryItemId = newInventoryItemId;

        _context.InventoryTransfers.Add(inventoryTransfer);

        return inventoryTransfer;
    }

    public async Task<string> PrepareInventoryTransfer(string inventoryItemId, decimal transferQuantity)
    {
        var stamp = DateTime.Now;
        var newInventoryItemId = await _utilityService.GetNextSequence("InventoryItem");


        // 1. Fetch inventory item:
        var inventoryItem = await _context.InventoryItems
            .Where(item => item.InventoryItemId == inventoryItemId)
            .FirstOrDefaultAsync();

        // 2. Inventory item type check:
        if (inventoryItem!.InventoryItemTypeId == "NON_SERIAL_INV_ITEM")
            // 4. Split item if transfer quantity is less than QOH or ATP:
            if (transferQuantity < inventoryItem.QuantityOnHandTotal ||
                transferQuantity < inventoryItem.AvailableToPromiseTotal)
            {
                // get the next sequence for inventory item
                // populate new inventory item with the same values as the original item
                var newInventoryItem = new InventoryItem
                {
                    InventoryItemId = newInventoryItemId,
                    InventoryItemTypeId = "NON_SERIAL_INV_ITEM",
                    FacilityId = inventoryItem.FacilityId,
                    ProductId = inventoryItem.ProductId,
                    CurrencyUomId = inventoryItem.CurrencyUomId,
                    PartyId = inventoryItem.PartyId,
                    OwnerPartyId = inventoryItem.OwnerPartyId,
                    DatetimeReceived = inventoryItem.DatetimeReceived,
                    UnitCost = inventoryItem.UnitCost,
                    AvailableToPromiseTotal = 0,
                    QuantityOnHandTotal = 0,
                    CreatedStamp = stamp,
                    LastUpdatedStamp = stamp
                };
                _context.InventoryItems.Add(newInventoryItem);


                // 6. Create inventory details for both items:
                var originalItemDetail = new CreateInventoryItemDetailParam
                {
                    InventoryItemId = inventoryItem.InventoryItemId,
                    AvailableToPromiseDiff = -transferQuantity,
                    QuantityOnHandDiff = -transferQuantity,
                    AccountingQuantityDiff = -transferQuantity
                };

                var newInventoryItemDetail = new CreateInventoryItemDetailParam
                {
                    InventoryItemId = newInventoryItemId,
                    AvailableToPromiseDiff = transferQuantity,
                    QuantityOnHandDiff = transferQuantity,
                    AccountingQuantityDiff = transferQuantity
                };

                await CreateInventoryItemDetail(originalItemDetail);
                await CreateInventoryItemDetail(newInventoryItemDetail);
            }

        // setup values so that no one will grab the inventory during the move
        if (inventoryItem!.InventoryItemTypeId == "NON_SERIAL_INV_ITEM")
        {
            var inventoryItemDetail = new CreateInventoryItemDetailParam
            {
                InventoryItemId = newInventoryItemId,
                AvailableToPromiseDiff = -transferQuantity,
                QuantityOnHandDiff = 0,
                AccountingQuantityDiff = 0
            };
            await CreateInventoryItemDetail(inventoryItemDetail);
        }

        return newInventoryItemId;
    }


    public async Task<InventoryTotals> GetProductInventoryAvailable(string facilityId, string productId,
        string statusId = "")
    {
        // Fetch inventory items from the change tracker
        var inventoryItemsFromTracker = _context.ChangeTracker.Entries<InventoryItem>()
            .Where(e => (e.Entity.FacilityId == facilityId && e.Entity.ProductId == productId &&
                         e.State == EntityState.Added) || e.State == EntityState.Modified)
            .Select(e => e.Entity)
            .ToList();

        var trackedItemIds = inventoryItemsFromTracker.Select(item => item.InventoryItemId).ToList();

        // Fetch inventory items from the database
        var inventoryItemsQuery = _context.InventoryItems
            .Where(item => item.FacilityId == facilityId && item.ProductId == productId);

        if (!string.IsNullOrEmpty(statusId))
            inventoryItemsQuery = inventoryItemsQuery.Where(item => item.StatusId == statusId);
        else
            inventoryItemsQuery = inventoryItemsQuery
                .Where(item =>
                    item.StatusId == "INV_AVAILABLE" ||
                    item.StatusId == "INV_NS_RETURNED" ||
                    item.InventoryItemTypeId == "SERIALIZED_INV_ITEM" ||
                    item.StatusId == null);

        // Materialize the query
        var inventoryItems = await inventoryItemsQuery.ToListAsync();

        // Filter out items that are already in the change tracker
        inventoryItems = inventoryItems.Where(item => !trackedItemIds.Contains(item.InventoryItemId)).ToList();

        // Merge results from both tracker and database
        inventoryItems.AddRange(inventoryItemsFromTracker);

        // Summing up quantities based on conditions
        var availableToPromiseTotal = inventoryItems.Sum(item => item.AvailableToPromiseTotal);
        var quantityOnHandTotal = inventoryItems.Sum(item => item.QuantityOnHandTotal);
        var accountingQuantityTotal = inventoryItems.Sum(item => item.AccountingQuantityTotal);

        // Return the results
        return new InventoryTotals
        {
            AvailableToPromiseTotal = availableToPromiseTotal,
            QuantityOnHandTotal = quantityOnHandTotal,
            AccountingQuantityTotal = accountingQuantityTotal
        };
    }

    private async Task CreateInventoryItemCheckSetAtpQoh(CreateInventoryItemParam param)
    {
        try
        {
            // Check if AvailableToPromise or QuantityOnHand is set
            if (param.AvailableToPromise.HasValue || param.QuantityOnHand.HasValue)
            {
                // Prepare InventoryItemDetailParam object
                var inventoryItemDetailParam = new CreateInventoryItemDetailParam
                {
                    InventoryItemId = param.InventoryItemId,
                    AvailableToPromiseDiff = param.AvailableToPromise ?? 0, // Default to 0 if null
                    QuantityOnHandDiff = param.QuantityOnHand ?? 0 // Default to 0 if null
                };

                // Call CreateInventoryItemDetail
                await CreateInventoryItemDetail(inventoryItemDetailParam);
            }
        }
        catch (Exception ex)
        {
            // Log the exception and rethrow
            // _logger.LogError(ex, "Error in CreateInventoryItemCheckSetAtpQoh.");
            throw new Exception($"Error in CreateInventoryItemCheckSetAtpQoh: {ex.Message}", ex);
        }
    }


    private async Task<InventoryItem> InventoryItemCheckSetDefaultValues(InventoryItem inventoryItem,
        CreateInventoryItemParam param)
    {
        if (!string.IsNullOrEmpty(inventoryItem.FacilityId) &&
            !string.IsNullOrEmpty(inventoryItem.OwnerPartyId) &&
            !string.IsNullOrEmpty(inventoryItem.CurrencyUomId) &&
            inventoryItem.UnitCost.HasValue)
            // All fields are already filled, return with success
            return inventoryItem;

        if (string.IsNullOrEmpty(inventoryItem.OwnerPartyId))
        {
            // Get ownerPartyId from the facility
            var facilityOwnerPartyId = await _facilityService.GetFacilityOwnerPartyId(param.FacilityId!);
            inventoryItem.OwnerPartyId = facilityOwnerPartyId;
        }

        if (string.IsNullOrEmpty(inventoryItem.CurrencyUomId))
        {
            // Get currencyUomId from party accounting preferences
            var currencyId = await _productStoreService.GetProductStoreDefaultCurrencyId();
            inventoryItem.CurrencyUomId = currencyId;
        }

        if (!inventoryItem.UnitCost.HasValue || inventoryItem.UnitCost.Value < 0)
        {
            // Get unitCost from the product's standard cost
            var unitCost =
                await _costService.GetProductCost(inventoryItem.ProductId, inventoryItem.CurrencyUomId, "EST_STD");
            inventoryItem.UnitCost = unitCost;
        }

        return inventoryItem;
    }

    // Service method to update InventoryItem from InventoryItemDetail
    private async Task UpdateInventoryItemFromDetail(string inventoryItemId)
    {
        // Attempt to fetch InventoryItem from the change tracker
        var inventoryItem = await _utilityService.FindLocalOrDatabaseAsync<InventoryItem>(inventoryItemId);

        if (inventoryItem != null)
        {
            // Fetch relevant data from InventoryItemDetail table from the change tracker
            var addedOrModifiedDetails = _context.ChangeTracker.Entries<InventoryItemDetail>()
                .Where(e => e.Entity.InventoryItemId == inventoryItemId &&
                            (e.State == EntityState.Added || e.State == EntityState.Modified))
                .Select(e => e.Entity)
                .ToList();

            // Fetch existing InventoryItemDetails from the database
            var existingInventoryItemDetails = await _context.InventoryItemDetails
                .Where(iid => iid.InventoryItemId == inventoryItemId)
                .ToListAsync();

            // Merge the two lists, ensuring no duplication
            var inventoryItemDetails = existingInventoryItemDetails
                .Concat(addedOrModifiedDetails)
                .GroupBy(detail => detail.InventoryItemDetailSeqId) // Assuming InventoryItemDetailSeqId is unique
                .Select(group => group.First())
                .ToList();


            // Calculate sums
            var availableToPromiseTotal = inventoryItemDetails.Sum(detail => detail.AvailableToPromiseDiff);
            var quantityOnHandTotal = inventoryItemDetails.Sum(detail => detail.QuantityOnHandDiff);
            var accountingQuantityTotal = inventoryItemDetails.Sum(detail => detail.AccountingQuantityDiff);

            // Update InventoryItem with the calculated sums
            inventoryItem.AvailableToPromiseTotal = availableToPromiseTotal;
            inventoryItem.QuantityOnHandTotal = quantityOnHandTotal;
            inventoryItem.AccountingQuantityTotal = accountingQuantityTotal;
        }
    }

    private async Task SetLastInventoryCount(string inventoryItemId)
    {
        // Fetch the InventoryItem from the change tracker
        var inventoryItemEntry = _context.ChangeTracker.Entries<InventoryItem>()
            .FirstOrDefault(e =>
                (e.Entity.InventoryItemId == inventoryItemId && e.State == EntityState.Added) ||
                e.State == EntityState.Modified);

        InventoryItem? inventoryItem = null;

        if (inventoryItemEntry != null)
            inventoryItem = inventoryItemEntry.Entity;
        else
            // If InventoryItem is not found in the change tracker, fetch it from the database
            inventoryItem = await _context.InventoryItems
                .FirstOrDefaultAsync(ii => ii.InventoryItemId == inventoryItemId);

        if (inventoryItem != null)
        {
            // Fetch ProductFacility based on inventoryItem data
            var productFacility = await _context.ProductFacilities
                .FirstOrDefaultAsync(pf =>
                    pf.ProductId == inventoryItem!.ProductId &&
                    pf.FacilityId == inventoryItem!.FacilityId);

            // Update ProductFacility data
            if (productFacility != null)
            {
                // Call service to get available inventory by facility
                var inventoryTotals =
                    await GetProductInventoryAvailable(inventoryItem.FacilityId, inventoryItem.ProductId);

                // Update lastInventoryCount in ProductFacility
                productFacility.LastInventoryCount = inventoryTotals.AvailableToPromiseTotal;
            }
        }
    }

    public async Task<OperationResult> BalanceInventoryItems(string inventoryItemId, string priorityOrderId = null,
        string priorityOrderItemSeqId = null)
    {
        var result = new OperationResult();
        try
        {
            var nowTimestamp = DateTime.Now;

            // Step 1: Query InventoryItem using inventoryItemId
            var inventoryItem = await _context.InventoryItems
                .FirstOrDefaultAsync(ii => ii.InventoryItemId == inventoryItemId);

            if (inventoryItem == null)
            {
                result.ErrorMessage = $"InventoryItem not found for InventoryItemId {inventoryItemId}";
                return result;
            }

            // Step 2: Call reassignInventoryReservations service
            await ReassignInventoryReservations(inventoryItem.ProductId, inventoryItem.FacilityId, nowTimestamp,
                priorityOrderId, priorityOrderItemSeqId);

            // Consider output handling if required (noLongerOnBackOrderIdSet equivalent)

            result.Success = true;
            return result; // Successful completion
        }
        catch (Exception ex)
        {
            // Log the exception (logging mechanism not shown here)
            // Logger.LogError(ex.Message, ex);
            result.ErrorMessage = ex.Message;
            return result; // Return failure with error message
        }
    }

    public async Task<ReassignInventoryReservationsResult> ReassignInventoryReservations(string productId,
        string facilityId, DateTime fromDate, string? priorityOrderId = null, string? priorityOrderItemSeqId = null)
    {
        var result = new ReassignInventoryReservationsResult
        {
            Success = true,
            NoLongerOnBackOrderIdSet = new HashSet<string>()
        };

        try
        {
            var relatedRes = await (from ii in _context.InventoryItems
                join oisgir in _context.OrderItemShipGrpInvRes on ii.InventoryItemId equals oisgir.InventoryItemId
                where ii.ProductId == productId
                      && ii.FacilityId == facilityId
                      && ii.InventoryItemTypeId == "NON_SERIAL_INV_ITEM"
                      && (oisgir.CurrentPromisedDate > fromDate
                          || oisgir.QuantityNotAvailable > 0
                          || ii.AvailableToPromiseTotal == null
                          || ii.AvailableToPromiseTotal == 0
                          || ii.AvailableToPromiseTotal < 0)
                orderby oisgir.Priority, oisgir.CurrentPromisedDate, oisgir.ReservedDatetime, oisgir.SequenceId
                select new OrderItemShipGrpInvResDto
                {
                    OrderId = oisgir.OrderId,
                    OrderItemSeqId = oisgir.OrderItemSeqId,
                    InventoryItemId = oisgir.InventoryItemId,
                    ShipGroupSeqId = oisgir.ShipGroupSeqId,
                    ReserveOrderEnumId = oisgir.ReserveOrderEnumId,
                    Quantity = oisgir.Quantity,
                    QuantityNotAvailable = oisgir.QuantityNotAvailable,
                    ReservedDatetime = oisgir.ReservedDatetime,
                    CreatedDatetime = oisgir.CreatedDatetime,
                    PromisedDatetime = oisgir.PromisedDatetime,
                    CurrentPromisedDate = oisgir.CurrentPromisedDate,
                    Priority = oisgir.Priority,
                    SequenceId = oisgir.SequenceId
                }).ToListAsync();

            var privilegedReservations = new List<OrderItemShipGrpInvResDto>();
            var reservations = new List<OrderItemShipGrpInvResDto>();

            foreach (var oneRelatedRes in relatedRes)
            {
                var picklistItemList = await (from pl in _context.Picklists
                    join plb in _context.PicklistBins on pl.PicklistId equals plb.PicklistId
                    join pli in _context.PicklistItems on plb.PicklistBinId equals pli.PicklistBinId
                    where pli.OrderId == oneRelatedRes.OrderId
                          && pli.ShipGroupSeqId == oneRelatedRes.ShipGroupSeqId
                          && pli.OrderItemSeqId == oneRelatedRes.OrderItemSeqId
                          && pli.InventoryItemId == oneRelatedRes.InventoryItemId
                          && pli.ItemStatusId != "PICKLIST_CANCELLED"
                          && pli.ItemStatusId != "PICKLIST_PICKED"
                    select pli).ToListAsync();

                if (picklistItemList.Count == 0)
                {
                    if (priorityOrderId != null && priorityOrderItemSeqId != null &&
                        priorityOrderId == oneRelatedRes.OrderId &&
                        priorityOrderItemSeqId == oneRelatedRes.OrderItemSeqId)
                    {
                        privilegedReservations.Add(oneRelatedRes);
                    }
                    else
                    {
                        reservations.Add(oneRelatedRes);
                    }
                }
            }

            var allReservations = privilegedReservations.Concat(reservations).ToList();

            // FIRST, cancel all the reservations
            foreach (var oisgir in allReservations)
            {
                await CancelOrderItemShipGrpInvRes(oisgir.OrderId, oisgir.OrderItemSeqId, oisgir.InventoryItemId,
                    oisgir.ShipGroupSeqId);
            }

            // THEN, re-reserve the cancelled items
            var touchedOrderIdMap = new Dictionary<string, string>();
            foreach (var oisgir in allReservations)
            {
                if (oisgir.QuantityNotAvailable > 0)
                {
                    touchedOrderIdMap[oisgir.OrderId] = "Y";
                }

                var orderHeader = await _context.OrderHeaders
                    .Where(oh => oh.OrderId == oisgir.OrderId)
                    .FirstOrDefaultAsync();

                await ReserveProductInventory(
                    quantity: (decimal)oisgir.Quantity,
                    containerId: productId, // or provide a value if available
                    productId: null, // or provide a value if available
                    lotId: null, // or provide a value if available
                    orderId: oisgir.OrderId,
                    orderItemSeqId: oisgir.OrderItemSeqId,
                    shipGroupSeqId: oisgir.ShipGroupSeqId,
                    reserveOrderEnumId: oisgir.ReserveOrderEnumId,
                    reservedDatetime: (DateTime)oisgir.ReservedDatetime,
                    requireInventory: "N",
                    sequenceId: oisgir.SequenceId,
                    priority: orderHeader?.Priority == "1" ? 1 : 2
                );
            }

            var noLongerOnBackOrderIdMap = new Dictionary<string, string>();
            foreach (var touchedOrderId in touchedOrderIdMap.Keys)
            {
                var isBackOrder = await CheckOrderIsOnBackOrder(touchedOrderId);

                if (!isBackOrder)
                {
                    noLongerOnBackOrderIdMap[touchedOrderId] = "Y";
                }
            }

            if (noLongerOnBackOrderIdMap.Any())
            {
                result.NoLongerOnBackOrderIdSet = noLongerOnBackOrderIdMap.Keys.ToHashSet();
            }

            return result;
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.Message;
            return result;
        }
    }

    public async Task CancelOrderItemShipGrpInvRes(
        string orderId,
        string orderItemSeqId,
        string inventoryItemId,
        string shipGroupSeqId,
        decimal? cancelQuantity = null)
    {
        // Validate input parameters
        if (string.IsNullOrWhiteSpace(orderId))
            throw new ArgumentException("Order ID is required.", nameof(orderId));
        if (string.IsNullOrWhiteSpace(orderItemSeqId))
            throw new ArgumentException("Order Item Sequence ID is required.", nameof(orderItemSeqId));
        if (string.IsNullOrWhiteSpace(inventoryItemId))
            throw new ArgumentException("Inventory Item ID is required.", nameof(inventoryItemId));
        if (string.IsNullOrWhiteSpace(shipGroupSeqId))
            throw new ArgumentException("Ship Group Sequence ID is required.", nameof(shipGroupSeqId));
        if (cancelQuantity.HasValue && cancelQuantity <= 0)
            throw new ArgumentException("Cancel quantity must be positive.", nameof(cancelQuantity));

        OrderItemShipGrpInvRes? orderItemShipGrpInvRes = null;
        var trackedReservation = _context.ChangeTracker.Entries<OrderItemShipGrpInvRes>()
            .FirstOrDefault(e =>
                e.Entity.OrderId == orderId &&
                e.Entity.OrderItemSeqId == orderItemSeqId &&
                e.Entity.ShipGroupSeqId == shipGroupSeqId &&
                e.Entity.InventoryItemId == inventoryItemId);

        if (trackedReservation == null)
        {
            orderItemShipGrpInvRes = await _context.OrderItemShipGrpInvRes
                .AsNoTracking()
                .FirstOrDefaultAsync(r =>
                    r.OrderId == orderId &&
                    r.OrderItemSeqId == orderItemSeqId &&
                    r.ShipGroupSeqId == shipGroupSeqId &&
                    r.InventoryItemId == inventoryItemId);
        }
        else
        {
            orderItemShipGrpInvRes = trackedReservation.Entity;
        }

        if (orderItemShipGrpInvRes == null)
        {
            _logger.LogWarning(
                "Reservation not found for OrderId: {OrderId}, OrderItemSeqId: {OrderItemSeqId}, " +
                "ShipGroupSeqId: {ShipGroupSeqId}, InventoryItemId: {InventoryItemId}",
                orderId, orderItemSeqId, shipGroupSeqId, inventoryItemId);
        }

        // Retrieve the related InventoryItem
        var inventoryItem = await _context.InventoryItems
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.InventoryItemId == inventoryItemId);

        if (inventoryItem == null)
        {
            _logger.LogWarning("Inventory item not found for InventoryItemId: {InventoryItemId}", inventoryItemId);
        }

        // Round quantities to 6 decimal places to match MiniLang precision
        decimal reservedQuantity = (decimal)orderItemShipGrpInvRes.Quantity;
        decimal? effectiveCancelQuantity = cancelQuantity.HasValue
            ? Math.Round(cancelQuantity.Value, 6)
            : null;

        if (inventoryItem.InventoryItemTypeId == "SERIALIZED_INV_ITEM")
        {
            _logger.LogInformation(
                "Re-enabling serialized inventory for InventoryItemId: {InventoryItemId}",
                inventoryItemId);
            inventoryItem.StatusId = "INV_AVAILABLE";
            _context.OrderItemShipGrpInvRes.Remove(orderItemShipGrpInvRes);
            _context.InventoryItems.Update(inventoryItem);
        }
        else if (inventoryItem.InventoryItemTypeId == "NON_SERIAL_INV_ITEM")
        {
            _logger.LogInformation(
                "Incrementing ATP for non-serialized inventory item: {InventoryItemId}",
                inventoryItemId);

            // Default to full quantity if cancelQuantity is not provided
            effectiveCancelQuantity ??= reservedQuantity;

            // Validate cancelQuantity
            if (effectiveCancelQuantity > reservedQuantity)
            {
                _logger.LogWarning(
                    "Cancel quantity {CancelQuantity} exceeds reserved quantity {ReservedQuantity} for InventoryItemId: {InventoryItemId}",
                    effectiveCancelQuantity, reservedQuantity, inventoryItemId);
                throw new Exception("Cancel quantity exceeds reserved quantity.");
            }

            // Create InventoryItemDetail
            var inventoryItemDetail = new CreateInventoryItemDetailParam
            {
                InventoryItemId = inventoryItem.InventoryItemId,
                OrderId = orderId,
                OrderItemSeqId = orderItemSeqId,
                ShipGroupSeqId = shipGroupSeqId,
                AvailableToPromiseDiff = effectiveCancelQuantity
            };

            await CreateInventoryItemDetail(inventoryItemDetail);

            // Update or remove reservation
            if (effectiveCancelQuantity < reservedQuantity)
            {
                orderItemShipGrpInvRes.Quantity = Math.Round(reservedQuantity - effectiveCancelQuantity.Value, 6);
            }
            else
            {
                _context.OrderItemShipGrpInvRes.Remove(orderItemShipGrpInvRes);
                _logger.LogInformation(
                    "Removed reservation for InventoryItemId: {InventoryItemId}",
                    inventoryItemId);
            }
        }
        else
        {
            _logger.LogWarning(
                "Invalid InventoryItemTypeId: {InventoryItemTypeId} for InventoryItemId: {InventoryItemId}",
                inventoryItem.InventoryItemTypeId, inventoryItemId);
        }
    }


    private string GetOrderByString(string reserveOrderEnumId)
    {
        string orderByString = "+datetimeReceived"; // Default

        switch (reserveOrderEnumId)
        {
            case "INVRO_GUNIT_COST":
                orderByString = "-unitCost";
                break;
            case "INVRO_LUNIT_COST":
                orderByString = "+unitCost";
                break;
            case "INVRO_FIFO_EXP":
                orderByString = "+expireDate";
                break;
            case "INVRO_LIFO_EXP":
                orderByString = "-expireDate";
                break;
            case "INVRO_LIFO_REC":
                orderByString = "-datetimeReceived";
                break;
            default:
                orderByString = "+datetimeReceived"; // Default to FIFO based on datetimeReceived
                break;
        }

        return orderByString;
    }

    public async Task<List<InventoryItemAndLocationsDto>> GetInventoryItems(string productId, string facilityId,
        string containerId, string lotId, string orderByString, string locationTypeEnumId = null)
    {
        var inventoryItemsQuery = from ii in _context.InventoryItems
            join pr in _context.Products on ii.ProductId equals pr.ProductId
            join fl in _context.FacilityLocations on new { ii.FacilityId, ii.LocationSeqId } equals new
                { fl.FacilityId, fl.LocationSeqId }
            where ii.ProductId == productId &&
                  (string.IsNullOrEmpty(facilityId) || ii.FacilityId == facilityId) &&
                  (string.IsNullOrEmpty(containerId) || ii.ContainerId == containerId) &&
                  (string.IsNullOrEmpty(lotId) || ii.LotId == lotId) &&
                  (ii.AvailableToPromiseTotal ?? 0) > 0 &&
                  (string.IsNullOrEmpty(locationTypeEnumId) || ii.LocationSeqId == locationTypeEnumId) &&
                  ii.StatusId != "INV_NS_DEFECTIVE" &&
                  ii.StatusId != "INV_DEFECTIVE"
            orderby EF.Property<object>(ii, orderByString)
            select new InventoryItemAndLocationsDto
            {
                InventoryItem = ii,
                Product = pr,
                FacilityLocation = fl
            };

        return await inventoryItemsQuery.ToListAsync();
    }

    public async Task<decimal> ReserveProductInventory(
        decimal quantity,
        string productId,
        string orderId,
        string orderItemSeqId,
        string shipGroupSeqId,
        string reserveOrderEnumId,
        string productFeatureId = null,
        string facilityId = null,
        DateTime? reservedDatetime = null,
        string requireInventory = "N",
        string containerId = null,
        string lotId = null,
        string? sequenceId = null,
        int? priority = 1,
        string reserveReasonEnumId = null)
    {
        _logger.LogInformation(
            "ReserveProductInventory parameters: {productId}, {orderId}, {orderItemSeqId}, {shipGroupSeqId}, {quantity}, {facilityId}, {requireInventory}, {reserveOrderEnumId}, {reserveReasonEnumId}",
            productId, orderId, orderItemSeqId, shipGroupSeqId, quantity, facilityId, requireInventory,
            reserveOrderEnumId, reserveReasonEnumId);

        try
        {
            if (string.IsNullOrEmpty(productId)) throw new ArgumentException("Product ID is required.");
            if (string.IsNullOrEmpty(orderId)) throw new ArgumentException("Order ID is required.");
            if (string.IsNullOrEmpty(orderItemSeqId)) throw new ArgumentException("Order Item Seq ID is required.");
            if (string.IsNullOrEmpty(requireInventory))
                throw new ArgumentException("Require Inventory flag is required.");
            if (string.IsNullOrEmpty(reserveOrderEnumId))
                throw new ArgumentException("Reserve Order Enum ID is required.");

            var product = await _context.Products
                .Where(p => p.ProductId == productId)
                .Select(p => new { p.ProductId, p.ProductTypeId })
                .FirstOrDefaultAsync();
            if (product == null) throw new Exception("Product not found");

            var productType = await _context.ProductTypes
                .FirstOrDefaultAsync(p => p.ProductTypeId == product.ProductTypeId);
            if (productType == null) throw new Exception("ProductType not found");

            var facility = string.IsNullOrEmpty(facilityId)
                ? null
                : await _context.Facilities
                    .FirstOrDefaultAsync(f => f.FacilityId == facilityId);

            if (productType.IsPhysical == "N")
            {
                return 0;
            }

            var reserveProductResult = new ReserveProductResult { QuantityNotReserved = quantity };
            InventoryItem lastNonSerInventoryItem = null;

            var inventoryItemQuery = _context.InventoryItems
                .Where(ii => ii.ProductId == productId && ii.AvailableToPromiseTotal > 0 &&
                             ii.StatusId != "INV_NS_DEFECTIVE" && ii.StatusId != "INV_DEFECTIVE");

            if (!string.IsNullOrEmpty(productFeatureId))
            {
                inventoryItemQuery = from ii in inventoryItemQuery
                    join iif in _context.InventoryItemFeatures
                        on ii.InventoryItemId equals iif.InventoryItemId
                    where iif.ProductFeatureId == productFeatureId
                    select ii;
            }
            
            if (!string.IsNullOrEmpty(facilityId))
            {
                inventoryItemQuery = inventoryItemQuery.Where(ii => ii.FacilityId == facilityId);
            }

            if (!string.IsNullOrEmpty(containerId))
            {
                inventoryItemQuery = inventoryItemQuery.Where(ii => ii.ContainerId == containerId);
            }

            if (!string.IsNullOrEmpty(lotId))
            {
                inventoryItemQuery = inventoryItemQuery.Where(ii => ii.LotId == lotId);
            }

            switch (reserveOrderEnumId)
            {
                case "INVRO_GUNIT_COST":
                    inventoryItemQuery = inventoryItemQuery.OrderByDescending(ii => ii.UnitCost);
                    break;
                case "INVRO_LUNIT_COST":
                    inventoryItemQuery = inventoryItemQuery.OrderBy(ii => ii.UnitCost);
                    break;
                case "INVRO_FIFO_EXP":
                    inventoryItemQuery = inventoryItemQuery.OrderBy(ii => ii.ExpireDate);
                    break;
                case "INVRO_LIFO_EXP":
                    inventoryItemQuery = inventoryItemQuery.OrderByDescending(ii => ii.ExpireDate);
                    break;
                case "INVRO_LIFO_REC":
                    inventoryItemQuery = inventoryItemQuery.OrderByDescending(ii => ii.DatetimeReceived);
                    break;
                default:
                    inventoryItemQuery = inventoryItemQuery.OrderBy(ii => ii.DatetimeReceived);
                    reserveOrderEnumId = "INVRO_FIFO_REC";
                    break;
            }

            foreach (var locationType in new[] { "FLT_PICKLOC", "FLT_BULK", null })
            {
                if (reserveProductResult.QuantityNotReserved <= 0) break;

                var query = inventoryItemQuery;
                if (locationType != null)
                {
                    query = query.Where(ii => _context.FacilityLocations
                        .Any(fl => fl.LocationSeqId == ii.LocationSeqId && fl.LocationTypeEnumId == locationType));
                }
                else
                {
                    query = query.Where(ii => string.IsNullOrEmpty(ii.LocationSeqId));
                }

                var inventoryItems = await query.ToListAsync();
                foreach (var inventoryItem in inventoryItems)
                {
                    if (reserveProductResult.QuantityNotReserved <= 0) break;

                    await ReserveForInventoryItemInline(
                        inventoryItem: inventoryItem,
                        reserveProductResult: reserveProductResult,
                        orderId: orderId,
                        orderItemSeqId: orderItemSeqId,
                        shipGroupSeqId: shipGroupSeqId,
                        reserveOrderEnumId: reserveOrderEnumId,
                        reservedDatetime: reservedDatetime ?? DateTime.UtcNow,
                        sequenceId: sequenceId?.ToString(),
                        priority: priority);

                    if (inventoryItem.InventoryItemTypeId == "NON_SERIAL_INV_ITEM")
                    {
                        lastNonSerInventoryItem = inventoryItem;
                    }
                }
            }

            if (reserveProductResult.QuantityNotReserved != 0)
            {
                if (requireInventory == "Y")
                {
                    throw new InsufficientInventoryException(productId, quantity,
                        reserveProductResult.QuantityNotReserved);
                }

                var productFacility = await _context.ProductFacilities
                    .FirstOrDefaultAsync(pf => pf.ProductId == productId && pf.FacilityId == facilityId);
                long daysToShip = productFacility?.DaysToShip ??
                                  (facility?.DefaultDaysToShip ?? 30);
                var orderHeader = await _context.OrderHeaders
                    .FirstOrDefaultAsync(oh => oh.OrderId == orderId);
                DateTime? promisedDatetime = orderHeader?.OrderDate != null
                    ? orderHeader.OrderDate.Value.AddDays(daysToShip)
                    : DateTime.UtcNow.AddDays(daysToShip);
                if (lastNonSerInventoryItem != null)
                {
                    var createDetailParam = new CreateInventoryItemDetailParam
                    {
                        InventoryItemId = lastNonSerInventoryItem.InventoryItemId,
                        OrderId = orderId,
                        OrderItemSeqId = orderItemSeqId,
                        AvailableToPromiseDiff = -reserveProductResult.QuantityNotReserved,
                        ReasonEnumId = reserveReasonEnumId
                    };
                    await CreateInventoryItemDetail(createDetailParam);

                    await ReserveOrderItemInventory(
                        orderId: orderId,
                        orderItemSeqId: orderItemSeqId,
                        shipGroupSeqId: shipGroupSeqId,
                        inventoryItemId: lastNonSerInventoryItem.InventoryItemId,
                        quantity: reserveProductResult.QuantityNotReserved,
                        reserveOrderEnumId: reserveOrderEnumId,
                        reservedDatetime: reservedDatetime ?? DateTime.UtcNow,
                        promisedDatetime: promisedDatetime,
                        quantityNotAvailable: reserveProductResult.QuantityNotReserved,
                        priority: priority);
                }
                else
                {
                    var inventoryItemId = await _utilityService.GetNextSequence("InventoryItem");
                    // check if facilityId is null and if yes, get the productStore for the logged-in user
                    string facilityIdToUse = null;
                    var productStore = await _productStoreService.GetProductStoreForLoggedInUser();
                    if (facilityId == null && productStore != null)
                    {
                        facilityIdToUse = productStore.InventoryFacilityId;
                    }
                    else
                    {
                        facilityIdToUse = facilityId;
                    }

                    var newInventoryItem = new InventoryItem
                    {
                        InventoryItemId = inventoryItemId,
                        ProductId = productId,
                        FacilityId = facilityIdToUse,
                        ContainerId = containerId,
                        InventoryItemTypeId = "NON_SERIAL_INV_ITEM",
                        AvailableToPromiseTotal = -reserveProductResult.QuantityNotReserved
                    };
                    _context.InventoryItems.Add(newInventoryItem);

                    var createDetailParam = new CreateInventoryItemDetailParam
                    {
                        InventoryItemId = newInventoryItem.InventoryItemId,
                        OrderId = orderId,
                        OrderItemSeqId = orderItemSeqId,
                        AvailableToPromiseDiff = -reserveProductResult.QuantityNotReserved,
                        ReasonEnumId = reserveReasonEnumId
                    };
                    await CreateInventoryItemDetail(createDetailParam);

                    await ReserveOrderItemInventory(
                        orderId: orderId,
                        orderItemSeqId: orderItemSeqId,
                        shipGroupSeqId: shipGroupSeqId,
                        inventoryItemId: newInventoryItem.InventoryItemId,
                        quantity: reserveProductResult.QuantityNotReserved,
                        reserveOrderEnumId: reserveOrderEnumId,
                        reservedDatetime: reservedDatetime ?? DateTime.UtcNow,
                        promisedDatetime: promisedDatetime,
                        quantityNotAvailable: reserveProductResult.QuantityNotReserved,
                        priority: priority);
                }

                reserveProductResult.QuantityNotReserved = 0;
            }

            return reserveProductResult.QuantityNotReserved;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ReserveProductInventory for product {ProductId}", productId);
            throw;
        }
    }

    private async Task ReserveForInventoryItemInline(
        InventoryItem inventoryItem,
        ReserveProductResult reserveProductResult,
        string orderId,
        string orderItemSeqId,
        string shipGroupSeqId,
        string reserveOrderEnumId,
        DateTime reservedDatetime,
        string reserveReasonEnumId = null,
        string sequenceId = null,
        int? priority = null)
    {
        try
        {
            if (reserveProductResult.QuantityNotReserved <= 0)
            {
                return;
            }

            if (inventoryItem.InventoryItemTypeId == "SERIALIZED_INV_ITEM" &&
                inventoryItem.StatusId == "INV_AVAILABLE")
            {
                // Change status on inventoryItem
                await UpdateInventoryItem(
                    inventoryItemId: inventoryItem.InventoryItemId,
                    statusId: "INV_PROMISED");

                // Store OrderItemShipGrpInvRes record
                DateTime? promisedDatetime = await GetPromisedDateTime(
                    DateTime.Now,
                    productId: inventoryItem.ProductId,
                    facilityId: inventoryItem.FacilityId);

                // REFACTOR: Used named parameters for ReserveOrderItemInventory
                // Purpose: Improves readability and aligns with method signature
                await ReserveOrderItemInventory(
                    orderId: orderId,
                    orderItemSeqId: orderItemSeqId,
                    shipGroupSeqId: shipGroupSeqId,
                    inventoryItemId: inventoryItem.InventoryItemId,
                    quantity: 1m,
                    reserveOrderEnumId: reserveOrderEnumId,
                    reservedDatetime: reservedDatetime,
                    promisedDatetime: promisedDatetime,
                    currentPromisedDate: promisedDatetime,
                    priority: priority,
                    sequenceId: string.IsNullOrEmpty(sequenceId) ? null : sequenceId);

                reserveProductResult.QuantityNotReserved -= 1m;
            }

            if (inventoryItem.InventoryItemTypeId == "NON_SERIAL_INV_ITEM")
            {
                string ebayReserveReasonEnumId = null;
                if (!string.IsNullOrEmpty(reserveReasonEnumId) && reserveReasonEnumId == "EBAY_INV_RES")
                {
                    ebayReserveReasonEnumId = reserveReasonEnumId;
                }

                // REFACTOR: Added explicit null check for AvailableToPromiseTotal
                // Purpose: Aligns with OFBiz's <not><if-empty> check
                if (inventoryItem.StatusId != "INV_NS_ON_HOLD" &&
                    inventoryItem.StatusId != "INV_NS_DEFECTIVE" &&
                    inventoryItem.AvailableToPromiseTotal.HasValue &&
                    inventoryItem.AvailableToPromiseTotal.Value > 0)
                {
                    decimal deductAmount = reserveProductResult.QuantityNotReserved >
                                           inventoryItem.AvailableToPromiseTotal.Value
                        ? inventoryItem.AvailableToPromiseTotal.Value
                        : reserveProductResult.QuantityNotReserved;

                    var createDetailParam = new CreateInventoryItemDetailParam
                    {
                        InventoryItemId = inventoryItem.InventoryItemId,
                        OrderId = orderId,
                        OrderItemSeqId = orderItemSeqId,
                        AvailableToPromiseDiff = -deductAmount
                    };
                    if (!string.IsNullOrEmpty(ebayReserveReasonEnumId))
                    {
                        createDetailParam.ReasonEnumId = reserveReasonEnumId;
                    }

                    await CreateInventoryItemDetail(createDetailParam);

                    if (string.IsNullOrEmpty(ebayReserveReasonEnumId))
                    {
                        DateTime? promisedDatetime = await GetPromisedDateTime(
                            DateTime.Now,
                            productId: inventoryItem.ProductId,
                            facilityId: inventoryItem.FacilityId);

                        // REFACTOR: Used named parameters for ReserveOrderItemInventory
                        // Purpose: Improves readability and aligns with method signature
                        await ReserveOrderItemInventory(
                            orderId: orderId,
                            orderItemSeqId: orderItemSeqId,
                            shipGroupSeqId: shipGroupSeqId,
                            inventoryItemId: inventoryItem.InventoryItemId,
                            quantity: deductAmount,
                            reserveOrderEnumId: reserveOrderEnumId,
                            reservedDatetime: reservedDatetime,
                            promisedDatetime: promisedDatetime,
                            currentPromisedDate: promisedDatetime,
                            priority: priority,
                            sequenceId: string.IsNullOrEmpty(sequenceId) ? null : sequenceId);
                    }

                    reserveProductResult.QuantityNotReserved -= deductAmount;
                }

                // REFACTOR: Moved lastNonSerInventoryItem tracking to caller
                // Purpose: Matches OFBiz's behavior, handled in ReserveProductInventory
            }
        }
        catch (Exception ex)
        {
            // REFACTOR: Replaced Console.WriteLine with ILogger
            // Purpose: Improves logging robustness
            _logger.LogError(ex, "Error in ReserveForInventoryItemInline for inventory item {InventoryItemId}",
                inventoryItem.InventoryItemId);
            throw;
        }
    }


    public async Task<UpdateInventoryItemResult> UpdateInventoryItem(
        string inventoryItemId,
        decimal? unitCost = null,
        string lotId = null,
        string statusId = null,
        string ownerPartyId = null,
        string productId = null,
        string facilityId = null)
    {
        try
        {
            // Find the inventory item
            var inventoryItem = await _context.InventoryItems.FindAsync(inventoryItemId);

            if (inventoryItem == null)
            {
                throw new Exception("Inventory item not found");
            }

            // Check and set ownerPartyId if empty
            if (string.IsNullOrEmpty(inventoryItem.OwnerPartyId))
            {
                var facility = await _context.Facilities.FindAsync(inventoryItem.FacilityId);
                if (facility != null)
                {
                    inventoryItem.OwnerPartyId = facility.OwnerPartyId;
                }
            }

            // Capture old values
            string oldOwnerPartyId = inventoryItem.OwnerPartyId;
            string oldStatusId = inventoryItem.StatusId;
            string oldProductId = inventoryItem.ProductId;
            decimal? oldUnitCost = inventoryItem.UnitCost;

            // Validate unit cost if provided
            if (unitCost.HasValue && unitCost.Value < 0)
            {
                throw new Exception("FacilityInventoryItemsUnitCostCannotBeNegative");
            }

            // Handle lot creation if lotId is provided
            if (!string.IsNullOrEmpty(lotId))
            {
                var lot = await _context.Lots.FindAsync(lotId);
                if (lot == null)
                {
                    lot = new Lot { LotId = lotId };
                    _context.Lots.Add(lot);
                }
            }

            // Update non-PK fields
            if (unitCost.HasValue)
            {
                inventoryItem.UnitCost = unitCost.Value;
            }

            if (!string.IsNullOrEmpty(statusId))
            {
                inventoryItem.StatusId = statusId;
            }

            if (!string.IsNullOrEmpty(ownerPartyId))
            {
                inventoryItem.OwnerPartyId = ownerPartyId;
            }

            if (!string.IsNullOrEmpty(productId))
            {
                inventoryItem.ProductId = productId;
            }

            if (!string.IsNullOrEmpty(facilityId))
            {
                inventoryItem.FacilityId = facilityId;
            }

            // Update the inventory item
            _context.InventoryItems.Update(inventoryItem);

            // If unit cost has changed, create an InventoryItemDetail
            if (unitCost.HasValue && oldUnitCost != unitCost.Value)
            {
                var createInventoryItemDetailParam = new CreateInventoryItemDetailParam
                {
                    InventoryItemId = inventoryItem.InventoryItemId,
                    AvailableToPromiseDiff = unitCost.Value - (oldUnitCost ?? 0)
                };
                await CreateInventoryItemDetail(createInventoryItemDetailParam);
            }

            // Return the old values
            return new UpdateInventoryItemResult
            {
                OldOwnerPartyId = oldOwnerPartyId,
                OldStatusId = oldStatusId,
                OldProductId = oldProductId
            };
        }
        catch (Exception ex)
        {
            // Log the exception details
            Console.WriteLine($"An error occurred in UpdateInventoryItem: {ex.Message}");
            throw;
        }
    }

    public async Task ReserveOrderItemInventory(
        string orderId,
        string orderItemSeqId,
        string shipGroupSeqId,
        string inventoryItemId,
        decimal quantity,
        string reserveOrderEnumId,
        DateTime? reservedDatetime = null,
        DateTime? promisedDatetime = null,
        DateTime? currentPromisedDate = null,
        int? priority = null,
        decimal? quantityNotAvailable = null,
        string sequenceId = null)
    {
        try
        {
            var checkOisgirEntity = await _context.OrderItemShipGrpInvRes
                .FirstOrDefaultAsync(o =>
                    o.OrderId == orderId && o.OrderItemSeqId == orderItemSeqId && o.ShipGroupSeqId == shipGroupSeqId &&
                    o.InventoryItemId == inventoryItemId);

            var orderItem = await _context.OrderItems
                .FirstOrDefaultAsync(oi => oi.OrderId == orderId && oi.OrderItemSeqId == orderItemSeqId);

            if (orderItem != null)
            {
                promisedDatetime = orderItem.ShipBeforeDate;
                currentPromisedDate = orderItem.ShipBeforeDate;
            }

            if (checkOisgirEntity == null)
            {
                var nowTimestamp = DateTime.Now;
                var newOisgirEntity = new OrderItemShipGrpInvRes
                {
                    OrderId = orderId,
                    OrderItemSeqId = orderItemSeqId,
                    ShipGroupSeqId = shipGroupSeqId,
                    InventoryItemId = inventoryItemId,
                    Quantity = quantity,
                    ReserveOrderEnumId = reserveOrderEnumId,
                    ReservedDatetime = reservedDatetime ?? nowTimestamp,
                    PromisedDatetime = promisedDatetime,
                    CurrentPromisedDate = currentPromisedDate,
                    Priority = priority,
                    CreatedDatetime = nowTimestamp,
                    // REFACTOR: Explicitly set QuantityNotAvailable
                    // Purpose: Ensures default value aligns with OFBiz
                    QuantityNotAvailable = quantityNotAvailable ?? 0,
                    SequenceId = sequenceId
                };

                _context.OrderItemShipGrpInvRes.Add(newOisgirEntity);
            }
            else
            {
                checkOisgirEntity.Quantity += quantity;
                // REFACTOR: Update all non-PK fields
                // Purpose: Aligns with OFBiz's set-nonpk-fields behavior
                checkOisgirEntity.ReserveOrderEnumId = reserveOrderEnumId;
                checkOisgirEntity.ReservedDatetime = reservedDatetime ?? DateTime.Now;
                checkOisgirEntity.PromisedDatetime = promisedDatetime;
                checkOisgirEntity.CurrentPromisedDate = currentPromisedDate;
                checkOisgirEntity.Priority = priority;
                if (quantityNotAvailable.HasValue)
                {
                    checkOisgirEntity.QuantityNotAvailable += quantityNotAvailable.Value;
                }

                checkOisgirEntity.SequenceId = sequenceId;

                _context.OrderItemShipGrpInvRes.Update(checkOisgirEntity);
            }
        }
        catch (Exception ex)
        {
            // REFACTOR: Replaced Console.WriteLine with ILogger
            // Purpose: Improves logging robustness
            _logger.LogError(ex, "Error in ReserveOrderItemInventory for order {OrderId}, item {OrderItemSeqId}",
                orderId, orderItemSeqId);
            throw;
        }
    }


    public async Task<DateTime?> GetPromisedDateTime(DateTime? orderDate, string productId, string facilityId)
    {
        try
        {
            // REFACTOR: Replaced FindAsync with FirstOrDefaultAsync
            // Purpose: Consistent with ReserveProductInventory's query style
            var productFacility = await _context.ProductFacilities
                .FirstOrDefaultAsync(pf => pf.ProductId == productId && pf.FacilityId == facilityId);

            var facility = await _context.Facilities
                .FirstOrDefaultAsync(f => f.FacilityId == facilityId);

            long daysToShip = 30;
            if (productFacility != null && productFacility.DaysToShip.HasValue)
            {
                daysToShip = productFacility.DaysToShip.Value;
            }
            else if (facility != null && facility.DefaultDaysToShip.HasValue)
            {
                daysToShip = facility.DefaultDaysToShip.Value;
            }

            // REFACTOR: Added null check for orderDate
            // Purpose: Matches OFBiz's fallback to current timestamp
            DateTime baseDate = orderDate ?? DateTime.UtcNow;
            var promisedDatetime = baseDate.AddDays(daysToShip);

            return promisedDatetime;
        }
        catch (Exception ex)
        {
            // REFACTOR: Replaced Console.WriteLine with ILogger
            // Purpose: Improves logging robustness
            _logger.LogError(ex, "Error in GetPromisedDateTime for product {ProductId}, facility {FacilityId}",
                productId, facilityId);
            throw;
        }
    }

    public async Task<bool> CheckOrderIsOnBackOrder(string orderId)
    {
        // Initialize variables
        decimal zeroEnv = 0;

        // Query OrderItemShipGrpInvRes
        var orderItemShipGrpInvResList = await _context.OrderItemShipGrpInvRes
            .Where(o => o.OrderId == orderId &&
                        o.QuantityNotAvailable != null &&
                        o.QuantityNotAvailable != zeroEnv)
            .ToListAsync();

        // Determine if order is on back order
        bool isBackOrder = orderItemShipGrpInvResList.Any();

        return isBackOrder;
    }


    public Lot CreateLot(string lotId = null, DateTime? creationDate = null, decimal? quantity = 1,
        DateTime? expirationDate = null)
    {
        try
        {
            var newLot = new Lot
            {
                LotId = lotId ?? Guid.NewGuid().ToString(), // Generate a new GUID if LotId is not provided
                CreationDate = creationDate ?? DateTime.UtcNow,
                Quantity = quantity ?? 1m,
                ExpirationDate = expirationDate,
                CreatedStamp = DateTime.UtcNow,
                LastUpdatedStamp = DateTime.UtcNow
            };

            _context.Lots.Add(newLot);

            return newLot;
        }
        catch (Exception ex)
        {
            // Handle exceptions as necessary
            throw new Exception($"Error creating Lot: {ex.Message}");
        }
    }

    /// <summary>
    /// Creates an inventory item status record.
    /// </summary>
    /// <param name="inventoryItemId">The ID of the inventory item.</param>
    /// <param name="statusId">The status ID to associate with the inventory item.</param>
    /// <param name="parameters">Optional additional parameters for the inventory item status.</param>
    /// <param name="userLoginId">The ID of the user performing the operation.</param>
    public async Task CreateInventoryItemStatus(string inventoryItemId, string statusId, dynamic parameters,
        string userLoginId)
    {
        try
        {
            var nowTimestamp = DateTime.UtcNow;

            // Find the most recent InventoryItemStatus record and set the statusEndDatetime
            var oldInventoryItemStatus = await _context.InventoryItemStatuses
                .Where(iis => iis.InventoryItemId == inventoryItemId)
                .OrderByDescending(iis => iis.StatusDatetime)
                .FirstOrDefaultAsync();

            if (oldInventoryItemStatus != null)
            {
                oldInventoryItemStatus.StatusEndDatetime = nowTimestamp;
                _context.InventoryItemStatuses.Update(oldInventoryItemStatus);
            }

            // Create a new InventoryItemStatus entity
            var inventoryItemStatus = new InventoryItemStatus
            {
                InventoryItemId = inventoryItemId,
                StatusId = statusId,
                StatusDatetime = nowTimestamp,
                ChangeByUserLoginId = userLoginId
            };

            // Map additional parameters to non-PK fields
            if (parameters != null)
            {
                foreach (var prop in parameters.GetType().GetProperties())
                {
                    var propValue = prop.GetValue(parameters);
                    var entityProp = inventoryItemStatus.GetType().GetProperty(prop.Name);
                    if (entityProp != null && entityProp.Name != nameof(inventoryItemStatus.InventoryItemId) &&
                        entityProp.Name != nameof(inventoryItemStatus.StatusId))
                    {
                        entityProp.SetValue(inventoryItemStatus, propValue);
                    }
                }
            }

            // Ensure the productId is set
            if (string.IsNullOrEmpty(inventoryItemStatus.ProductId))
            {
                var inventoryItem = await _context.InventoryItems
                    .FirstOrDefaultAsync(ii => ii.InventoryItemId == inventoryItemId);
                if (inventoryItem != null)
                {
                    inventoryItemStatus.ProductId = inventoryItem.ProductId;
                }
            }

            // Add the new InventoryItemStatus record
            await _context.InventoryItemStatuses.AddAsync(inventoryItemStatus);
        }
        catch (Exception ex)
        {
            // Log the exception
            Console.Error.WriteLine($"Error in CreateInventoryItemStatus: {ex.Message}");
            throw;
        }
    }

    public async Task<Result<string>> CreatePhysicalInventoryAndVariance(PhysicalInventoryVarianceDto dto)
    {
        try
        {
            // ✅ Step 1: Create Physical Inventory
            var createPhysicalInventoryDto = new CreatePhysicalInventoryDto
            {
                PhysicalInventoryDate = dto.PhysicalInventoryDate,
                PartyId = dto.PartyId,
                GeneralComments = dto.GeneralComments
            };

            var physicalInventoryResult = CreatePhysicalInventory(createPhysicalInventoryDto);

            // 🔴 Check if creation failed
            if (!physicalInventoryResult.IsSuccess)
            {
                return Result<string>.Failure(
                    $"Failed to create Physical Inventory: {physicalInventoryResult.Error}");
            }

            // ✅ Step 2: Retrieve Physical Inventory ID from result
            var physicalInventoryId = physicalInventoryResult.Value;

            // ✅ Step 3: Create Inventory Item Variance
            dto.PhysicalInventoryId = physicalInventoryId; // Pass ID to Variance
            var varianceResult = await CreateInventoryItemVariance(dto);
            if (!varianceResult.IsSuccess)
            {
                return Result<string>.Failure($"Failed to create InventoryItemVariance: {varianceResult.Error}");
            }

            // 🟢 Step 4: Integrated Event (ECA) - On Commit, Call AcctgTrans
            // Event: On success (commit), Condition: physicalInventoryId is not empty
            if (!string.IsNullOrWhiteSpace(physicalInventoryId))
            {
                var acctgTransDto = new CreateAcctgTransForPhysicalInventoryVarianceDto
                {
                    PhysicalInventoryId = physicalInventoryId
                };

                var acctgTransResult =
                    await _generalLedgerService.Value.CreateAcctgTransForPhysicalInventoryVariance(acctgTransDto);
            }

            // ✅ Return the Physical Inventory ID on success
            return Result<string>.Success(physicalInventoryId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Physical Inventory and Variance");
            return Result<string>.Failure("An error occurred during creation.");
        }
    }

    private Result<string> CreatePhysicalInventory(CreatePhysicalInventoryDto dto)
    {
        try
        {
            // Step 2: Create Physical Inventory Record
            var physicalInventory = new PhysicalInventory
            {
                PhysicalInventoryId = Guid.NewGuid().ToString(),
                PhysicalInventoryDate = dto.PhysicalInventoryDate ?? DateTime.UtcNow,
                PartyId = dto.PartyId,
                GeneralComments = dto.GeneralComments,
                CreatedStamp = DateTime.UtcNow,
                LastUpdatedStamp = DateTime.UtcNow,
            };

            _context.PhysicalInventories.Add(physicalInventory);

            return Result<string>.Success(physicalInventory.PhysicalInventoryId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create Physical Inventory");
            return Result<string>.Failure("An error occurred while creating Physical Inventory.");
        }
    }

    public async Task<Result<bool>> CreateInventoryItemVariance(PhysicalInventoryVarianceDto dto)
    {
        try
        {
            // Step 1: Find InventoryItem by Primary Key
            var inventoryItem = await _context.InventoryItems
                .FirstOrDefaultAsync(i => i.InventoryItemId == dto.InventoryItemId);

            if (inventoryItem == null)
            {
                return Result<bool>.Failure("InventoryItem not found.");
            }

            // Step 2: Validate InventoryItem Type
            if (inventoryItem.InventoryItemTypeId != "NON_SERIAL_INV_ITEM")
            {
                return Result<bool>.Failure(
                    "Can only create an InventoryItemVariance for a Non-Serialized Inventory Item.");
            }

            // Step 3: Create InventoryItemDetail

            var inventoryItemDetail = new CreateInventoryItemDetailParam
            {
                InventoryItemId = dto.InventoryItemId,
                PhysicalInventoryId = dto.PhysicalInventoryId,
                AvailableToPromiseDiff = dto.AvailableToPromiseVar,
                QuantityOnHandDiff = dto.QuantityOnHandVar,
                AccountingQuantityDiff = dto.QuantityOnHandVar,
                ReasonEnumId = dto.VarianceReasonId,
                Description = dto.Comments,
            };

            await CreateInventoryItemDetail(inventoryItemDetail);

            // Step 4: Create InventoryItemVariance
            var inventoryItemVariance = new InventoryItemVariance
            {
                InventoryItemId = dto.InventoryItemId,
                PhysicalInventoryId = dto.PhysicalInventoryId,
                AvailableToPromiseVar = dto.AvailableToPromiseVar ?? 0,
                QuantityOnHandVar = dto.QuantityOnHandVar ?? 0,
                VarianceReasonId = dto.VarianceReasonId,
                Comments = dto.Comments,
                CreatedStamp = DateTime.UtcNow,
                CreatedTxStamp = DateTime.UtcNow
            };
            _context.InventoryItemVariances.Add(inventoryItemVariance);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating InventoryItemVariance");
            return Result<bool>.Failure("An error occurred while creating InventoryItemVariance.");
        }
    }
}