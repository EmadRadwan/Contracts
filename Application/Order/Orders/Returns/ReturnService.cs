using Application.Accounting.Services;



using Application.Core;
using Application.Interfaces;
using Application.Order.Orders.Returns.ReturnTypes;
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Order.Orders.Returns;

public interface IReturnService
{
    Task<ReturnDto> CreateReturnHeader(CreateReturnHeaderParameters parameters);
    Task<ReturnableItemsResult> GetReturnableItems(string orderId);
    Task<OrderShippingAmountResult> GetOrderShippingAmount(string orderId);
    Task<ReturnItemOrAdjustmentResult> CreateReturnItemOrAdjustment(ReturnItemOrAdjustmentContext context);
}

public class ReturnService : IReturnService
{
    private readonly DataContext _context;
    private readonly ILogger _logger;

    private readonly IAcctgMiscService _acctgMiscService;
    private readonly IUtilityService _utilityService;
    private readonly IUserAccessor _userAccessor;
    private readonly IInvoiceUtilityService _invoiceUtilityService;


    public ReturnService(DataContext context, IUtilityService utilityService,
        IUserAccessor userAccessor, ILogger<ReturnService> logger, IAcctgMiscService acctgMiscService,
        IInvoiceUtilityService invoiceUtilityService)
    {
        _context = context;
        _utilityService = utilityService;
        _acctgMiscService = acctgMiscService;
        _userAccessor = userAccessor;
        _invoiceUtilityService = invoiceUtilityService;
        _logger = logger;
    }

    public async Task<ReturnDto> CreateReturnHeader(CreateReturnHeaderParameters parameters)
    {
        try
        {
            // Log the input parameters
            _logger.LogInformation("Creating Return Header with Parameters: {@parameters}", parameters);

            // 1. Retrieve Current Timestamp
            var nowTimestamp = DateTime.UtcNow;

            // 2. Set returnHeaderTypeId from parameters
            string returnHeaderTypeId = parameters.ReturnHeaderTypeId
                                        ?? throw new ArgumentNullException(nameof(parameters.ReturnHeaderTypeId),
                                            "ReturnHeaderTypeId is required.");

            // 3. Check if `toPartyId` is Provided
            string? toPartyId = parameters.ToPartyId;

            if (string.IsNullOrEmpty(toPartyId))
            {
                // No toPartyId specified, use destination facility to determine the party of the return
                if (returnHeaderTypeId == "CUSTOMER_RETURN" && !string.IsNullOrEmpty(parameters.DestinationFacilityId))
                {
                    var destinationFacility = await _context.Facilities
                        .FirstOrDefaultAsync(f => f.FacilityId == parameters.DestinationFacilityId);

                    if (destinationFacility?.OwnerPartyId != null)
                    {
                        toPartyId = destinationFacility.OwnerPartyId;
                    }
                }
            }

            // 4. Validate Party Role for `toPartyId`
            if (!string.IsNullOrEmpty(toPartyId))
            {
                if (returnHeaderTypeId.Contains("CUSTOMER_"))
                {
                    var partyRole = await _context.PartyRoles
                        .FirstOrDefaultAsync(pr => pr.PartyId == toPartyId && pr.RoleTypeId == "INTERNAL_ORGANIZATIO");

                    if (partyRole == null)
                    {
                        throw new InvalidOperationException("Invalid party role for customer return.");
                    }
                }
                else
                {
                    var partyRole = await _context.PartyRoles
                        .FirstOrDefaultAsync(pr => pr.PartyId == toPartyId && pr.RoleTypeId == "SUPPLIER");

                    if (partyRole == null)
                    {
                        throw new InvalidOperationException("Invalid party role for supplier return.");
                    }
                }
            }

            // 5. Check Payment Method (if provided)
            if (!string.IsNullOrEmpty(parameters.PaymentMethodId))
            {
                var paymentMethod = await _context.PaymentMethods
                    .FirstOrDefaultAsync(pm => pm.PaymentMethodId == parameters.PaymentMethodId);

                if (paymentMethod == null)
                {
                    throw new InvalidOperationException("Invalid payment method.");
                }
            }

            // 6. Set Default for `needsInventoryReceive`
            string needsInventoryReceive = string.IsNullOrEmpty(parameters.NeedsInventoryReceive)
                ? "N"
                : parameters.NeedsInventoryReceive;

            // 7. Get UserLogin
            var userLogin = await _context.Users.FirstOrDefaultAsync(x =>
                x.UserName == _userAccessor.GetUsername());

            if (userLogin == null)
            {
                throw new InvalidOperationException("UserLogin cannot be null.");
            }

            // 8. Create ReturnHeader Entity
            var newEntity = new ReturnHeader
            {
                ReturnHeaderTypeId = returnHeaderTypeId,
                StatusId = returnHeaderTypeId == "CUSTOMER_RETURN" ? "RETURN_REQUESTED" : "SUP_RETURN_REQUESTED",
                EntryDate = nowTimestamp,
                FromPartyId = parameters.FromPartyId,
                ToPartyId = toPartyId,
                DestinationFacilityId = parameters.DestinationFacilityId,
                CurrencyUomId = parameters.CurrencyUomId,
                CreatedBy = userLogin.PartyId,
                CreatedStamp = nowTimestamp,
                LastUpdatedStamp = nowTimestamp
            };

            // 9. Check Party Accounting Preferences
            var partyAccountingPreferences = await _acctgMiscService.GetPartyAccountingPreferences(toPartyId);

            if (partyAccountingPreferences?.UseInvoiceIdForReturns == "Y")
            {
                // Get the next invoice number
                var newInvoiceSequence = _invoiceUtilityService.GetNextInvoiceNumber(partyAccountingPreferences);
                newEntity.ReturnId = newInvoiceSequence;
            }
            else
            {
                // Simulate sequenced ID generation
                newEntity.ReturnId = await _utilityService.GetNextSequence("ReturnHeader");
            }

            // 10. Set Entry Date if not already set
            if (newEntity.EntryDate == default)
            {
                newEntity.EntryDate = nowTimestamp;
            }

            // 11. Save Return Header Entity
            await _context.ReturnHeaders.AddAsync(newEntity);

            await CreateReturnStatus(newEntity.ReturnId);

            // 12. Generate Success Message
            var successMessage = $"Return Request #{newEntity.ReturnId} was created successfully.";
            _logger.LogInformation(successMessage);

            var statusDescription = await _context.StatusItems.FindAsync(newEntity.StatusId);
            // 13. Return DTO
            return new ReturnDto
            {
                ReturnId = newEntity.ReturnId,
                ReturnHeaderTypeId = newEntity.ReturnHeaderTypeId,
                StatusId = newEntity.StatusId,
                StatusDescription = statusDescription?.Description,
                EntryDate = newEntity.EntryDate
            };
        }
        catch (Exception ex)
        {
            // Log the exception
            _logger.LogError(ex, "Error creating return header.");
            throw new Exception("An error occurred while creating the return header.", ex);
        }
    }

