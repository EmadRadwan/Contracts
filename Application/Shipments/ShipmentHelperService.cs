using Application.Accounting.Services;
using Application.Core;
using Application.Errors;
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Shipments;

public interface IShipmentHelperService
{
    Task<QuickShipResult> QuickShipEntireOrder(
        string orderId,
        string? originFacilityId,
        string? setPackedOnly,
        DateTime? eventDate);

    Task<OperationResult> UpdatePurchaseShipmentFromReceipt(string shipmentId);
    Task<string> CreateShipment(ShipmentContext parameters);
    Task<UpdateShipmentResult> UpdateShipment(ShipmentUpdateParameters parameters);
    Task<string> AddShipmentContentToPackage(string shipmentPackageSeqId, decimal quantity, string shipmentId);
    //Task<List<ShipmentReceipt>> GetShipmentReceipts(IEnumerable<string> shipmentIds);
}

public class ShipmentHelperService : IShipmentHelperService
{
    private readonly DataContext _context;
    private readonly IIssuanceService _issuanceService;
    private readonly ILogger _logger;
    private readonly IInvoiceHelperService _invoiceHelperService;
    private readonly IUtilityService _utilityService;
    private readonly Lazy<IShipmentService> _shipmentService;
    //private readonly IEntityRepository _repository;


    public ShipmentHelperService(DataContext context, IUtilityService utilityService,
        ILogger<ShipmentService> logger,
        IIssuanceService issuanceService, IInvoiceHelperService invoiceHelperService,
        Lazy<IShipmentService> shipmentService)
    {
        _context = context;
        _utilityService = utilityService;
        _logger = logger;
        _issuanceService = issuanceService;
        _invoiceHelperService = invoiceHelperService;
        _shipmentService = shipmentService;
       // _repository = repository;
    }


    public async Task<QuickShipResult> QuickShipEntireOrder(
        string orderId,
        string? originFacilityId,
        string? setPackedOnly,
        DateTime? eventDate)
    {
        var shipmentShipGroupFacilityList = new List<Shipment>();

        var shipmentIds = new List<string>();

        try
        {
            // Fetch the order header directly
            var orderHeader = await (from oh in _context.OrderHeaders
                where oh.OrderId == orderId
                select oh).FirstOrDefaultAsync();

            if (orderHeader == null) throw new Exception("Order header not found.");

            // Ensure the product store exists
            if (string.IsNullOrEmpty(orderHeader.ProductStoreId))
                throw new Exception("Facility shipment missing product store.");

            // Fetch the product store directly
            var productStore = await (from ps in _context.ProductStores
                where ps.ProductStoreId == orderHeader.ProductStoreId
                select ps).FirstOrDefaultAsync();

            if (productStore == null) throw new Exception("Product store not found.");

            // Check if the product store reserves inventory
            if (productStore.ReserveInventory != "Y")
                throw new Exception("No shipment created for non-reserve inventory.");

            // Check if order items explode
            if (productStore.ExplodeOrderItems == "Y")
                throw new Exception("No shipment created for exploded order items.");

            // Fetch approved order items using the underlying tables directly
            var orderItemAndShipGrpInvResAndItemList = await (from oi in _context.OrderItems
                join oisga in _context.OrderItemShipGroupAssocs
                    on new { oi.OrderId, oi.OrderItemSeqId } equals new { oisga.OrderId, oisga.OrderItemSeqId }
                join oisgir in _context.OrderItemShipGrpInvRes
                    on new { oi.OrderId, oi.OrderItemSeqId, oisga.ShipGroupSeqId } equals new
                        { oisgir.OrderId, oisgir.OrderItemSeqId, oisgir.ShipGroupSeqId }
                join ii in _context.InventoryItems on oisgir.InventoryItemId equals
                    ii.InventoryItemId // Joining InventoryItems for additional info
                where oi.OrderId == orderId && oi.StatusId == "ITEM_APPROVED"
                select new
                {
                    oi.OrderId,
                    oi.OrderItemSeqId,
                    oisga.ShipGroupSeqId,
                    OrderItemQuantity = oi.Quantity, // Accessing quantity
                    OrderItemCancelQuantity = oi.CancelQuantity, // Accessing cancel quantity
                    ii.FacilityId // Including the FacilityId if needed
                }).ToListAsync();


            // Ensure there's something to ship
            if (!orderItemAndShipGrpInvResAndItemList.Any())
                throw new Exception("No approved items available to ship.");

            // Call to get order item ship group lists
            var orderItemShipGroupResult = await GetOrderItemShipGroupLists(orderHeader.OrderId);
            var orderItemAndShipGroupAssocList = orderItemShipGroupResult.OrderItemAndShipGroupAssocList;
            var orderItemListByShGrpMap = orderItemShipGroupResult.OrderItemListByShGrpMap;

            // Assuming 'orderItemAndShipGroupAssocList' is List<OrderItemShipGroup>
            if (orderItemAndShipGroupAssocList.Count > 1)
            {
                // Group the list by ShipGroupSeqId
                var groupedByShipGroupSeqId = orderItemAndShipGroupAssocList
                    .GroupBy(x => x.ShipGroupSeqId)
                    .ToList();
                
                // Or if you just want a new list with one representative per group
                var newList = groupedByShipGroupSeqId
                    .Select(g => g.First()) // or do some aggregation here if needed
                    .ToList();

                orderItemAndShipGroupAssocList = newList;
            }


            // Create a list to hold unique facilities
            var uniqueFacilities = new List<Facility>();
            foreach (var item in orderItemAndShipGrpInvResAndItemList)
                if (!uniqueFacilities.Any(f => f.FacilityId == item.FacilityId))
                    uniqueFacilities.Add(new Facility
                    {
                        FacilityId = item.FacilityId
                    });

            // Traverse facilities and create shipments for each
            foreach (var facility in uniqueFacilities)
            {
                var shipmentInfo = await _shipmentService.Value.CreateShipmentForFacilityAndShipGroup(
                    orderHeader,
                    facility.FacilityId,
                    orderItemAndShipGroupAssocList,
                    eventDate ?? DateTime.UtcNow,
                    null
                );

                //shipmentShipGroupFacilityList.Add(shipmentInfo);
            }

            // Check if any shipments were created
            //if (shipmentShipGroupFacilityList.Count == 0) throw new Exception("Facility shipment not created.");

            // Return the result
            return new QuickShipResult
            {
                ShipmentShipGroupFacilityList = shipmentShipGroupFacilityList,
                ShipmentIds = shipmentIds
            };
        }
        catch (Exception ex)
        {
            // Log the exception (consider using a logging framework)
            Console.WriteLine($"An error occurred: {ex.Message}");
            throw; // Optionally rethrow the exception for further handling
        }
    }

