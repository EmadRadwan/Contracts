using Application.Accounting.Services;
using Application.Catalog.Products;
using Application.Catalog.Products.Services.Cost;
using Application.Catalog.Products.Services.Inventory;
using Application.Core;
using Application.Facilities;
using Application.Order.Orders;
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Shipments;

public interface IShipmentService
{
    OrderItemShipGroup CreateOrderItemShipGroup(string orderId);

    Task<OperationResult> AddOrderItemShipGroupAssoc(
        string orderId,
        string orderItemSeqId,
        string? shipGroupSeqId,
        decimal quantity);

    Task<ItemIssuance> GetItemIssuance(OrderItemDto2 orderItem);
    Task<ShipmentReceipt> GetShipmentReceipt(OrderItemDto2 orderItem);
    Task<ShipmentItem> CreateShipmentItem(ShipmentItemCreateParameters parameters);

    Task<OperationResult> CreateShipmentForFacilityAndShipGroup(
        OrderHeader orderHeader,
        string facilityId,
        List<OrderItemShipGroup> orderItemShipGroupList,
        DateTime eventDate,
        string? setPackedOnly = null);

    Task<ReceiveInventoryResult> ReceiveInventoryProduct(
        string inventoryItemTypeId,
        decimal quantityAccepted,
        decimal quantityRejected,
        string? serialNumber,
        string? currentInventoryItemId,
        string? statusId,
        string orderId,
        string orderItemSeqId,
        string productId,
        string facilityId,
        string? returnId,
        decimal? orderCurrencyUnitPrice = null,
        string? color = null);

    Task<OperationResult> QuickReceivePurchaseOrder(string orderId, string facilityId);
}

public class ShipmentService : IShipmentService
{
    private readonly DataContext _context;
    private readonly ICostService _costService;
    private readonly IFacilityService _facilityService;
    private readonly IGeneralLedgerService _generalLedgerService;
    private readonly IInventoryService _inventoryService;

    private readonly ILogger _logger;
    private readonly IProductService _productService;
    private readonly IUtilityService _utilityService;
    private readonly IShipmentHelperService _shipmentHelperService;
    private readonly IIssuanceService _issuanceService;

    public ShipmentService(DataContext context, IUtilityService utilityService,
        IFacilityService facilityService, ILogger<ShipmentService> logger,
        IProductService productService, IGeneralLedgerService generalLedgerService, IInventoryService inventoryService
        , ICostService costService, IShipmentHelperService shipmentHelperService, IIssuanceService issuanceService)
    {
        _context = context;
        _utilityService = utilityService;
        _facilityService = facilityService;
        _productService = productService;
        _generalLedgerService = generalLedgerService;
        _inventoryService = inventoryService;
        _logger = logger;
        _costService = costService;
        _shipmentHelperService = shipmentHelperService;
        _issuanceService = issuanceService;
    }


    public async Task<ItemIssuance> GetItemIssuance(OrderItemDto2 orderItem)
    {
        var itemIssuance = _context.ItemIssuances.Local
            .FirstOrDefault(x => x.OrderId == orderItem.OrderId && x.OrderItemSeqId == orderItem.OrderItemSeqId);
        return await Task.FromResult(itemIssuance);
    }

    public async Task<ShipmentReceipt> GetShipmentReceipt(OrderItemDto2 orderItem)
    {
        var shipmentReceipt = _context.ShipmentReceipts.Local
            .FirstOrDefault(x => x.OrderId == orderItem.OrderId && x.OrderItemSeqId == orderItem.OrderItemSeqId);
        return await Task.FromResult(shipmentReceipt);
    }

    public OrderItemShipGroup CreateOrderItemShipGroup(string orderId)
    {
        var stamp = DateTime.UtcNow;
        var orderItemShipGroup = new OrderItemShipGroup
        {
            OrderId = orderId,
            ShipGroupSeqId = "01",
            LastUpdatedStamp = stamp,
            CreatedStamp = stamp
        };
        _context.OrderItemShipGroups.Add(orderItemShipGroup);

        return orderItemShipGroup;
    }
    