    public async Task<string> CreateReturnStatus(string returnId, string returnItemSeqId = null)
    {
        try
        {
            // Create a new ReturnStatus entity
            var newEntity = new ReturnStatus();

            // 1. Check if returnItemSeqId is provided
            if (string.IsNullOrEmpty(returnItemSeqId))
            {
                // If no returnItemSeqId, fetch ReturnHeader and set statusId
                var returnHeader = await _context.ReturnHeaders
                    .FindAsync(returnId);

                if (returnHeader == null)
                {
                    throw new InvalidOperationException("ReturnHeader not found.");
                }

                newEntity.StatusId = returnHeader.StatusId;
            }
            else
            {
                // If returnItemSeqId is provided, fetch ReturnItem and set its statusId
                var returnItem = await _context.ReturnItems
                    .FirstOrDefaultAsync(ri => ri.ReturnItemSeqId == returnItemSeqId);

                if (returnItem == null)
                {
                    throw new InvalidOperationException("ReturnItem not found.");
                }

                newEntity.ReturnItemSeqId = returnItem.ReturnItemSeqId;
                newEntity.StatusId = returnItem.StatusId;
            }

            // 2. Generate a sequenced ID for the ReturnStatus entity
            newEntity.ReturnStatusId = await _utilityService.GetNextSequence("ReturnStatus");

            // 3. Set additional properties

            var userLogin = await _context.Users.FirstOrDefaultAsync(x =>
                x.UserName == _userAccessor.GetUsername());

            if (userLogin == null)
            {
                throw new ArgumentNullException(nameof(userLogin), "UserLogin cannot be null.");
            }

            newEntity.ReturnId = returnId;
            //newEntity.ChangeByUserLoginId = userLogin.PartyId;
            newEntity.StatusDatetime = DateTime.UtcNow;
            newEntity.CreatedStamp = DateTime.UtcNow;
            newEntity.LastUpdatedStamp = DateTime.UtcNow;

            // 4. Save the ReturnStatus entity
            await _context.ReturnStatuses.AddAsync(newEntity);

            // 5. Return the ReturnStatusId as confirmation
            return newEntity.ReturnStatusId;
        }
        catch (Exception ex)
        {
            // Log the exception
            _logger.LogError($"Error creating return status. Exception: {ex.Message}", ex);
            throw new Exception("An error occurred while creating the return status.", ex);
        }
    }