    public async Task<string> CreateShipment(ShipmentContext parameters)
    {
        // Get the next sequence ID for Shipment
        var newShipmentSequence = await _utilityService.GetNextSequence("Shipment");

        // Create a new Shipment entity
        var newEntity = new Shipment
        {
            ShipmentId = newShipmentSequence,
            ShipmentTypeId = parameters.ShipmentTypeId,
            PrimaryShipGroupSeqId = string.IsNullOrEmpty(parameters.PrimaryShipGroupSeqId) ? "01" : parameters.PrimaryShipGroupSeqId, PrimaryOrderId = parameters.PrimaryOrderId,
            StatusId = parameters.StatusId,
            OriginFacilityId = parameters.OriginFacilityId,
            DestinationFacilityId = parameters.DestinationFacilityId,
            PartyIdFrom = parameters.PartyIdFrom,
            PartyIdTo = parameters.PartyIdTo,
            CreatedDate = DateTime.UtcNow,
            LastModifiedDate = DateTime.UtcNow,
            CreatedStamp = DateTime.UtcNow,
            LastUpdatedStamp = DateTime.UtcNow
        };

        // Add the new shipment to the context
        await _context.Shipments.AddAsync(newEntity);


        // ECA 2: setShipmentSettingsFromFacilities (sync, if originFacilityId is not empty)
        if (!string.IsNullOrEmpty(parameters.OriginFacilityId))
        {
            try
            {
                await SetShipmentSettingsFromFacilities(newEntity.ShipmentId);
            }
            catch (Exception)
            {
                // Ignore errors per ECA settings
            }
        }

        // ECA 3: setShipmentSettingsFromFacilities (sync, if destinationFacilityId is not empty)
        if (!string.IsNullOrEmpty(parameters.DestinationFacilityId))
        {
            try
            {
                await SetShipmentSettingsFromFacilities(newEntity.ShipmentId);
            }
            catch (Exception)
            {
                // Ignore errors per ECA settings
            }
        }

        // ECA 4: setShipmentSettingsFromPrimaryOrder (sync, if primaryOrderId is not empty)
        if (!string.IsNullOrEmpty(parameters.PrimaryOrderId))
        {
            try
            {
                await SetShipmentSettingsFromPrimaryOrder(newEntity.ShipmentId);
            }
            catch (Exception)
            {
                // Ignore errors per ECA settings
            }
        }

        // Create ShipmentStatus history if StatusId is provided
        if (!string.IsNullOrEmpty(newEntity.StatusId))
        {
            try
            {
                await CreateShipmentStatus(newEntity.StatusId, newEntity.ShipmentId);
            }
            catch (Exception)
            {
                // Optional: Ignore errors or log, depending on your requirements
            }
        }

        // Return the Shipment ID
        return newEntity.ShipmentId;
    }