    public async Task<CreateShipmentReceiptResult> CreateShipmentReceipt(CreateShipmentReceiptParameters parameters)
    {
        var result = new CreateShipmentReceiptResult();

        try
        {
            // Generate a unique receiptId
            var newShipmentReceiptSequence = await _utilityService.GetNextSequence("ShipmentReceipt");

            // Create a new ShipmentReceipt entity
            var shipmentReceipt = new ShipmentReceipt
            {
                InventoryItemId = parameters.InventoryItemId,
                OrderId = parameters.OrderId,
                OrderItemSeqId = parameters.OrderItemSeqId,
                ProductId = parameters.ProductId,
                QuantityAccepted = parameters.QuantityAccepted,
                ShipmentId = parameters.ShipmentId,
                ShipmentItemSeqId = parameters.ShipmentItemSeqId,
                ReturnId = parameters.ReturnId,
                ReturnItemSeqId = parameters.ReturnItemSeqId,
                QuantityRejected = parameters.QuantityRejected ?? 0,
                DatetimeReceived = DateTime.UtcNow,
                CreatedStamp = DateTime.UtcNow,
                LastUpdatedStamp = DateTime.UtcNow,
                ReceiptId = newShipmentReceiptSequence
            };

            // Save the ShipmentReceipt
            _context.ShipmentReceipts.Add(shipmentReceipt);

            // Return the receiptId
            result.ReceiptId = shipmentReceipt.ReceiptId;

            // Update InventoryItemDetail with receiptId if inventoryItemDetailSeqId is provided
            if (!string.IsNullOrEmpty(parameters.InventoryItemDetailSeqId))
            {
                InventoryItemDetail? inventoryItemDetail;

                // Fetch relevant data from the change tracker or database
                var addedInventoryItemDetail = _context.ChangeTracker.Entries<InventoryItemDetail>()
                    .Where(e => e.State == EntityState.Added && e.Entity.InventoryItemId == parameters.InventoryItemId)
                    .Select(e => e.Entity)
                    .FirstOrDefault();

                if (addedInventoryItemDetail != null)
                    inventoryItemDetail = addedInventoryItemDetail;
                else
                    inventoryItemDetail = await _context.InventoryItemDetails
                        .FirstOrDefaultAsync(x => x.InventoryItemId == parameters.InventoryItemId);

                if (inventoryItemDetail != null)
                {
                    inventoryItemDetail.ReceiptId = newShipmentReceiptSequence;
                }
            }

            // Determine affectAccounting
            bool affectAccounting = true;

            var product = await _context.Products.FindAsync(parameters.ProductId);
            if (product != null)
            {
                if (product.ProductTypeId == "SERVICE_PRODUCT" ||
                    product.ProductTypeId == "ASSET_USAGE_OUT_IN" ||
                    product.ProductTypeId == "AGGREGATEDSERV_CONF")
                {
                    affectAccounting = false;
                }
            }

            // Create accounting transaction for shipment receipt
            await _generalLedgerService.CreateAcctgTransForShipmentReceipt(newShipmentReceiptSequence);

            // Update order status
            await UpdateOrderStatusFromReceipt(parameters.OrderId);

            // Update purchase shipment from receipt
            await _shipmentHelperService.UpdatePurchaseShipmentFromReceipt(parameters.ShipmentId);

            result.AffectAccounting = affectAccounting;
        }
        catch (Exception ex)
        {
            // Log the exception (assuming you have a logger available)
            _logger.LogError(ex, "Error occurred while creating shipment receipt");

            // Return a result indicating failure
            result.ErrorMessage = $"An error occurred: {ex.Message}";
        }

        return result;
    }

    public async Task<ReceiveInventoryResult> ReceiveInventoryProduct(
        string inventoryItemTypeId,
        decimal quantityAccepted,
        decimal quantityRejected,
        string? serialNumber,
        string? currentInventoryItemId,
        string? statusId,
        string orderId,
        string orderItemSeqId,
        string productId,
        string facilityId,
        string? returnId,
        decimal? orderCurrencyUnitPrice = null,
        string? color = null)
    {
        try
        {
            // PART 1: Initialization of Variables
            double loops = 1;

            // PART 2: Validation for Serialized Inventory
            if (inventoryItemTypeId == "SERIALIZED_INV_ITEM")
            {
                if ((!string.IsNullOrEmpty(serialNumber) || !string.IsNullOrEmpty(currentInventoryItemId)) &&
                    quantityAccepted > 1)
                {
                    throw new Exception(
                        "Error: For serialized items, quantityAccepted must not be greater than 1 when a serial number or currentInventoryItemId is provided.");
                }

                // PART 3: Adjustments for Serialized Inventory
                loops = (double)quantityAccepted;
                quantityAccepted = 1;
            }

            // PART 4: Setting Default Parameters
            decimal quantityOnHandDiff = quantityAccepted;
            decimal availableToPromiseDiff = quantityAccepted;

            // PART 5: Error Checking
            // Validation logic assumed external, similar to <check-errors/>.

            // PART 6: Handling Status for Non-Serialized Items
            if (inventoryItemTypeId == "NON_SERIAL_INV_ITEM")
            {
                if (statusId == "INV_DEFECTIVE")
                {
                    statusId = "INV_NS_DEFECTIVE";
                }
                else if (statusId == "INV_ON_HOLD")
                {
                    statusId = "INV_NS_ON_HOLD";
                }
                else if (statusId == "INV_RETURNED")
                {
                    statusId = "INV_NS_RETURNED";
                }
                else if (statusId != "INV_NS_DEFECTIVE" && statusId != "INV_NS_ON_HOLD" &&
                         statusId != "INV_NS_RETURNED")
                {
                    statusId = null;
                }
            }

            string supplierPartyId = null;

            // PART 6: Supplier Party Handling
            if (!string.IsNullOrEmpty(orderId))
            {
                var orderRoles = await _context.OrderRoles
                    .Where(o => o.OrderId == orderId && o.RoleTypeId == "SUPPLIER_AGENT")
                    .ToListAsync();

                if (orderRoles.Any())
                {
                    supplierPartyId = orderRoles.First().PartyId;
                    // Logic for assigning supplierPartyId to inventory updates can go here
                }
            }

            var order = await _context.OrderHeaders.FindAsync(orderId);
            var currencyUomId = order.CurrencyUom;


            // PART 7: Main Processing Loop
            for (int currentLoop = 0; currentLoop < loops; currentLoop++)
            {
                //Console.WriteLine($"Processing inventory item - Loop {currentLoop + 1}");

                // PART 8: Inventory Item Handling
                if (string.IsNullOrEmpty(currentInventoryItemId))
                {
                    // create inventory item param object and call CreateInventoryItem
                    var createInventoryItemParam = new CreateInventoryItemParam
                    {
                        FacilityId = facilityId,
                        QuantityOnHandTotal = quantityAccepted,
                        ProductId = productId,
                        SupplierId = supplierPartyId,
                        FacilityOwnerPartyId = facilityId,
                        CurrencyUomId = currencyUomId,
                        OrderId = orderId,
                        OrderItemSeqId = orderItemSeqId,
                    };

                    currentInventoryItemId = await _inventoryService.CreateInventoryItem(
                        createInventoryItemParam);
                    
                    if (!string.IsNullOrEmpty(color))
                    {
                        var inventoryItemFeature = new InventoryItemFeature
                        {
                            InventoryItemId = currentInventoryItemId,
                            ProductFeatureId = color,
                            ProductId = productId
                        };
                        _context.InventoryItemFeatures.Add(inventoryItemFeature);
                    }
                }
                else
                {
                    // I'm not currently handling serialized inventory
                    /*_inventoryService.UpdateInventoryItem(currentInventoryItemId, statusId, quantityOnHandDiff,
                        availableToPromiseDiff);*/
                }


                // PART 10: Creating Inventory Item Details (only for non-serialized items)
                if (inventoryItemTypeId != "SERIALIZED_INV_ITEM")
                {
                    var inventoryItemDetailParam = new CreateInventoryItemDetailParam
                    {
                        InventoryItemId = currentInventoryItemId,
                        QuantityOnHandDiff = quantityAccepted,
                        AvailableToPromiseDiff = quantityAccepted,
                        AccountingQuantityDiff = 0,
                        OrderId = orderId,
                        OrderItemSeqId = orderItemSeqId
                    };
                    await _inventoryService.CreateInventoryItemDetail(inventoryItemDetailParam);
                }

                var shipmentId = await GetShipmentIdFromOrderShipment(orderId);

                // Create shipment receipt
                var shipmentReceipt = new CreateShipmentReceiptParameters
                {
                    InventoryItemId = currentInventoryItemId,
                    OrderId = orderId,
                    OrderItemSeqId = orderItemSeqId,
                    ProductId = productId,
                    QuantityAccepted = quantityAccepted,
                    QuantityRejected = quantityRejected,
                    FacilityId = facilityId,
                    ShipmentId = shipmentId,
                };

                // PART 11: Shipment Receipt
                await CreateShipmentReceipt(shipmentReceipt);

                // PART 12: Status Update for Serialized Items
                if (inventoryItemTypeId == "SERIALIZED_INV_ITEM" && string.IsNullOrEmpty(returnId))
                {
                    var inventoryItem = await _context.InventoryItems.FindAsync(currentInventoryItemId);
                    if (inventoryItem != null &&
                        inventoryItem.StatusId != "INV_PROMISED" &&
                        inventoryItem.StatusId != "INV_ON_HOLD")
                    {
                        inventoryItem.StatusId = "INV_AVAILABLE";
                    }
                }

                // PART 13: Balancing Inventory
                //await _inventoryService.BalanceInventoryItems(currentInventoryItemId, quantityOnHandDiff);
            }

            // PART 15: Returning Results
            //return currentInventoryItemId;
            return new ReceiveInventoryResult();
        }
        catch (Exception ex)
        {
            // PART 16: Exception Handling
            Console.WriteLine($"Error: {ex.Message}");
            throw;
        }
    }