    public async Task<ReturnableItemsResult> GetReturnableItems(string orderId)
    {
        // Initialize the result object to store the outcome of the function
        ReturnableItemsResult result = new ReturnableItemsResult();
        result.ReturnableItems = new List<ReturnableItemInfo>();

        // Initialize the orderHeader object to store the OrderHeader entity
        OrderHeader? orderHeader = null;

        try
        {
            // Fetch the OrderHeader entity based on the provided orderId using Entity Framework
            orderHeader = await _context.OrderHeaders.FindAsync(orderId);
        }
        catch (Exception e)
        {
            // Log the exception and return an error message
            result.Success = false;
            result.Message = $"Error: Unable to get return item information for orderId: {orderId}";
            return result;
        }

        // Check if the orderHeader is null, indicating the order was not found
        if (orderHeader == null)
        {
            // Return an error indicating the order header was not found
            result.Success = false;
            result.Message = $"Error: Unable to find order header for orderId: {orderId}";
            return result;
        }

        try
        {
            // Reconstruct the OrderItemQuantityReportGroupByItem view logic using LINQ

            // Fetch OrderItems associated with the orderId
            var orderItems = await _context.OrderItems
                .Where(oi => oi.OrderId == orderId)
                .ToListAsync();


            // Reconstruct the ItemIssuanceQuantitySum view logic using LINQ
            var itemIssuanceSums = await _context.ItemIssuances
                .GroupBy(ii => new { ii.OrderId, ii.OrderItemSeqId })
                .Select(g => new
                {
                    OrderId = g.Key.OrderId,
                    OrderItemSeqId = g.Key.OrderItemSeqId,
                    IssuedQuantitySum = g.Sum(ii => ii.Quantity ?? 0)
                })
                .Where(ii => ii.OrderId == orderId)
                .ToListAsync();

            // Build the query to compute QuantityOrdered, QuantityIssued, and QuantityOpen
            var orderItemQuantitiesIssued = (from oi in orderItems
                join ii in itemIssuanceSums
                    on new { oi.OrderId, oi.OrderItemSeqId } equals new { ii.OrderId, ii.OrderItemSeqId }
                    into oiGroup
                from ii in oiGroup.DefaultIfEmpty()
                where oi.StatusId == "ITEM_APPROVED" || oi.StatusId == "ITEM_COMPLETED"
                select new
                {
                    oi.OrderId,
                    oi.OrderItemSeqId,
                    QuantityOrdered = (oi.Quantity ?? 0) - (oi.CancelQuantity ?? 0),
                    QuantityIssued = ii?.IssuedQuantitySum ?? 0,
                    QuantityOpen = ((oi.Quantity ?? 0) - (oi.CancelQuantity ?? 0)) - (ii?.IssuedQuantitySum ?? 0),
                    OrderItem = oi
                }).ToList();


            // Check if any order items were found
            if (orderItemQuantitiesIssued.Count == 0)
            {
                // Return an error if no order items were found
                result.Success = false;
                result.Message = $"Error: No order items found for orderId: {orderId}";
                return result;
            }

            // Iterate over each order item quantity to process returnable items
            foreach (var orderItemQuantityIssued in orderItemQuantitiesIssued)
            {
                // Retrieve the order item from the projection
                OrderItem item = orderItemQuantityIssued.OrderItem;

                // For SALES_ORDER type, check if the item is physical and if it has been issued
                if (orderHeader.OrderTypeId == "SALES_ORDER")
                {
                    decimal quantityIssued = orderItemQuantityIssued.QuantityIssued;

                    // If quantityIssued is zero, check if the product is physical
                    if (quantityIssued == 0)
                    {
                        try
                        {
                            // Fetch the related Product entity for the item
                            var itemProduct =
                                await _context.Products.Include(x => x.ProductType).FirstOrDefaultAsync(x => x.ProductId == item.ProductId);

                            var isPhysical = itemProduct.ProductType.IsPhysical;
                            // If the product is physical, skip processing this item
                            if (itemProduct != null && isPhysical == "Y")
                            {
                                continue;
                            }
                        }
                        catch (Exception e)
                        {
                            // Log the exception and return an error message
                            result.Success = false;
                            result.Message = $"Error: Unable to get the item returnable product for orderId: {orderId}";
                            return result;
                        }
                    }
                }

                // Initialize the serviceResult to store the returnable quantity and price
                ReturnableQuantityResult serviceResult = null;

                try
                {
                    // Call the getReturnableQuantity method to get returnable details for the item
                    serviceResult = await GetReturnableQuantity(item);

                    // If the serviceResult indicates an error, return the error message
                    if (!serviceResult.Success)
                    {
                        result.Success = false;
                        result.Message = serviceResult.Message;
                        return result;
                    }
                }
                catch (Exception e)
                {
                    // Log the exception and return an error message
                    result.Success = false;
                    result.Message = $"Error: Unable to get the item returnable quantity for orderId: {orderId}";
                    return result;
                }

                // If the returnable quantity is zero, skip adding this item to the returnable items list
                if (serviceResult.ReturnableQuantity == 0)
                {
                    continue;
                }
                
                // Initialize the itemTypeKey with a default value
                string itemTypeKey = "FINISHED_GOOD";

                // Initialize the product variable to store the Product entity
                Product? product = null;

                if (!string.IsNullOrEmpty(item.ProductId))
                {
                    try
                    {
                        // Fetch the related Product entity for the item
                        product = await _context.Products.FindAsync(item.ProductId);
                    }
                    catch (Exception e)
                    {
                        // Log the exception and return an error message
                        result.Success = false;
                        result.Message = $"Error: Unable to get order item information for orderId: {orderId}";
                        return result;
                    }
                }

                // Determine the itemTypeKey based on the product or order item type
                if (product != null)
                {
                    itemTypeKey = product.ProductTypeId;
                }
                else if (!string.IsNullOrEmpty(item.OrderItemTypeId))
                {
                    itemTypeKey = item.OrderItemTypeId;
                }

                // Create a new ReturnableItemInfo object to hold the return information
                result.ReturnableItems.Add(new ReturnableItemInfo
                {
                    OrderId = item.OrderId,
                    ItemType = "OrderItem",
                    ItemTypeKey = itemTypeKey,
                    OrderItemSeqId = item.OrderItemSeqId,
                    ProductId = item.ProductId,
                    ItemDescription = item.ItemDescription,
                    UnitPrice = item.UnitPrice,
                    StatusId = item.StatusId,
                    ReturnableQuantity = serviceResult.ReturnableQuantity,
                    OrderedQuantity = item.Quantity,
                    ReturnablePrice = serviceResult.ReturnablePrice
                });

                // Initialize the itemAdjustments list to store OrderAdjustment entities
                List<OrderAdjustment> itemAdjustments = null;

                // Fetch the related OrderAdjustments for the item
                itemAdjustments = await _context.OrderAdjustments
                    .Where(oa => oa.OrderId == item.OrderId && oa.OrderItemSeqId == item.OrderItemSeqId)
                    .ToListAsync();

                // Check if there are any item adjustments
                if (itemAdjustments.Count > 0)
                {
                    // Iterate over each item adjustment
                    foreach (var itemAdjustment in itemAdjustments)
                    {
                        // Create a new ReturnableItemInfo object for the adjustment
                        result.ReturnableItems.Add(new ReturnableItemInfo
                        {
                            OrderId = itemAdjustment.OrderId,
                            OrderItemSeqId = itemAdjustment.OrderItemSeqId,
                            ItemType = "OrderAdjustment",
                            OrderAdjustmentId = itemAdjustment.OrderAdjustmentId,
                            ReturnAdjustmentTypeId = itemAdjustment.OrderAdjustmentTypeId,
                            Description = itemAdjustment.Description,
                            Amount = itemAdjustment.Amount,
                            ReturnableQuantity = 1,
                            OrderedQuantity = 1,
                            ReturnablePrice = itemAdjustment.Amount ?? 0
                        });
                    }
                }
                
                var orh = new OrderReadHelper(orderId)
                {
                    Context = _context
                };
                orh.InitializeOrder();
                var orderHeaderAdjustments = await orh.GetAvailableOrderHeaderAdjustments();

                // Check if there are any item adjustments
                if (orderHeaderAdjustments.Count > 0)
                {
                    // Iterate over each item adjustment
                    foreach (var itemAdjustment in orderHeaderAdjustments)
                    {
                        var adjustmentTypeDescription =
                            await _context.OrderAdjustmentTypes.FindAsync(itemAdjustment.OrderAdjustmentTypeId);
                        // Create a new ReturnableItemInfo object for the adjustment
                        result.ReturnableItems.Add(new ReturnableItemInfo
                        {
                            OrderId = itemAdjustment.OrderId,
                            ItemType = "Order Level Adjustment",
                            OrderAdjustmentId = itemAdjustment.OrderAdjustmentId,
                            ReturnAdjustmentTypeId = itemAdjustment.OrderAdjustmentTypeId,
                            Description = adjustmentTypeDescription?.Description,
                            Amount = itemAdjustment.Amount,
                            ReturnableQuantity = 1,
                            ReturnablePrice = itemAdjustment.Amount ?? 0,
                            ReturnItemSeqId = "_NA_"
                        });
                    }
                }
            }
        }
        catch (Exception e)
        {
            // Log the exception and return an error message
            result.Success = false;
            result.Message = $"Error processing returnable items for orderId: {orderId}";
            return result;
        }

        // Set the success flag to true and return the result with the list of returnable items
        result.Success = true;
        return result;
    }

