using Application.Accounting.Services;
using Application.Catalog.Products.Services.Inventory;
using Application.Core;
using Application.Interfaces;
using Application.Order.Orders;
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Shipments;

public interface IIssuanceService
{
    Task<string> IssueOrderItemToShipment(IssueOrderItemParameters parameters);

    Task<IssueOrderItemResult> IssueOrderItemShipGrpInvResToShipment(IssueOrderItemShipGrpInvResParameters parameters);
    Task<ErrorResult> CheckCanChangeShipmentStatusDelivered(string shipmentId);
    Task<ErrorResult> CheckCanChangeShipmentStatusPacked(string shipmentId);
}

public class IssuanceService : IIssuanceService
{
    private readonly DataContext _context;
    private readonly IInventoryService _inventoryService;
    private readonly ILogger _logger;
    private readonly IUtilityService _utilityService;
    private readonly IUserAccessor _userAccessor;
    private readonly IGeneralLedgerService _generalLedgerService;
    private readonly Lazy<IOrderHelperService> _orderHelperService;
    private readonly Lazy<IShipmentService> _shipmentService;


    public IssuanceService(DataContext context, IUtilityService utilityService,
        ILogger<IssuanceService> logger,
        IInventoryService inventoryService,
        IUserAccessor userAccessor,
        IGeneralLedgerService generalLedgerService, Lazy<IShipmentService> shipmentService, Lazy<IOrderHelperService> orderHelperService)
    {
        _context = context;
        _userAccessor = userAccessor;
        _utilityService = utilityService;
        _inventoryService = inventoryService;
        _generalLedgerService = generalLedgerService;
        _logger = logger;
        _shipmentService = shipmentService;
        _orderHelperService = orderHelperService;
    }

    public async Task<string> IssueOrderItemToShipment(IssueOrderItemParameters parameters)
    {
        try
        {
            // Check if the shipment status can be changed to packed
            // Uncomment the following line if the method is implemented
            var result = await CheckCanChangeShipmentStatusPacked(parameters.ShipmentId);
            if (result.HasError)
            {
                throw new InvalidOperationException(result.ErrorMessage);
            }

            // Get the order header
            var orderHeader = await _context.OrderHeaders.FindAsync(parameters.OrderId);
            if (orderHeader == null)
                throw new Exception("Order not found.");

            // Ensure the order is NOT of orderTypeId: SALES_ORDER
            if (orderHeader.OrderTypeId == "SALES_ORDER")
                throw new InvalidOperationException(
                    $"Cannot issue order item to shipment [{parameters.ShipmentId}] because the order [{orderHeader.OrderId}] is a Sales Order.");

            // Get the order item
            var orderItem = await _context.OrderItems
                .FirstOrDefaultAsync(oi =>
                    oi.OrderId == parameters.OrderId && oi.OrderItemSeqId == parameters.OrderItemSeqId);
            if (orderItem == null)
                throw new Exception("Order item not found.");

            // Get the order item ship group association
            var orderItemShipGroupAssoc = await _context.OrderItemShipGroupAssocs
                .FirstOrDefaultAsync(o => o.OrderId == orderItem.OrderId &&
                                          o.OrderItemSeqId == orderItem.OrderItemSeqId &&
                                          o.ShipGroupSeqId == parameters.ShipGroupSeqId);
            if (orderItemShipGroupAssoc == null)
                throw new Exception("Order item ship group association not found.");

            // Get the shipment
            var shipment = await _context.Shipments.FindAsync(parameters.ShipmentId);
            if (shipment == null)
                throw new Exception("Shipment not found.");

            // Call the method to find and create the issue shipment item
            var shipmentItem =
                await FindCreateIssueShipmentItem(orderItem, shipment.ShipmentId, parameters.Quantity, null,
                    orderItemShipGroupAssoc, null);

            return shipmentItem.ShipmentItemSeqId; // Return the shipment item sequence ID
        }
        catch (InvalidOperationException ex)
        {
            // Log specific exceptions and rethrow if necessary
            _logger.LogWarning(ex, "Operation invalid in IssueOrderItemToShipment");
            throw;
        }
        catch (Exception ex)
        {
            // Log the exception and throw a new exception with additional context
            _logger.LogError(ex, "Error in IssueOrderItemToShipment");
            throw new Exception("An error occurred while issuing the order item to shipment.", ex);
        }
    }