    private async Task<string> GetShipmentIdFromOrderShipment(string orderId)
    {
        var shipment =
            await _context.OrderShipments.FirstOrDefaultAsync(x => x.OrderId == orderId);
        return shipment?.ShipmentId;
    }

    private async Task<string> UpdateOrderStatusFromReceipt(string orderId)
    {
        var stamp = DateTime.UtcNow;
        // Fetch OrderHeader entity
        var orderHeader = await _context.OrderHeaders.FirstOrDefaultAsync(o => o.OrderId == orderId);

        if (orderHeader == null)
            // Handle case where order is not found
            return null;


        var shipmentReceipts = await _utilityService.FindLocalOrDatabaseListAsync<ShipmentReceipt>(
            query => query.Where(sr => sr.OrderId == orderId));

        // Create a list to store cumulative totals
        var cumulativeTotalsList = new List<TotalsInfo>();


        // Iterate through shipmentReceipts
        foreach (var receipt in shipmentReceipts)
        {
            // Check if cumulativeTotalsList contains TotalsInfo for the current OrderItemSeqId
            var currentTotalsInfo =
                cumulativeTotalsList.FirstOrDefault(t => t.OrderItemSeqId == receipt.OrderItemSeqId);

            if (currentTotalsInfo == null)
            {
                // If not found, create a new TotalsInfo and add it to the list
                currentTotalsInfo = new TotalsInfo { OrderItemSeqId = receipt.OrderItemSeqId, CumulativeTotal = 0 };
                cumulativeTotalsList.Add(currentTotalsInfo);
            }

            currentTotalsInfo.CumulativeTotal += receipt.QuantityAccepted + receipt.QuantityRejected;

            // Fetch OrderItem entity
            var orderItem = _context.OrderItems
                .FirstOrDefault(item => item.OrderId == orderId && item.OrderItemSeqId == receipt.OrderItemSeqId);

            //TODO: check what is the status of the order item at this point
            if (orderItem != null && orderItem.StatusId != "ITEM_COMPLETED" &&
                currentTotalsInfo.CumulativeTotal >= orderItem.Quantity)
            {
                // Update the status for the item
                orderItem.StatusId = "ITEM_COMPLETED";

                // Create status change history
                var newValue = new OrderStatus
                {
                    OrderStatusId = Guid.NewGuid().ToString(),
                    OrderItemSeqId = orderItem.OrderItemSeqId,
                    OrderId = orderItem.OrderId,
                    StatusId = "ITEM_COMPLETED",
                    StatusDatetime = stamp,
                    CreatedStamp = DateTime.UtcNow,
                    LastUpdatedStamp = DateTime.UtcNow
                };

                _context.OrderStatuses.Add(newValue);
            }
        }

        // Check to see if all items have been completed
        var allOrderItems = await _context.OrderItems
            .Where(item => item.OrderId == orderId)
            .ToListAsync();

        var allCompleted = true;

        foreach (var item in allOrderItems)
            if (item.StatusId != "ITEM_COMPLETED")
            {
                allCompleted = false;
                break;
            }

        if (allCompleted)
        {
            // Update the order header
            orderHeader.StatusId = "ORDER_COMPLETED";

            // Create the status history
            var newValue = new OrderStatus
            {
                OrderStatusId = Guid.NewGuid().ToString(),
                OrderId = orderHeader.OrderId,
                StatusId = "ORDER_COMPLETED",
                StatusDatetime = stamp,
                CreatedStamp = DateTime.UtcNow,
                LastUpdatedStamp = DateTime.UtcNow
            };

            _context.OrderStatuses.Add(newValue);
        }

        // Return the current order header status
        return orderHeader.StatusId;
    }