    public async Task<UpdateShipmentResult> UpdateShipment(ShipmentUpdateParameters parameters)
    {
        // Set the operation name (for logging/auditing purposes)
        var operationName = "Update Shipment";
        var errorList = new List<string>();


        // Check if the shipment status can be changed
        await _issuanceService.CheckCanChangeShipmentStatusDelivered(parameters.ShipmentId);

        // Lookup shipment by primary key
        var lookedUpValue = await _context.Shipments.FindAsync(parameters.ShipmentId);
        if (lookedUpValue == null) throw new Exception("Shipment not found.");

        // Get shipment type
        var shipmentTypeId = lookedUpValue.ShipmentTypeId;

        // Validate status change if provided and different
        if (!string.IsNullOrEmpty(parameters.StatusId) && parameters.StatusId != lookedUpValue.StatusId)
        {
            var isValidChange = await _context.StatusValidChanges
                .AnyAsync(svc => svc.StatusId == lookedUpValue.StatusId && svc.StatusIdTo == parameters.StatusId);
            if (!isValidChange)
                throw new Exception(
                    $"Changing the status from {lookedUpValue.StatusId} to {parameters.StatusId} is not allowed.");

            // Create shipment status record
            await CreateShipmentStatus(parameters.StatusId, parameters.ShipmentId);
        }

        // --- WorkEffort Updates ---

        // Check for pickup WorkEffort changes (estimated ship date, origin facility, or status change to specific values)
        if ((parameters.EstimatedShipDate.HasValue &&
             parameters.EstimatedShipDate != lookedUpValue.EstimatedShipDate) ||
            (!string.IsNullOrEmpty(parameters.OriginFacilityId) &&
             parameters.OriginFacilityId != lookedUpValue.OriginFacilityId) ||
            (!string.IsNullOrEmpty(parameters.StatusId) && parameters.StatusId != lookedUpValue.StatusId &&
             (parameters.StatusId == "SHIPMENT_CANCELLED" || parameters.StatusId == "SHIPMENT_PACKED" ||
              parameters.StatusId == "SHIPMENT_SHIPPED")))
        {
            var estShipWe = await _context.WorkEfforts.FindAsync(lookedUpValue.EstimatedShipWorkEffId);
            if (estShipWe != null)
            {
                estShipWe.EstimatedStartDate = parameters.EstimatedShipDate ?? estShipWe.EstimatedStartDate;
                estShipWe.EstimatedCompletionDate = parameters.EstimatedShipDate ?? estShipWe.EstimatedCompletionDate;
                estShipWe.FacilityId = parameters.OriginFacilityId ?? estShipWe.FacilityId;

                // Map shipment status to work effort status if changed
                if (!string.IsNullOrEmpty(parameters.StatusId) && parameters.StatusId != lookedUpValue.StatusId)
                {
                    estShipWe.CurrentStatusId = parameters.StatusId switch
                    {
                        "SHIPMENT_CANCELLED" => "CAL_CANCELLED",
                        "SHIPMENT_PACKED" => "CAL_CONFIRMED",
                        "SHIPMENT_SHIPPED" => "CAL_COMPLETED",
                        _ => estShipWe.CurrentStatusId
                    };
                }

                //_context.Set<WorkEffort>().Update(estShipWe);
            }
        }

        // Check for arrival WorkEffort changes (estimated arrival date or destination facility)
        if ((parameters.EstimatedArrivalDate.HasValue &&
             parameters.EstimatedArrivalDate != lookedUpValue.EstimatedArrivalDate) ||
            (!string.IsNullOrEmpty(parameters.DestinationFacilityId) &&
             parameters.DestinationFacilityId != lookedUpValue.DestinationFacilityId))
        {
            var estimatedArrivalWe =
                await _context.WorkEfforts.FindAsync(lookedUpValue.EstimatedArrivalWorkEffId);
            if (estimatedArrivalWe != null)
            {
                estimatedArrivalWe.EstimatedStartDate =
                    parameters.EstimatedArrivalDate ?? estimatedArrivalWe.EstimatedStartDate;
                estimatedArrivalWe.EstimatedCompletionDate =
                    parameters.EstimatedArrivalDate ?? estimatedArrivalWe.EstimatedCompletionDate;
                estimatedArrivalWe.FacilityId = parameters.DestinationFacilityId ?? estimatedArrivalWe.FacilityId;
                //_context.Set<WorkEffort>().Update(estimatedArrivalWe);
            }
        }

        // --- Party Assignment (if changed) ---
        if (!string.IsNullOrEmpty(parameters.PartyIdFrom) && parameters.PartyIdFrom != lookedUpValue.PartyIdFrom &&
            !string.IsNullOrEmpty(lookedUpValue.EstimatedShipWorkEffId))
        {
            //await _productRunService.AssignPartyToWorkEffort(lookedUpValue.EstimatedShipWorkEffId, parameters.PartyIdFrom);
        }

        if (!string.IsNullOrEmpty(parameters.PartyIdTo) && parameters.PartyIdTo != lookedUpValue.PartyIdTo &&
            !string.IsNullOrEmpty(lookedUpValue.EstimatedArrivalWorkEffId))
        {
            //await _productRunService.AssignPartyToWorkEffort(lookedUpValue.EstimatedArrivalWorkEffId, parameters.PartyIdTo);
        }

        // --- Capture old values before updating shipment ---
        var oldStatusId = lookedUpValue.StatusId;
        var oldPrimaryOrderId = lookedUpValue.PrimaryOrderId;
        var oldOriginFacilityId = lookedUpValue.OriginFacilityId;
        var oldDestinationFacilityId = lookedUpValue.DestinationFacilityId;

        // --- Update shipment non-PK fields ---
        lookedUpValue.OriginFacilityId = parameters.OriginFacilityId ?? lookedUpValue.OriginFacilityId;
        lookedUpValue.StatusId = parameters.StatusId ?? lookedUpValue.StatusId;
        lookedUpValue.DestinationFacilityId = parameters.DestinationFacilityId ?? lookedUpValue.DestinationFacilityId;
        lookedUpValue.EstimatedShipDate = parameters.EstimatedShipDate ?? lookedUpValue.EstimatedShipDate;
        lookedUpValue.EstimatedArrivalDate = parameters.EstimatedArrivalDate ?? lookedUpValue.EstimatedArrivalDate;
        lookedUpValue.PartyIdFrom = parameters.PartyIdFrom ?? lookedUpValue.PartyIdFrom;
        lookedUpValue.PartyIdTo = parameters.PartyIdTo ?? lookedUpValue.PartyIdTo;
        lookedUpValue.PrimaryOrderId = parameters.PrimaryOrderId ?? lookedUpValue.PrimaryOrderId;
        lookedUpValue.AdditionalShippingCharge =
            parameters.AdditionalShippingCharge ?? lookedUpValue.AdditionalShippingCharge;
        lookedUpValue.AddtlShippingChargeDesc =
            parameters.AddtlShippingChargeDesc ?? lookedUpValue.AddtlShippingChargeDesc;
        lookedUpValue.CurrencyUomId = parameters.CurrencyUomId ?? lookedUpValue.CurrencyUomId;
        lookedUpValue.DestinationContactMechId =
            parameters.DestinationContactMechId ?? lookedUpValue.DestinationContactMechId;
        lookedUpValue.DestinationTelecomNumberId =
            parameters.DestinationTelecomNumberId ?? lookedUpValue.DestinationTelecomNumberId;
        lookedUpValue.EstimatedReadyDate = parameters.EstimatedReadyDate ?? lookedUpValue.EstimatedReadyDate;
        lookedUpValue.EstimatedShipCost = parameters.EstimatedShipCost ?? lookedUpValue.EstimatedShipCost;
        lookedUpValue.HandlingInstructions = parameters.HandlingInstructions ?? lookedUpValue.HandlingInstructions;
        lookedUpValue.LatestCancelDate = parameters.LatestCancelDate ?? lookedUpValue.LatestCancelDate;
        lookedUpValue.OriginContactMechId = parameters.OriginContactMechId ?? lookedUpValue.OriginContactMechId;
        lookedUpValue.OriginTelecomNumberId = parameters.OriginTelecomNumberId ?? lookedUpValue.OriginTelecomNumberId;
        lookedUpValue.PicklistBinId = parameters.PicklistBinId ?? lookedUpValue.PicklistBinId;
        lookedUpValue.PrimaryReturnId = parameters.PrimaryReturnId ?? lookedUpValue.PrimaryReturnId;
        lookedUpValue.PrimaryShipGroupSeqId = parameters.PrimaryShipGroupSeqId ?? lookedUpValue.PrimaryShipGroupSeqId;

        // Update last modified information
        lookedUpValue.LastModifiedDate = DateTime.Now;

        // ECA logic
        if (!string.IsNullOrEmpty(parameters.StatusId) && parameters.StatusId != oldStatusId)
        {
            // 1. Cancellation for SALES_SHIPMENT
            if (parameters.StatusId == "SHIPMENT_CANCELLED" && shipmentTypeId == "SALES_SHIPMENT")
            {
                try
                {
                    //await CancelItemIssuanceAndOrderShipmentFromShipment(parameters.ShipmentId);
                }
                catch (Exception ex)
                {
                    errorList.Add($"Error during cancellation ECA: {ex.Message}");
                }
            }

            // 2. Invoice creation for SALES_SHIPMENT (SHIPMENT_PICKED)
            if (parameters.StatusId == "SHIPMENT_PICKED" && shipmentTypeId == "SALES_SHIPMENT")
            {
                try
                {
                    var invoiceResult = await _invoiceHelperService.CreateInvoicesFromShipments(
                        new List<string> { parameters.ShipmentId }, false, parameters.EventDate ?? DateTime.UtcNow);
                    if (!invoiceResult.IsSuccess)
                        errorList.Add("Failed to create invoices for SHIPMENT_PICKED.");
                }
                catch (Exception ex)
                {
                    errorList.Add($"Error during invoice creation for SHIPMENT_PICKED: {ex.Message}");
                }
            }

            // 3. Invoice creation and set ready for SALES_SHIPMENT (SHIPMENT_PACKED)
            if (parameters.StatusId == "SHIPMENT_PACKED" && shipmentTypeId == "SALES_SHIPMENT")
            {
                try
                {
                    var invoiceResult = await _invoiceHelperService.CreateInvoicesFromShipments(
                        new List<string> { parameters.ShipmentId }, false, parameters.EventDate ?? DateTime.UtcNow);
                    if (!invoiceResult.IsSuccess)
                        errorList.Add("Failed to create invoices for SHIPMENT_PACKED.");
                    await _invoiceHelperService.SetInvoicesToReadyFromShipment(parameters.ShipmentId);
                }
                catch (Exception ex)
                {
                    errorList.Add($"Error during invoice creation for SHIPMENT_PACKED: {ex.Message}");
                }
            }

            // 4. Notification for SALES_SHIPMENT (SHIPMENT_SHIPPED)
            if (parameters.StatusId == "SHIPMENT_SHIPPED" && shipmentTypeId == "SALES_SHIPMENT")
            {
                try
                {
                    //await Task.Run(() => SendShipmentCompleteNotification(parameters.ShipmentId));
                }
                catch (Exception ex)
                {
                    errorList.Add($"Error during shipment complete notification: {ex.Message}");
                }
            }

            // 5. PURCHASE_SHIPMENT (PURCH_SHIP_RECEIVED)
            if (parameters.StatusId == "PURCH_SHIP_RECEIVED" && shipmentTypeId == "PURCHASE_SHIPMENT")
            {
                try
                {
                    await BalanceItemIssuancesForShipment(parameters.ShipmentId);
                    var invoiceResult = await _invoiceHelperService.CreateInvoicesFromShipments(
                        new List<string> { parameters.ShipmentId }, false, parameters.EventDate ?? DateTime.UtcNow);
                    if (!invoiceResult.IsSuccess)
                        errorList.Add("Failed to create invoices for PURCH_SHIP_RECEIVED.");
                }
                catch (Exception ex)
                {
                    errorList.Add($"Error during ECA actions for PURCH_SHIP_RECEIVED: {ex.Message}");
                }
            }

            // 6. DROP_SHIPMENT (PURCH_SHIP_SHIPPED)
            if (parameters.StatusId == "PURCH_SHIP_SHIPPED" && shipmentTypeId == "DROP_SHIPMENT")
            {
                try
                {
                    var invoiceResult = await _invoiceHelperService.CreateInvoicesFromShipments(
                        new List<string> { parameters.ShipmentId }, false, parameters.EventDate ?? DateTime.UtcNow);
                    if (!invoiceResult.IsSuccess)
                        errorList.Add("Failed to create invoices for PURCH_SHIP_SHIPPED.");
                }
                catch (Exception ex)
                {
                    errorList.Add($"Error during invoice creation for PURCH_SHIP_SHIPPED: {ex.Message}");
                }
            }

            // 7. DROP_SHIPMENT (PURCH_SHIP_RECEIVED)
            if (parameters.StatusId == "PURCH_SHIP_RECEIVED" && shipmentTypeId == "DROP_SHIPMENT")
            {
                try
                {
                    /*
                    var invoiceResult = await _invoiceHelperService.CreateSalesInvoicesFromDropShipment(
                        new List<string> { parameters.ShipmentId }, parameters.EventDate ?? DateTime.UtcNow);
                    if (!invoiceResult.IsSuccess)
                        errorList.Add("Failed to create sales invoices for PURCH_SHIP_RECEIVED on DROP_SHIPMENT.");
                */
                }
                catch (Exception ex)
                {
                    errorList.Add($"Error during sales invoice creation for DROP_SHIPMENT: {ex.Message}");
                }
            }

            // 8. SALES_RETURN (PURCH_SHIP_RECEIVED)
            if (parameters.StatusId == "PURCH_SHIP_RECEIVED" && shipmentTypeId == "SALES_RETURN")
            {
                try
                {
                    /*var invoiceResult = await _invoiceHelperService.CreateInvoicesFromReturnShipment(
                        new List<string> { parameters.ShipmentId }, parameters.EventDate ?? DateTime.UtcNow);
                    if (!invoiceResult.IsSuccess)
                        errorList.Add("Failed to create invoices for PURCH_SHIP_RECEIVED on SALES_RETURN.");
                */
                }
                catch (Exception ex)
                {
                    errorList.Add($"Error during invoice creation for SALES_RETURN: {ex.Message}");
                }
            }

            // 9. PURCHASE_RETURN (SHIPMENT_SHIPPED)
            if (parameters.StatusId == "SHIPMENT_SHIPPED" && shipmentTypeId == "PURCHASE_RETURN")
            {
                try
                {
                    /*
                    var invoiceResult = await _invoiceHelperService.CreateInvoicesFromReturnShipment(
                        new List<string> { parameters.ShipmentId }, parameters.EventDate ?? DateTime.UtcNow);
                    if (!invoiceResult.IsSuccess)
                        errorList.Add("Failed to create invoices for SHIPMENT_SHIPPED on PURCHASE_RETURN.");
                */
                }
                catch (Exception ex)
                {
                    errorList.Add($"Error during invoice creation for PURCHASE_RETURN: {ex.Message}");
                }
            }

            // 10. Notification for SHIPMENT_SCHEDULED
            if (parameters.StatusId == "SHIPMENT_SCHEDULED")
            {
                try
                {
                    //await Task.Run(() => SendShipmentScheduledNotification(parameters.ShipmentId));
                }
                catch (Exception ex)
                {
                    errorList.Add($"Error during shipment scheduled notification: {ex.Message}");
                }
            }
        }

        // 11. Facility settings update
        if (!string.IsNullOrEmpty(parameters.OriginFacilityId) && parameters.OriginFacilityId != oldOriginFacilityId)
        {
            try
            {
                await SetShipmentSettingsFromFacilities(parameters.ShipmentId);
            }
            catch (Exception ex)
            {
                errorList.Add($"Error updating origin facility settings: {ex.Message}");
            }
        }

        if (!string.IsNullOrEmpty(parameters.DestinationFacilityId) &&
            parameters.DestinationFacilityId != oldDestinationFacilityId)
        {
            try
            {
                await SetShipmentSettingsFromFacilities(parameters.ShipmentId);
            }
            catch (Exception ex)
            {
                errorList.Add($"Error updating destination facility settings: {ex.Message}");
            }
        }

        // 12. Primary order settings update
        if (!string.IsNullOrEmpty(parameters.PrimaryOrderId) && parameters.PrimaryOrderId != oldPrimaryOrderId)
        {
            try
            {
                await SetShipmentSettingsFromPrimaryOrder(parameters.ShipmentId);
            }
            catch (Exception ex)
            {
                errorList.Add($"Error updating primary order settings: {ex.Message}");
            }
        }


        // Return the result with old values and the shipment ID
        return new UpdateShipmentResult
        {
            OldStatusId = oldStatusId,
            OldPrimaryOrderId = oldPrimaryOrderId,
            OldOriginFacilityId = oldOriginFacilityId,
            OldDestinationFacilityId = oldDestinationFacilityId,
            ShipmentId = lookedUpValue.ShipmentId,
            ShipmentTypeId = shipmentTypeId
        };
    }