    public async Task<IssueOrderItemResult> IssueOrderItemShipGrpInvResToShipment(
        IssueOrderItemShipGrpInvResParameters parameters)
    {
        _logger.LogDebug(
            "Starting IssueOrderItemShipGrpInvResToShipment for ShipmentId: {ShipmentId}, OrderId: {OrderId}, OrderItemSeqId: {OrderItemSeqId}",
            parameters.ShipmentId, parameters.OrderId, parameters.OrderItemSeqId);

        try
        {
            // Parameter validation - Ensure a valid quantity is provided for issuance; business must avoid processing negative or zero quantities.
            if (parameters.Quantity <= 0) throw new ArgumentException("Quantity must be greater than zero.");

            // Set the operation name - Naming the operation for clarity in logs and debugging.
            var operationName = "Issue OrderItemShipGrpInvRes to Shipment";

            // Check if the shipment status can be changed to packed - Ensure the shipment is ready for inventory issuance by verifying its status.
            _logger.LogDebug("Checking if shipment status can be changed to packed for ShipmentId: {ShipmentId}",
                parameters.ShipmentId);
            //await _shipmentService.CheckCanChangeShipmentStatusPacked(parameters.ShipmentId);

            // Get orderItemShipGrpInvRes - Retrieve existing reservation for the order item to ensure it can be issued.
            var orderItemShipGrpInvRes = await _context.OrderItemShipGrpInvRes
                .FirstOrDefaultAsync(o =>
                    o.OrderId == parameters.OrderId &&
                    o.OrderItemSeqId == parameters.OrderItemSeqId &&
                    o.ShipGroupSeqId == parameters.ShipGroupSeqId);

            if (orderItemShipGrpInvRes == null)
            {
                // Log and throw error if reservation is not found - Ensure accurate inventory tracking by preventing issuance without a valid reservation.
                _logger.LogError(
                    "Order Item Ship Group Inventory Reservation not found for OrderId: {OrderId}, OrderItemSeqId: {OrderItemSeqId}, ShipGroupSeqId: {ShipGroupSeqId}",
                    parameters.OrderId, parameters.OrderItemSeqId, parameters.ShipGroupSeqId);
                throw new Exception("Order Item Ship Group Inventory Reservation not found.");
            }

            // Get orderHeader - Retrieve order details to validate order type.
            var orderHeader = await _context.OrderHeaders.FindAsync(parameters.OrderId);
            if (orderHeader == null)
            {
                // Log and throw error if order header is not found - Prevent inventory issuance for non-existent orders.
                _logger.LogError("Order Header not found for OrderId: {OrderId}", parameters.OrderId);
                throw new Exception("Order Header not found.");
            }

            // Ensure the order is of orderTypeId: SALES_ORDER - Confirm the order is a sales order; inventory issuance should only occur for sales orders.
            if (orderHeader.OrderTypeId != "SALES_ORDER")
            {
                throw new Exception(
                    $"Not issuing Order Item Ship Group Inventory Reservation to shipment [{parameters.ShipmentId}] because the order is not a Sales Order for order [{orderItemShipGrpInvRes.OrderId}] order item [{orderItemShipGrpInvRes.OrderItemSeqId}] inventory item [{orderItemShipGrpInvRes.InventoryItemId}].");
            }

            // Validate the specified quantity - Prevent over-issuance of inventory beyond the reserved amount.
            if (parameters.Quantity > orderItemShipGrpInvRes.Quantity)
            {
                _logger.LogError(
                    "Specified quantity {Quantity} exceeds reserved quantity {ReservedQuantity} for OrderId: {OrderId}, OrderItemSeqId: {OrderItemSeqId}",
                    parameters.Quantity, orderItemShipGrpInvRes.Quantity, parameters.OrderId,
                    parameters.OrderItemSeqId);
                throw new Exception("Quantity cannot be greater than the reserved quantity left to be issued.");
            }

            var orderItem = await _context.OrderItems.FindAsync(parameters.OrderId, parameters.OrderItemSeqId);
            var inventoryItem = await _context.InventoryItems.FindAsync(orderItemShipGrpInvRes.InventoryItemId);
            var shipment = await _context.Shipments.FindAsync(parameters.ShipmentId);

            // Ensure items are retrieved successfully - Inventory issuance should only proceed if all entities are available.
            if (orderItem == null || inventoryItem == null || shipment == null)
            {
                _logger.LogError(
                    "One or more required entities could not be found: OrderItem, InventoryItem, or Shipment for OrderId: {OrderId}, OrderItemSeqId: {OrderItemSeqId}, ShipmentId: {ShipmentId}",
                    parameters.OrderId, parameters.OrderItemSeqId, parameters.ShipmentId);
                throw new Exception("One or more required entities could not be found.");
            }

            var orderShipment = await _context.OrderShipments
                .FirstOrDefaultAsync(os =>
                    os.OrderId == orderItem.OrderId &&
                    os.ShipGroupSeqId == parameters.ShipGroupSeqId &&
                    os.OrderItemSeqId == orderItem.OrderItemSeqId);

            string shipmentItemSeqId = null; // Default value

            if (orderShipment != null)
            {
                shipmentItemSeqId = orderShipment.ShipmentItemSeqId;
            }

            var qtyForShipmentItem = await CalcQtyForShipmentItemInline(
                orderItemShipGrpInvRes.InventoryItemId,
                parameters.OrderId,
                parameters.OrderItemSeqId,
                parameters.ShipGroupSeqId,
                parameters.ShipmentId,
                parameters.Quantity);
            _logger.LogDebug("Calculated qtyForShipmentItem: {QtyForShipmentItem}", qtyForShipmentItem);


            // 9. Handle Based on qtyForShipmentItem
            ShipmentItem shipmentItem = null;
            if (qtyForShipmentItem >= 0)
            {
                // Always call FindCreateIssueShipmentItem when qtyForShipmentItem >= 0
                shipmentItem = await FindCreateIssueShipmentItem(orderItem, shipment.ShipmentId, qtyForShipmentItem,
                    shipmentItemSeqId, null, orderItemShipGrpInvRes);
            }
            else
            {
                // A reduction in the quantity, so OrderShipment must exist
                if (orderShipment == null)
                {
                    string errorMessage = "OrderShipment must exist for quantity reduction.";
                    _logger.LogError(errorMessage);
                    throw new Exception(errorMessage);
                }

                // Subtract quantity from OrderShipment
                _logger.LogDebug("Reducing OrderShipment Quantity by {Quantity}", parameters.Quantity);
                orderShipment.Quantity -= parameters.Quantity;

                // Delegate to FindCreateIssueShipmentItem to handle ShipmentItem
                shipmentItem = await FindCreateIssueShipmentItem(orderItem, shipment.ShipmentId, qtyForShipmentItem,
                    orderShipment.ShipmentItemSeqId, null, null);
            }


            // Create item issuance - Log the issuance for future tracking of inventory movement.
            var itemIssuanceId = await FindCreateItemIssuance(orderItem, shipmentItem, null, orderItemShipGrpInvRes,
                parameters.Quantity, DateTime.UtcNow);

            // Associate roles related to the issuance.
            await AssociateIssueRoles(itemIssuanceId);

            // 11. Decrement Reservation Quantity
            orderItemShipGrpInvRes.Quantity -= parameters.Quantity;
            List<OrderItemShipGrpInvRes> otherOiirs = new List<OrderItemShipGrpInvRes>();

            if (orderItemShipGrpInvRes.Quantity <= 0)
            {
                _logger.LogDebug(
                    "Removing OrderItemShipGrpInvRes as quantity has been fully issued for OrderId: {OrderId}, OrderItemSeqId: {OrderItemSeqId}",
                    parameters.OrderId, parameters.OrderItemSeqId);
                _context.OrderItemShipGrpInvRes.Remove(orderItemShipGrpInvRes);

                otherOiirs = _context.ChangeTracker.Entries<OrderItemShipGrpInvRes>()
                    .Where(e => e.Entity.OrderId == orderItem.OrderId &&
                                e.Entity.OrderItemSeqId == orderItem.OrderItemSeqId && e.State != EntityState.Deleted)
                    .Select(e => e.Entity)
                    .ToList();


                // If no more reservations exist, attempt to mark the order item as completed
                if (!otherOiirs.Any())
                {
                    // Check shipment status before updating order item status
                    if (shipment.StatusId != "SHIPMENT_SCHEDULED")
                    {
                        _logger.LogDebug(
                            "Calling ChangeOrderItemStatus to set ITEM_COMPLETED for OrderId: {OrderId}, OrderItemSeqId: {OrderItemSeqId}",
                            parameters.OrderId, parameters.OrderItemSeqId);

                        // Call the ChangeOrderItemStatus service
                        var statusResponse = await _orderHelperService.Value.ChangeOrderItemStatus(new SetItemStatusRequest
                        {
                            OrderId = orderItem.OrderId,
                            OrderItemSeqId = orderItem.OrderItemSeqId,
                            StatusId = "ITEM_COMPLETED",
                            FromStatusId = orderItem.StatusId, // Optional: Current status for validation
                            StatusDateTime = DateTime.UtcNow,  // Set current time for status change
                            ChangeReason = "All inventory reservations issued", // Reason for audit trail
                        });

                        if (!statusResponse.Success)
                        {
                            _logger.LogWarning(
                                "Failed to update order item status to ITEM_COMPLETED for OrderId: {OrderId}, OrderItemSeqId: {OrderItemSeqId}. Message: {Message}",
                                parameters.OrderId, parameters.OrderItemSeqId, statusResponse.Message);
                        }
                    }
                    else
                    {
                        _logger.LogInformation(
                            "Items issued but cannot set order item status to ITEM_COMPLETED because shipment status is SHIPMENT_SCHEDULED for OrderId: {OrderId}, OrderItemSeqId: {OrderItemSeqId}",
                            orderItem.OrderId, orderItem.OrderItemSeqId);
                    }
                }
            }


            // Create inventory item detail - Record inventory movement to keep stock records accurate.
            _logger.LogDebug(
                "Calling CreateInventoryItemDetail for InventoryItemId: {InventoryItemId}, OrderId: {OrderId}",
                inventoryItem.InventoryItemId, orderItem.OrderId);
            await _inventoryService.CreateInventoryItemDetail(new CreateInventoryItemDetailParam
            {
                InventoryItemId = inventoryItem.InventoryItemId,
                OrderId = orderItem.OrderId,
                OrderItemSeqId = orderItem.OrderItemSeqId,
                ItemIssuanceId = itemIssuanceId,
                QuantityOnHandDiff = -parameters.Quantity // Use negative to decrement
            });

            // Update other reservations with remaining quantity.
            foreach (var oisgir in otherOiirs)
            {
                oisgir.Quantity -= parameters.Quantity;
                _context.OrderItemShipGrpInvRes.Update(oisgir);
            }
            
            _logger.LogDebug("Completed IssueOrderItemShipGrpInvResToShipment for ShipmentId: {ShipmentId}",
                parameters.ShipmentId);

            // Return result - Provide the identifier of the item issuance for tracking purposes.
            return new IssueOrderItemResult
            {
                ItemIssuanceId = itemIssuanceId,
                ShipmentItemSeqId = shipmentItem.ShipmentItemSeqId,
            };
        }
        catch (Exception ex)
        {
            // Log error - Ensure any issues are recorded to help with debugging and troubleshooting.
            _logger.LogError(ex,
                "Error occurred in IssueOrderItemShipGrpInvResToShipment for ShipmentId: {ShipmentId}, OrderId: {OrderId}, OrderItemSeqId: {OrderItemSeqId}",
                parameters.ShipmentId, parameters.OrderId, parameters.OrderItemSeqId);
            throw;
        }
    }