    /// <summary>
    /// Quick receives an entire purchase order by creating shipments for each ship group in a facility.
    /// </summary>
    /// <param name="orderId">The ID of the order to receive.</param>
    /// <param name="facilityId">The facility ID where the order will be received.</param>
    /// <returns>A result containing the list of created shipment IDs.</returns>
    public async Task<OperationResult> QuickReceivePurchaseOrder(string orderId, string facilityId)
    {
        try
        {
            // Retrieve the order header and validate existence
            var orderHeader = await _context.OrderHeaders.FindAsync(orderId);
            if (orderHeader == null)
                return OperationResult.Failure("Order not found.");

            // Retrieve the facility and validate existence
            var facility = await _context.Facilities.FindAsync(facilityId);
            if (facility == null)
                return OperationResult.Failure("Facility not found.");

            // Call GetOrderItemShipGroupLists to retrieve ship groups
            var getShipGroupListResult = await GetOrderItemShipGroupLists(orderId);
            if (!getShipGroupListResult.IsSuccess)
            {
                return OperationResult.Failure(getShipGroupListResult.Message);
            }

            var orderItemShipGroupList = getShipGroupListResult.ShipGroupList;

            // Call CreateShipmentForFacilityAndShipGroup to create shipments
            var createShipmentResult = await CreateShipmentForFacilityAndShipGroup(
                orderHeader,
                facilityId,
                orderItemShipGroupList,
                DateTime.UtcNow,
                null // Assuming 'setPackedOnly' is a flag indicating whether to set only packed status
            );

            if (!createShipmentResult.IsSuccess)
            {
                return OperationResult.Failure(createShipmentResult.Message);
            }

            /*// Log the completion of the quick receive process
            Console.WriteLine(
                $"Finished quickReceivePurchaseOrder for orderId {orderId} and destination facilityId {facilityId}, shipment(s) created: {string.Join(", ", createShipmentResult.ShipmentIds)}");
                */

            // Return the result with the list of shipment IDs created
            return OperationResult.Success("Quick receive process completed successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in QuickReceivePurchaseOrderAsync: {ex.Message}");
            return OperationResult.Failure($"An error occurred: {ex.Message}");
        }
    }


    /// <summary>
    /// Retrieves order item ship group lists for a given order, used by quickReceivePurchaseOrder.
    /// </summary>
    /// <param name="orderId">The ID of the order.</param>
    /// <returns>A result containing the list of OrderItemShipGroups.</returns>
    private async Task<ShipGroupListResult> GetOrderItemShipGroupLists(string orderId)
    {
        // Retrieve all approved OrderItem and OrderItemShipGroupAssoc entries joined on the defined fields
        var orderItemAndShipGroupAssocList = await (
            from oi in _context.OrderItems
            join oisga in _context.OrderItemShipGroupAssocs
                on new { oi.OrderId, oi.OrderItemSeqId } equals new { oisga.OrderId, oisga.OrderItemSeqId }
            where oi.OrderId == orderId && oi.StatusId == "ITEM_APPROVED"
            select new
            {
                oi.OrderId,
                oi.OrderItemSeqId,
                oisga.ShipGroupSeqId,
                oi.StatusId,
                oi.Quantity,
                oi.CancelQuantity
            }).ToListAsync();

        // Ensure there are items available to ship
        if (!orderItemAndShipGroupAssocList.Any())
        {
            return ShipGroupListResult.Failure(
                "FacilityNoItemsAvailableToShip: No items are available to ship for this order.");
        }

        // Retrieve related OrderItemShipGroup records for the order
        var shipGroupSeqIds = orderItemAndShipGroupAssocList.Select(o => o.ShipGroupSeqId).Distinct().ToList();

        var orderItemShipGroupList = await _context.OrderItemShipGroups
            .Where(oisg => oisg.OrderId == orderId && shipGroupSeqIds.Contains(oisg.ShipGroupSeqId))
            .ToListAsync();

        // Log the list of order item ship groups for debugging
        Console.WriteLine("Order item ship groups retrieved: " +
                          string.Join(", ", orderItemShipGroupList.Select(g => g.ShipGroupSeqId)));

        return ShipGroupListResult.Success(orderItemShipGroupList);
    }