    public async Task<bool> SetShipmentSettingsFromPrimaryOrder(string shipmentId)
    {
        // Fetch shipment
        var shipment = await _context.Shipments
            .FindAsync(shipmentId);

        if (shipment == null)
        {
            _logger.LogWarning($"Shipment with ID {shipmentId} not found.");
            return false;
        }

        // Check if primary order or ship group is missing
        if (string.IsNullOrEmpty(shipment.PrimaryOrderId))
        {
            _logger.LogInformation($"No primaryOrderId for shipmentId {shipmentId}. Skipping.");
            return true;
        }

        if (string.IsNullOrEmpty(shipment.PrimaryShipGroupSeqId))
        {
            _logger.LogInformation($"No primaryShipGroupSeqId for shipmentId {shipmentId}. Skipping.");
            return true;
        }

        // Fetch order header
        var orderHeader = await _context.OrderHeaders
            .FindAsync(shipment.PrimaryOrderId);

        if (orderHeader == null)
        {
            _logger.LogWarning($"Order with ID {shipment.PrimaryOrderId} not found.");
            return false;
        }

        // Fetch order item ship group
        var orderItemShipGroup = await _context.OrderItemShipGroups
            .FirstOrDefaultAsync(g =>
                g.OrderId == shipment.PrimaryOrderId && g.ShipGroupSeqId == shipment.PrimaryShipGroupSeqId);

        // Set shipment type
        if (orderHeader.OrderTypeId == "SALES_ORDER")
        {
            shipment.ShipmentTypeId = "SALES_SHIPMENT";
        }
        else if (orderHeader.OrderTypeId == "PURCHASE_ORDER" && shipment.ShipmentTypeId != "DROP_SHIPMENT")
        {
            shipment.ShipmentTypeId = "PURCHASE_SHIPMENT";
        }

        // Set origin facility for sales shipments from product store
        if (string.IsNullOrEmpty(shipment.OriginFacilityId) && shipment.ShipmentTypeId == "SALES_SHIPMENT" &&
            !string.IsNullOrEmpty(orderHeader.ProductStoreId))
        {
            var productStore = await _context.ProductStores
                .FindAsync(orderHeader.ProductStoreId);

            if (productStore?.OneInventoryFacility == "Y")
            {
                shipment.OriginFacilityId = productStore.InventoryFacilityId;
            }
        }

        // Fetch order roles
        var orderRoles = await _context.OrderRoles
            .Where(r => r.OrderId == shipment.PrimaryOrderId)
            .ToListAsync();

        // Set partyIdFrom
        if (string.IsNullOrEmpty(shipment.PartyIdFrom))
        {
            var shipFromVendor = orderRoles.FirstOrDefault(r => r.RoleTypeId == "SHIP_FROM_VENDOR");
            shipment.PartyIdFrom = shipFromVendor?.PartyId ??
                                   orderRoles.FirstOrDefault(r => r.RoleTypeId == "VENDOR")?.PartyId;
        }

        // Set partyIdTo
        if (string.IsNullOrEmpty(shipment.PartyIdTo))
        {
            var shipToCustomer = orderRoles.FirstOrDefault(r => r.RoleTypeId == "SHIP_TO_CUSTOMER");
            shipment.PartyIdTo = shipToCustomer?.PartyId ??
                                 orderRoles.FirstOrDefault(r => r.RoleTypeId == "CUSTOMER")?.PartyId;
        }

        // Fetch order contact mechanisms
        var orderContactMechs = await _context.OrderContactMeches
            .Where(oc => oc.OrderId == shipment.PrimaryOrderId)
            .ToListAsync();

        // Set destination contact mechanism
        if (string.IsNullOrEmpty(shipment.DestinationContactMechId))
        {
            var shippingLocation =
                orderContactMechs.FirstOrDefault(oc => oc.ContactMechPurposeTypeId == "SHIPPING_LOCATION");
            if (shippingLocation != null)
            {
                shipment.DestinationContactMechId = shippingLocation.ContactMechId;
            }
            else
            {
                _logger.LogWarning($"Cannot find a shipping destination address for order {shipment.PrimaryOrderId}.");
            }
        }

        // Set origin contact mechanism (skip for purchase shipments)
        if (shipment.ShipmentTypeId != "PURCHASE_SHIPMENT" && string.IsNullOrEmpty(shipment.OriginContactMechId))
        {
            var shipOriginLocation =
                orderContactMechs.FirstOrDefault(oc => oc.ContactMechPurposeTypeId == "SHIP_ORIG_LOCATION");
            if (shipOriginLocation != null)
            {
                shipment.OriginContactMechId = shipOriginLocation.ContactMechId;
            }
            else
            {
                _logger.LogWarning($"Cannot find a shipping origin address for order {shipment.PrimaryOrderId}.");
            }
        }

        // Set destination telecom number
        if (string.IsNullOrEmpty(shipment.DestinationTelecomNumberId))
        {
            var phoneShipping = orderContactMechs.FirstOrDefault(oc => oc.ContactMechPurposeTypeId == "PHONE_SHIPPING");
            if (phoneShipping != null)
            {
                shipment.DestinationTelecomNumberId = phoneShipping.ContactMechId;
            }
            else
            {
                var phoneNumber = await _context.Parties
                    .Join(_context.PartyContactMeches,
                        p => p.PartyId,
                        pcm => pcm.PartyId,
                        (p, pcm) => new { Party = p, PartyContactMech = pcm })
                    .Join(_context.TelecomNumbers,
                        pcm => pcm.PartyContactMech.ContactMechId,
                        tn => tn.ContactMechId,
                        (pcm, tn) => new { pcm.Party, pcm.PartyContactMech, TelecomNumber = tn })
                    .Where(joined => joined.Party.PartyId == shipment.PartyIdTo &&
                                     (joined.PartyContactMech.ThruDate == null ||
                                      joined.PartyContactMech.ThruDate > DateTime.UtcNow))
                    .Select(joined => new { joined.TelecomNumber.ContactMechId })
                    .FirstOrDefaultAsync();
                if (phoneNumber != null)
                {
                    shipment.DestinationTelecomNumberId = phoneNumber.ContactMechId;
                }
                else
                {
                    _logger.LogWarning(
                        $"Cannot find a shipping destination phone number for order {shipment.PrimaryOrderId}.");
                }
            }
        }

        // Set origin telecom number
        if (string.IsNullOrEmpty(shipment.OriginTelecomNumberId))
        {
            var phoneShipOrigin =
                orderContactMechs.FirstOrDefault(oc => oc.ContactMechPurposeTypeId == "PHONE_SHIP_ORIG");
            if (phoneShipOrigin != null)
            {
                shipment.OriginTelecomNumberId = phoneShipOrigin.ContactMechId;
            }
            else
            {
                _logger.LogWarning($"Cannot find a shipping origin phone number for order {shipment.PrimaryOrderId}.");
            }
        }

        // Set destination facility for purchase shipments
        if (string.IsNullOrEmpty(shipment.DestinationFacilityId) && shipment.ShipmentTypeId == "PURCHASE_SHIPMENT")
        {
            var facilityContactMech = await _context.FacilityContactMeches
                .FirstOrDefaultAsync(fc => fc.ContactMechId == shipment.DestinationContactMechId);
            if (facilityContactMech != null)
            {
                shipment.DestinationFacilityId = facilityContactMech.FacilityId;
            }
        }

        // Override destination details for sales orders from ship group
        if (orderItemShipGroup != null && orderHeader.OrderTypeId == "SALES_ORDER")
        {
            shipment.DestinationContactMechId = orderItemShipGroup.ContactMechId;
            shipment.DestinationTelecomNumberId = orderItemShipGroup.TelecomContactMechId;
        }

        // Calculate estimated shipping cost
        if (shipment.EstimatedShipCost == null)
        {
            var orderItems = await _context.OrderItems
                .Where(oi => oi.OrderId == orderHeader.OrderId && oi.StatusId != "ITEM_CANCELLED")
                .ToListAsync();
            var orderAdjustments = await _context.OrderAdjustments
                .Where(oa => oa.OrderId == orderHeader.OrderId)
                .ToListAsync();
            var orderHeaderAdjustments = orderAdjustments.Where(oa => oa.OrderItemSeqId == null).ToList();

            decimal orderSubTotal = orderItems.Sum(oi => (oi.UnitPrice ?? 0) * (oi.Quantity ?? 0));
            decimal shippingAmount = orderAdjustments
                .Where(oa => oa.OrderAdjustmentTypeId == "SHIPPING_CHARGES")
                .Sum(oa => oa.Amount ?? 0);
            shippingAmount += orderHeaderAdjustments
                .Where(oa => oa.OrderAdjustmentTypeId == "SHIPPING_CHARGES")
                .Sum(oa => oa.Amount ?? 0);

            shipment.EstimatedShipCost = shippingAmount;
        }

        // Create shipment route segment if none exists
        var shipmentRouteSegments = await _context.ShipmentRouteSegments
            .Where(srs => srs.ShipmentId == shipment.ShipmentId)
            .ToListAsync();

        if (!shipmentRouteSegments.Any())
        {
            var routeSegment = new ShipmentRouteSegment
            {
                ShipmentId = shipment.ShipmentId,
                ShipmentRouteSegmentId = Guid.NewGuid().ToString(),
                EstimatedStartDate = shipment.EstimatedShipDate,
                EstimatedArrivalDate = shipment.EstimatedArrivalDate,
                OriginFacilityId = shipment.OriginFacilityId,
                OriginContactMechId = shipment.OriginContactMechId,
                OriginTelecomNumberId = shipment.OriginTelecomNumberId,
                DestFacilityId = shipment.DestinationFacilityId,
                DestContactMechId = shipment.DestinationContactMechId,
                DestTelecomNumberId = shipment.DestinationTelecomNumberId,
                CarrierPartyId = orderItemShipGroup?.CarrierPartyId,
                ShipmentMethodTypeId = orderItemShipGroup?.ShipmentMethodTypeId
            };

            _context.ShipmentRouteSegments.Add(routeSegment);
        }


        return true;
    }