    public async Task<ErrorResult> CheckCanChangeShipmentStatusDelivered(string shipmentId)
    {
        var fromStatusId = "SHIPMENT_DELIVERED";

        // Call the general status check method
        var result = await CheckCanChangeShipmentStatusGeneral(shipmentId, fromStatusId);
        return result;
    }

    private async Task<ShipmentItem> FindCreateIssueShipmentItem(
        OrderItem orderItem,
        string shipmentId,
        decimal quantity,
        string? shipmentItemSeqId, OrderItemShipGroupAssoc? orderItemShipGroupAssoc,
        OrderItemShipGrpInvRes orderItemShipGrpInvRes)
    {
        try
        {
            ShipmentItem shipmentItem = null;

            // Check if the orderItem has a productId
            if (!string.IsNullOrEmpty(orderItem.ProductId))
            {
                // Try to find an existing ShipmentItem using the custom utility method
                shipmentItem =
                    await _utilityService.FindLocalOrDatabaseAsync<ShipmentItem>(shipmentId, shipmentItemSeqId);
            }

            if (shipmentItem == null)
            {
                // Create a new ShipmentItem if none exists
                var parameters = new ShipmentItemCreateParameters
                {
                    ShipmentId = shipmentId,
                    ProductId = orderItem.ProductId,
                    Quantity = quantity
                };

                shipmentItem = await _shipmentService.Value.CreateShipmentItem(parameters);
                _logger.LogInformation("Created new ShipmentItem: {@ShipmentItem}", shipmentItem);
            }
            else
            {
                // Update the existing ShipmentItem's quantity
                shipmentItem.Quantity += quantity;
                shipmentItem.LastUpdatedStamp = DateTime.UtcNow; // Update the timestamp
                _context.ShipmentItems.Update(shipmentItem);
                _logger.LogInformation("Updated ShipmentItem: {@ShipmentItem}", shipmentItem);
            }


            // Call to create or update the associated Order Shipment with all required parameters
            await CreateOrUpdateOrderShipmentInline(
                shipmentId,
                shipmentItem.ShipmentItemSeqId,
                orderItem.OrderId,
                orderItem.OrderItemSeqId,
                quantity,
                orderItemShipGroupAssoc,
                orderItemShipGrpInvRes
            );

            // Return the ShipmentItem object
            return shipmentItem;
        }
        catch (Exception ex)
        {
            // Log the exception with relevant details
            _logger.LogError(ex,
                "Error occurred while finding or creating ShipmentItem for ShipmentId: {ShipmentId}, OrderId: {OrderId}, OrderItemSeqId: {OrderItemSeqId}",
                shipmentId, orderItem.OrderId, orderItem.OrderItemSeqId);

            // Wrap and rethrow the exception to maintain stack trace
            throw new ApplicationException("An error occurred while processing the ShipmentItem.", ex);
        }
    }