    public async Task<OperationResult> CreateShipmentForFacilityAndShipGroup(
        OrderHeader orderHeader,
        string facilityId,
        List<OrderItemShipGroup> orderItemShipGroupList,
        DateTime eventDate,
        string? setPackedOnly = null)
    {
        var shipmentIds = new List<string>();

        foreach (var orderItemShipGroup in orderItemShipGroupList)
        {
            try
            {
                // Retrieve approved items for this ship group
                var perShipGroupItemList = await (
                    from oisga in _context.OrderItemShipGroupAssocs
                    join oi in _context.OrderItems on new { oisga.OrderId, oisga.OrderItemSeqId } equals new
                        { oi.OrderId, oi.OrderItemSeqId }
                    where oisga.OrderId == orderHeader.OrderId &&
                          oisga.ShipGroupSeqId == orderItemShipGroup.ShipGroupSeqId &&
                          oi.StatusId == "ITEM_APPROVED"
                    select new
                    {
                        oi.OrderId,
                        oi.OrderItemSeqId,
                        oisga.ShipGroupSeqId,
                        oi.Quantity,
                        oi.CancelQuantity,
                        oi.ProductId
                        // Include other necessary fields
                    }).ToListAsync();

                if (!perShipGroupItemList.Any())
                {
                    Console.WriteLine(
                        $"No items available to ship for shipGroupSeqId {orderItemShipGroup.ShipGroupSeqId}");
                    continue;
                }

                // Retrieve facilityId from OrderItemShipGrpInvRes
                var orderItemShipGrpInvResFacilityId = await (
                    from oisgir in _context.OrderItemShipGrpInvRes
                    where oisgir.OrderId == orderHeader.OrderId &&
                          oisgir.ShipGroupSeqId == orderItemShipGroup.ShipGroupSeqId
                    select oisgir.InventoryItem.FacilityId
                ).FirstOrDefaultAsync();

                if (string.IsNullOrEmpty(orderItemShipGrpInvResFacilityId) && orderHeader.OrderTypeId == "SALES_ORDER")
                {
                    throw new Exception("FacilityId could not be determined from OrderItemShipGrpInvRes.");
                }

                // Initialize shipment context
                var shipmentContext = new ShipmentContext
                {
                    PrimaryOrderId = orderHeader.OrderId,
                    PrimaryShipGroupSeqId = orderItemShipGroup.ShipGroupSeqId,
                };

                if (orderHeader.OrderTypeId == "SALES_ORDER")
                {
                    shipmentContext.StatusId = "SHIPMENT_INPUT";
                    shipmentContext.OriginFacilityId = orderItemShipGrpInvResFacilityId;
                    shipmentContext.ShipmentTypeId = "SALES_SHIPMENT";

                    // Determine PartyIdFrom
                    string partyIdFrom = null;

                    if (!string.IsNullOrEmpty(orderItemShipGroup.VendorPartyId))
                    {
                        partyIdFrom = orderItemShipGroup.VendorPartyId;
                    }
                    else
                    {
                        // Get facility.ownerPartyId
                        var facilityOwnerPartyId = await _context.Facilities
                            .Where(f => f.FacilityId == orderItemShipGrpInvResFacilityId)
                            .Select(f => f.OwnerPartyId)
                            .FirstOrDefaultAsync();

                        if (!string.IsNullOrEmpty(facilityOwnerPartyId))
                        {
                            partyIdFrom = facilityOwnerPartyId;
                        }
                        else
                        {
                            // Get OrderRole with SHIP_FROM_VENDOR
                            var orderRolePartyId = await _context.OrderRoles
                                .Where(or => or.OrderId == orderHeader.OrderId && or.RoleTypeId == "SHIP_FROM_VENDOR")
                                .Select(or => or.PartyId)
                                .FirstOrDefaultAsync();

                            if (!string.IsNullOrEmpty(orderRolePartyId))
                            {
                                partyIdFrom = orderRolePartyId;
                            }
                            else
                            {
                                // Get OrderRole with BILL_FROM_VENDOR
                                orderRolePartyId = await _context.OrderRoles
                                    .Where(or =>
                                        or.OrderId == orderHeader.OrderId && or.RoleTypeId == "BILL_FROM_VENDOR")
                                    .Select(or => or.PartyId)
                                    .FirstOrDefaultAsync();

                                if (!string.IsNullOrEmpty(orderRolePartyId))
                                {
                                    partyIdFrom = orderRolePartyId;
                                }
                                else
                                {
                                    throw new Exception("PartyIdFrom could not be determined.");
                                }
                            }
                        }
                    }

                    shipmentContext.PartyIdFrom = partyIdFrom;
                }
                else if (orderHeader.OrderTypeId == "PURCHASE_ORDER")
                {
                    shipmentContext.StatusId = "PURCH_SHIP_CREATED";
                    shipmentContext.DestinationFacilityId = facilityId;
                    shipmentContext.ShipmentTypeId = "PURCHASE_SHIPMENT";
                    shipmentContext.PrimaryOrderId = orderHeader.OrderId;
                }
                else
                {
                    throw new Exception($"Unsupported OrderTypeId: {orderHeader.OrderTypeId}");
                }

                // Create the shipment
                string shipmentId;
                try
                {
                    shipmentId = await _shipmentHelperService.CreateShipment(shipmentContext);
                }
                catch (Exception ex)
                {
                    throw new Exception("Failed to create shipment.", ex);
                }

                // Issue items to the shipment
                if (orderHeader.OrderTypeId == "SALES_ORDER")
                {
                    foreach (var orderItem in perShipGroupItemList)
                    {
                        // Retrieve OrderItemShipGrpInvRes records
                        var itemResList = await (
                            from oisgir in _context.OrderItemShipGrpInvRes
                            where oisgir.OrderId == orderItem.OrderId &&
                                  oisgir.OrderItemSeqId == orderItem.OrderItemSeqId &&
                                  oisgir.ShipGroupSeqId == orderItem.ShipGroupSeqId &&
                                  oisgir.InventoryItem.FacilityId == orderItemShipGrpInvResFacilityId
                            select oisgir
                        ).ToListAsync();

                        foreach (var itemRes in itemResList)
                        {
                            var issueContext = new IssueOrderItemShipGrpInvResParameters
                            {
                                ShipmentId = shipmentId,
                                OrderId = itemRes.OrderId,
                                OrderItemSeqId = itemRes.OrderItemSeqId,
                                ShipGroupSeqId = itemRes.ShipGroupSeqId,
                                InventoryItemId = itemRes.InventoryItemId,
                                Quantity = (decimal)itemRes.Quantity,
                                EventDate = eventDate
                            };

                            try
                            {
                                var issueResult =
                                    await _issuanceService.IssueOrderItemShipGrpInvResToShipment(issueContext);
                            }
                            catch (Exception ex)
                            {
                                throw new Exception($"Failed to issue order item {itemRes.OrderItemSeqId} to shipment.",
                                    ex);
                            }
                        }
                    }
                }
                else // PURCHASE_ORDER
                {
                    foreach (var item in perShipGroupItemList)
                    {
                        var issueContext = new IssueOrderItemParameters
                        {
                            ShipmentId = shipmentId,
                            OrderId = item.OrderId,
                            OrderItemSeqId = item.OrderItemSeqId,
                            ShipGroupSeqId = item.ShipGroupSeqId,
                            Quantity = (decimal)item.Quantity
                        };

                        try
                        {
                            var issueResult = await _issuanceService.IssueOrderItemToShipment(issueContext);
                        }
                        catch (Exception ex)
                        {
                            throw new Exception($"Failed to issue order item {item.OrderItemSeqId} to shipment.", ex);
                        }
                    }
                }
                
                
                var itemIssuances = await _utilityService.FindLocalOrDatabaseListAsync<ItemIssuance>(
                    query => query.Where(ii =>
                        ii.OrderId == orderHeader.OrderId && ii.ShipGroupSeqId == orderItemShipGroup.ShipGroupSeqId &&
                        ii.ShipmentId == shipmentId));

                // Place all issued items into a single package
                var shipmentPackageSeqId = "New";
                foreach (var itemIssuance in itemIssuances)
                {
                    try
                    {
                        // var addPackageResult = await _shipmentHelperService.AddShipmentContentToPackage(shipmentPackageSeqId, (decimal)itemIssuance.Quantity, itemIssuance.ShipmentId);

                        //shipmentPackageSeqId = addPackageResult.ShipmentPackageSeqId;
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Failed to add items to package.", ex);
                    }
                }

                // Update shipment status to PACKED for Sales Orders only
                if (orderHeader.OrderTypeId == "SALES_ORDER")
                {
                    var packedContext = new ShipmentUpdateParameters
                    {
                        ShipmentId = shipmentId,
                        StatusId = "SHIPMENT_PACKED"
                    };

                    try
                    {
                        await _shipmentHelperService.UpdateShipment(packedContext);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Failed to update shipment status to PACKED.", ex);
                    }
                }

                // Conditionally update shipment status to SHIPPED or PURCH_SHIP_SHIPPED
                var finalStatusContext = new ShipmentUpdateParameters
                {
                    ShipmentId = shipmentId
                };

                if (orderHeader.OrderTypeId == "SALES_ORDER" && string.IsNullOrEmpty(setPackedOnly))
                {
                    finalStatusContext.StatusId = "SHIPMENT_SHIPPED";
                }
                else if (orderHeader.OrderTypeId == "PURCHASE_ORDER")
                {
                    finalStatusContext.StatusId = "PURCH_SHIP_SHIPPED";
                }

                if (!string.IsNullOrEmpty(finalStatusContext.StatusId))
                {
                    try
                    {
                        await _shipmentHelperService.UpdateShipment(finalStatusContext);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Failed to update shipment status to {finalStatusContext.StatusId}.", ex);
                    }
                }


                // Add the shipment ID to the list
                shipmentIds.Add(shipmentId);
            }
            catch (Exception ex)
            {
                // Handle exception and continue with the next ship group
                Console.WriteLine($"Failed to process ship group {orderItemShipGroup.ShipGroupSeqId}: {ex.Message}");
                return OperationResult.Failure(
                    $"Failed to process ship group {orderItemShipGroup.ShipGroupSeqId}: {ex.Message}");
            }
        }

        return OperationResult.Success("Shipments created successfully.", shipmentIds);
    }