    private Task CreateShipmentStatus(string statusId, string shipmentId)
    {
        try
        {
            var stamp = DateTime.UtcNow;
            var shipmentStatus = new ShipmentStatus
            {
                StatusId = statusId,
                ShipmentId = shipmentId,
                StatusDate = stamp,
                LastUpdatedStamp = stamp,
                CreatedStamp = stamp
            };
            _context.ShipmentStatuses.Add(shipmentStatus);
        }
        catch (Exception ex)
        {
            throw new CreateStatusException("Failed to create shipment status.", ex);
        }

        return Task.CompletedTask;
    }

    public async Task<string> AddShipmentContentToPackage(string shipmentPackageSeqId, decimal quantity,
        string shipmentId)
    {
        // Set the operation name
        var operationName = "Create ShipmentPackageContent";

        // Check if the shipment status can be changed to packed
        var result = await _issuanceService.CheckCanChangeShipmentStatusPacked(shipmentId);
        if (result.HasError)
        {
            throw new InvalidOperationException(result.ErrorMessage);
        }

        // Create a new ShipmentPackageContent entity
        var newEntity = new ShipmentPackageContent
        {
            ShipmentPackageSeqId = shipmentPackageSeqId, // Using named parameter
            Quantity = quantity // Using named parameter
        };

        // Check if the ShipmentPackageContent already exists
        var shipmentPackageContent = await _context.ShipmentPackageContents
            .FirstOrDefaultAsync(spc => spc.ShipmentPackageSeqId == newEntity.ShipmentPackageSeqId);

        // Log the attempt
        //Console.WriteLine($"In addShipmentContentToPackage trying values: {newEntity}");

        if (shipmentPackageContent == null)
        {
            // Create new ShipmentPackageContent
            _context.ShipmentPackageContents.Add(newEntity);
        }
        else
        {
            // Update existing ShipmentPackageContent
            shipmentPackageContent.Quantity += quantity; // Add the quantities
            _context.ShipmentPackageContents.Update(shipmentPackageContent);
        }

        // Log information about the new shipment package
        //Console.WriteLine($"Shipment package: {newEntity}");

        // Return the shipment package sequence ID
        return newEntity.ShipmentPackageSeqId;
    }