    private async Task CreateOrUpdateOrderShipmentInline(string shipmentId, string shipmentItemSeqId, string orderId,
        string orderItemSeqId, decimal quantity, OrderItemShipGroupAssoc? orderItemShipGroupAssoc,
        OrderItemShipGrpInvRes? orderItemShipGrpInvRes)
    {
        try
        {
            // Create a new OrderShipment object to hold the details
            var orderShipmentCreate = new OrderShipment
            {
                ShipmentId = shipmentId,
                ShipmentItemSeqId = shipmentItemSeqId,
                OrderId = orderId,
                OrderItemSeqId = orderItemSeqId,
                Quantity = quantity,
                CreatedStamp = DateTime.UtcNow,
                LastUpdatedStamp = DateTime.UtcNow
            };

            // Set the ship group sequence ID if available
            if (orderItemShipGroupAssoc != null)
                orderShipmentCreate.ShipGroupSeqId = orderItemShipGroupAssoc.ShipGroupSeqId;
            else if (orderItemShipGrpInvRes != null)
                orderShipmentCreate.ShipGroupSeqId = orderItemShipGrpInvRes.ShipGroupSeqId;

            // Check if the OrderShipment already exists
            var existingOrderShipments = await _context.OrderShipments
                .Where(os =>
                    os.ShipmentId == orderShipmentCreate.ShipmentId &&
                    os.ShipmentItemSeqId == orderShipmentCreate.ShipmentItemSeqId)
                .ToListAsync();

            // Get the first order shipment if it exists
            var orderShipment = existingOrderShipments.FirstOrDefault();

            if (orderShipment == null)
            {
                // Create a new OrderShipment
                await _context.OrderShipments.AddAsync(orderShipmentCreate);
            }
            else
            {
                // Update the existing OrderShipment's quantity
                orderShipment.Quantity += quantity; // Increment the quantity
            }
        }
        catch (DbUpdateException dbEx)
        {
            // Handle specific database update exceptions
            throw new ApplicationException("An error occurred while updating the OrderShipment in the database.", dbEx);
        }
        catch (InvalidOperationException invalidOpEx)
        {
            // Handle invalid operations, such as null objects or invalid state
            throw new ApplicationException("An invalid operation occurred while processing the OrderShipment.",
                invalidOpEx);
        }
        catch (Exception ex)
        {
            // Log and throw a general exception for any other unexpected issues
            // Example: _logger.LogError(ex, "An error occurred while creating/updating the OrderShipment.");
            throw new ApplicationException("An unexpected error occurred while processing the OrderShipment.", ex);
        }
    }