    public async Task<ShipmentItem> CreateShipmentItem(ShipmentItemCreateParameters parameters)
    {
        // Step 2: Call checkCanChangeShipmentStatusPacked
        var result = await _issuanceService.CheckCanChangeShipmentStatusPacked(parameters.ShipmentId);

        // Step 3: Check for errors (Assuming that exceptions are thrown in the previous method if errors occur)
        if (result.HasError)
        {
            throw new InvalidOperationException(result.ErrorMessage);
        }

        // Step 4: Create a new ShipmentItem entity
        var newEntity = new ShipmentItem();

        // Step 5: Set primary key fields from parameters to newEntity
        newEntity.ShipmentId = parameters.ShipmentId;
        // ShipmentItemSeqId will be set in the next steps

        // Step 6: Set non-primary key fields from parameters to newEntity
        newEntity.ProductId = parameters.ProductId;
        newEntity.Quantity = parameters.Quantity;
        // Add other non-PK fields as necessary

        newEntity.CreatedStamp = DateTime.UtcNow;
        newEntity.LastUpdatedStamp = DateTime.UtcNow;

        // Step 7: If shipmentItemSeqId is not provided, generate one based on existing items
        if (string.IsNullOrEmpty(parameters.ShipmentItemSeqId))
        {
            var nextSeqId = await GenerateNextShipmentItemSeqId(parameters.ShipmentId);
            newEntity.ShipmentItemSeqId = nextSeqId;
            parameters.ShipmentItemSeqId = nextSeqId; // Update the INOUT parameter
        }
        else
        {
            newEntity.ShipmentItemSeqId = parameters.ShipmentItemSeqId;
        }

        // Step 8: Set shipmentItemSeqId to result
        // Already done by updating parameters.ShipmentItemSeqId

        // Step 9: Create the new entity in the database
        await _context.ShipmentItems.AddAsync(newEntity);

        // Return the shipmentItemSeqId
        return newEntity;
    }