    public async Task<ReturnableQuantityResult> GetReturnableQuantity(OrderItem orderItem)
    {
        // Initialize the result object to store the outcome of the method
        var result = new ReturnableQuantityResult();

        // Initialize the Product object
        Product? product = null;

        // Check if the OrderItem has a ProductId
        if (!string.IsNullOrEmpty(orderItem.ProductId))
        {
            try
            {
                // Fetch the Product entity related to the OrderItem using Entity Framework
                product = await _context.Products.FindAsync(orderItem.ProductId);
            }
            catch (Exception e)
            {
                // Return error result
                result.Success = false;
                result.Message = $"Error: Unable to get Product from OrderItem for productId: {orderItem.ProductId}";
                return result;
            }
        }

        // Check returnable status
        bool returnable = true;

        // First, check the 'returnable' flag on the product
        if (product != null && !string.IsNullOrEmpty(product.Returnable) &&
            product.Returnable.Equals("N", StringComparison.OrdinalIgnoreCase))
        {
            // The product is not returnable at all
            returnable = false;
        }

        // Next, check the 'supportDiscontinuationDate' on the product
        if (product != null && product.SupportDiscontinuationDate != null &&
            DateTime.Now >= product.SupportDiscontinuationDate)
        {
            // Support discontinued either now or in the past
            returnable = false;
        }

        // Get the item status from the OrderItem
        string itemStatus = orderItem.StatusId;

        // Get the ordered quantity from the OrderItem
        decimal orderQty = orderItem.Quantity ?? 0;

        // Subtract the cancel quantity if present
        if (orderItem.CancelQuantity.HasValue)
        {
            orderQty -= orderItem.CancelQuantity.Value;
        }

        // Initialize the returnable quantity
        decimal returnableQuantity = 0;

        // Check if the item is returnable and if the status is ITEM_APPROVED or ITEM_COMPLETED
        if (returnable && (itemStatus == "ITEM_APPROVED" || itemStatus == "ITEM_COMPLETED"))
        {
            List<ReturnItem> returnedItems = null;

            try
            {
                // Fetch the ReturnItem entities related to the OrderItem
                returnedItems = await _context.ReturnItems
                    .Where(ri => ri.OrderId == orderItem.OrderId && ri.OrderItemSeqId == orderItem.OrderItemSeqId)
                    .ToListAsync();
            }
            catch (Exception e)
            {
                // Return error result
                result.Success = false;
                result.Message = "Error: Unable to get ReturnItem information";
                return result;
            }

            // Check if there are no returned items
            if (returnedItems.Count == 0)
            {
                // If there are no returned items, the returnable quantity is the order quantity
                returnableQuantity = orderQty;
            }
            else
            {
                // Initialize the total returned quantity
                decimal returnedQty = 0;

                // Iterate over each ReturnItem to sum up the returned quantities
                foreach (var returnItem in returnedItems)
                {
                    ReturnHeader? returnHeader = null;

                    try
                    {
                        // Fetch the ReturnHeader related to the ReturnItem
                        returnHeader = await _context.ReturnHeaders.FindAsync(returnItem.ReturnId);
                    }
                    catch (Exception e)
                    {
                        // Return error result
                        result.Success = false;
                        result.Message = "Error: Unable to get ReturnHeader from ReturnItem";
                        return result;
                    }

                    // Get the return status from the ReturnHeader
                    string returnStatus = returnHeader.StatusId;

                    // Check if the return is not cancelled
                    if (!"RETURN_CANCELLED".Equals(returnStatus))
                    {
                        // Add the return quantity if it's not null
                        if (returnItem.ReturnQuantity.HasValue)
                        {
                            returnedQty += returnItem.ReturnQuantity.Value;
                        }
                    }
                }

                // Calculate the returnable quantity based on the total returned quantity
                if (returnedQty < orderQty)
                {
                    returnableQuantity = orderQty - returnedQty;
                }
            }
        }

        // Get the returnable price, which equals the OrderItem's unit price
        decimal returnablePrice = orderItem.UnitPrice ?? 0;

        // Set the calculated values in the result object
        result.ReturnableQuantity = returnableQuantity;
        result.ReturnablePrice = returnablePrice;
        result.Success = true;

        // Return the result object
        return result;
    }