    public async Task<OrderItemShipGroupResult> GetOrderItemShipGroupLists(string orderId)
    {
        // List to hold the approved OrderItemShipGroupAssocs
        var orderItemAndShipGroupAssocList = await (from oi in _context.OrderItems
            join oisga in _context.OrderItemShipGroupAssocs
                on new { oi.OrderId, oi.OrderItemSeqId } equals new { oisga.OrderId, oisga.OrderItemSeqId }
            where oi.OrderId == orderId && oi.StatusId == "ITEM_APPROVED"
            select new OrderItemShipGroup
            {
                OrderId = oi.OrderId,
                ShipGroupSeqId = oisga.ShipGroupSeqId
            }).ToListAsync();

        // Check if there are approved items to ship
        if (!orderItemAndShipGroupAssocList.Any()) throw new Exception("No items available to ship.");

        // Initialize a dictionary to group OrderItemShipGroupAssocs by shipGroupSeqId
        var orderItemListByShGrpMap = new Dictionary<string, List<OrderItemShipGroup>>();

        // Group order items by shipGroupSeqId
        foreach (var orderItemAndShipGroupAssoc in orderItemAndShipGroupAssocList)
        {
            if (!orderItemListByShGrpMap.ContainsKey(orderItemAndShipGroupAssoc.ShipGroupSeqId))
                orderItemListByShGrpMap[orderItemAndShipGroupAssoc.ShipGroupSeqId] =
                    new List<OrderItemShipGroup>();

            orderItemListByShGrpMap[orderItemAndShipGroupAssoc.ShipGroupSeqId].Add(orderItemAndShipGroupAssoc);
        }

        // Return both the approved list and the grouped map encapsulated in the result class
        return new OrderItemShipGroupResult(orderItemAndShipGroupAssocList, orderItemListByShGrpMap);
    }
    