    public async Task<string> FindCreateItemIssuance(
        OrderItem orderItem,
        ShipmentItem shipmentItem,
        OrderItemShipGroupAssoc orderItemShipGroupAssoc,
        OrderItemShipGrpInvRes orderItemShipGrpInvRes,
        decimal quantity,
        DateTime? eventDate = null,
        string userLoginId = null)
    {
        if (orderItem == null)
            throw new ArgumentNullException(nameof(orderItem));
        if (shipmentItem == null)
            throw new ArgumentNullException(nameof(shipmentItem));
        if (string.IsNullOrEmpty(shipmentItem.ShipmentItemSeqId))
            throw new ArgumentException("ShipmentItemSeqId is missing in ShipmentItem.");

        try
        {
            // Query OrderHeader using orderItem.OrderId
            var orderHeader = await _context.OrderHeaders
                .AsNoTracking()
                .FirstOrDefaultAsync(oh => oh.OrderId == orderItem.OrderId);
            if (orderHeader == null)
                throw new ApplicationException($"Order with ID {orderItem.OrderId} not found.");

            // If the order type is not SALES_ORDER, search for existing ItemIssuance
            if (orderHeader.OrderTypeId != "SALES_ORDER")
            {
                var itemIssuance = await _context.ItemIssuances
                    .Where(ii => ii.OrderId == orderItem.OrderId &&
                                 ii.OrderItemSeqId == orderItem.OrderItemSeqId &&
                                 ii.ShipmentId == shipmentItem.ShipmentId &&
                                 ii.ShipmentItemSeqId == shipmentItem.ShipmentItemSeqId &&
                                 ii.ShipGroupSeqId == orderItemShipGroupAssoc.ShipGroupSeqId)
                    .OrderByDescending(ii => ii.IssuedDateTime)
                    .FirstOrDefaultAsync();

                if (itemIssuance != null)
                {
                    itemIssuance.Quantity += quantity;
                    _context.ItemIssuances.Update(itemIssuance);
                    await _context.SaveChangesAsync(); // Persist update immediately
                    return itemIssuance.ItemIssuanceId;
                }
            }

            // Prepare data for CreateItemIssuance
            string inventoryItemId = orderItemShipGrpInvRes?.InventoryItemId;
            string shipGroupSeqId = orderItemShipGrpInvRes?.ShipGroupSeqId ?? orderItemShipGroupAssoc?.ShipGroupSeqId;
            DateTime issuedDateTime = eventDate ?? DateTime.Now;

            // Call CreateItemIssuance with all parameters
            var result = await CreateItemIssuance(
                orderId: orderItem.OrderId,
                orderItemSeqId: orderItem.OrderItemSeqId,
                shipmentId: shipmentItem.ShipmentId,
                shipmentItemSeqId: shipmentItem.ShipmentItemSeqId,
                inventoryItemId: inventoryItemId,
                shipGroupSeqId: shipGroupSeqId,
                quantity: quantity,
                issuedDateTime: issuedDateTime);

            return result.ItemIssuanceId;
        }
        catch (Exception ex)
        {
            throw new ApplicationException("An error occurred while finding or creating the ItemIssuance.", ex);
        }
    }