    public async Task<OrderShippingAmountResult> GetOrderShippingAmount(string orderId)
    {
        // Initialize the result object to store the outcome of the function
        OrderShippingAmountResult result = new OrderShippingAmountResult();

        // Initialize the OrderHeader object
        OrderHeader orderHeader = null;

        try
        {
            // Fetch the OrderHeader entity based on the provided orderId using Entity Framework
            orderHeader = await _context.OrderHeaders
                .FirstOrDefaultAsync(o => o.OrderId == orderId);
        }
        catch (Exception e)
        {
            result.Success = false;
            result.Message = $"Error: Could not get order information ({e.Message}).";
            return result;
        }

        if (orderHeader != null)
        {
            try
            {
                // Create an OrderReadHelper instance with the order header and context
                var orh = new OrderReadHelper(orderHeader.OrderId);

                // Get valid order items (excluding cancelled items)
                var orderItems = await orh.GetValidOrderItems();

                // Get all order adjustments
                var orderAdjustments = await orh.GetAdjustments();

                // Get order header adjustments (adjustments not associated with specific order items)
                var orderHeaderAdjustments = await orh.GetOrderHeaderAdjustments();

                // Calculate the order items subtotal
                decimal orderSubTotal = await orh.GetOrderItemsSubTotal();

                // Calculate shipping amount from order items adjustments
                // The last parameter 'includeShipping' is true to include shipping adjustments
                decimal shippingAmount =
                    orh.GetAllOrderItemsAdjustmentsTotal(orderItems, orderAdjustments, false, false, true);

                // Add shipping adjustments from order header adjustments
                shippingAmount +=
                    orh.CalcOrderAdjustments(orderHeaderAdjustments, orderSubTotal, false, false, true);

                // Set the result
                result.Success = true;
                result.ShippingAmount = shippingAmount;
                return result;
            }
            catch (Exception e)
            {
                result.Success = false;
                result.Message = $"Error: Could not calculate shipping amount ({e.Message}).";
                return result;
            }
        }

        // Order header not found
        result.Success = false;
        result.Message = "Error: Unable to find order header. Cannot get shipping amount.";
        return result;
    }

    public async Task<ReturnItemOrAdjustmentResult> CreateReturnItemOrAdjustment(ReturnItemOrAdjustmentContext context)
    {
        try
        {
            // Determine whether to create a return item or return adjustment based on the presence of OrderItemSeqId
            if (!string.IsNullOrEmpty(context.OrderItemSeqId))
            {
                // Call CreateReturnItem
                context.IncludeAdjustments = "N"; // Ensure adjustments are not included
                var createReturnItemContext = new CreateReturnItemContext
                {
                    ReturnId = context.ReturnId,
                    ReturnItemTypeId = context.ReturnItemTypeId,
                    ReturnTypeId = context.ReturnTypeId,
                    OrderId = context.OrderId,
                    OrderItemSeqId = context.OrderItemSeqId,
                    ReturnQuantity = (decimal)context.ReturnQuantity,
                    ReturnPrice = (decimal)context.ReturnPrice,
                    IncludeAdjustments = context.IncludeAdjustments,
                    UserLoginPartyId = context.UserLoginPartyId
                };
                var result = await CreateReturnItem(createReturnItemContext);
                if (result.IsSuccess)
                {
                    return ReturnItemOrAdjustmentResult.SuccessResult(new { result.ReturnItemSeqId });
                }

                return ReturnItemOrAdjustmentResult.Error(result.Message);
            }
            else
            {
                // Call CreateReturnAdjustment
                var createReturnAdjustmentContext = new CreateReturnAdjustmentContext
                {
                    ReturnId = context.ReturnId,
                    //ReturnItemSeqId = context.ReturnItemSeqId,
                    ReturnAdjustmentTypeId = context.ReturnAdjustmentTypeId,
                    OrderAdjustmentId = context.OrderAdjustmentId,
                    Description = context.Description,
                    Amount = context.Amount
                };
                var result = await CreateReturnAdjustment(createReturnAdjustmentContext);
                if (result.IsSuccess)
                {
                    return ReturnItemOrAdjustmentResult.SuccessResult(new { result.ReturnAdjustmentId });
                }

                return ReturnItemOrAdjustmentResult.Error(result.Message);
            }
        }
        catch (Exception ex)
        {
            // Log the error
            Console.WriteLine($"Error: {ex.Message}");
            return ReturnItemOrAdjustmentResult.Error($"Error occurred while processing: {ex.Message}");
        }
    }