    /// <summary>
    /// Release the purchase order's items assigned to the shipment but not actually received.
    /// </summary>
    /// <param name="shipmentId">The ID of the shipment.</param>
    /// <returns>Task representing the asynchronous operation.</returns>
    public async Task BalanceItemIssuancesForShipment(string shipmentId)
    {
        try
        {
            // Retrieve the shipment entity
            var shipment = await _context.Shipments.FindAsync(shipmentId);
            if (shipment == null) throw new Exception("Shipment not found.");

            // Retrieve related item issuances for the shipment
            var issuances = await _context.ItemIssuances
                .Where(i => i.ShipmentId == shipmentId)
                .ToListAsync();

            foreach (var issuance in issuances)
            {
                // Retrieve related shipment receipts for the issuance
                var receipts = await _context.ShipmentReceipts
                    .Where(r => r.ShipmentId == shipmentId
                                && r.OrderId == issuance.OrderId
                                && r.OrderItemSeqId == issuance.OrderItemSeqId)
                    .ToListAsync();

                // Calculate the issuance quantity based on receipts
                decimal? issuanceQuantity = (decimal)issuance.Quantity; // Initialize with existing issuance quantity

                foreach (var receipt in receipts)
                {
                    issuanceQuantity += receipt.QuantityAccepted - receipt.QuantityRejected;
                }

                // Update the issuance quantity
                issuance.Quantity = issuanceQuantity;

                // Save the updated issuance back to the database
                _context.ItemIssuances.Update(issuance);

                // Clear issuanceQuantity after processing
                issuanceQuantity = 0;
            }
        }
        catch (Exception ex)
        {
            // Handle exceptions (e.g., logging)
            Console.WriteLine($"Error in BalanceItemIssuancesForShipmentAsync: {ex.Message}");
        }
    }

    /// <summary>
    /// Updates the shipment status based on the receipt of items.
    /// </summary>
    /// <param name="shipmentId">The ID of the shipment.</param>
    /// <returns>Task representing the asynchronous operation.</returns>
    public async Task<OperationResult> UpdatePurchaseShipmentFromReceipt(string shipmentId)
    {
        try
        {
            // Retrieve all shipment receipts for the shipment ID
            var shipmentReceipts = await _utilityService.FindLocalOrDatabaseListAsync<ShipmentReceipt>(
                query => query.Where(sr => sr.ShipmentId == shipmentId));

            if (!shipmentReceipts.Any())
            {
                // No shipment receipts found, so exit successfully
                return OperationResult.Success("No shipment receipts found.");
            }

            // Retrieve the shipment entity and check if status is PURCH_SHIP_CREATED
            var shipment = await _context.Shipments.FindAsync(shipmentId);
            if (shipment == null) throw new Exception("Shipment not found.");

            if (shipment.StatusId == "PURCH_SHIP_CREATED")
            {
                // Update shipment status to PURCH_SHIP_SHIPPED
                var updateShipmentResult = await UpdateShipment(new ShipmentUpdateParameters
                {
                    ShipmentId = shipmentId,
                    StatusId = "PURCH_SHIP_SHIPPED"
                });
            }

            // Retrieve items with status PURCH_SHIP_SHIPPED from ShipmentItem table
            var shippedItems = await _utilityService.FindLocalOrDatabaseListAsync<ShipmentItem>(
                query => query.Where(si => si.ShipmentId == shipmentId && shipment.StatusId == "PURCH_SHIP_SHIPPED"));

            if (!shippedItems.Any())
            {
                // No items shipped, so exit successfully
                return OperationResult.Success("No items shipped with status PURCH_SHIP_SHIPPED.");
            }

            // Calculate shipped quantities for each product in a dictionary keyed by productId
            var shippedCountMap = shippedItems
                .GroupBy(item => item.ProductId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(item => item.Quantity ?? 0)
                );

            // Calculate received quantities for each product in a dictionary keyed by productId
            var receivedCountMap = shipmentReceipts
                .GroupBy(receipt => receipt.ProductId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(receipt => receipt.QuantityAccepted ?? 0)
                );

            // Compare shipped and received maps
            if (!shippedCountMap.SequenceEqual(receivedCountMap))
            {
                // Shipped and received quantities do not match, so exit successfully
                return OperationResult.Success("Shipped and received quantities do not match.");
            }

            // All items have been received; update the shipment status to PURCH_SHIP_RECEIVED
            var serviceResult = await UpdateShipment(new ShipmentUpdateParameters
            {
                ShipmentId = shipmentId,
                StatusId = "PURCH_SHIP_RECEIVED"
            });


            return OperationResult.Success("Shipment status updated to PURCH_SHIP_RECEIVED.");
        }
        catch (Exception ex)
        {
            // Log or handle exceptions
            Console.WriteLine($"Error in UpdatePurchaseShipmentFromReceipt: {ex.Message}");
            return OperationResult.Failure(ex.Message);
        }
    }

