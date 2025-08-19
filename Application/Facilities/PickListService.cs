using Application.Core;
using Application.Interfaces;
using Application.Order.Orders;
using Application.Shipments;
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Facilities;

public interface IPickListService
{
    Task<List<PickMoveInfoGroupDto>> FindOrdersToPickMove(
        string facilityId,
        string shipmentMethodTypeId,
        string isRushOrder,
        long? maxNumberOfOrders,
        List<OrderHeader>? orderHeaderList,
        string groupByNoOfOrderItems,
        string groupByWarehouseArea,
        string groupByShippingMethod,
        string orderId
    );

    Task<CreatePicklistFromOrdersResult> CreatePicklistFromOrders(CreatePicklistFromOrdersInput input);

    Task<GetPicklistDisplayInfoResult> GetPicklistDisplayInfo(
        string facilityId,
        int? viewIndexParam,
        int? viewSizeParam);

    Task<FindStockMovesNeededResult> FindStockMovesNeeded(string facilityId);

    Task<UpdatePicklistResult> UpdatePicklist(UpdatePicklistInput input);

    Task<LoadPackingDataResult> LoadPackingData(LoadPackingDataInput input);

    Task UpdatePicklistItem(string picklistBinId, string orderId, string orderItemSeqId,
        string shipGroupSeqId, string inventoryItemId, string itemStatusId, decimal? quantity);

    Task<List<OrderPicklistBinDto>> GetValidOrderPicklistBins(string facilityId);
}

public class PickListService : IPickListService
{
    private readonly DataContext _context;
    private readonly ILogger _logger;
    private readonly ILogger<PackingSession> _logger2;
    private readonly IUtilityService _utilityService;
    private readonly IUserAccessor _userAccessor;
    private readonly IIssuanceService _issuanceService;
    private readonly IShipmentHelperService _shipmentHelperService;


    public PickListService(DataContext context, IUtilityService utilityService,
        ILogger<PickListService> logger, IUserAccessor userAccessor, IIssuanceService issuanceService,
        IShipmentHelperService shipmentHelperService, ILogger<PackingSession> logger2)
    {
        _context = context;
        _utilityService = utilityService;
        _logger = logger;
        _logger2 = logger2;
        _userAccessor = userAccessor;
        _issuanceService = issuanceService;
        _shipmentHelperService = shipmentHelperService;
    }