    public async Task AssociateIssueRoles(string itemIssuanceId)
    {
        try
        {
            var userLogin = await _context.Users.FirstOrDefaultAsync(x =>
                x.UserName == _userAccessor.GetUsername());

            if (userLogin == null)
            {
                throw new ArgumentNullException(nameof(userLogin), "UserLogin cannot be null.");
            }

            // Ensure the party is in the PACKER role
            var partyRole = new PartyRole
            {
                PartyId = userLogin.PartyId,
                RoleTypeId = "PACKER"
            };

            // Check if the PartyRole already exists
            var existingPartyRole =
                await _utilityService.FindLocalOrDatabaseAsync<PartyRole>(partyRole.PartyId, partyRole.RoleTypeId);

            if (existingPartyRole == null)
            {
                // Create a new PartyRole if it doesn't exist
                await _context.PartyRoles.AddAsync(partyRole);
            }

            // Check if ItemIssuanceRole already exists for this party
            var itemIssuanceRole = await _context.ItemIssuanceRoles
                .FirstOrDefaultAsync(ir =>
                    ir.ItemIssuanceId == itemIssuanceId && ir.PartyId == userLogin.PartyId &&
                    ir.RoleTypeId == "PACKER");

            if (itemIssuanceRole == null)
            {
                // Create a new ItemIssuanceRole if it doesn't exist
                var itemIssuanceRoleCreate = new ItemIssuanceRole
                {
                    ItemIssuanceId = itemIssuanceId,
                    PartyId = userLogin.PartyId,
                    RoleTypeId = "PACKER"
                };

                await _context.ItemIssuanceRoles.AddAsync(itemIssuanceRoleCreate);
            }
        }
        catch (ArgumentNullException argEx)
        {
            // Handle null argument exceptions
            throw new ApplicationException("UserLogin parameter cannot be null.", argEx);
        }
        catch (DbUpdateException dbEx)
        {
            // Handle database update exceptions
            throw new ApplicationException(
                "An error occurred while updating the database for the AssociateIssueRoles operation.", dbEx);
        }
        catch (Exception ex)
        {
            // Catch all other exceptions
            throw new ApplicationException("An unexpected error occurred during AssociateIssueRoles operation.", ex);
        }
    }