    private async Task<ErrorResult> SetShipmentSettingsFromFacilities(string shipmentId)
    {
        try
        {
            // Set the operation name
            var operationName = "Set Shipment Settings From Facilities";

            // 1. Check if the shipment status can be changed to packed (mirrors checkCanChangeShipmentStatusPacked)
            var statusCheckResult = await _issuanceService.CheckCanChangeShipmentStatusPacked(shipmentId);
            if (statusCheckResult.HasError)
            {
                // Simulate <check-errors/>: stop execution if errors are found
                return statusCheckResult;
            }

            // Convert "-fromDate" to a list (as per <string-to-list string="-fromDate" list="descendingFromDateOrder"/>)
            // Although not explicitly used later, we recreate the logic as closely as possible.
            var descendingFromDateOrder = new List<string> { "-fromDate" };

            // 2. Get the Shipment entity (mirrors <entity-one entity-name="Shipment" value-field="shipment"/>)
            var shipment = await _context.Shipments.FindAsync(shipmentId);
            if (shipment == null)
            {
                return new ErrorResult { HasError = true, ErrorMessage = "Shipment not found." };
            }

            // 3. Clone the shipment value (mirrors <clone-value value-field="shipment" new-value-field="shipmentCopy"/>)
            // Instead of an actual clone, store original values to detect changes after modifications.
            var originalOriginFacilityId = shipment.OriginFacilityId;
            var originalOriginContactMechId = shipment.OriginContactMechId;
            var originalOriginTelecomNumberId = shipment.OriginTelecomNumberId;
            var originalDestinationFacilityId = shipment.DestinationFacilityId;
            var originalDestinationContactMechId = shipment.DestinationContactMechId;
            var originalDestinationTelecomNumberId = shipment.DestinationTelecomNumberId;

            // 4. If originFacilityId is not empty, try to set originContactMechId and originTelecomNumberId if they are empty
            // if (!string.IsNullOrEmpty(shipment.OriginFacilityId))
            // {
            //     // If originContactMechId is empty
            //     if (string.IsNullOrEmpty(shipment.OriginContactMechId))
            //     {
            //         // Equivalent logic to the Groovy script in Ofbiz
            //         // Try to find a contact mech with purposes SHIP_ORIG_LOCATION or PRIMARY_LOCATION
            //         var facilityContactMech = await _GetFacilityContactMechByPurpose(
            //             shipment.OriginFacilityId,
            //             new List<string> { "SHIP_ORIG_LOCATION", "PRIMARY_LOCATION" });
            //
            //         if (facilityContactMech != null)
            //         {
            //             shipment.OriginContactMechId = facilityContactMech.ContactMechId;
            //         }
            //     }
            //
            //     // If originTelecomNumberId is empty
            //     if (string.IsNullOrEmpty(shipment.OriginTelecomNumberId))
            //     {
            //         // Check for PHONE_SHIP_ORIG or PRIMARY_PHONE contact mech
            //         var facilityContactMech = await _GetFacilityContactMechByPurpose(
            //             shipment.OriginFacilityId,
            //             new List<string> { "PHONE_SHIP_ORIG", "PRIMARY_PHONE" });
            //
            //         if (facilityContactMech != null)
            //         {
            //             shipment.OriginTelecomNumberId = facilityContactMech.ContactMechId;
            //         }
            //     }
            // }
            //
            // // 5. If destinationFacilityId is not empty, try to set destinationContactMechId and destinationTelecomNumberId if empty
            // if (!string.IsNullOrEmpty(shipment.DestinationFacilityId))
            // {
            //     // If destinationContactMechId is empty
            //     if (string.IsNullOrEmpty(shipment.DestinationContactMechId))
            //     {
            //         var facilityContactMech = await _GetFacilityContactMechByPurpose(
            //             shipment.DestinationFacilityId,
            //             new List<string> { "SHIPPING_LOCATION", "PRIMARY_LOCATION" });
            //
            //         if (facilityContactMech != null)
            //         {
            //             shipment.DestinationContactMechId = facilityContactMech.ContactMechId;
            //         }
            //     }
            //
            //     // If destinationTelecomNumberId is empty
            //     if (string.IsNullOrEmpty(shipment.DestinationTelecomNumberId))
            //     {
            //         var facilityContactMech = await _GetFacilityContactMechByPurpose(
            //             shipment.DestinationFacilityId,
            //             new List<string> { "PHONE_SHIPPING", "PRIMARY_PHONE" });
            //
            //         if (facilityContactMech != null)
            //         {
            //             shipment.DestinationTelecomNumberId = facilityContactMech.ContactMechId;
            //         }
            //     }
            // }

            // 6. Compare shipment fields to the original copy to determine if updates are needed
            bool hasChanges =
                shipment.OriginFacilityId != originalOriginFacilityId ||
                shipment.OriginContactMechId != originalOriginContactMechId ||
                shipment.OriginTelecomNumberId != originalOriginTelecomNumberId ||
                shipment.DestinationFacilityId != originalDestinationFacilityId ||
                shipment.DestinationContactMechId != originalDestinationContactMechId ||
                shipment.DestinationTelecomNumberId != originalDestinationTelecomNumberId;

            // 7. If changes detected, update the shipment (mirrors <if-compare-field> and <call-service service-name="updateShipment"/>)
            if (hasChanges)
            {
                var updateParams = new ShipmentUpdateParameters
                {
                    ShipmentId = shipment.ShipmentId,
                    // For this refactoring we update facility fields.
                    // You can extend ShipmentUpdateParameters and pass additional fields as needed.
                    OriginFacilityId = shipment.OriginFacilityId,
                    DestinationFacilityId = shipment.DestinationFacilityId
                };

                var updateResult = await UpdateShipment(updateParams);
                if (updateResult == null || string.IsNullOrEmpty(updateResult.ShipmentId))
                {
                    return new ErrorResult { HasError = true, ErrorMessage = "Failed to update shipment settings." };
                }
            }

            // If we reach here without errors, return success
            return new ErrorResult { HasError = false };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in SetShipmentSettingsFromFacilities for shipmentId: {shipmentId}", shipmentId);
            return new ErrorResult
            {
                HasError = true,
                ErrorMessage = "An unexpected error occurred while setting shipment settings from facilities."
            };
        }
    }
    
    /*public async Task<List<ShipmentReceipt>> GetShipmentReceipts(IEnumerable<string> shipmentIds)
    {
        var localShipmentReceipts = _context.ChangeTracker.Entries<ShipmentReceipt>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
            .Select(e => e.Entity)
            .Where(sr => shipmentIds.Contains(sr.ShipmentId))
            .ToList();

        if (localShipmentReceipts.Any())
        {
            return localShipmentReceipts;
        }

        var dbShipmentReceipts = await _context.Set<ShipmentReceipt>()
            .AsNoTracking()
            .Where(sr => shipmentIds.Contains(sr.ShipmentId))
            .ToListAsync();

        return _repository.CombineEntities(localShipmentReceipts, dbShipmentReceipts);
    }*/
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