    public async Task<List<PickMoveInfoGroupDto>> FindOrdersToPickMove(
        string facilityId,
        string shipmentMethodTypeId,
        string isRushOrder,
        long? maxNumberOfOrders,
        List<OrderHeader>? orderHeaderList,
        string groupByNoOfOrderItems,
        string groupByWarehouseArea,
        string groupByShippingMethod,
        string orderId)
    {
        // PART 1: Parameter Initialization & Order Retrieval
        DateTime nowTimestamp = DateTime.UtcNow;
        if (maxNumberOfOrders == null)
        {
            maxNumberOfOrders = long.MaxValue;
        }

        List<PickMoveInfoGroupDto> pickMoveInfoList = new List<PickMoveInfoGroupDto>();
        long numberSoFar = 0;

        try
        {
            // If an orderId is provided, try to fetch that single order.
            if (!string.IsNullOrEmpty(orderId))
            {
                OrderHeader? singleOrderHeader = null;
                try
                {
                    singleOrderHeader = await _context.OrderHeaders.FindAsync(orderId);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error fetching OrderHeader by orderId {OrderId}: {Exception}", orderId, ex);
                }

                if (orderHeaderList == null)
                    orderHeaderList = new List<OrderHeader>();

                if (singleOrderHeader != null)
                    orderHeaderList.Add(singleOrderHeader);
            }
            else
            {
                // If no order header list is provided, query the DB.
                if (orderHeaderList == null || orderHeaderList.Count == 0)
                {
                    _logger.LogWarning("No order header list found in parameters; finding orders to pick.");
                    try
                    {
                        var query = _context.OrderHeaders.Where(oh =>
                            oh.OrderTypeId == "SALES_ORDER" && oh.StatusId == "ORDER_APPROVED");
                        if (!string.IsNullOrEmpty(isRushOrder))
                        {
                            query = query.Where(oh => oh.IsRushOrder == isRushOrder);
                        }

                        orderHeaderList = await query.OrderBy(oh => oh.OrderDate).ToListAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("Error fetching OrderHeaders: {Exception}", ex);
                        orderHeaderList = new List<OrderHeader>();
                    }
                }
                else
                {
                    _logger.LogWarning("Found orderHeaderList in parameters; using provided list.");
                }
            }

            // PART 2: Iterate over each order and process grouping and pickability.
            foreach (var orderHeader in orderHeaderList)
            {
                if (orderHeader == null)
                    continue; // Defensive check

                _logger.LogInformation("Checking order #{OrderId} to add to picklist", orderHeader.OrderId);

                // Retrieve all ship groups for the order.
                List<OrderItemShipGroup> orderItemShipGroupList = new List<OrderItemShipGroup>();
                try
                {
                    orderItemShipGroupList = await _context.OrderItemShipGroups
                        .Where(oisg => oisg.OrderId == orderHeader.OrderId)
                        .OrderBy(oisg => oisg.ShipGroupSeqId)
                        .ToListAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error fetching OrderItemShipGroupList for order {OrderId}: {Exception}",
                        orderHeader.OrderId, ex);
                }

                // Count order items.
                int orderItemCount = 0;
                try
                {
                    orderItemCount = await _context.OrderItems.CountAsync(oi => oi.OrderId == orderHeader.OrderId);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error counting OrderItems for order {OrderId}: {Exception}", orderHeader.OrderId,
                        ex);
                }

                // Initialize grouping variables to empty strings.
                string groupName = "";
                string groupName1 = "";
                string groupName2 = "";
                string groupName3 = "";
                string noOfOrderItemsLabel = "";

                bool noGroupingSelected = string.IsNullOrEmpty(groupByShippingMethod) &&
                                          string.IsNullOrEmpty(groupByWarehouseArea) &&
                                          string.IsNullOrEmpty(groupByNoOfOrderItems);

                // PART 3: Build the group name.
                if (noGroupingSelected)
                {
                    groupName = orderHeader.OrderId;
                }
                else
                {
                    List<OrderHeaderAndItemFacilityLocation> orderHeaderAndItemFacilityLocationList =
                        new List<OrderHeaderAndItemFacilityLocation>();
                    try
                    {
                        orderHeaderAndItemFacilityLocationList = await (
                            from oh in _context.OrderHeaders
                            join oisgir in _context.OrderItemShipGrpInvRes on oh.OrderId equals oisgir.OrderId
                            join oisg in _context.OrderItemShipGroups
                                on new { oisgir.OrderId, oisgir.ShipGroupSeqId } equals new
                                    { oisg.OrderId, oisg.ShipGroupSeqId }
                            join ii in _context.InventoryItems on oisgir.InventoryItemId equals ii.InventoryItemId
                            join fl in _context.FacilityLocations
                                on new { ii.FacilityId, ii.LocationSeqId } equals new
                                    { fl.FacilityId, fl.LocationSeqId } into flGroup
                            from fl in flGroup.DefaultIfEmpty() // optional join
                            where oh.OrderId == orderHeader.OrderId &&
                                  (fl.LocationTypeEnumId == "FLT_PICKLOC" || fl.LocationTypeEnumId == null)
                            select new OrderHeaderAndItemFacilityLocation
                            {
                                OrderId = oh.OrderId,
                                ShipmentMethodTypeId = oisg.ShipmentMethodTypeId,
                                AreaId = fl != null ? fl.AreaId : null,
                                LocationTypeEnumId = (fl == null ? null : fl.LocationTypeEnumId)
                            }
                        ).Distinct().ToListAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(
                            "Error fetching OrderHeaderAndItemFacilityLocationList for order {OrderId}: {Exception}",
                            orderHeader.OrderId, ex);
                    }

                    List<string> locations = new List<string>();
                    foreach (var ohafl in orderHeaderAndItemFacilityLocationList)
                    {
                        // Refactoring Note: Using empty string if null.
                        if (groupByShippingMethod == "Y")
                        {
                            groupName1 = ohafl.ShipmentMethodTypeId ?? "";
                        }

                        if (groupByWarehouseArea == "Y")
                        {
                            groupName2 = string.IsNullOrEmpty(ohafl.AreaId) ? "N/A" : ohafl.AreaId;
                            if (!string.IsNullOrEmpty(groupName2) && !locations.Contains(groupName2))
                            {
                                locations.Add(groupName2);
                            }
                        }

                        if (groupByNoOfOrderItems == "Y")
                        {
                            if (orderItemCount < 3)
                            {
                                noOfOrderItemsLabel = ProductUiLabels.FacilityNumberOfItemsLessThanThree;
                                groupName3 = "Items_Less_Than_3";
                            }
                            else
                            {
                                noOfOrderItemsLabel = ProductUiLabels.FacilityNumberOfItemsThreeOrMore;
                                groupName3 = "Items_Three_Or_More";
                            }
                        }

                        groupName = $"{groupName1}{groupName2}{groupName3}";
                    }

                    // If multiple distinct locations, update groupName2 to reflect MULTI_LOCATIONS.
                    if (locations.Count > 1)
                    {
                        groupName2 = ProductUiLabels.FacilityMultipleLocations; // e.g. "MULTI_LOCATIONS"
                        groupName = $"{groupName1}{groupName2}{groupName3}";
                    }
                }

                // PART 4: Evaluate each ship group to see if the order is pickable.
                bool finalPickThisOrder = false;
                bool finalNeedsStockMove = false;

                // Refactoring Note: Replace string flags ("Y"/"N") with booleans.
                foreach (var orderItemShipGroup in orderItemShipGroupList)
                {
                    // Skip this ship group if the shipAfterDate is in the future.
                    if (orderItemShipGroup.ShipAfterDate != null && nowTimestamp < orderItemShipGroup.ShipAfterDate)
                        continue;

                    List<OrderItemShipGrpInvRes> orderItemShipGrpInvResList = new List<OrderItemShipGrpInvRes>();
                    try
                    {
                        orderItemShipGrpInvResList = await _context.OrderItemShipGrpInvRes
                            .Include(x => x.OrderI)
                            .Include(x => x.InventoryItem)
                            .Where(oisgir => oisgir.OrderId == orderItemShipGroup.OrderId &&
                                             oisgir.ShipGroupSeqId == orderItemShipGroup.ShipGroupSeqId)
                            .ToListAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("Error fetching OrderItemShipGrpInvResList for order {OrderId}: {Exception}",
                            orderItemShipGroup.OrderId, ex);
                    }

                    List<OrderItemAndShipGroupAssoc> orderItemAndShipGroupAssocList =
                        new List<OrderItemAndShipGroupAssoc>();
                    try
                    {
                        orderItemAndShipGroupAssocList = await (
                            from oi in _context.OrderItems
                            join oisga in _context.OrderItemShipGroupAssocs
                                on new { oi.OrderId, oi.OrderItemSeqId } equals new
                                    { oisga.OrderId, oisga.OrderItemSeqId }
                            where oisga.OrderId == orderItemShipGroup.OrderId &&
                                  oisga.ShipGroupSeqId == orderItemShipGroup.ShipGroupSeqId
                            orderby oisga.OrderItemSeqId
                            select new OrderItemAndShipGroupAssoc
                            {
                                OrderId = oi.OrderId,
                                OrderItemSeqId = oi.OrderItemSeqId,
                                ProductId = oi.ProductId,
                                StatusId = oi.StatusId,
                                OrderItemQuantity = oi.Quantity ?? 0,
                                OrderItemCancelQuantity = oi.CancelQuantity ?? 0,
                                ShipGroupSeqId = oisga.ShipGroupSeqId,
                                Quantity = oisga.Quantity ?? 0,
                                CancelQuantity = oisga.CancelQuantity ?? 0
                            }
                        ).ToListAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(
                            "Error fetching OrderItemAndShipGroupAssocList for order {OrderId}: {Exception}",
                            orderItemShipGroup.OrderId, ex);
                    }

                    // Use booleans instead of string flags.
                    bool pickThisOrder = true;
                    bool needsStockMove = false;
                    bool allPickStarted = true;
                    bool hasStockToPick = false;

                    // Evaluate each inventory reservation entry.
                    foreach (var oisgir in orderItemShipGrpInvResList)
                    {
                        // Defensive: Check that the related OrderItem exists.
                        if (oisgir.OrderI == null)
                        {
                            _logger.LogWarning("OrderItem is null for OrderItemShipGrpInvRes for order {OrderId}",
                                oisgir.OrderId);
                            pickThisOrder = false;
                            continue;
                        }

                        if (oisgir.OrderI.StatusId != "ITEM_APPROVED")
                        {
                            pickThisOrder = false;
                        }

                        if (pickThisOrder)
                        {
                            var inventoryItem = oisgir.InventoryItem;
                            if (inventoryItem == null)
                            {
                                _logger.LogWarning(
                                    "InventoryItem is null for OrderItemShipGrpInvRes for order {OrderId}",
                                    oisgir.OrderId);
                                continue;
                            }

                            List<PicklistItem> picklistItemList = new List<PicklistItem>();
                            try
                            {
                                picklistItemList = await _context.PicklistItems
                                    .Where(pi => pi.OrderId == oisgir.OrderId &&
                                                 pi.ShipGroupSeqId == oisgir.ShipGroupSeqId &&
                                                 pi.OrderItemSeqId == oisgir.OrderItemSeqId &&
                                                 pi.InventoryItemId == oisgir.InventoryItemId &&
                                                 pi.ItemStatusId != "PICKLIST_CANCELLED" &&
                                                 pi.ItemStatusId != "PICKITEM_CANCELLED")
                                    .ToListAsync();
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError("Error fetching PicklistItems for order {OrderId}: {Exception}",
                                    oisgir.OrderId, ex);
                            }

                            decimal pickedItemQuantity = picklistItemList.Sum(pi => pi.Quantity ?? 0m);
                            decimal remainingQuantityToBePicked = (oisgir.Quantity ?? 0m) - pickedItemQuantity;

                            if (remainingQuantityToBePicked > 0)
                            {
                                allPickStarted = false;

                                // If order item cannot be split and some quantity is not available, mark as not pickable.
                                if (!string.Equals(orderItemShipGroup.MaySplit, "Y",
                                        StringComparison.OrdinalIgnoreCase) &&
                                    ((oisgir.QuantityNotAvailable ?? 0) > 0))
                                {
                                    pickThisOrder = false;
                                }

                                // Check if the facility matches.
                                if (pickThisOrder && !string.IsNullOrEmpty(facilityId) &&
                                    !facilityId.Equals(inventoryItem.FacilityId, StringComparison.OrdinalIgnoreCase))
                                {
                                    pickThisOrder = false;
                                }

                                if (pickThisOrder)
                                {
                                    // Determine if stock is available.
                                    bool itemHasStock = !oisgir.QuantityNotAvailable.HasValue ||
                                                        oisgir.QuantityNotAvailable.Value == 0 ||
                                                        (remainingQuantityToBePicked >
                                                         (oisgir.QuantityNotAvailable ?? 0m) &&
                                                         orderItemShipGroup.MaySplit.Equals("Y",
                                                             StringComparison.OrdinalIgnoreCase));

                                    if (itemHasStock)
                                    {
                                        hasStockToPick = true;

                                        FacilityLocation facilityLocation = null;
                                        try
                                        {
                                            facilityLocation =
                                                await _context.FacilityLocations.FindAsync(inventoryItem.FacilityId,
                                                    inventoryItem.LocationSeqId);
                                        }
                                        catch (Exception ex)
                                        {
                                            _logger.LogError(
                                                "Error fetching FacilityLocation for facility {FacilityId}: {Exception}",
                                                inventoryItem.FacilityId, ex);
                                        }

                                        if (facilityLocation != null &&
                                            facilityLocation.LocationTypeEnumId == "FLT_BULK")
                                        {
                                            needsStockMove = true;
                                        }

                                        _logger.LogInformation(
                                            "Found item to pick: Order {OrderId}, OrderItemSeqId {OrderItemSeqId}",
                                            oisgir.OrderId, oisgir.OrderItemSeqId);
                                    }
                                }
                            }
                        }
                    } // End loop over inventory reservations.

                    // Final decision: if no stock is available, mark as not pickable.
                    if (!hasStockToPick)
                    {
                        pickThisOrder = false;
                    }

                    // Check if we've reached the max number of orders.
                    if (numberSoFar >= maxNumberOfOrders.Value)
                    {
                        _logger.LogWarning("Reached max number of orders: {MaxNumberOfOrders}", maxNumberOfOrders);
                        pickThisOrder = false;
                    }
                    else
                    {
                        _logger.LogInformation("Orders so far: {NumberSoFar} of max {MaxNumberOfOrders}", numberSoFar,
                            maxNumberOfOrders);
                    }

                    if (pickThisOrder && !allPickStarted)
                    {
                        finalPickThisOrder = true;
                        if (needsStockMove)
                        {
                            finalNeedsStockMove = true;
                        }

                        numberSoFar++;
                        _logger.LogInformation(
                            "Added order #{OrderId} to pick list [{NumberSoFar} of {MaxNumberOfOrders}]",
                            orderHeader.OrderId, numberSoFar, maxNumberOfOrders);
                        break; // Stop checking other ship groups for this order.
                    }
                    else
                    {
                        _logger.LogInformation(
                            "Order #{OrderId} not added for this ship group; pickThisOrder: {PickThisOrder}, allPickStarted: {AllPickStarted}",
                            orderHeader.OrderId, pickThisOrder, allPickStarted);
                    }
                } // End loop over ship groups.

                // PART 5: If the order qualifies, add it to the appropriate group.
                if (finalPickThisOrder)
                {
                    var pmGroup = pickMoveInfoList.FirstOrDefault(x => x.GroupName == groupName);
                    if (pmGroup == null)
                    {
                        pmGroup = new PickMoveInfoGroupDto
                        {
                            GroupName = groupName,
                            GroupName1 = groupName1,
                            GroupName2 = groupName2,
                            GroupName3 = groupName3,
                            ShipmentMethodTypeId = shipmentMethodTypeId,
                            OrderReadyToPickInfoList = new List<string>(),
                            OrderNeedsStockMoveInfoList = new List<string>()
                        };
                        pickMoveInfoList.Add(pmGroup);
                    }

                    if (finalNeedsStockMove)
                    {
                        pmGroup.OrderNeedsStockMoveInfoList.Add(orderHeader.OrderId);
                    }
                    else
                    {
                        pmGroup.OrderReadyToPickInfoList.Add(orderHeader.OrderId);
                    }
                }

                if (numberSoFar >= maxNumberOfOrders.Value)
                {
                    break;
                }
            } // End loop over order headers.