    public async Task<decimal> CalcQtyForShipmentItemInline(string inventoryItemId, string orderId,
        string orderItemSeqId, string shipGroupSeqId, string shipmentId, decimal quantity)
    {
        decimal otherInventoryItemQuantity = 0;

        // If inventoryItemId is provided, calculate the quantity from other inventory items
        if (!string.IsNullOrEmpty(inventoryItemId))
        {
            var itemIssuances = await _context.ItemIssuances
                .Where(ii => ii.OrderId == orderId &&
                             ii.OrderItemSeqId == orderItemSeqId &&
                             ii.ShipGroupSeqId == shipGroupSeqId &&
                             ii.ShipmentId == shipmentId)
                .OrderByDescending(ii => ii.IssuedDateTime)
                .ToListAsync();

            // Iterate through itemIssuances to calculate otherInventoryItemQuantity
            foreach (var itemIssuance in itemIssuances)
                if (itemIssuance.InventoryItemId != inventoryItemId)
                    otherInventoryItemQuantity +=
                        (decimal)itemIssuance.Quantity; // Assuming quantity is of type decimal
        }

        // Calculate orderShipmentAmount
        var orderShipment = await _context.OrderShipments
            .FirstOrDefaultAsync(os =>
                os.OrderId == orderId && os.OrderItemSeqId == orderItemSeqId && os.ShipGroupSeqId == shipGroupSeqId);

        if (orderShipment != null)
        {
            // Calculate the order shipment amount
            var orderShipmentAmount = (decimal)(orderShipment.Quantity - otherInventoryItemQuantity);

            // Calculate qtyForShipmentItem
            var qtyForShipmentItem = quantity - orderShipmentAmount;

            // Return the calculated quantity for shipment item
            return qtyForShipmentItem;
        }

        // If no order shipment found, return the original quantity
        return quantity;
    }