    public async Task<CreateReturnItemResult> CreateReturnItem(CreateReturnItemContext context)
    {
        try
        {
            // 1. Retrieve ReturnHeader
            var returnHeader = await _context.ReturnHeaders
                .FirstOrDefaultAsync(rh => rh.ReturnId == context.ReturnId);

            if (returnHeader == null)
            {
                return CreateReturnItemResult.Error("ReturnHeader not found for the given ReturnId.");
            }


            // 3. Validate returnItemTypeId
            if (string.IsNullOrEmpty(context.ReturnItemTypeId))
            {
                return CreateReturnItemResult.Error("ReturnItemTypeId is not defined.");
            }

            // 4. Validate payment method for specific return types
            if (string.IsNullOrEmpty(returnHeader.PaymentMethodId) &&
                returnHeader.StatusId == "RETURN_ACCEPTED" &&
                (context.ReturnTypeId == "RTN_CSREPLACE" || context.ReturnTypeId == "RTN_REPAIR_REPLACE"))
            {
                return CreateReturnItemResult.Error("Payment method is required for this type of return.");
            }

            // 5. Validate returnQuantity
            if (context.ReturnQuantity <= 0)
            {
                return CreateReturnItemResult.Error("No return quantity available. Previous returns may exist.");
            }

            // 6. Initialize default values
            decimal returnableQuantity = 0;
            decimal returnablePrice = 0;

            // 7. Fetch related OrderItem if orderItemSeqId is provided
            OrderItem orderItem = null;
            if (!string.IsNullOrEmpty(context.OrderItemSeqId))
            {
                orderItem = await _context.OrderItems
                    .FirstOrDefaultAsync(oi =>
                        oi.OrderId == context.OrderId && oi.OrderItemSeqId == context.OrderItemSeqId);

                if (orderItem != null)
                {
                    // Log fetched OrderItem
                    Console.WriteLine($"Fetched OrderItem: {orderItem.OrderItemSeqId}");

                    // 8. Get returnableQuantity and returnablePrice
                    var returnableValues = await GetReturnableQuantity(orderItem);
                    returnableQuantity = returnableValues.ReturnableQuantity;
                    returnablePrice = returnableValues.ReturnablePrice;
                }
            }

            // 9. Validate returnable quantities and prices
            if (context.ReturnQuantity > returnableQuantity)
            {
                return CreateReturnItemResult.Error(
                    "Requested return quantity not available. Previous returns may exist.");
            }

            if (orderItem != null && context.ReturnQuantity > orderItem.Quantity)
            {
                return CreateReturnItemResult.Error("Return quantity cannot exceed the ordered quantity.");
            }

            if (context.ReturnPrice > returnablePrice)
            {
                return CreateReturnItemResult.Error("Return price cannot exceed the purchase price.");
            }

            // Check if the item has been fully returned
            if (returnableQuantity == 0)
            {
                Console.WriteLine($"Order {context.OrderId} item {context.OrderItemSeqId} has been returned in full.");
                return CreateReturnItemResult.Error("The item has already been fully returned.");
            }

            // 10. Create ReturnItem
            var newReturnItem = new ReturnItem
            {
                ReturnId = context.ReturnId,
                ReturnItemSeqId = Guid.NewGuid().ToString(),
                ReturnItemTypeId = context.ReturnItemTypeId,
                OrderId = context.OrderId,
                OrderItemSeqId = context.OrderItemSeqId,
                StatusId = "RETURN_REQUESTED",
                ReturnQuantity = context.ReturnQuantity,
                ReturnPrice = context.ReturnPrice,
                CreatedStamp = DateTime.UtcNow,
                LastUpdatedStamp = DateTime.UtcNow
            };

            _context.ReturnItems.Add(newReturnItem);

            // 11. Include adjustments if requested
            if (string.IsNullOrEmpty(context.IncludeAdjustments) || context.IncludeAdjustments == "Y")
            {
                if (orderItem != null)
                {
                    var orderAdjustments = await _context.OrderAdjustments
                        .Where(oa => oa.OrderId == orderItem.OrderId && oa.OrderItemSeqId == orderItem.OrderItemSeqId)
                        .ToListAsync();

                    foreach (var orderAdjustment in orderAdjustments)
                    {
                        // Log adjustment creation
                        Console.WriteLine(
                            $"Creating ReturnAdjustment for OrderAdjustmentId: {orderAdjustment.OrderAdjustmentId}");

                        var newReturnAdjustment = new ReturnAdjustment
                        {
                            ReturnId = context.ReturnId,
                            ReturnItemSeqId = newReturnItem.ReturnItemSeqId,
                            ReturnTypeId = context.ReturnTypeId,
                            OrderAdjustmentId = orderAdjustment.OrderAdjustmentId
                        };

                        _context.ReturnAdjustments.Add(newReturnAdjustment);
                    }
                }
            }

            return CreateReturnItemResult.Success(newReturnItem.ReturnItemSeqId);
        }
        catch (Exception ex)
        {
            return CreateReturnItemResult.Error($"An error occurred: {ex.Message}");
        }
    }