    private async Task<string> GenerateNextShipmentItemSeqId(string shipmentId)
    {
        // Get the max ShipmentItemSeqId from the database for the given ShipmentId
        var maxSeqIdInDb = await _context.ShipmentItems
            .Where(si => si.ShipmentId == shipmentId)
            .Select(si => si.ShipmentItemSeqId)
            .OrderByDescending(id => id)
            .FirstOrDefaultAsync();

        // Get the max ShipmentItemSeqId from the unsaved ShipmentItems in the context
        var maxSeqIdInContext = _context.ChangeTracker.Entries<ShipmentItem>()
            .Where(e => e.State != EntityState.Deleted && e.Entity.ShipmentId == shipmentId)
            .Select(e => e.Entity.ShipmentItemSeqId)
            .OrderByDescending(id => id)
            .FirstOrDefault();

        // Determine the highest SeqId between the database and the context
        var maxSeqId = MaxSeqId(maxSeqIdInDb, maxSeqIdInContext);

        // Generate the next SeqId
        int nextSeqNum = 1;
        if (int.TryParse(maxSeqId, out int maxSeqNum))
        {
            nextSeqNum = maxSeqNum + 1;
        }

        return nextSeqNum.ToString("D2"); // Formats as two-digit number
    }

// Helper method to compare two SeqIds and return the higher one
    private string MaxSeqId(string seqId1, string seqId2)
    {
        if (string.IsNullOrEmpty(seqId1))
            return seqId2;

        if (string.IsNullOrEmpty(seqId2))
            return seqId1;

        // Compare the two SeqIds numerically
        if (int.TryParse(seqId1, out int seqNum1) && int.TryParse(seqId2, out int seqNum2))
        {
            return seqNum1 >= seqNum2 ? seqId1 : seqId2;
        }

        // If parsing fails, default to the higher string value
        return string.CompareOrdinal(seqId1, seqId2) >= 0 ? seqId1 : seqId2;
    }

    public async Task<OperationResult> AddOrderItemShipGroupAssoc(
        string orderId,
        string orderItemSeqId,
        string? shipGroupSeqId,
        decimal quantity)
    {
        try
        {
            // Define the main error message prefix
            string mainErrorMessage = "Unable to add item to Order Item Ship Group.";

            var orderItem =
                await _utilityService.FindLocalOrDatabaseAsync<OrderItem>(orderId, orderItemSeqId);

            // Check if the order item exists
            if (orderItem == null)
            {
                // Construct an error message indicating the order item was not found
                string errMsg =
                    $"{mainErrorMessage} Order item not found for orderId '{orderId}' and orderItemSeqId '{orderItemSeqId}'.";

                // Return an error result with the constructed message
                return OperationResult.Failure(errMsg);
            }

            // Retrieve the statusId of the order item to check its current status
            string statusId = orderItem.StatusId;

            // Proceed only if the order item's status is ITEM_CREATED or ITEM_APPROVED
            if (statusId == "ITEM_CREATED" || statusId == "ITEM_APPROVED")
            {
                // If shipGroupSeqId is "new", we need to create a new OrderItemShipGroup
                if (shipGroupSeqId == null)
                {
                    // Prepare a new OrderItemShipGroup entity
                    var newShipGroup = new OrderItemShipGroup
                    {
                        OrderId = orderId,
                        // Additional properties will be set based on existing data
                    };

                    // Retrieve existing OrderItemShipGroupAssocs for the order item
                    var oisgas = await _context.OrderItemShipGroupAssocs
                        .Where(oisga => oisga.OrderId == orderId && oisga.OrderItemSeqId == orderItemSeqId)
                        .ToListAsync();

                    // If there are existing associations, get default values from the first one
                    if (oisgas.Any())
                    {
                        var oisga = oisgas.First();

                        // Retrieve the associated OrderItemShipGroup
                        var existingShipGroup = await _context.OrderItemShipGroups
                            .FirstOrDefaultAsync(sg =>
                                sg.OrderId == oisga.OrderId && sg.ShipGroupSeqId == oisga.ShipGroupSeqId);

                        if (existingShipGroup != null)
                        {
                            // Set default carrier and contact data for the new ship group
                            newShipGroup.ShipmentMethodTypeId = existingShipGroup.ShipmentMethodTypeId;
                            newShipGroup.CarrierPartyId = existingShipGroup.CarrierPartyId;
                            newShipGroup.CarrierRoleTypeId = existingShipGroup.CarrierRoleTypeId;
                            newShipGroup.ContactMechId = existingShipGroup.ContactMechId;
                        }
                    }

                    // Add the new ship group to the database context
                    _context.OrderItemShipGroups.Add(newShipGroup);

                    // Update shipGroupSeqId with the newly created ship group sequence ID
                    shipGroupSeqId = newShipGroup.ShipGroupSeqId;
                }

                // Retrieve the OrderItemShipGroup using the orderId and shipGroupSeqId
                var orderItemShipGroup =
                    await _utilityService.FindLocalOrDatabaseAsync<OrderItemShipGroup>(orderId, shipGroupSeqId);


                // Check if the OrderItemShipGroup exists
                if (orderItemShipGroup == null)
                {
                    // Construct an error message indicating the ship group was not found
                    string errMsg = $"{mainErrorMessage} Ship group not found with shipGroupSeqId '{shipGroupSeqId}'.";

                    // Return an error result with the constructed message
                    return OperationResult.Failure(errMsg);
                }

                // Validate that the quantity is greater than zero
                if (quantity <= 0)
                {
                    // Construct an error message indicating invalid quantity
                    string errMsg = $"{mainErrorMessage} Quantity must be greater than zero.";

                    // Return an error result with the constructed message
                    return OperationResult.Failure(errMsg);
                }

                // Check if an association between the order item and ship group already exists
                var existingAssoc = await _context.OrderItemShipGroupAssocs
                    .FirstOrDefaultAsync(a =>
                        a.OrderId == orderId && a.OrderItemSeqId == orderItemSeqId &&
                        a.ShipGroupSeqId == shipGroupSeqId);

                if (existingAssoc != null)
                {
                    // Construct an error message indicating the association already exists
                    string errMsg = $"{mainErrorMessage} Order item already related to ship group.";

                    // Return an error result with the constructed message
                    return OperationResult.Failure(errMsg);
                }

                // Create a new OrderItemShipGroupAssoc entity
                var newAssoc = new OrderItemShipGroupAssoc
                {
                    OrderId = orderId,
                    OrderItemSeqId = orderItemSeqId,
                    ShipGroupSeqId = shipGroupSeqId,
                    Quantity = quantity,
                    LastUpdatedStamp = DateTime.UtcNow,
                    CreatedStamp = DateTime.UtcNow
                };

                // Add the new association to the database context
                _context.OrderItemShipGroupAssocs.Add(newAssoc);

                // Return a success result indicating the association was added successfully
                return OperationResult.Success("Order item ship group association added successfully.");
            }
            else
            {
                // Construct an error message indicating the order item's status does not allow modification
                string errMsg =
                    $"{mainErrorMessage} Order item cannot be modified due to its current status '{statusId}'.";

                // Return an error result with the constructed message
                return OperationResult.Failure(errMsg);
            }
        }
        catch (Exception ex)
        {
            // Log the exception details
            Console.WriteLine($"Error in addOrderItemShipGroupAssoc: {ex.Message}");

            // Return an error result with the exception message
            return OperationResult.Failure($"An error occurred: {ex.Message}");
        }
    }
}