    private async Task<CreateItemIssuanceResult> CreateItemIssuance(
        string orderId,
        string orderItemSeqId,
        string shipmentId,
        string shipmentItemSeqId,
        string inventoryItemId,
        string shipGroupSeqId,
        decimal quantity,
        DateTime? issuedDateTime)
    {
        // Validate Shipment Status if shipmentId is provided
        if (!string.IsNullOrEmpty(shipmentId))
        {
            var canChangeShipmentStatusPacked = await CheckCanChangeShipmentStatusPacked(shipmentId);
            if (canChangeShipmentStatusPacked.HasError)
                throw new InvalidOperationException("Cannot change shipment status to packed.");
        }

        // get new item issuance sequence
        var itemIssuanceId = await _utilityService.GetNextSequence("ItemIssuance");

        // Create ItemIssuance
        var itemIssuance = new ItemIssuance
        {
            ItemIssuanceId = itemIssuanceId,
            ShipmentId = shipmentId,
            OrderId = orderId,
            OrderItemSeqId = orderItemSeqId,
            ShipGroupSeqId = shipGroupSeqId,
            ShipmentItemSeqId = shipmentItemSeqId,
            InventoryItemId = inventoryItemId,
            Quantity = quantity,
            IssuedDateTime = issuedDateTime ?? DateTime.UtcNow,
            CreatedStamp = DateTime.UtcNow,
            LastUpdatedStamp = DateTime.UtcNow
        };
        _context.ItemIssuances.Add(itemIssuance);

        var affectAccounting = true;

        // Check if InventoryItem is serialized and update status
        var inventoryItem = await _context.InventoryItems.FindAsync(inventoryItemId);
        if (inventoryItem != null && inventoryItem.InventoryItemTypeId == "SERIALIZED_INV_ITEM")
        {
            inventoryItem.StatusId = "INV_DELIVERED";

            // Check Product type for accounting effects
            var product = await _context.Products.FindAsync(inventoryItem.ProductId);
            if (product != null &&
                (product.ProductTypeId == "SERVICE_PRODUCT" ||
                 product.ProductTypeId == "ASSET_USAGE_OUT_IN" ||
                 product.ProductTypeId == "AGGREGATEDSERV_CONF"))
                affectAccounting = false;
        }

        await _generalLedgerService.CreateAcctgTransForSalesShipmentIssuance(itemIssuanceId);

        return new CreateItemIssuanceResult
        {
            ItemIssuanceId = itemIssuance.ItemIssuanceId,
            AffectAccounting = affectAccounting
        };
    }

    public async Task<ErrorResult> CheckCanChangeShipmentStatusPacked(string shipmentId)
    {
        var shipment = await _context.Shipments.FindAsync(shipmentId);
        if (shipment == null) return new ErrorResult { HasError = true, ErrorMessage = "Shipment not found." };

        var fromStatusId = "SHIPMENT_PACKED";
        return await CheckCanChangeShipmentStatusGeneral(shipment.ShipmentId, fromStatusId);
    }

    public async Task<ErrorResult> CheckCanChangeShipmentStatusGeneral(string shipmentId, string fromStatusId)
    {
        try
        {
            // Retrieve the shipment entity
            var shipment = await _context.Shipments.FindAsync(shipmentId);
            if (shipment == null)
            {
                return new ErrorResult { HasError = true, ErrorMessage = "Shipment not found." };
            }

            // Initialize the flag
            bool isOperationRestricted = false;

            // Replicate the Ofbiz conditions

            // Condition 1
            if ((string.IsNullOrEmpty(fromStatusId) || fromStatusId == "SHIPMENT_PACKED") &&
                shipment.StatusId == "SHIPMENT_PACKED")
            {
                isOperationRestricted = true;
            }
            // Condition 2
            else if ((fromStatusId == "SHIPMENT_PACKED" || fromStatusId == "SHIPMENT_SHIPPED") &&
                     shipment.StatusId == "SHIPMENT_SHIPPED")
            {
                isOperationRestricted = true;
            }
            // Condition 3
            else if ((fromStatusId == "SHIPMENT_PACKED" || fromStatusId == "SHIPMENT_SHIPPED" ||
                      fromStatusId == "SHIPMENT_DELIVERED") &&
                     shipment.StatusId == "SHIPMENT_DELIVERED")
            {
                isOperationRestricted = true;
            }
            // Condition 4
            else if (shipment.StatusId == "SHIPMENT_CANCELLED")
            {
                isOperationRestricted = true;
            }

            // If any condition is met, restrict the operation
            if (isOperationRestricted)
            {
                // Fetch related StatusItem
                var statusItem = await _context.StatusItems.FindAsync(shipment.StatusId);
                var statusDescription = statusItem?.Description ?? "Unknown";

                // Construct error message
                var errorMessage =
                    $"Cannot perform operation when the shipment is in the {statusDescription} [{shipment.StatusId}] status.";
                return new ErrorResult { HasError = true, ErrorMessage = errorMessage };
            }

            // Operation is allowed
            return new ErrorResult { HasError = false };
        }
        catch (Exception ex)
        {
            // Log exception
            _logger.LogError(ex, "Error in CheckCanChangeShipmentStatusGeneral for shipmentId: {shipmentId}",
                shipmentId);

            // Return generic error
            return new ErrorResult
                { HasError = true, ErrorMessage = "An unexpected error occurred while checking shipment status." };
        }
    }
}