            return pickMoveInfoList;
        }
        catch (Exception ex)
        {
            _logger.LogError("Exception in FindOrdersToPickMove: {Exception}", ex);
            return new List<PickMoveInfoGroupDto>();
        }
    }


    public async Task<CreatePicklistFromOrdersResult> CreatePicklistFromOrders(CreatePicklistFromOrdersInput input)
    {
        var result = new CreatePicklistFromOrdersResult();
        try
        {
            // STEP 1: Current timestamp
            DateTime nowTimestamp = DateTime.UtcNow;

            // 1) Validate that we have a list of order IDs
            if (input.OrderIdList == null || input.OrderIdList.Count == 0)
            {
                result.HasError = true;
                result.ErrorMessage = "No orderIds provided.";
                return result;
            }

            // 2) Fetch the OrderHeader records from DB by IDs
            var fetchedOrders = await _context.OrderHeaders
                .Where(oh => input.OrderIdList.Contains(oh.OrderId))
                .ToListAsync();

            // If no matching orders found
            if (fetchedOrders.Count == 0)
            {
                result.HasError = true;
                result.ErrorMessage = "No valid orders found for the given orderIdList.";
                return result;
            }

            // 3) Build arguments for FindOrdersToPickMove
            //    Provide the full orderHeaderList that we just fetched
            var facilityId = input.FacilityId;
            var shipmentMethodTypeId = input.ShipmentMethodTypeId;
            long? maxNumberOfOrders = input.MaxNumberOfOrders ?? long.MaxValue;
            string isRushOrder = input.IsRushOrder;


            // 4) Call FindOrdersToPickMove, providing the fetched orderHeaderList
            List<PickMoveInfoGroupDto> pickMoveInfoList = await FindOrdersToPickMove(
                facilityId: facilityId,
                shipmentMethodTypeId: shipmentMethodTypeId,
                isRushOrder: isRushOrder,
                maxNumberOfOrders: maxNumberOfOrders,
                orderHeaderList: fetchedOrders,
                groupByNoOfOrderItems: null, // or "Y" if you have UI toggles
                groupByWarehouseArea: null, // same reasoning
                groupByShippingMethod: null, // same reasoning
                orderId: null
            );

            // 5) pickMoveInfoList now has minimal data: order IDs in OrderReadyToPickInfoList
            //    We will re-fetch the actual item details for picklist creation.

            // Reconstruct the final OrderHeaderInfo objects (Option B approach)
            var orderHeaderInfoList = new List<OrderHeaderInfo>();
            foreach (var groupDto in pickMoveInfoList)
            {
                foreach (var orderId in groupDto.OrderReadyToPickInfoList)
                {
                    var ohInfos = await ReconstructOrderHeaderInfosFromDb(orderId);
                    orderHeaderInfoList.AddRange(ohInfos);
                }
            }


            // If no orders to process => return error
            if (orderHeaderInfoList.Count == 0)
            {
                result.HasError = true;
                result.ErrorMessage = "No orders ready to pick; not creating picklist.";
                return result;
            }

            var user = await _context.Users.SingleOrDefaultAsync(x => x.UserName == _userAccessor.GetUsername());
            var newPickListSequence = await _utilityService.GetNextSequence("Picklist");


            // STEP 4: Create the new picklist
            var newPicklist = new Picklist
            {
                PicklistId = newPickListSequence,
                FacilityId = input.FacilityId,
                ShipmentMethodTypeId = input.ShipmentMethodTypeId,
                StatusId = "PICKLIST_INPUT",
                PicklistDate = nowTimestamp,
                CreatedStamp = nowTimestamp,
                LastUpdatedStamp = nowTimestamp
            };
            await _context.Picklists.AddAsync(newPicklist);

            result.PicklistId = newPicklist.PicklistId;

            long binLocationNumber = 1;

            // STEP 5: For each rebuilt OrderHeaderInfo, create bins & items
            foreach (var orderHeaderInfo in orderHeaderInfoList)
            {
                var newPickListBinSequence = await _utilityService.GetNextSequence("PicklistBin");


                var picklistBin = new PicklistBin
                {
                    PicklistBinId = newPickListBinSequence,
                    PicklistId = newPicklist.PicklistId,
                    BinLocationNumber = (int?)binLocationNumber,
                    PrimaryOrderId = orderHeaderInfo.OrderItemShipGroup.OrderId,
                    PrimaryShipGroupSeqId = orderHeaderInfo.OrderItemShipGroup.ShipGroupSeqId,
                    CreatedStamp = nowTimestamp,
                    LastUpdatedStamp = nowTimestamp
                };
                await _context.PicklistBins.AddAsync(picklistBin);

                binLocationNumber++;
                long itemsInBin = 0;

                // Create picklist items for each OISGIR
                foreach (var orderItemInfo in orderHeaderInfo.OrderItemInfoList)
                {
                    foreach (var oisgir in orderItemInfo.OrderItemShipGrpInvResList)
                    {
                        decimal? quantityToPick = oisgir.Quantity ?? 0m;
                        decimal? quantityNotAvailable = oisgir.QuantityNotAvailable ?? 0m;

                        if (quantityNotAvailable > 0m)
                        {
                            quantityToPick -= quantityNotAvailable;
                            if (quantityToPick < 0m) quantityToPick = 0m;
                        }

                        if (quantityToPick > 0m)
                        {
                            var picklistItem = new PicklistItem
                            {
                                PicklistBinId = picklistBin.PicklistBinId,
                                OrderId = oisgir.OrderId,
                                ShipGroupSeqId = oisgir.ShipGroupSeqId,
                                OrderItemSeqId = oisgir.OrderItemSeqId,
                                InventoryItemId = oisgir.InventoryItemId,
                                Quantity = quantityToPick,
                                ItemStatusId = "PICKITEM_PENDING",
                                CreatedStamp = nowTimestamp,
                                LastUpdatedStamp = nowTimestamp
                            };
                            await _context.PicklistItems.AddAsync(picklistItem);

                            itemsInBin++;
                        }
                    }
                }

                // If bin is empty, remove it
                if (itemsInBin == 0)
                {
                    _context.PicklistBins.Remove(picklistBin);
                }
            }

            // STEP 6: Return success
            return result;
        }
        catch (Exception ex)
        {
            // Handle errors
            result.HasError = true;
            result.ErrorMessage = $"Exception while creating picklist: {ex.Message}";
            return result;
        }
    }

    /// <summary>
    /// Given an orderId, fetch OrderHeader, ship groups, items, 
    /// and build a list of fully-populated OrderHeaderInfo objects.
    /// </summary>
    private async Task<List<OrderHeaderInfo>> ReconstructOrderHeaderInfosFromDb(string orderId)
    {
        var resultList = new List<OrderHeaderInfo>();

        // 1) Fetch the order header
        var orderHeader = await _context.OrderHeaders
            .FindAsync(orderId);
        if (orderHeader == null) return resultList; // Not found => return empty

        // 2) Fetch all ship groups for this order
        var shipGroups = await _context.OrderItemShipGroups
            .Where(sg => sg.OrderId == orderId)
            .ToListAsync();

        // 3) For each ship group, build OrderHeaderInfo
        foreach (var sg in shipGroups)
        {
            var orderHeaderInfo = new OrderHeaderInfo
            {
                OrderHeader = orderHeader,
                OrderItemShipGroup = sg,
                OrderItemInfoList = new List<OrderItemInfo>()
            };

            // 4) For each OrderItem in this order, fetch OISGIR relevant to this ship group
            var orderItems = await _context.OrderItems
                .Where(oi => oi.OrderId == orderId)
                .ToListAsync();

            foreach (var oi in orderItems)
            {
                // Retrieve OISGIR for this (OrderId, OrderItemSeqId, ShipGroupSeqId)
                var oisgirList = await _context.OrderItemShipGrpInvRes
                    .Where(r => r.OrderId == orderId
                                && r.OrderItemSeqId == oi.OrderItemSeqId
                                && r.ShipGroupSeqId == sg.ShipGroupSeqId)
                    .ToListAsync();

                if (oisgirList.Any())
                {
                    var orderItemInfo = new OrderItemInfo
                    {
                        // Fill in other fields if needed
                        OrderItemShipGrpInvResList = oisgirList
                    };
                    orderHeaderInfo.OrderItemInfoList.Add(orderItemInfo);
                }
            }

            // Add the fully-populated info
            resultList.Add(orderHeaderInfo);
        }

        return resultList;
    }

    public async Task<GetPicklistDisplayInfoResult> GetPicklistDisplayInfo(
        string facilityId,
        int? viewIndexParam,
        int? viewSizeParam)
    {
        try
        {
            // 1) Initialize pagination
            int viewIndex = viewIndexParam ?? 0;
            int viewSize = viewSizeParam ?? 20;

            var result = new GetPicklistDisplayInfoResult
            {
                ViewIndex = viewIndex,
                ViewSize = viewSize
            };

            // 2) Count matching picklists
            long picklistCount = await _context.Picklists
                .Where(p => p.FacilityId == facilityId
                            && p.StatusId != "PICKLIST_PICKED"
                            && p.StatusId != "PICKLIST_CANCELLED")
                .LongCountAsync();

            result.PicklistCount = picklistCount;

            // 3) Paginated picklists, ordered by picklistDate
            var picklistList = await _context.Picklists
                .Where(p => p.FacilityId == facilityId
                            && p.StatusId != "PICKLIST_PICKED"
                            && p.StatusId != "PICKLIST_CANCELLED")
                .OrderBy(p => p.PicklistDate)
                .Skip(viewIndex * viewSize)
                .Take(viewSize)
                .ToListAsync();

            var picklistInfoList = new List<PicklistInfo>();

            // ========== ITERATE picklists => replicate getPicklistSingleInfoInline ==========
            foreach (var picklist in picklistList)
            {
                var picklistInfo = new PicklistInfo
                {
                    Picklist = new Picklist
                    {
                        PicklistId = picklist.PicklistId,
                        FacilityId = picklist.FacilityId,
                        PicklistDate = picklist.PicklistDate,
                        StatusId = picklist.StatusId
                    }
                };

                // facility
                /*if (!string.IsNullOrEmpty(picklist.FacilityId))
                {
                    picklistInfo.Facility = await _context.Facilities
                        .FindAsync(picklist.FacilityId);
                }*/

                // shipmentMethodType
                if (!string.IsNullOrEmpty(picklist.ShipmentMethodTypeId))
                {
                    picklistInfo.ShipmentMethodType = await _context.ShipmentMethodTypes
                        .Where(smt => smt.ShipmentMethodTypeId == picklist.ShipmentMethodTypeId)
                        .Select(smt => new ShipmentMethodType
                        {
                            ShipmentMethodTypeId = smt.ShipmentMethodTypeId,
                            Description = smt.Description
                        })
                        .FirstOrDefaultAsync();
                }

                // statusItem
                if (!string.IsNullOrEmpty(picklist.StatusId))
                {
                    var statusItem = await _context.StatusItems
                        .Where(s => s.StatusId == picklist.StatusId)
                        .Select(s => new StatusItem
                        {
                            StatusId = s.StatusId,
                            Description = s.Description
                        })
                        .FirstOrDefaultAsync();

                    picklistInfo.StatusItem = statusItem;
                }

                // replicate "StatusValidChangeToDetail" => manual join of StatusValidChange + StatusItem
                var svcList = await _context.StatusValidChanges
                    .Where(svc => svc.StatusId == picklist.StatusId)
                    .OrderBy(svc => svc.StatusIdTo) // or SequenceId if you store it
                    .ToListAsync();

                var statusChangeDetails = new List<StatusValidChangeToDetailDto>();
                foreach (var svc in svcList)
                {
                    var toStatus = await _context.StatusItems
                        .FindAsync(svc.StatusIdTo);

                    statusChangeDetails.Add(new StatusValidChangeToDetailDto
                    {
                        StatusId = svc.StatusId,
                        StatusIdTo = svc.StatusIdTo,
                        ToStatusId = toStatus?.StatusId,
                        StatusCode = toStatus?.StatusCode
                    });
                }

                picklistInfo.StatusValidChangeToDetailList = statusChangeDetails;

                // ========== picklistRoleInfoList ==========
                var picklistRoleList = await _context.PicklistRoles
                    .Where(r => r.PicklistId == picklist.PicklistId)
                    .ToListAsync();

                var picklistRoleInfoList = new List<PicklistRoleInfo>();
                foreach (var picklistRole in picklistRoleList)
                {
                    var picklistRoleInfo = new PicklistRoleInfo
                    {
                        PicklistRole = picklistRole
                    };

                    // party => person/partyGroup => build PartyNameViewDto
                    if (!string.IsNullOrEmpty(picklistRole.PartyId))
                    {
                        var party = await _context.Parties
                            .FindAsync(picklistRole.PartyId);

                        if (party != null)
                        {
                            // find Person
                            var person = await _context.Persons
                                .FindAsync(party.PartyId);

                            // find PartyGroup
                            var partyGroup = await _context.PartyGroups
                                .FindAsync(party.PartyId);

                            var partyNameViewDto = new PartyNameViewDto
                            {
                                PartyId = party.PartyId,
                                PartyTypeId = party.PartyTypeId,
                                StatusId = party.StatusId,
                                Description = party.Description,

                                FirstName = person?.FirstName,
                                MiddleName = person?.MiddleName,
                                LastName = person?.LastName,
                                // etc...
                                GroupName = partyGroup?.GroupName
                                // etc...
                            };

                            picklistRoleInfo.PartyNameViewDto = partyNameViewDto;
                        }
                    }

                    // roleType
                    if (!string.IsNullOrEmpty(picklistRole.RoleTypeId))
                    {
                        picklistRoleInfo.RoleType = await _context.RoleTypes
                            .FindAsync(picklistRole.RoleTypeId);
                    }

                    picklistRoleInfoList.Add(picklistRoleInfo);
                }

                picklistInfo.PicklistRoleInfoList = picklistRoleInfoList;

                // ========== picklistStatusHistoryInfoList ==========
                var picklistStatusHistoryList = await _context.PicklistStatusHistories
                    .Where(psh => psh.PicklistId == picklist.PicklistId)
                    .ToListAsync();

                var picklistStatusHistoryInfoList = new List<PicklistStatusHistoryInfo>();
                foreach (var psh in picklistStatusHistoryList)
                {
                    var histInfo = new PicklistStatusHistoryInfo
                    {
                        PicklistStatusHistory = psh
                    };

                    if (!string.IsNullOrEmpty(psh.StatusId))
                    {
                        histInfo.StatusItem = await _context.StatusItems
                            .FindAsync(psh.StatusId);
                    }

                    if (!string.IsNullOrEmpty(psh.StatusIdTo))
                    {
                        histInfo.StatusItemTo = await _context.StatusItems
                            .FindAsync(psh.StatusIdTo);
                    }

                    picklistStatusHistoryInfoList.Add(histInfo);
                }

                picklistInfo.PicklistStatusHistoryInfoList = picklistStatusHistoryInfoList;

                // ========== picklistBinInfoList ==========
                var picklistBinList = await _context.PicklistBins
                    .Where(b => b.PicklistId == picklist.PicklistId)
                    .OrderBy(b => b.BinLocationNumber)
                    .Select(b => new PicklistBin
                    {
                        PicklistBinId = b.PicklistBinId,
                        BinLocationNumber = b.BinLocationNumber,
                        PrimaryOrderId = b.PrimaryOrderId
                    })
                    .ToListAsync();

                var picklistBinInfoList = new List<PicklistBinInfo>();
                foreach (var picklistBin in picklistBinList)
                {
                    var picklistBinInfo = new PicklistBinInfo
                    {
                        PicklistBin = picklistBin
                    };

                    // primaryOrderHeader
                    /*if (!string.IsNullOrEmpty(picklistBin.PrimaryOrderId))
                    {
                        picklistBinInfo.PrimaryOrderHeader = await _context.OrderHeaders
                            .FindAsync(picklistBin.PrimaryOrderId);
                    }*/

                    // primaryOrderItemShipGroup
                    /*if (!string.IsNullOrEmpty(picklistBin.PrimaryOrderId) &&
                        !string.IsNullOrEmpty(picklistBin.PrimaryShipGroupSeqId))
                    {
                        picklistBinInfo.PrimaryOrderItemShipGroup = await _context.OrderItemShipGroups
                            .FirstOrDefaultAsync(osg => osg.OrderId == picklistBin.PrimaryOrderId
                                                        && osg.ShipGroupSeqId == picklistBin.PrimaryShipGroupSeqId);
                    }*/

                    // productStore if needed
                    /*if (picklistBinInfo.PrimaryOrderHeader != null &&
                        !string.IsNullOrEmpty(picklistBinInfo.PrimaryOrderHeader.ProductStoreId))
                    {
                        picklistBinInfo.ProductStore = await _context.ProductStores
                            .FindAsync(picklistBinInfo.PrimaryOrderHeader.ProductStoreId);
                    }*/

                    // picklistItemInfoList
                    var picklistItemList = await _context.PicklistItems
                        .Where(pi => pi.PicklistBinId == picklistBin.PicklistBinId)
                        .ToListAsync();

                    var picklistItemInfoList = new List<PicklistItemInfo>();
                    foreach (var picklistItem in picklistItemList)
                    {
                        var itemInfo = new PicklistItemInfo
                        {
                            PicklistItem = picklistItem,
                            PicklistBin = picklistBin
                        };

                        // OrderItem
                        if (!string.IsNullOrEmpty(picklistItem.OrderId) &&
                            !string.IsNullOrEmpty(picklistItem.OrderItemSeqId))
                        {
                            itemInfo.OrderItem = await _context.OrderItems
                                .Where(oi => oi.OrderId == picklistItem.OrderId
                                             && oi.OrderItemSeqId == picklistItem.OrderItemSeqId)
                                .Select(oi => new OrderItem
                                {
                                    OrderId = oi.OrderId,
                                    OrderItemSeqId = oi.OrderItemSeqId,
                                    ProductId = oi.ProductId
                                })
                                .FirstOrDefaultAsync();
                        }

                        // Product from the OrderItem
                        if (itemInfo.OrderItem != null && !string.IsNullOrEmpty(itemInfo.OrderItem.ProductId))
                        {
                            itemInfo.Product = await _context.Products
                                .Where(p => p.ProductId == itemInfo.OrderItem.ProductId)
                                .Select(p => new Domain.Product
                                {
                                    ProductId = p.ProductId,
                                    InternalName = p.InternalName
                                })
                                .FirstOrDefaultAsync();
                        }

                        // Replicate "InventoryItemAndLocation" => manual join
                        if (!string.IsNullOrEmpty(picklistItem.InventoryItemId))
                        {
                            var invItem = await _context.InventoryItems
                                .Where(ii => ii.InventoryItemId == picklistItem.InventoryItemId)
                                .Select(ii => new InventoryItemAndLocationDto
                                {
                                    InventoryItemId = ii.InventoryItemId,
                                    FacilityId = ii.FacilityId,
                                    LocationSeqId = ii.LocationSeqId,
                                    LotId = ii.LotId,
                                })
                                .FirstOrDefaultAsync();

                            itemInfo.InventoryItemAndLocation = invItem;


                            /*if (invItem != null)
                            {
                                var facLoc = await _context.FacilityLocations
                                    .FindAsync(invItem.FacilityId,
                                        invItem.LocationSeqId);

                                var product = await _context.Products
                                    .FindAsync(invItem.ProductId);

                                var invDto = new InventoryItemAndLocationDto
                                {
                                    InventoryItemId = invItem.InventoryItemId,
                                    FacilityId = invItem.FacilityId,
                                    LocationSeqId = invItem.LocationSeqId,
                                    ProductId = invItem.ProductId,
                                    LocationTypeEnumId = facLoc?.LocationTypeEnumId
                                };

                                itemInfo.InventoryItemAndLocation = invDto;
                            }*/
                        }

                        // get-related => OrderItemShipGrpInvRes
                        if (!string.IsNullOrEmpty(picklistItem.OrderId) &&
                            !string.IsNullOrEmpty(picklistItem.ShipGroupSeqId) &&
                            !string.IsNullOrEmpty(picklistItem.OrderItemSeqId) &&
                            !string.IsNullOrEmpty(picklistItem.InventoryItemId))
                        {
                            itemInfo.OrderItemShipGrpInvRes = await _context.OrderItemShipGrpInvRes
                                .FirstOrDefaultAsync(r => r.OrderId == picklistItem.OrderId
                                                          && r.ShipGroupSeqId == picklistItem.ShipGroupSeqId
                                                          && r.OrderItemSeqId == picklistItem.OrderItemSeqId
                                                          && r.InventoryItemId == picklistItem.InventoryItemId);
                        }

                        // get-related => ItemIssuance
                        var itemIssuanceList = await _context.ItemIssuances
                            .Where(ii => ii.OrderId == picklistItem.OrderId
                                         && ii.OrderItemSeqId == picklistItem.OrderItemSeqId
                                         && ii.ShipGroupSeqId == picklistItem.ShipGroupSeqId
                                         && ii.InventoryItemId == picklistItem.InventoryItemId)
                            .ToListAsync();
                        itemInfo.ItemIssuanceList = itemIssuanceList;

                        picklistItemInfoList.Add(itemInfo);
                    }

                    picklistBinInfo.PicklistItemInfoList = picklistItemInfoList;
                    picklistBinInfoList.Add(picklistBinInfo);
                }

                picklistInfo.PicklistBinInfoList = picklistBinInfoList;
                picklistInfoList.Add(picklistInfo);
            }

            // Store final picklistInfoList
            result.PicklistInfoList = picklistInfoList;

            // 5) Calculate lowIndex / highIndex
            int lowIndex = (viewIndex * viewSize) + 1;
            long highIndex = (viewIndex + 1L) * viewSize;
            if (highIndex > picklistCount) highIndex = picklistCount;
            if (viewSize > picklistCount) highIndex = picklistCount;

            result.LowIndex = lowIndex;
            result.HighIndex = (int)highIndex; // cast if safe

            return result;
        }
        catch (Exception ex)
        {
            // Log and re-throw or handle as needed
            _logger.LogError(ex, "Error in GetPicklistDisplayInfo for Facility [{FacilityId}].", facilityId);
            throw new ApplicationException($"Error retrieving picklist display info: {ex.Message}", ex);
        }
    }

    public async Task<FindStockMovesNeededResult> FindStockMovesNeeded(string facilityId)
    {
        var result = new FindStockMovesNeededResult();
        var moveByOisgirInfoList = new List<MoveByOisgirInfo>();
        var stockMoveHandled = new Dictionary<string, string>(); // e.g. <locationSeqId, "Y">
        var warningMessageList = new List<string>();

        try
        {
            // 1) Get all OISGIR in FLT_BULK location, order item status = ITEM_APPROVED, for facility
            // This replicates:
            // <entity-and entity-name="OrderItemShipGrpInvResAndItemLocation" list="orderItemShipGrpInvResAndItemLocationList">
            //     <field-map field-name="locationTypeEnumId" value="FLT_BULK"/>
            //     <field-map field-name="orderItemStatusId" value="ITEM_APPROVED"/>
            //     <field-map field-name="facilityId" from-field="parameters.facilityId"/>
            // </entity-and>

            // Because we do NOT have the view entity "OrderItemShipGrpInvResAndItemLocation", 
            // we must do a manual join:
            //   - OISGIR -> OrderItem (status=ITEM_APPROVED) 
            //   - OISGIR -> InventoryItem (join by inventoryItemId) -> FacilityLocation (locationTypeEnumId=FLT_BULK, facilityId=?)
            // This can be done with a multi-join LINQ or separate queries.

            var orderItemShipGrpInvResAndItemLocationList =
                await (
                    from oisgir in _context.OrderItemShipGrpInvRes
                    join oi in _context.OrderItems
                        on new { oisgir.OrderId, oisgir.OrderItemSeqId } equals new { oi.OrderId, oi.OrderItemSeqId }
                    join inv in _context.InventoryItems
                        on oisgir.InventoryItemId equals inv.InventoryItemId
                    join fl in _context.FacilityLocations
                        on new { inv.FacilityId, inv.LocationSeqId } equals new { fl.FacilityId, fl.LocationSeqId }
                    where oi.StatusId == "ITEM_APPROVED"
                          && fl.LocationTypeEnumId == "FLT_BULK"
                          && fl.FacilityId == facilityId
                    select new
                    {
                        oisgir, // The OrderItemShipGrpInvRes record
                        oi, // The OrderItem
                        inv, // The InventoryItem
                        fl // The FacilityLocation
                    }
                ).ToListAsync();

            // 2) Group by locationSeqId => then by productId
            // " <field-to-list field="orderItemShipGrpInvResAndItemLocation" list="oiirailByLocMap[orderItemShipGrpInvResAndItemLocation.locationSeqId]"/> "
            var groupedByLoc = orderItemShipGrpInvResAndItemLocationList
                .GroupBy(x => x.fl.LocationSeqId);

            // Iterate each location
            foreach (var locGroup in groupedByLoc)
            {
                var locationSeqId = locGroup.Key;

                // "split up by productId"
                var groupedByProduct = locGroup.GroupBy(x => x.inv.ProductId);

                // For each product in that location
                foreach (var productGroup in groupedByProduct)
                {
                    var productId = productGroup.Key;

                    // create "moveInfo"
                    var moveInfo = new MoveByOisgirInfo();

                    // => get product
                    var product = await _context.Products
                        .FindAsync(productId);

                    moveInfo.Product = product;

                    // => get facilityLocationFrom
                    var facilityLocationFrom = await _context.FacilityLocations
                        .FindAsync(facilityId,
                            locationSeqId);
                    moveInfo.FacilityLocationFrom = facilityLocationFrom;

                    // => find the first FLT_PICKLOC location
                    // replicates:
                    // <entity-and entity-name="ProductFacilityLocationView" list="productFacilityLocationViewList">
                    //   <field-map field-name="productId" from-field="productId"/>
                    //   <field-map field-name="facilityId" from-field="parameters.facilityId"/>
                    //   <field-map field-name="locationTypeEnumId" value="FLT_PICKLOC"/>
                    // </entity-and>

                    var productFacilityLocationViewList =
                        await (
                            from pfl in _context.ProductFacilityLocations
                            join flTo in _context.FacilityLocations
                                on new { pfl.FacilityId, pfl.LocationSeqId } equals new
                                    { flTo.FacilityId, flTo.LocationSeqId }
                            where pfl.ProductId == productId
                                  && pfl.FacilityId == facilityId
                                  && flTo.LocationTypeEnumId == "FLT_PICKLOC"
                            select new
                            {
                                productFacilityLocation = pfl,
                                facilityLocation = flTo
                            }
                        ).ToListAsync();

                    if (!productFacilityLocationViewList.Any())
                    {
                        // add a warning: "Error in stock move, could not find a pick/primary location..."
                        warningMessageList.Add(
                            $"Error in stock move, could not find a pick/primary location for facility [{facilityId}] and product [{productId}]");
                    }
                    else
                    {
                        // choose the first
                        var productFacilityLocationView = productFacilityLocationViewList.First();
                        var facilityLocationTo = await _context.FacilityLocations
                            .FindAsync(
                                productFacilityLocationView.facilityLocation.FacilityId,
                                productFacilityLocationView.facilityLocation.LocationSeqId);
                        moveInfo.FacilityLocationTo = facilityLocationTo;

                        var targetProductFacilityLocation = await _context.ProductFacilityLocations
                            .FirstOrDefaultAsync(pfl =>
                                pfl.ProductId == productFacilityLocationView.productFacilityLocation.ProductId
                                && pfl.FacilityId == productFacilityLocationView.productFacilityLocation.FacilityId
                                && pfl.LocationSeqId ==
                                productFacilityLocationView.productFacilityLocation.LocationSeqId);
                        moveInfo.TargetProductFacilityLocation = targetProductFacilityLocation;

                        // => get totalQuantity from group items
                        // replicate "iterate ... calculate field=moveInfo.totalQuantity"
                        decimal totalQty = 0;
                        foreach (var row in productGroup)
                        {
                            // sum up row.oisgir.Quantity
                            totalQty += row.oisgir.Quantity ?? 0m;
                        }

                        moveInfo.TotalQuantity = totalQty;

                        // => check quantityOnHand in that location
                        var invItemsInLoc = await _context.InventoryItems
                            .Where(ii => ii.ProductId == productId
                                         && ii.FacilityId == facilityId
                                         && ii.LocationSeqId == locationSeqId)
                            .ToListAsync();

                        decimal totalQOH = 0;
                        decimal totalATP = 0;
                        foreach (var invIt in invItemsInLoc)
                        {
                            totalQOH += invIt.QuantityOnHandTotal ?? 0m;
                            totalATP += invIt.AvailableToPromiseTotal ?? 0m;
                        }

                        moveInfo.QuantityOnHandTotalFrom = totalQOH;
                        moveInfo.AvailableToPromiseTotalFrom = totalATP;

                        if (totalQOH < totalQty)
                        {
                            // replicate: <string-to-list string="Warning in stock move... add to warningMessageList"/>
                            warningMessageList.Add(
                                $"Warning in stock move: facility [{facilityId}] product [{productId}] going from location [{locationSeqId}] to location [{facilityLocationTo?.LocationSeqId}] requested quantity [{totalQty}] but only [{totalQOH}] on hand.");
                            // set totalQty = totalQOH
                            moveInfo.TotalQuantity = totalQOH;
                        }
                        else
                        {
                            // replicate the else: check if we do "pre-emptive transfer" for min stock
                            // (the minStock logic is skipped for brevity, but you can add it.)

                            // => get target location's total QOH / ATP
                            decimal targetQOH = 0;
                            decimal targetATP = 0;
                            var targetInvItems = await _context.InventoryItems
                                .Where(ii => ii.FacilityId == targetProductFacilityLocation.FacilityId
                                             && ii.LocationSeqId == targetProductFacilityLocation.LocationSeqId
                                             && ii.ProductId == targetProductFacilityLocation.ProductId)
                                .ToListAsync();
                            foreach (var targetInv in targetInvItems)
                            {
                                targetQOH += targetInv.QuantityOnHandTotal ?? 0m;
                                targetATP += targetInv.AvailableToPromiseTotal ?? 0m;
                            }

                            moveInfo.QuantityOnHandTotalTo = targetQOH;
                            moveInfo.AvailableToPromiseTotalTo = targetATP;

                            // if targetATP < targetProductFacilityLocation.MinimumStock, do "some logic"
                            // etc. add to "stockMoveHandled" if needed...
                        }

                        // Add the moveInfo to the result list
                        moveByOisgirInfoList.Add(moveInfo);
                    }
                }
            }

            // Assign final values
            result.MoveByOisgirInfoList = moveByOisgirInfoList;
            result.StockMoveHandled = stockMoveHandled;
            // Possibly fill stockMoveHandled or do more logic if minStock restocking is done
            result.WarningMessageList = warningMessageList;

            return result;
        }
        catch (Exception ex)
        {
            // log or rethrow as needed
            throw new ApplicationException($"Error in FindStockMovesNeeded for facility {facilityId}: {ex.Message}",
                ex);
        }
    }

    public async Task<UpdatePicklistResult> UpdatePicklist(UpdatePicklistInput input)
    {
        // No transaction or SaveChanges here: purely domain logic
        if (string.IsNullOrEmpty(input.PicklistId))
        {
            throw new ArgumentException("picklistId is required.", nameof(input.PicklistId));
        }

        // 1) Find the existing picklist
        var picklist = await _context.Picklists
            .FirstOrDefaultAsync(p => p.PicklistId == input.PicklistId);

        if (picklist == null)
        {
            throw new KeyNotFoundException($"No Picklist found for ID = {input.PicklistId}");
        }

        // store oldStatusId for returning
        var oldStatusId = picklist.StatusId;

        // 2) If a new status is provided and differs...
        if (!string.IsNullOrEmpty(input.StatusId)
            && !input.StatusId.Equals(picklist.StatusId, StringComparison.OrdinalIgnoreCase))
        {
            // Validate status change
            var svc = await _context.StatusValidChanges
                .FirstOrDefaultAsync(sv => sv.StatusId == picklist.StatusId
                                           && sv.StatusIdTo == input.StatusId);

            if (svc == null)
            {
                // not a valid status transition
                throw new InvalidOperationException(
                    $"Changing status from '{picklist.StatusId}' to '{input.StatusId}' is not allowed.");
            }

            // Create a status history record
            var newHistory = new PicklistStatusHistory
            {
                PicklistId = picklist.PicklistId,
                StatusId = picklist.StatusId,
                StatusIdTo = input.StatusId,
                ChangeDate = DateTime.UtcNow,
                ChangeUserLoginId = input.UserLoginId
            };
            await _context.PicklistStatusHistories.AddAsync(newHistory);

            // Update picklist status
            picklist.StatusId = input.StatusId;
        }

        // 3) Set non-PK fields from input
        // e.g. picklist.SomeField = input.SomeField;
        // picklist.SomeOtherField = input.SomeOtherField;

        // 4) lastModifiedByUserLogin
        picklist.LastModifiedByUserLogin = input.UserLoginId;
        picklist.LastUpdatedStamp = DateTime.UtcNow;

        // Return old status
        return new UpdatePicklistResult
        {
            OldStatusId = oldStatusId
        };
    }

    /*public async Task<LoadPackingDataResult> LoadPackingData(LoadPackingDataInput input)
    {
        var result = new LoadPackingDataResult();

        try
        {
            // 1) Create or retrieve the packSession 
            //    (like 'packSession = new PackingSession(dispatcher, userLogin)')
            var currentUser = await _context.Users
                .SingleOrDefaultAsync(x => x.UserName == _userAccessor.GetUsername());
            var userLogin = await _context.UserLogins
                .SingleOrDefaultAsync(x => x.PartyId == currentUser.PartyId);

            var packSession = new PackingSession(
                userLogin,
                input.FacilityId,
                input.PicklistBinId,
                input.OrderId,
                input.ShipGroupSeqId
            )
            {
                Context = _context,
                IssuanceService = _issuanceService,
                PickListService = this,
                ShipmentHelperService = _shipmentHelperService,
                UtilityService = _utilityService,
                Logger = _logger2
            };

            // 2) Validate mandatory facilityId
            if (string.IsNullOrEmpty(input.FacilityId))
            {
                throw new ArgumentException("facilityId is required.", nameof(input.FacilityId));
            }

            result.FacilityId = input.FacilityId;

            // => fetch facility
            var facility = await _context.Facilities
                .FirstOrDefaultAsync(f => f.FacilityId == input.FacilityId);
            // result.Facility = facility;

            // 3) If shipmentId is given, fetch shipment & invoiceIds
            if (!string.IsNullOrEmpty(input.ShipmentId))
            {
                result.ShipmentId = input.ShipmentId;
                var shipmentEntity = await _context.Shipments
                    .FirstOrDefaultAsync(s => s.ShipmentId == input.ShipmentId);
                result.Shipment = shipmentEntity;

                if (shipmentEntity != null && !string.IsNullOrEmpty(shipmentEntity.PrimaryOrderId))
                {
                    var orderItemBillingList = await _context.OrderItemBillings
                        .Where(oib => oib.OrderId == shipmentEntity.PrimaryOrderId)
                        .OrderBy(oib => oib.InvoiceId)
                        .ToListAsync();

                    var invoiceIds = orderItemBillingList
                        .Select(oib => oib.InvoiceId)
                        .Distinct()
                        .ToList();

                    if (invoiceIds.Any())
                    {
                        result.InvoiceIds = invoiceIds;
                    }
                }
            }

            // 4) Check for orderId/shipGroup in 'orderId/shipGroup' format
            var orderId = input.OrderId;
            var shipGroupSeqId = input.ShipGroupSeqId;
            if (!string.IsNullOrEmpty(orderId) && string.IsNullOrEmpty(shipGroupSeqId) && orderId.Contains("/"))
            {
                var parts = orderId.Split('/');
                orderId = parts[0];
                shipGroupSeqId = parts[1];
            }
            else if (!string.IsNullOrEmpty(orderId) && string.IsNullOrEmpty(shipGroupSeqId))
            {
                shipGroupSeqId = "01";
            }

            // 5) Handle picklist bin if present, replicate the logic
            var picklistBinId = input.PicklistBinId;
            if (!string.IsNullOrEmpty(picklistBinId))
            {
                var bin = await _context.PicklistBins
                    .FirstOrDefaultAsync(b => b.PicklistBinId == picklistBinId);
                if (bin != null)
                {
                    orderId = bin.PrimaryOrderId;
                    shipGroupSeqId = bin.PrimaryShipGroupSeqId;

                    // In Groovy: packSession.addItemInfo(bin.getRelated(...))
                    var pendingItems = await _context.PicklistItems
                        .Where(pi => pi.PicklistBinId == bin.PicklistBinId
                                     && pi.ItemStatusId == "PICKITEM_PENDING")
                        .Select(pi => new ShippableItemDto()
                        {
                            PicklistBinId = pi.PicklistBinId,
                            OrderId = pi.OrderId,
                            OrderItemSeqId = pi.OrderItemSeqId,
                            InventoryItemId = pi.InventoryItemId,
                            Quantity = (decimal)pi.Quantity
                        })
                        .ToListAsync();

                    var pendingItems2 = await _context.PicklistItems
                        .Where(pi => pi.PicklistBinId == bin.PicklistBinId
                                     && pi.ItemStatusId == "PICKITEM_PENDING")
                        .Select(pi => new PicklistItem
                        {
                            PicklistBinId = pi.PicklistBinId,
                            OrderId = pi.OrderId,
                            OrderItemSeqId = pi.OrderItemSeqId,
                            InventoryItemId = pi.InventoryItemId,
                            Quantity = (decimal)pi.Quantity
                        })
                        .ToListAsync();

                    packSession.AddItemInfo(pendingItems);
                    result.PicklistItems = pendingItems2;
                }

                result.PicklistBinId = picklistBinId;
            }

            // If invoiceIds exist => orderId = null
            if (result.InvoiceIds.Any())
            {
                orderId = null;
            }

            result.OrderId = orderId;
            result.ShipGroupSeqId = shipGroupSeqId;

            // 6) Check if there's an existing SHIPMENT_PICKED for that order
            Shipment existingShipment = null;
            if (!string.IsNullOrEmpty(orderId))
            {
                existingShipment = await _context.Shipments
                    .Where(s => s.PrimaryOrderId == orderId && s.StatusId == "SHIPMENT_PICKED")
                    .FirstOrDefaultAsync();
            }

            // 7) If we have an orderId => load orderHeader, do validations
            if (!string.IsNullOrEmpty(orderId))
            {
                var orderHeader = await _context.OrderHeaders
                    .Where(oh => oh.OrderId == orderId)
                    .FirstOrDefaultAsync();

                if (orderHeader != null)
                {
                    result.OrderHeader = new OrderHeaderDto
                    {
                        OrderId = orderHeader.OrderId,
                        StatusId = orderHeader.StatusId
                    };

                    var orh = new OrderReadHelper(orderId)
                    {
                        Context = _context
                    };
                    orh.InitializeOrder();

                    var orderItemShipGroup = await orh.GetOrderItemShipGroups();

                    // EXACT MIRROR: if (\"ORDER_APPROVED\".equals(orderHeader.statusId)) { ... }
                    if (orderHeader.StatusId == "ORDER_APPROVED")
                    {
                        if (!string.IsNullOrEmpty(shipGroupSeqId))
                        {
                            if (existingShipment == null)
                            {
                                // THE ADDED LOGIC: Generate cost estimate for the ship group
                                // (like the Groovy code: orh.getProductStoreId(), etc.)
                                // We'll mimic the approach:
                                // 1) get productStoreId
                                // 2) get shippableItemInfo
                                // 3) get from \"OrderItemAndShipGrpInvResAndItemSum\"
                                // 4) compute totals
                                // 5) call packSession.getShipmentCostEstimate(...) if method info is set
                                // 6) if no picklistBin => packSession.addItemInfo(shippableItems)

                                // 7a) productStoreId
                                //     (In Ofbiz, 'orh.getProductStoreId()' uses store from order. 
                                //      We'll do a placeholder approach or similar if you have a store link.)
                                var productStoreId = orderHeader.ProductStoreId;
                                result.ProductStoreId = productStoreId;

                                // 7b) shippableItemInfo => mimic 'orh.getOrderItemAndShipGroupAssoc(shipGroupSeqId)'
                                //     We'll do a sample query. Adjust as needed for your DB structure:
                                var shippableItemInfo = await orh.GetOrderItemAndShipGroupAssoc(shipGroupSeqId);

                                // 7c) shippableItems => from(\"OrderItemAndShipGrpInvResAndItemSum\")
                                //     We'll do a sample entity table approach:
                                var shippableItems = await (
                                    from oi in _context.OrderItems
                                    join oisgir in _context.OrderItemShipGrpInvRes
                                        on new { oi.OrderId, oi.OrderItemSeqId }
                                        equals new { oisgir.OrderId, oisgir.OrderItemSeqId }
                                    join ii in _context.InventoryItems
                                        on oisgir.InventoryItemId equals ii.InventoryItemId
                                    join p in _context.Products // Join Product to get product name
                                        on oi.ProductId equals p.ProductId
                                    where oi.OrderId == orderId
                                          && oisgir.ShipGroupSeqId == shipGroupSeqId
                                    group new { oi, oisgir, ii, p } by new
                                    {
                                        oi.OrderId,
                                        oi.OrderItemSeqId,
                                        oi.ProductId,       // Product ID from OrderItem
                                        ii.InventoryItemId, // Correct InventoryItem ID
                                        oisgir.ShipGroupSeqId
                                    }
                                    into g
                                    select new ShippableItemDto
                                    {
                                        OrderId = g.Key.OrderId,
                                        OrderItemSeqId = g.Key.OrderItemSeqId,
                                        ProductId = g.Key.ProductId,          // From OrderItem
                                        ProductName = g.First().p.ProductName, // From joined Product
                                        InventoryItemId = g.Key.InventoryItemId, // Actual InventoryItem ID
                                        ShipGroupSeqId = g.Key.ShipGroupSeqId,
                                        QuantityOrdered = g.Sum(x => 
                                            (x.oi.Quantity ?? 0) - (x.oi.CancelQuantity ?? 0)),
                                        TotQuantityReserved = g.Sum(x => x.oisgir.Quantity ?? 0),
                                        TotQuantityNotAvailable = g.Sum(x => x.oisgir.QuantityNotAvailable ?? 0),
                                        TotQuantityAvailable = g.Sum(x => 
                                            (x.oisgir.Quantity ?? 0) - (x.oisgir.QuantityNotAvailable ?? 0))
                                    }
                                ).ToListAsync();
                                
                                var bulkPackItems = new List<BulkPackItemDto>();

                                
                                var orderItems = await _context.OrderItems
                                    .Where(oi => oi.OrderId == orderId)
                                    .ToListAsync();
                                foreach (var item in orderItems)
                                {
                                    var bulkPackItem = new BulkPackItemDto
                                    {
                                        OrderId = item.OrderId,
                                        OrderItemSeqId = item.OrderItemSeqId,
                                        ShippedQuantity = await orh.GetItemShippedQuantity(item)
                                    };
                                    bulkPackItems.Add(bulkPackItem);
                                }
                                
                                result.BulkPackItems = bulkPackItems;


                                // 7d) shippableTotal, shippableWeight, shippableQuantity
                                //     Suppose you have separate methods to compute:
                                decimal shippableTotal = await orh.GetShippableTotal();
                                decimal shippableQuantity = await orh.GetShippableQuantity();


                                // set context.productStoreId = productStoreId
                                // we store in result
                                // if (!picklistBinId) { packSession.addItemInfo(shippableItems) }
                                // We only do the addItemInfo if we have no picklist bin
                                if (string.IsNullOrEmpty(picklistBinId))
                                {
                                    // packSession.addItemInfo(shippableItems)
                                    // We'll do some quick 'shippableItems' => Dto to pass in


                                    packSession.AddItemInfo(shippableItems);
                                }
                            }
                            else
                            {
                                // else => existing shipment => error
                                result.ErrorMessage =
                                    "Order has already been verified (SHIPMENT_PICKED).";
                            }
                        }
                        else
                        {
                            // no ship group => error
                            result.ErrorMessage = "No ship group sequence ID. Cannot process.";
                        }
                    }
                    else
                    {
                        // not ORDER_APPROVED => \"Order # is not approved for packing.\"
                        result.ErrorMessage =
                            "Order #{orderId} is not approved for packing";
                    }
                }
                else
                {
                    // orderHeader is null => \"cannot be found.\"
                    result.ErrorMessage = "Order #{orderId} cannot be found.";
                }
            }

            // 8) Determine defaultWeightUomId
            string defaultWeightUomId = null;
            if (facility != null && !string.IsNullOrEmpty(facility.DefaultWeightUomId))
            {
                defaultWeightUomId = facility.DefaultWeightUomId;
            }

            if (string.IsNullOrEmpty(defaultWeightUomId))
            {
                // fallback to \"WT_kg\"
                defaultWeightUomId = "WT_kg";
            }

            result.DefaultWeightUomId = defaultWeightUomId;

            // fetch the Uom
            var defaultWeightUom = await _context.Uoms
                .FirstOrDefaultAsync(u => u.UomId == defaultWeightUomId);
            result.DefaultWeightUom = defaultWeightUom;

            packSession.SetFacilityId(input.FacilityId);
            packSession.SetPrimaryOrderId(orderId);
            packSession.SetPrimaryShipGroupSeqId(shipGroupSeqId);
            packSession.SetPicklistBinId(picklistBinId);

            var rawLines = packSession.GetLines();  // List<PackingSessionLine>
            result.PackedLines = rawLines
                .Select(line => new PackingSessionLineDto
                {
                    OrderId = line.OrderId,
                    OrderItemSeqId = line.OrderItemSeqId,
                    ShipGroupSeqId = line.ShipGroupSeqId,
                    ProductId = line.ProductId,
                    InventoryItemId = line.InventoryItemId,
                    Quantity = line.Quantity,
                    Weight = line.Weight,
                    PackageSeq = line.PackageSeq
                }).ToList();
            
            result.ItemDisplays = packSession.GetItemInfos();

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading packing data for facility {FacilityId}", input.FacilityId);
            throw new ApplicationException("Failed to load packing data: {ex.Message}", ex);
        }
    }
    */

    public async Task<LoadPackingDataResult> LoadPackingData(LoadPackingDataInput input)
    {
        var result = new LoadPackingDataResult();
        var orderId = input.OrderId;
        var shipGroupSeqId = input.ShipGroupSeqId;

        try
        {
            // 1) Validate inputs
            if (string.IsNullOrEmpty(input.FacilityId))
            {
                throw new ArgumentException("FacilityId is required.", nameof(input.FacilityId));
            }

            if (string.IsNullOrEmpty(orderId))
            {
                throw new ArgumentException("OrderId is required.", nameof(input.OrderId));
            }

            result.FacilityId = input.FacilityId;
            result.OrderId = orderId;
            result.ShipGroupSeqId = shipGroupSeqId;

            // 2) Handle orderId/shipGroupSeqId format
            if (orderId.Contains("/"))
            {
                var parts = orderId.Split('/');
                orderId = parts[0];
                shipGroupSeqId = parts[1];
                result.OrderId = orderId;
                result.ShipGroupSeqId = shipGroupSeqId;
            }
            else if (string.IsNullOrEmpty(shipGroupSeqId))
            {
                shipGroupSeqId = "00001";
                result.ShipGroupSeqId = shipGroupSeqId;
            }

            // 3) Fetch InvoiceIds from OrderItemBillings
            var invoiceIds = await _context.OrderItemBillings
                .Where(oib => oib.OrderId == orderId)
                .Select(oib => oib.InvoiceId)
                .Distinct()
                .OrderBy(id => id)
                .ToListAsync();
            if (invoiceIds.Any())
            {
                result.InvoiceIds = invoiceIds;
                _logger.LogDebug("Found invoice IDs for OrderId: {OrderId}: {InvoiceIds}",
                    orderId, string.Join(", ", invoiceIds));
            }

            // 4) Fetch Items for Kendo Grid
            var orh = new OrderReadHelper(orderId) { Context = _context };
            orh.InitializeOrder();

            var itemsQuery = await (
                from oi in _context.OrderItems
                join p in _context.Products
                    on oi.ProductId equals p.ProductId
                from oisgir in _context.OrderItemShipGrpInvRes
                    .Where(x => x.OrderId == oi.OrderId && x.OrderItemSeqId == oi.OrderItemSeqId && x.ShipGroupSeqId == shipGroupSeqId)
                    .DefaultIfEmpty()
                from ii in _context.InventoryItems
                    .Where(x => x.InventoryItemId == oisgir.InventoryItemId)
                    .DefaultIfEmpty()
                where oi.OrderId == orderId
                select new
                {
                    OrderId = oi.OrderId,
                    OrderItemSeqId = oi.OrderItemSeqId,
                    ProductId = oi.ProductId,
                    ProductName = p.ProductName,
                    InventoryItemId = ii != null ? ii.InventoryItemId : null,
                    ShipGroupSeqId = oisgir != null ? oisgir.ShipGroupSeqId : shipGroupSeqId,
                    Quantity = (oi.Quantity ?? 0) - (oi.CancelQuantity ?? 0),
                    OrderItem = oi
                }
            ).ToListAsync();

            var packingItems = new List<PackingItemDto>();
            foreach (var item in itemsQuery)
            {
                var shippedQuantity = await orh.GetItemShippedQuantity(item.OrderItem);
                var quantityToShip = item.Quantity - shippedQuantity;
                packingItems.Add(new PackingItemDto
                {
                    OrderId = item.OrderId,
                    OrderItemSeqId = item.OrderItemSeqId,
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    InventoryItemId = item.InventoryItemId,
                    ShipGroupSeqId = item.ShipGroupSeqId,
                    Quantity = item.Quantity,
                    ShippedQuantity = shippedQuantity,
                    QuantityToShip = quantityToShip,
                    IncludeThisItem = true
                });
                _logger.LogDebug("Item for OrderId: {OrderId}, OrderItemSeqId: {OrderItemSeqId}: Quantity={Quantity}, ShippedQuantity={ShippedQuantity}, QuantityToShip={QuantityToShip}, InventoryItemId={InventoryItemId}",
                    item.OrderId, item.OrderItemSeqId, item.Quantity, shippedQuantity, quantityToShip, item.InventoryItemId);
            }

            result.Items = packingItems;
            _logger.LogInformation("LoadPackingData for OrderId: {OrderId}, Items: {Count}, InvoiceIds: {InvoiceCount}",
                orderId, result.Items.Count, result.InvoiceIds.Count);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading packing data for OrderId: {OrderId}", orderId);
            throw new ApplicationException($"Failed to load packing data: {ex.Message}", ex);
        }
    }

    public async Task<List<OrderPicklistBinDto>> GetValidOrderPicklistBins(string facilityId)
    {
        // Validate the facilityId parameter.
        if (string.IsNullOrEmpty(facilityId))
        {
            throw new ArgumentException("facilityId is required.", nameof(facilityId));
        }

        // Query the PicklistBins that are associated with a Picklist
        // in the specified facility, and that are not in a terminal status.
        var query = from bin in _context.PicklistBins
            join picklist in _context.Picklists
                on bin.PicklistId equals picklist.PicklistId
            where picklist.FacilityId == facilityId
                  && picklist.StatusId != "PICKLIST_PICKED"
                  && picklist.StatusId != "PICKLIST_CANCELLED"
            select new OrderPicklistBinDto
            {
                // The picklist bin’s PrimaryOrderId will be used as the valid OrderId.
                OrderId = bin.PrimaryOrderId,
                PicklistBinId = bin.PicklistBinId
            };

        // Use Distinct() if there are duplicates and return the list.
        var result = await query.Distinct().ToListAsync();
        return result;
    }

    public async Task UpdatePicklistItem(string picklistBinId, string orderId, string orderItemSeqId,
        string shipGroupSeqId, string inventoryItemId, string itemStatusId, decimal? quantity)
    {
        try
        {
            // Step 1: Build the primary key fields manually instead of using a dictionary
            var lookupPKMap = new
            {
                PicklistBinId = picklistBinId,
                OrderId = orderId,
                OrderItemSeqId = orderItemSeqId,
                ShipGroupSeqId = shipGroupSeqId,
                InventoryItemId = inventoryItemId
            };

            // Step 2: Query the database using LINQ to find the PicklistItem based on primary key fields
            var lookedUpValue = await _context.PicklistItems
                .Where(pi => pi.PicklistBinId == lookupPKMap.PicklistBinId &&
                             pi.OrderId == lookupPKMap.OrderId &&
                             pi.OrderItemSeqId == lookupPKMap.OrderItemSeqId &&
                             pi.ShipGroupSeqId == lookupPKMap.ShipGroupSeqId &&
                             pi.InventoryItemId == lookupPKMap.InventoryItemId)
                .FirstOrDefaultAsync();

            if (lookedUpValue == null)
            {
                // If the PicklistItem is not found, log the error and throw an exception
                _logger.LogError(
                    $"PicklistItem not found for BinId: {lookupPKMap.PicklistBinId}, OrderId: {lookupPKMap.OrderId}");
                throw new Exception("PicklistItem not found.");
            }

            // Step 3: Check if the itemStatusId is being changed
            if (!string.IsNullOrEmpty(itemStatusId) && lookedUpValue.ItemStatusId != itemStatusId)
            {
                // Query to check if the status change is valid (i.e., a valid transition exists between the current and new status)
                var statusValidChange = await _context.StatusValidChanges
                    .Where(svc => svc.StatusId == lookedUpValue.ItemStatusId && svc.StatusIdTo == itemStatusId)
                    .FirstOrDefaultAsync();

                if (statusValidChange == null)
                {
                    // If no valid status change exists, log the error and throw an exception
                    _logger.LogError(
                        $"Changing status from {lookedUpValue.ItemStatusId} to {itemStatusId} is not allowed.");
                    throw new Exception(
                        $"Changing the status from {lookedUpValue.ItemStatusId} to {itemStatusId} is not allowed.");
                }
            }

            // Step 4: Check for any other errors, such as missing or invalid parameters
            // No explicit error-checking logic is provided here, but you can add validations if needed (e.g., check if quantity is provided if required)

            // Step 5: Before updating non-primary key fields, save the old item status ID for returning it later
            string oldItemStatusId = lookedUpValue.ItemStatusId;

            // Step 6: Update non-primary key fields (like itemStatusId, quantity, etc.) if provided
            if (!string.IsNullOrEmpty(itemStatusId))
            {
                lookedUpValue.ItemStatusId = itemStatusId; // Update the item status ID
            }

            // Check if quantity is provided and update if necessary
            if (quantity.HasValue)
            {
                lookedUpValue.Quantity = quantity.Value;
            }

            // Step 7: Save the changes to the database using EF
            _context.PicklistItems.Update(lookedUpValue);

            // Step 8: Log the old item status ID (before update)
            _logger.LogInformation($"Old Item Status ID: {oldItemStatusId}");

            // Step 9: Return the result containing the old item status ID as part of the response
            // The oldItemStatusId can be returned if needed by the caller
        }
        catch (Exception ex)
        {
            // Log the exception and rethrow it for higher-level handling
            _logger.LogError(ex, "Error occurred in UpdatePicklistItem method.");
            throw;
        }
    }
}