// ReceiveInventoryProduct logic

// 1. Get supplier_id using the SUPPLIER_AGENT role
// 2. Get facilityOwnerPartyId

// 3. Call CreateInventoryItem
// 3.1. Get the next sequence for inventory item
// 3.2. Create inventory item

// 3.3. Call InventoryItemCheckSetDefaultValues
// Initial Data Validation
//Set Owner Party ID (If Missing)
//Set Currency ID (If Missing)
//Set Unit Cost (If Missing or Invalid) via 'GetProductCost'

// 3.3.1. GetProductCost
//Retrieve Standard Cost Components
//Calculate Cost from Components
//Get Purchase Cost from Supplier (If Standard Cost is Zero)
//Convert Currency (If Purchase Cost in Different Currency)

// 3.4    CreateInventoryItemCheckSetAtpQoh
//Check for ATP/QOH Values (Checks if either availableToPromiseTotal or quantityOnHandTotal have values)
//Create Inventory Item Detail (Calls the CreateInventoryItemDetail function)

// 3.4.1  CreateInventoryItemDetail
//Generate Sequence ID: Gets a new sequence ID for the InventoryItemDetail record.
//Create Inventory Item Detail Entity: Initializes a new InventoryItemDetail object.
//Set Effective Date: Determines the effective date based on the presence of an itemIdssuanceId.
//Set Default Values: Assigns default values (0) to AvailableToPromiseDiff, QuantityOnHandDiff, and AccountingQuantityDiff if they are not provided.
//Save Inventory Item Detail: Adds the new record to the database context and saves the changes.
//Update Inventory Item: Calls UpdateInventoryItemFromDetail to update related inventory item information.

// 3.4.1.1  UpdateInventoryItemFromDetail
//Fetch Inventory Item: Retrieves the InventoryItem record based on the provided inventoryItemId.
//Fetch Inventory Item Details: Queries and retrieves associated InventoryItemDetails records for the given inventoryItemId.
//Calculate Totals: Calculates the following totals from the fetched InventoryItemDetails:
//availableToPromiseTotal
//quantityOnHandTotal
//accountingQuantityTotal
//Update Inventory Item: Updates the corresponding InventoryItem record with the newly calculated totals.


//3.4.1.2      //Set Last Inventory Count (Conditional): If AvailableToPromiseDiff is not zero, calls SetLastInventoryCount (probably to adjust inventory status).
//Fetch Inventory Item: Retrieves the InventoryItem record matching the provided inventoryItemId.
//Fetch Product Facility: Retrieves the associated ProductFacility record based on the ProductId and FacilityId from the fetched InventoryItem.
//Get Inventory Totals: Calls the GetProductInventoryAvailable function to get the available inventory totals for the product and facility.

// 3.4.1.2.1        GetProductInventoryAvailable
//Build Inventory Item Query
//Calculate Totals
//Return Inventory Totals
//Update Last Inventory Count: Updates the LastInventoryCount field of the ProductFacility record with the AvailableToPromiseTotal value retrieved from inventory totals.

// 4.0  CreateInventoryItemDetail -> need more investigation

// 5.0  CreateShipmentReceipt
//Create Shipment Receipt
//Financial Recording (Create AcctgTrans) via CreateAcctgTransForShipmentReceipt
//Update Order Status from Receipt via UpdateOrderStatusFromReceipt

// 6.0 UpdateProductAverageCostOnReceiveInventory