    public async Task<CreateReturnAdjustmentResult> CreateReturnAdjustment(CreateReturnAdjustmentContext context)
    {
        try
        {
            // 1. Retrieve OrderAdjustment if orderAdjustmentId is provided
            OrderAdjustment orderAdjustment = null;
            if (!string.IsNullOrEmpty(context.OrderAdjustmentId))
            {
                orderAdjustment = await _context.OrderAdjustments
                    .FirstOrDefaultAsync(oa => oa.OrderAdjustmentId == context.OrderAdjustmentId);

                if (orderAdjustment == null)
                {
                    return CreateReturnAdjustmentResult.Error(
                        $"OrderAdjustment not found for orderAdjustmentId: {context.OrderAdjustmentId}");
                }
            }

            // 2. Retrieve ReturnHeader and determine returnHeaderTypeId
            var returnHeader = await _context.ReturnHeaders
                .FirstOrDefaultAsync(rh => rh.ReturnId == context.ReturnId);

            string returnHeaderTypeId = returnHeader?.ReturnHeaderTypeId ?? "CUSTOMER_RETURN";

            // 3. Retrieve ReturnItemTypeMap based on returnHeaderTypeId and OrderAdjustmentTypeId
            var returnItemTypeMap = await _context.ReturnItemTypeMaps
                .FirstOrDefaultAsync(ritm =>
                    ritm.ReturnHeaderTypeId == returnHeaderTypeId &&
                    ritm.ReturnItemMapKey == orderAdjustment.OrderAdjustmentTypeId);

            // 4. Determine returnAdjustmentTypeId
            if (string.IsNullOrEmpty(context.ReturnAdjustmentTypeId))
            {
                context.ReturnAdjustmentTypeId = returnItemTypeMap?.ReturnItemTypeId ?? "RET_MAN_ADJ";
            }

            // 5. Handle returnItemSeqId and retrieve associated ReturnItem and OrderItem
            ReturnItem returnItem = null;
            OrderItem orderItem = null;
            if (!string.IsNullOrEmpty(context.ReturnItemSeqId) && context.ReturnItemSeqId != "_NA_")
            {
                returnItem = await _context.ReturnItems
                    .FirstOrDefaultAsync(ri =>
                        ri.ReturnId == context.ReturnId && ri.ReturnItemSeqId == context.ReturnItemSeqId);

                if (returnItem != null)
                {
                    orderItem = await _context.OrderItems
                        .FirstOrDefaultAsync(oi =>
                            oi.OrderId == returnItem.OrderId && oi.OrderItemSeqId == returnItem.OrderItemSeqId);
                }
            }
            else if (orderAdjustment != null && !string.IsNullOrEmpty(orderAdjustment.OrderItemSeqId) &&
                     orderAdjustment.OrderItemSeqId != "_NA_")
            {
                returnItem = await _context.ReturnItems
                    .FirstOrDefaultAsync(ri =>
                        ri.ReturnId == context.ReturnId &&
                        ri.OrderId == orderAdjustment.OrderId &&
                        ri.OrderItemSeqId == orderAdjustment.OrderItemSeqId);

                if (returnItem != null)
                {
                    orderItem = await _context.OrderItems
                        .FirstOrDefaultAsync(oi =>
                            oi.OrderId == returnItem.OrderId && oi.OrderItemSeqId == returnItem.OrderItemSeqId);
                }
            }

            // 6. Copy fields from OrderAdjustment to context
            if (orderAdjustment != null)
            {
                context.Amount ??= orderAdjustment.Amount;
            }

            // 7. Calculate adjustment amount
            decimal? amount = context.Amount;
            if (returnItem != null)
            {
                if (NeedRecalculate(context.ReturnAdjustmentTypeId))
                {
                    var returnTotal = returnItem.ReturnPrice * returnItem.ReturnQuantity;
                    var orderTotal = orderItem.Quantity * orderItem.UnitPrice;
                    amount = GetAdjustmentAmount(
                        context.ReturnAdjustmentTypeId == "RET_SALES_TAX_ADJ",
                        returnTotal ?? 0,
                        orderTotal ?? 0,
                        orderAdjustment?.Amount ?? 0
                    );
                }
            }

            // 8. Create ReturnAdjustment entity
            var newReturnAdjustment = new ReturnAdjustment
            {
                ReturnAdjustmentId = Guid.NewGuid().ToString(),
                ReturnId = context.ReturnId,
                ReturnItemSeqId = string.IsNullOrEmpty(context.ReturnItemSeqId) ? "_NA_" : context.ReturnItemSeqId,
                ReturnAdjustmentTypeId = context.ReturnAdjustmentTypeId,
                Description = string.IsNullOrEmpty(context.Description)
                    ? returnItemTypeMap?.ReturnItemMapKey
                    : context.Description,
                Amount = amount ?? 0,
                TaxAuthorityRateSeqId = orderAdjustment?.TaxAuthorityRateSeqId
            };

            // Add to the database and save changes
            _context.ReturnAdjustments.Add(newReturnAdjustment);

            // Return success result
            return CreateReturnAdjustmentResult.Success(newReturnAdjustment.ReturnAdjustmentId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return CreateReturnAdjustmentResult.Error($"Failed to create ReturnAdjustment: {ex.Message}");
        }
    }

    /// <summary>
    /// Determines if a return adjustment type needs to be recalculated when the return item is updated.
    /// </summary>
    /// <param name="returnAdjustmentTypeId">The return adjustment type ID.</param>
    /// <returns>True if the return adjustment needs to be recalculated, otherwise false.</returns>
    public bool NeedRecalculate(string returnAdjustmentTypeId)
    {
        return returnAdjustmentTypeId == "RET_PROMOTION_ADJ" ||
               returnAdjustmentTypeId == "RET_DISCOUNT_ADJ" ||
               returnAdjustmentTypeId == "RET_SALES_TAX_ADJ";
    }

    public decimal GetAdjustmentAmount(bool isSalesTax, decimal returnTotal, decimal originalTotal, decimal amount)
    {
        var glSettings = _acctgMiscService.GetGlArithmeticSettingsInline();
        var ledgerDecimals = glSettings.DecimalScale;
        var roundingMode = glSettings.RoundingMode;

        // Scale returnTotal and originalTotal using CustomRound
        returnTotal = _acctgMiscService.CustomRound(returnTotal, (int)ledgerDecimals, roundingMode);
        originalTotal = _acctgMiscService.CustomRound(originalTotal, (int)ledgerDecimals, roundingMode);

        // Calculate the adjusted amount
        decimal newAmount;
        if (originalTotal != 0)
        {
            newAmount = _acctgMiscService.CustomRound(returnTotal * amount / originalTotal, (int)ledgerDecimals,
                roundingMode);
        }
        else
        {
            newAmount = 0;
        }

        return newAmount;
    }
    
    public async Task<string> UpdateReturnHeader(UpdateReturnHeaderDto parameters)
    {
        var returnHeader = await _context.ReturnHeaders
            .FirstOrDefaultAsync(r => r.ReturnId == parameters.ReturnId);
        if (returnHeader == null) return "Return header not found.";

        parameters.OldStatusId = returnHeader.StatusId;

        if (parameters.StatusId == "RETURN_ACCEPTED")
        {
            var returnItems = await _context.ReturnItems
                .Where(ri => ri.ReturnId == returnHeader.ReturnId)
                .ToListAsync();

            decimal returnTotalAmount = (decimal)returnItems.Sum(item => item.ReturnPrice * item.ReturnQuantity);

            foreach (var item in returnItems)
            {
                if ((item.ReturnTypeId == "RTN_CSREPLACE" || item.ReturnTypeId == "RTN_REPAIR_REPLACE")
                    && string.IsNullOrEmpty(parameters.PaymentMethodId) && string.IsNullOrEmpty(returnHeader.PaymentMethodId))
                {
                    return "Payment method is required for cross-ship returns.";
                }

                var orderTotal = await GetOrderAvailableReturnedTotal(item.OrderId);
                if (orderTotal.AvailableReturnTotal < returnTotalAmount)
                {
                    return "Return total cannot exceed the order total.";
                }
            }
        }

        if (!string.IsNullOrEmpty(parameters.StatusId) && parameters.StatusId != returnHeader.StatusId)
        {
            var validChange = await _context.StatusValidChanges
                .FirstOrDefaultAsync(s => s.StatusId == returnHeader.StatusId && s.StatusIdTo == parameters.StatusId);

            if (validChange == null)
            {
                return "Invalid status change.";
            }
        }

        _context.ReturnHeaders.Update(returnHeader);
        return "Return header updated successfully.";
    }
    
    public async Task<GetOrderAvailableReturnedTotalResult> GetOrderAvailableReturnedTotal(string orderId, decimal? adjustment = null, bool? countNewReturnItems = null)
    {
        if (string.IsNullOrEmpty(orderId)) throw new ArgumentException("Order ID cannot be null or empty.");

        var orderHelper = new OrderReadHelper(orderId)
        {
            Context = _context
        };
        orderHelper.InitializeOrder();

        // Retrieve adjustment value or default to zero
        decimal adj = adjustment ?? 0m;
        bool countNew = countNewReturnItems ?? false;

        // Get total amount of returned items from OrderReadHelper
        decimal returnTotal = await orderHelper.GetOrderReturnedTotal(countNew);

        // Get the grand total from OrderReadHelper
        decimal orderTotal = await orderHelper.GetOrderGrandTotal();

        // Calculate available return total
        decimal availableReturnTotal = orderTotal - returnTotal - adj;

        return new GetOrderAvailableReturnedTotalResult
        {
            AvailableReturnTotal = availableReturnTotal,
            OrderTotal = orderTotal,
            ReturnTotal = returnTotal
        };
    }


}