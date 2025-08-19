using Application._Base;
using Application.Accounting.Payments;
using Application.Accounting.Services;
using Application.Shipments.Payments;
using Application.Catalog.ProductStores;
using Application.Common;
using Application.Core;
using Application.Parties.Parties;
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Order.Orders;

public interface IOrderHelperService
{
    Task<bool> ApproveOrder(
        string orderId,
        bool holdOrder);

    Task<List<OrderPaymentPreference>> MakeAllOrderPaymentInfos(OrderPaymentInfoInput input);
    Task<string> ReceiveOfflinePayment(ReceiveOfflinePaymentInput input);
    Task<CreatePaymentFromOrderResult> CreatePaymentFromOrder(CreatePaymentFromOrderInput input);
    Task<List<BillingAccountModel>> MakePartyBillingAccountList(string partyId, string currencyUomId);
    Task<ChangeOrderStatusResponse> ChangeOrderStatus(ChangeOrderStatusRequest request);
    Task<decimal> GetBillingAccountNetBalance(string billingAccountId);

    Task OrderStatusChanges(
        string orderId,
        string orderStatus,
        string fromItemStatus,
        string toItemStatus,
        string digitalItemStatus);

    Task<decimal> GetBillingAccountBalance(BillingAccount billingAccount);
    Task<SetItemStatusResponse> ChangeOrderItemStatus(SetItemStatusRequest request);
}

public class OrderHelperService : BaseService, IOrderHelperService
{
    private readonly IProductStoreService _productStoreService;
    private readonly IAcctgMiscService _acctgMiscService;
    private readonly ICommonService _commonService;
    private readonly IPaymentHelperService _paymentHelperService;
    private readonly IPartyService _partyService;
    private readonly IInvoiceHelperService _invoiceHelperService;

    public OrderHelperService(DataContext context, IUtilityService utilityService,
        IProductStoreService productStoreService, IAcctgMiscService acctgMiscService, ICommonService commonService,
        IPaymentHelperService paymentHelperService, IPartyService partyService,
        ILogger<OrderHelperService> logger, IInvoiceHelperService invoiceHelperService) : base(context, utilityService,
        logger)
    {
        _productStoreService = productStoreService;
        _acctgMiscService = acctgMiscService;
        _commonService = commonService;
        _paymentHelperService = paymentHelperService;
        _partyService = partyService;
        _invoiceHelperService = invoiceHelperService;
    }

    public async Task<bool> ApproveOrder(
        string orderId,
        bool holdOrder)
    {
        // Retrieve the ProductStore for the logged-in user
        ProductStore productStore;
        try
        {
            productStore = await _productStoreService.GetProductStoreForLoggedInUser();
            if (productStore == null)
            {
                throw new ArgumentException(
                    $"Could not find ProductStore for orderId [{orderId}], cannot approve order.");
            }
        }
        catch (Exception ex)
        {
            // Log error and rethrow if there's an issue fetching the product store
            _logger.LogError(ex, "Error fetching ProductStore");
            throw new Exception("Error fetching ProductStore", ex);
        }

        // Default internal statuses for held orders
        string headerStatus = "ORDER_PROCESSING";
        string itemStatus = "ITEM_CREATED";
        string digitalItemStatus = "ITEM_APPROVED";

        // Update statuses based on ProductStore settings if holdOrder is false
        if (!holdOrder)
        {
            if (!string.IsNullOrEmpty(productStore.HeaderApprovedStatus))
            {
                headerStatus = productStore.HeaderApprovedStatus;
            }

            if (!string.IsNullOrEmpty(productStore.ItemApprovedStatus))
            {
                itemStatus = productStore.ItemApprovedStatus;
            }

            if (!string.IsNullOrEmpty(productStore.DigitalItemApprovedStatus))
            {
                digitalItemStatus = productStore.DigitalItemApprovedStatus;
            }
        }

        // Attempt to change order statuses
        try
        {
            await OrderStatusChanges(
                orderId,
                headerStatus,
                "ITEM_CREATED",
                itemStatus,
                digitalItemStatus);
        }
        catch (Exception ex)
        {
            // Log error and return false if status changes could not be updated
            _logger.LogError(ex, $"Service invocation error, status changes were not updated for order #{orderId}");
            return false;
        }

        return true;
    }

    public async Task OrderStatusChanges(
        string orderId,
        string orderStatus,
        string fromItemStatus,
        string toItemStatus,
        string digitalItemStatus)
    {
        try
        {
            // Set the status on the order header using ChangeOrderStatus
            var orderStatusRequest = new ChangeOrderStatusRequest
            {
                OrderId = orderId,
                StatusId = orderStatus,
            };

            var statusResult = await ChangeOrderStatus(orderStatusRequest);
            if (statusResult == null)
            {
                Console.WriteLine($"Error changing order status: Status result is null");
                throw new Exception("Order status change failed.");
            }

            // Set the status on the order item(s) using ChangeOrderItemStatus
            var itemStatusRequest = new SetItemStatusRequest
            {
                OrderId = orderId,
                StatusId = toItemStatus,
            };

            if (!string.IsNullOrEmpty(fromItemStatus))
            {
                itemStatusRequest.FromStatusId = fromItemStatus;
            }

            var itemStatusResult = await ChangeOrderItemStatus(itemStatusRequest);
            /*if (!itemStatusResult.Success)
            {
                string errorMessage = itemStatusResult.Message;
                Console.WriteLine($"Error changing order item status: {errorMessage}");
                throw new Exception(errorMessage);
            }*/

            // Now set the status for digital items
            if (!string.IsNullOrEmpty(digitalItemStatus) && digitalItemStatus != toItemStatus)
            {
                // Retrieve the order header
                var orderHeader = await _context.OrderHeaders
                    .SingleOrDefaultAsync(o => o.OrderId == orderId);

                if (orderHeader != null)
                {
                    // Retrieve related order items
                    var orderItems = await _context.OrderItems
                        .Where(oi => oi.OrderId == orderId)
                        .ToListAsync();

                    foreach (var orderItem in orderItems)
                    {
                        string orderItemSeqId = orderItem.OrderItemSeqId;

                        // Retrieve the product related to the order item
                        var product = await _context.Products
                            .SingleOrDefaultAsync(p => p.ProductId == orderItem.ProductId);

                        if (product != null)
                        {
                            // Retrieve the product type
                            var productType = await _context.ProductTypes
                                .SingleOrDefaultAsync(pt => pt.ProductTypeId == product.ProductTypeId);

                            if (productType != null)
                            {
                                string isDigital = productType.IsDigital;
                                if (!string.IsNullOrEmpty(isDigital) &&
                                    isDigital.Equals("Y", StringComparison.OrdinalIgnoreCase))
                                {
                                    // Update the status for digital items
                                    var digitalItemStatusRequest = new SetItemStatusRequest
                                    {
                                        OrderId = orderId,
                                        OrderItemSeqId = orderItemSeqId,
                                        StatusId = digitalItemStatus,
                                    };

                                    var digitalStatusChange = await ChangeOrderItemStatus(digitalItemStatusRequest);
                                    if (!digitalStatusChange.Success)
                                    {
                                        string errorMessage = digitalStatusChange.Message;
                                        Console.WriteLine($"Error changing digital item status: {errorMessage}");
                                        throw new Exception(errorMessage);
                                    }
                                }
                            }
                        }
                        else
                        {
                            // For non-product items, treat as a digital item
                            if (!orderItem.OrderItemTypeId.Equals("PRODUCT_ORDER_ITEM",
                                    StringComparison.OrdinalIgnoreCase))
                            {
                                var nonProductStatusRequest = new SetItemStatusRequest
                                {
                                    OrderId = orderId,
                                    OrderItemSeqId = orderItemSeqId,
                                    StatusId = digitalItemStatus,
                                };

                                var digitalStatusChange = await ChangeOrderItemStatus(nonProductStatusRequest);
                                if (!digitalStatusChange.Success)
                                {
                                    string errorMessage = digitalStatusChange.Message;
                                    Console.WriteLine($"Error changing non-product item status: {errorMessage}");
                                    throw new Exception(errorMessage);
                                }
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Log and rethrow the exception
            Console.WriteLine($"Exception in OrderStatusChangesAsync: {ex.Message}");
            throw;
        }
    }

    public async Task<ChangeOrderStatusResponse> ChangeOrderStatus(ChangeOrderStatusRequest request)
    {
        var response = new ChangeOrderStatusResponse();

        // Retrieve order header using orderId
        var orderHeader = await _utilityService.FindLocalOrDatabaseAsync<OrderHeader>(request.OrderId);

        if (orderHeader == null)
        {
            throw new Exception("Order cannot be found");
        }

        // Save old status for the response
        response.OldStatusId = orderHeader.StatusId;
        response.OrderTypeId = orderHeader.OrderTypeId;

        if (orderHeader.StatusId == request.StatusId)
        {
            return response;
        }

        // Check if the status change is valid
        var statusChange = await _context.StatusValidChanges
            .SingleOrDefaultAsync(s => s.StatusId == orderHeader.StatusId && s.StatusIdTo == request.StatusId);

        if (statusChange == null)
        {
            throw new Exception("Status change is not valid");
        }

        // Update the order status
        orderHeader.StatusId = request.StatusId;

        // Create and store the order status change record
        var orderStatus = new OrderStatus
        {
            OrderStatusId = Guid.NewGuid().ToString(), // Unique identifier
            StatusId = request.StatusId,
            OrderId = request.OrderId,
            StatusDatetime = DateTime.Now,
            StatusUserLogin = request.UserLoginId,
            ChangeReason = request.ChangeReason
        };
        await _context.OrderStatuses.AddAsync(orderStatus);

        // Populate additional response data
        response.NeedsInventoryIssuance = orderHeader.NeedsInventoryIssuance;
        response.GrandTotal = (decimal)orderHeader.GrandTotal;
        response.OrderStatusId = request.StatusId;

        // If item status update is needed
        if (request.SetItemStatus)
        {
            string newItemStatusId = request.StatusId switch
            {
                "ORDER_APPROVED" => "ITEM_APPROVED",
                "ORDER_COMPLETED" => "ITEM_COMPLETED",
                "ORDER_CANCELLED" => "ITEM_CANCELLED",
                _ => null
            };

            if (newItemStatusId != null)
            {
                var setItemStatusRequest = new SetItemStatusRequest
                {
                    OrderId = request.OrderId,
                    StatusId = newItemStatusId,
                    UserLoginId = request.UserLoginId,
                    StatusDateTime = DateTime.Now,
                    ChangeReason = request.ChangeReason
                };

                try
                {
                    var itemStatusResponse = await ChangeOrderItemStatus(setItemStatusRequest);
                    if (!itemStatusResponse.Success)
                    {
                        throw new Exception(
                            $"Error changing item status to {newItemStatusId}: {itemStatusResponse.Message}");
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error changing item status to {newItemStatusId}: {ex.Message}");
                }
            }
        }

        if (orderHeader.OrderTypeId == "PURCHASE_ORDER" &&
            orderHeader.StatusId == "ORDER_APPROVED")
        {
            // Trigger createPaymentFromOrder action

            var createPaymentFromOrderInput = new CreatePaymentFromOrderInput
            {
                OrderId = orderHeader.OrderId
            };
            await CreatePaymentFromOrder(createPaymentFromOrderInput);
        }

        if (orderHeader.OrderTypeId == "SALES_ORDER" &&
            request.StatusId == "ORDER_COMPLETED" &&
            request.StatusId != response.OldStatusId)
        {
            var createPaymentFromOrderInput = new CreatePaymentFromOrderInput
            {
                OrderId = orderHeader.OrderId
            };
            await CreatePaymentFromOrder(createPaymentFromOrderInput);
        }

        if (request.StatusId == "ORDER_COMPLETED" &&
            request.StatusId != response.OldStatusId)
        {
            await _invoiceHelperService.CreateInvoiceFromOrder(orderHeader.OrderId);

            await ResetGrandTotal(orderHeader.OrderId);
        }

        return response;
    }

    public async Task<ResetGrandTotalResult> ResetGrandTotal(string orderId)
    {
        // Retrieve the order header based on orderId.
        // Business: To obtain the order details for updating the grand total.
        // Technical: Using EF's FirstOrDefaultAsync to fetch the record.
        var orderHeader = await _context.OrderHeaders.FirstOrDefaultAsync(o => o.OrderId == orderId);
        if (orderHeader == null)
        {
            string errMsg = $"Order with id {orderId} not found.";
            return ResetGrandTotalResult.Failure(errMsg);
        }

        // Initialize OrderReadHelper with the orderId and set its context.
        // Business: This helper encapsulates various order calculations.
        // Technical: The helper is initialized and any necessary order data is loaded.
        var orh = new OrderReadHelper(orderId)
        {
            Context = _context
        };
        orh.InitializeOrder();

        // Retrieve current grand total and remaining sub-total from the order header.
        // Business: These values are used to determine if updates are needed.
        // Technical: They are nullable decimals.
        decimal? currentTotal = orderHeader.GrandTotal;
        decimal? currentSubTotal = orderHeader.RemainingSubTotal;

        // Compute the updated grand total using the helper.
        // Business: This represents the recalculated order total.
        // Technical: Await the asynchronous method call.
        decimal updatedTotal = await orh.GetOrderGrandTotal();

        // Retrieve the productStoreId from the order header.
        // Business: The product store determines pricing display (e.g., including VAT).
        // Technical: Simple property retrieval from the order header.
        string productStoreId = orderHeader.ProductStoreId;
        string showPricesWithVatTax = null;
        if (!string.IsNullOrEmpty(productStoreId))
        {
            // Retrieve the product store entity.
            // Business: To determine the display mode for prices.
            // Technical: Using EF to fetch the ProductStore record.
            var productStore = await _context.ProductStores
                .FirstOrDefaultAsync(p => p.ProductStoreId == productStoreId);
            if (productStore == null)
            {
                string errorMessage = $"Could not find product store with id {productStoreId}.";
                return ResetGrandTotalResult.Failure(errorMessage);
            }

            showPricesWithVatTax = productStore.ShowPricesWithVatTax;
        }

        // Calculate the new remaining sub-total.
        // Business: This is the net amount after accounting for returns and adjustments.
        // Technical: Two branches are defined based on the product store's pricing configuration.
        decimal remainingSubTotal = 0m;
        if (!string.IsNullOrEmpty(productStoreId) &&
            string.Equals(showPricesWithVatTax, "Y", StringComparison.OrdinalIgnoreCase))
        {
            // Branch 1: When prices are shown with VAT/tax included.
            // Calculation: grandTotal + taxes - (returnsTotal + shippingTotal)
            // Business: All returned items' amounts and full shipping charges are subtracted.
            decimal orderReturnedTotal = 0m;
            try
            {
                orderReturnedTotal = await orh.GetOrderReturnedTotal(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error calling GetOrderReturnedTotal: " + ex.Message);
                throw;
            }

            decimal shippingTotal = 0m;
            try
            {
                shippingTotal = await orh.GetShippingTotal();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error calling GetShippingTotal: " + ex.Message);
                throw;
            }

            remainingSubTotal = updatedTotal - orderReturnedTotal - shippingTotal;
        }
        else
        {
            // Branch 2: When prices are not displayed with VAT/tax included.
            // Calculation: grandTotal - returnsTotal - (tax + shipping of items not returned)
            // Business: Only non-returned items' tax and shipping adjustments are subtracted.
            decimal returnedTotal = 0m;
            try
            {
                returnedTotal = await orh.GetOrderReturnedTotal(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error calling GetOrderReturnedTotal: " + ex.Message);
                throw;
            }

            decimal nonReturnedTaxAndShipping = 0m;
            try
            {
                nonReturnedTaxAndShipping = await orh.GetOrderNonReturnedTaxAndShipping();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error calling GetOrderNonReturnedTaxAndShipping: " + ex.Message);
                throw;
            }

            // NOTE: We assign the result directly to remainingSubTotal (as in the Ofbiz version)
            remainingSubTotal = updatedTotal - returnedTotal - nonReturnedTaxAndShipping;
        }

        // Update the order header only if the totals have changed.
        // Business: Ensures that unnecessary writes to the database are avoided.
        // Technical: Compare updated values to the current values.
        if (!currentTotal.HasValue || !currentSubTotal.HasValue ||
            updatedTotal != currentTotal.Value || remainingSubTotal != currentSubTotal.Value)
        {
            orderHeader.GrandTotal = updatedTotal;
            orderHeader.RemainingSubTotal = remainingSubTotal;
        }

        return ResetGrandTotalResult.Success();
    }


    public async Task<SetItemStatusResponse> ChangeOrderItemStatus(SetItemStatusRequest request)
    {
        // Define the query condition based on the available parameters
        Func<IQueryable<OrderItem>, IQueryable<OrderItem>> additionalConditions = query =>
        {
            query = query.Where(o => o.OrderId == request.OrderId);

            // If OrderItemSeqId is provided, filter by it
            if (!string.IsNullOrEmpty(request.OrderItemSeqId))
            {
                query = query.Where(o => o.OrderItemSeqId == request.OrderItemSeqId);
            }

            // If FromStatusId is provided, filter by it; otherwise, exclude certain statuses
            if (!string.IsNullOrEmpty(request.FromStatusId))
            {
                query = query.Where(o => o.StatusId == request.FromStatusId);
            }
            else
            {
                query = query.Where(o => o.StatusId != "ITEM_COMPLETED" && o.StatusId != "ITEM_CANCELLED");
            }

            return query;
        };

        // Fetch the order items using FindLocalOrDatabaseListAsync
        var orderItems = await _utilityService.FindLocalOrDatabaseListAsync<OrderItem>(additionalConditions);

        if (!orderItems.Any())
        {
            return new SetItemStatusResponse
            {
                Success = false,
                Message = "No order items found or the status could not be changed."
            };
        }

        var statusDateTime = request.StatusDateTime ?? DateTime.Now;

        foreach (var orderItem in orderItems)
        {
            if (orderItem.StatusId == request.StatusId)
            {
                continue; // Status is the same, no need to change
            }

            // Check if the status change is valid
            var statusChange = await _context.StatusValidChanges
                .SingleOrDefaultAsync(s => s.StatusId == orderItem.StatusId && s.StatusIdTo == request.StatusId);

            if (statusChange == null)
            {
                _logger.LogWarning(
                    $"Invalid status change from {orderItem.StatusId} to {request.StatusId} for order item {orderItem.OrderItemSeqId}");
                continue; // Skip invalid status change
            }

            // Update the order item's status
            orderItem.StatusId = request.StatusId;

            // Create a new order status change record
            var orderStatus = new OrderStatus
            {
                OrderStatusId = Guid.NewGuid().ToString(),
                StatusId = request.StatusId,
                OrderId = request.OrderId,
                OrderItemSeqId = orderItem.OrderItemSeqId,
                ChangeReason = request.ChangeReason,
                StatusDatetime = statusDateTime,
                StatusUserLogin = request.UserLoginId
            };
            _context.OrderStatuses.Add(orderStatus);
        }

        // Check ECA rule condition and call CheckItemStatus if needed.
        // Business Purpose: Validates order header status after relevant item status changes.
        // Technical Purpose: Simulates OFBiz ECA rule by calling CheckItemStatus.
        if (request.StatusId is "ITEM_APPROVED" or "ITEM_CANCELLED" or "ITEM_COMPLETED")
        {
            // Call CheckItemStatus to validate order header status.
            // Business Purpose: Ensures order header status reflects item statuses.
            // Technical Purpose: Executes the ECA-triggered service.
            var checkResult = await CheckItemStatus(request.OrderId);

            // If CheckItemStatus returns an error, update the response.
            // Business Purpose: Propagates errors from order header validation.
            // Technical Purpose: Integrates CheckItemStatus outcome.
            if (!string.IsNullOrEmpty(checkResult.errorMessage))
            {
                return new SetItemStatusResponse
                {
                    Success = false,
                    Message =
                        $"Order item status updated, but order header validation failed: {checkResult.errorMessage}"
                };
            }
        }

        return new SetItemStatusResponse
        {
            Success = true,
            Message = "Order item status updated successfully."
        };
    }

    public async Task<List<OrderPaymentPreference>> MakeAllOrderPaymentInfos(OrderPaymentInfoInput input)
    {
        var allOpPrefs = new List<OrderPaymentPreference>();

        try
        {
            decimal remainingAmount = (decimal)(input.GrandTotal - input.PaymentTotal);

            // Ensure the amount is rounded to two decimal places
            remainingAmount = Math.Round(remainingAmount, 2);

            // Handling billing account logic
            if (!string.IsNullOrEmpty(input.BillingAccountId) && input.BillingAccountAmt <= 0)
            {
                try
                {
                    decimal billingAccountAvailableAmount = await AvailableAccountBalance(input.BillingAccountId);

                    // Adjust billing account amount based on available balance
                    if (input.BillingAccountAmt == 0 && billingAccountAvailableAmount > 0)
                    {
                        input.BillingAccountAmt = billingAccountAvailableAmount;
                    }

                    if (remainingAmount < input.BillingAccountAmt)
                    {
                        input.BillingAccountAmt = remainingAmount;
                    }

                    if (billingAccountAvailableAmount < input.BillingAccountAmt)
                    {
                        input.BillingAccountAmt = billingAccountAvailableAmount;
                    }
                }
                catch (Exception ex)
                {
                    // Log and rethrow to ensure billing account exceptions are handled
                    _logger.LogError(
                        $"Error handling billing account logic for BillingAccountId {input.BillingAccountId}: {ex.Message}");
                    throw new InvalidOperationException(
                        $"Failed to handle billing account for ID {input.BillingAccountId}.", ex);
                }
            }

            // Process payment information
            foreach (var inf in input.PaymentInfo)
            {
                try
                {
                    // If BillingAccountId is provided and BillingAccountAmt > 0, set Amount to BillingAccountAmt
                    if (!string.IsNullOrEmpty(input.BillingAccountId) && input.BillingAccountAmt > 0)
                    {
                        inf.Amount = input.BillingAccountAmt;
                        remainingAmount -=
                            (decimal)input.BillingAccountAmt; // Adjust remaining amount after assigning to payment
                    }
                    else if (inf.Amount == null)
                    {
                        // If no billing account is used and the amount is not set, assign the remaining amount
                        inf.Amount = remainingAmount;
                        remainingAmount = 0; // Set remaining amount to zero after assignment
                    }

                    var paymentInfos = await MakeOrderPaymentInfos(inf, input.OrderId);
                    allOpPrefs.AddRange(paymentInfos);
                }
                catch (Exception ex)
                {
                    // Log and rethrow to handle payment information errors
                    _logger.LogError(
                        $"Error processing payment information for PaymentMethodId {inf.PaymentMethodId}: {ex.Message}");
                    throw new InvalidOperationException(
                        $"Failed to process payment information for PaymentMethodId {inf.PaymentMethodId}.", ex);
                }
            }
        }
        catch (Exception ex)
        {
            // Log and throw a general exception for unexpected errors
            _logger.LogError($"Unexpected error in MakeAllOrderPaymentInfos: {ex.Message}");
            throw new InvalidOperationException("An error occurred while creating order payment preferences.", ex);
        }

        return allOpPrefs;
    }

    public async Task<List<OrderPaymentPreference>> MakeOrderPaymentInfos(CartPaymentInfo paymentInfo, string orderId)
    {
        var values = new List<OrderPaymentPreference>();

        try
        {
            // Fetch value object directly from the appropriate table based on available IDs
            object valueObj = null;

            try
            {
                if (!string.IsNullOrEmpty(paymentInfo.PaymentMethodId))
                {
                    // Query PaymentMethod table when paymentMethodId is present
                    valueObj = await _context.PaymentMethods
                        .FindAsync(paymentInfo.PaymentMethodId);
                }
                else if (!string.IsNullOrEmpty(paymentInfo.PaymentMethodTypeId))
                {
                    // Query PaymentMethodType table when paymentMethodTypeId is present
                    valueObj = await _context.PaymentMethodTypes
                        .FirstOrDefaultAsync(pmt => pmt.PaymentMethodTypeId == paymentInfo.PaymentMethodTypeId);
                }
                else
                {
                    throw new ArgumentException(
                        "Could not create value object because paymentMethodId and paymentMethodTypeId are null");
                }
            }
            catch (Exception ex)
            {
                // Log and rethrow to handle errors during value object retrieval
                _logger.LogError(
                    $"Error retrieving value object for PaymentMethodId: {paymentInfo.PaymentMethodId}, PaymentMethodTypeId: {paymentInfo.PaymentMethodTypeId}: {ex.Message}");
                throw new InvalidOperationException("Failed to retrieve payment method or payment type.", ex);
            }

            // Proceed only if the value object is found
            if (valueObj != null)
            {
                try
                {
                    var newOrderPaymentPreferenceSerial =
                        await _utilityService.GetNextSequence("OrderPaymentPreference");

                    // Creating OrderPaymentPreference record
                    var opp = new OrderPaymentPreference
                    {
                        OrderPaymentPreferenceId = newOrderPaymentPreferenceSerial,
                        OrderId = orderId,
                        PaymentMethodTypeId = valueObj is PaymentMethod
                            ? ((PaymentMethod)valueObj).PaymentMethodTypeId
                            : ((PaymentMethodType)valueObj).PaymentMethodTypeId,
                        PaymentMethodId = paymentInfo.PaymentMethodId,
                        FinAccountId = paymentInfo.FinAccountId,
                        BillingPostalCode = paymentInfo.BillingPostalCode,
                        MaxAmount = paymentInfo.Amount ?? 0, // Using Amount from paymentInfo instead of maxAmount logic
                        CreatedDate = DateTime.UtcNow,
                        CreatedStamp = DateTime.UtcNow,
                        LastUpdatedStamp = DateTime.UtcNow
                    };

                    // Setting additional fields if present
                    if (paymentInfo.RefNum != null && paymentInfo.RefNum.Length > 0)
                    {
                        opp.ManualRefNum = paymentInfo.RefNum[0];
                        opp.ManualAuthCode = paymentInfo.RefNum.Length > 1 ? paymentInfo.RefNum[1] : null;
                    }

                    if (!string.IsNullOrEmpty(paymentInfo.SecurityCode))
                    {
                        opp.SecurityCode = paymentInfo.SecurityCode;
                    }

                    if (!string.IsNullOrEmpty(paymentInfo.Track2))
                    {
                        opp.Track2 = paymentInfo.Track2;
                    }

                    // Setting status based on payment method type
                    if (!string.IsNullOrEmpty(paymentInfo.PaymentMethodId) ||
                        paymentInfo.PaymentMethodTypeId == "FIN_ACCOUNT")
                    {
                        opp.StatusId = "PAYMENT_NOT_AUTH";
                    }
                    else if (!string.IsNullOrEmpty(paymentInfo.PaymentMethodTypeId))
                    {
                        opp.StatusId = paymentInfo.PaymentMethodTypeId.StartsWith("EXT_")
                            ? "PAYMENT_NOT_RECEIVED"
                            : "PAYMENT_RECEIVED";
                    }

                    _context.OrderPaymentPreferences.Add(opp);
                    values.Add(opp);
                }
                catch (Exception ex)
                {
                    // Log and rethrow to handle errors during OrderPaymentPreference creation
                    _logger.LogError(
                        $"Error creating OrderPaymentPreference for PaymentMethodId: {paymentInfo.PaymentMethodId}: {ex.Message}");
                    throw new InvalidOperationException("Failed to create OrderPaymentPreference.", ex);
                }
            }
            else
            {
                _logger.LogWarning(
                    $"Value object not found for PaymentMethodId: {paymentInfo.PaymentMethodId}, PaymentMethodTypeId: {paymentInfo.PaymentMethodTypeId}");
            }
        }
        catch (Exception ex)
        {
            // Log and throw a general exception for unexpected errors
            _logger.LogError($"Unexpected error in MakeOrderPaymentInfos: {ex.Message}");
            throw new InvalidOperationException("An error occurred while creating order payment preferences.", ex);
        }

        return values;
    }

    public async Task<decimal> AvailableAccountBalance(string billingAccountId)
    {
        // Return zero if billing account ID is null or empty
        if (string.IsNullOrEmpty(billingAccountId))
        {
            return 0m;
        }

        try
        {
            // Call the service function to calculate the billing account balance
            var result = await CalcBillingAccountBalance(billingAccountId);

            // Check if the BillingAccountBalanceResult indicates an error by checking if the BillingAccount is null
            if (result.BillingAccount == null)
            {
                // Log an error message if the BillingAccount is not found or an error occurred
                _logger.LogError("Failed to calculate billing account balance for ID: {BillingAccountId}",
                    billingAccountId);
                return 0m;
            }

            // Retrieve the available balance from the result
            decimal availableBalance = result.AvailableBalance;
            return availableBalance;
        }
        catch (Exception ex)
        {
            // Log exception message
            _logger.LogError("Exception while calculating available account balance: {ExceptionMessage}", ex.Message);
            return 0m;
        }
    }


    public async Task<BillingAccountBalanceResult> CalcBillingAccountBalance(string billingAccountId)
    {
        // Check for null or empty billing account ID
        if (string.IsNullOrEmpty(billingAccountId))
        {
            _logger.LogWarning("Billing account ID is null or empty.");
            return new BillingAccountBalanceResult
            {
                AccountBalance = 0,
                NetAccountBalance = 0,
                AvailableBalance = 0,
                AvailableToCapture = 0,
                BillingAccount = null // No account found
            };
        }

        try
        {
            // Fetch the billing account from the database
            var billingAccount = await _context.BillingAccounts
                .FirstOrDefaultAsync(ba => ba.BillingAccountId == billingAccountId);

            // Check if the billing account exists
            if (billingAccount == null)
            {
                _logger.LogWarning($"Billing account with ID {billingAccountId} not found.");
                return new BillingAccountBalanceResult
                {
                    AccountBalance = 0,
                    NetAccountBalance = 0,
                    AvailableBalance = 0,
                    AvailableToCapture = 0,
                    BillingAccount = null
                };
            }

            try
            {
                // Calculate balances
                var accountBalance = await GetBillingAccountBalance(billingAccount);
                var netAccountBalance = await GetBillingAccountNetBalance(billingAccountId);
                var availableBalance = await GetBillingAccountAvailableBalance(billingAccount);
                var availableToCapture = await AvailableToCapture(billingAccount);

                return new BillingAccountBalanceResult
                {
                    AccountBalance = accountBalance,
                    NetAccountBalance = netAccountBalance,
                    AvailableBalance = availableBalance,
                    AvailableToCapture = availableToCapture,
                    BillingAccount = billingAccount // Returning the retrieved billing account
                };
            }
            catch (Exception balanceEx)
            {
                // Log error if balance calculations fail
                _logger.LogError(
                    $"Error calculating balances for billing account ID {billingAccountId}: {balanceEx.Message}");
                throw new InvalidOperationException("Failed to calculate billing account balances.", balanceEx);
            }
        }
        catch (Exception ex)
        {
            // Log the error
            _logger.LogError($"Error calculating billing account balance for ID {billingAccountId}: {ex.Message}");

            // Return a default result in case of an error
            return new BillingAccountBalanceResult
            {
                AccountBalance = 0,
                NetAccountBalance = 0,
                AvailableBalance = 0,
                AvailableToCapture = 0,
                BillingAccount = null
            };
        }
    }

    /// <summary>
    /// Calculates the amount that could be captured from a billing account, defined as the account limit minus the net balance.
    /// </summary>
    /// <param name="billingAccount">The billing account entity.</param>
    /// <returns>Returns the amount available to capture from the billing account.</returns>
    public async Task<decimal> AvailableToCapture(BillingAccount billingAccount)
    {
        // Check if billing account and account limit are not null
        if (billingAccount != null && billingAccount.AccountLimit.HasValue)
        {
            try
            {
                // Retrieve the net balance of the billing account
                decimal netBalance = await GetBillingAccountNetBalance(billingAccount.BillingAccountId);

                // Retrieve the account limit from the billing account
                decimal accountLimit = billingAccount.AccountLimit.Value;

                // Calculate the available amount to capture
                decimal availableToCapture = accountLimit - netBalance;

                // Round the result to two decimal places
                availableToCapture = Math.Round(availableToCapture, 2, MidpointRounding.AwayFromZero);

                return availableToCapture;
            }
            catch (Exception ex)
            {
                // Log the error if something goes wrong during the calculation
                _logger.LogError(
                    "Error calculating available to capture for billing account ID {BillingAccountId}: {ErrorMessage}",
                    billingAccount.BillingAccountId, ex.Message);
            }
        }
        else
        {
            // Log a warning if the billing account or its account limit is undefined
            _logger.LogWarning(
                "Available to capture requested for null billing account or undefined account limit, returning zero.");
        }

        return 0m; // Return zero if the billing account is null or account limit is undefined
    }


    /// <summary>
    /// Returns the amount which could be charged to a billing account, defined as the account limit minus account balance and minus the balance of outstanding orders.
    /// When trying to determine how much of a billing account can be used to pay for an outstanding order, use this method.
    /// </summary>
    /// <param name="billingAccount">The billing account entity.</param>
    /// <returns>Returns the amount which could be charged to a billing account.</returns>
    public async Task<decimal> GetBillingAccountAvailableBalance(BillingAccount billingAccount)
    {
        // Check if billing account and its account limit are not null
        if (billingAccount != null && billingAccount.AccountLimit.HasValue)
        {
            try
            {
                // Retrieve the account limit
                decimal accountLimit = billingAccount.AccountLimit.Value;

                // Get the current balance of the billing account
                decimal currentBalance = await GetBillingAccountBalance(billingAccount);

                // Calculate the available balance
                decimal availableBalance = accountLimit - currentBalance;

                // Round the result to two decimal places
                availableBalance = Math.Round(availableBalance, 2, MidpointRounding.AwayFromZero);

                return availableBalance;
            }
            catch (Exception ex)
            {
                // Log the error if something goes wrong during the available balance calculation
                _logger.LogWarning(
                    "Error calculating available balance for billing account ID {BillingAccountId}: {ErrorMessage}",
                    billingAccount.BillingAccountId, ex.Message);
            }
        }
        else
        {
            // Log a warning if the billing account or its limit is null
            _logger.LogWarning("Available balance requested for null or undefined account limit, returning zero.");
        }

        return 0m; // Return zero if the billing account is null or account limit is undefined
    }


    /// <summary>
    /// Calculates the net balance of a billing account, which is the sum of all amounts applied to invoices minus the sum of all amounts applied from payments.
    /// When charging or capturing an invoice to a billing account, use this method.
    /// </summary>
    /// <param name="billingAccountId">The billing account ID.</param>
    /// <returns>The net amount of the billing account that could be captured.</returns>
    public async Task<decimal> GetBillingAccountNetBalance(string billingAccountId)
    {
        decimal balance = 0m; // Initial balance set to zero.

        try
        {
            // Fetch all PaymentApplications associated with the given billing account ID.
            var paymentApplications = await _context.PaymentApplications
                .Where(pa => pa.BillingAccountId == billingAccountId)
                .ToListAsync();

            // Iterate through each payment application to adjust the balance.
            foreach (var paymentApplication in paymentApplications)
            {
                // Get the amount applied in each payment application.
                var amountApplied = paymentApplication.AmountApplied ?? 0m;

                // Attempt to fetch the related invoice (if any).
                var invoice = await _context.Invoices
                    .FirstOrDefaultAsync(inv => inv.InvoiceId == paymentApplication.InvoiceId);

                if (invoice != null)
                {
                    // Ensure the invoice is not cancelled and is not a "Customer return invoice".
                    if (invoice.InvoiceTypeId != "CUST_RTN_INVOICE" && invoice.StatusId != "INVOICE_CANCELLED")
                    {
                        // Add the amount to the balance if the invoice is valid.
                        balance += amountApplied;
                    }
                }
                else
                {
                    // Subtract the amount if the invoice is not found.
                    balance -= amountApplied;
                }
            }

            // Set the balance to a specific scale and rounding.
            balance = Math.Round(balance, 2, MidpointRounding.AwayFromZero);
        }
        catch (Exception ex)
        {
            // Log the error if something goes wrong during the balance calculation.
            _logger.LogError("Error calculating billing account net balance for ID {BillingAccountId}: {ErrorMessage}",
                billingAccountId, ex.Message);
        }

        return balance; // Return the calculated net balance.
    }


    public decimal GetAccountLimit(BillingAccount billingAccount)
    {
        // Check if the account limit is defined
        if (billingAccount.AccountLimit.HasValue)
        {
            return billingAccount.AccountLimit.Value;
        }

        // Log a warning if the account limit is not defined
        Console.WriteLine(
            $"Warning: Billing Account [{billingAccount.BillingAccountId}] does not have an account limit defined, assuming zero.");

        // Return zero as the default account limit
        return 0m;
    }


    public async Task<decimal> GetBillingAccountBalance(BillingAccount billingAccount)
    {
        // Initialize balance
        decimal balance = 0m;

        try
        {
            // Step 1: Get the account limit (mirror getAccountLimit in Ofbiz)
            decimal accountLimit = GetAccountLimit(billingAccount); // <-- Implement this method as needed.
            balance = accountLimit;

            /* 
             * Step 2: Query pending (not cancelled, rejected, or received) order payments.
             * This mirrors the Ofbiz view OrderPurchasePaymentSummary:
             * - Join OrderHeader (OH), OrderPaymentPreference (OPP), and PaymentMethodType (PMT)
             * - Filter on:
             *    OH.billingAccountId equals the billing account's ID.
             *    OPP.paymentMethodTypeId equals "EXT_BILLACT".
             *    OH.statusId NOT IN ["ORDER_CANCELLED", "ORDER_REJECTED"].
             *    OPP.statusId (preferenceStatusId) NOT IN ["PAYMENT_SETTLED", "PAYMENT_RECEIVED", "PAYMENT_DECLINED", "PAYMENT_CANCELLED"].
             * - Sum the OPP.maxAmount (the view uses a sum function on maxAmount).
             */

            var pendingPaymentsQuery =
                from oh in _context.OrderHeaders
                join opp in _context.OrderPaymentPreferences on oh.OrderId equals opp.OrderId
                join pmt in _context.PaymentMethodTypes on opp.PaymentMethodTypeId equals pmt.PaymentMethodTypeId
                where oh.BillingAccountId == billingAccount.BillingAccountId
                      && opp.PaymentMethodTypeId == "EXT_BILLACT"
                      && !new[] { "ORDER_CANCELLED", "ORDER_REJECTED" }.Contains(oh.StatusId)
                      && !new[] { "PAYMENT_SETTLED", "PAYMENT_RECEIVED", "PAYMENT_DECLINED", "PAYMENT_CANCELLED" }
                          .Contains(opp.StatusId)
                select (opp.MaxAmount);


            decimal pendingPaymentsSum = await pendingPaymentsQuery.SumAsync();


            // Subtract the summed pending payments from the account limit
            balance -= pendingPaymentsSum;

            /*
             * Step 3: Query Payment Applications.
             * For each payment application record that does not have an associated invoice,
             * add the applied amount back to the balance.
             */
            var paymentApplications = await _context.PaymentApplications
                .Where(pa => pa.BillingAccountId == billingAccount.BillingAccountId)
                .ToListAsync();

            foreach (var paymentAppl in paymentApplications)
            {
                if (string.IsNullOrEmpty(paymentAppl.InvoiceId))
                {
                    balance += paymentAppl.AmountApplied ?? 0m;
                }
            }

            // Step 4: Adjust the balance's scale (precision) and rounding (e.g., 2 decimal places, AwayFromZero)
            balance = Math.Round(balance, 2, MidpointRounding.AwayFromZero);
        }
        catch (Exception ex)
        {
            // Log the error as needed (for example, using your logging framework)
            Console.Error.WriteLine($"Error fetching billing account balance: {ex.Message}");
        }

        return balance;
    }

    public async Task<OperationResult> ChangeOrderPaymentStatus(string orderPaymentPreferenceId,
        string? changeReason = null)
    {
        try
        {
            // Retrieve the OrderPaymentPreference record based on the provided ID
            var orderPaymentPreference = await _context.OrderPaymentPreferences
                .FirstOrDefaultAsync(op => op.OrderPaymentPreferenceId == orderPaymentPreferenceId);

            if (orderPaymentPreference == null)
            {
                return OperationResult.CreateFailure("Order Payment Preference could not be found.");
            }

            string orderId = orderPaymentPreference.OrderId;
            string statusUserLogin = orderPaymentPreference.CreatedByUserLogin;

            // Retrieve the OrderHeader record based on the Order ID
            var orderHeader = await _context.OrderHeaders
                .FirstOrDefaultAsync(oh => oh.OrderId == orderId);

            if (orderHeader == null)
            {
                return OperationResult.CreateFailure("Order could not be found.");
            }

            string statusId = orderPaymentPreference.StatusId;

            // Create a new OrderStatus record
            var newOrderStatus = new OrderStatus
            {
                StatusId = statusId,
                OrderId = orderId,
                OrderPaymentPreferenceId = orderPaymentPreferenceId,
                StatusUserLogin = statusUserLogin,
                ChangeReason = changeReason,
            };

            // Retrieve the previous OrderStatus to check if the status has changed
            var previousStatus = await _context.OrderStatuses
                .Where(os => os.OrderId == orderId && os.OrderPaymentPreferenceId == orderPaymentPreferenceId)
                .OrderByDescending(os => os.StatusDatetime)
                .FirstOrDefaultAsync();

            if (previousStatus != null)
            {
                // Temporarily set some values on the new status for comparison
                newOrderStatus.OrderStatusId = previousStatus.OrderStatusId;
                newOrderStatus.StatusDatetime = previousStatus.StatusDatetime;

                if (newOrderStatus.Equals(previousStatus))
                {
                    // Status is the same, return without creating a new record
                    return OperationResult.CreateSuccess();
                }
            }

            // If the status has changed, create a new OrderStatus record
            newOrderStatus.OrderStatusId = Guid.NewGuid().ToString(); // Generate a new unique identifier
            newOrderStatus.StatusDatetime = DateTime.UtcNow;

            _context.OrderStatuses.Add(newOrderStatus);

            return OperationResult.CreateSuccess();
        }
        catch (Exception ex)
        {
            return OperationResult.CreateFailure($"Could not change order status: {ex.Message}");
        }
    }


    public async Task<string> ReceiveOfflinePayment(ReceiveOfflinePaymentInput input)
    {
        try
        {
            // Retrieve the order header
            var orderHeader = await _context.OrderHeaders.FirstOrDefaultAsync(o => o.OrderId == input.OrderId);
            if (orderHeader == null)
            {
                _logger.LogError("Order header not found for orderId: {0}", input.OrderId);
                return "OrderProblemsReadingOrderHeaderInformation";
            }

            decimal grandTotal = orderHeader.GrandTotal ?? 0;

            // Retrieve payment method types (excluding "EXT_OFFLINE")
            var paymentMethodTypes = await _context.PaymentMethodTypes
                .Where(pmt => pmt.PaymentMethodTypeId != "EXT_OFFLINE")
                .ToListAsync();

            if (paymentMethodTypes == null || paymentMethodTypes.Count == 0)
            {
                _logger.LogError("No payment method types found.");
                return "OrderProblemsWithPaymentTypeLookup";
            }

            var paymentMethods = await _context.PaymentMethods
                .Where(pm => pm.PartyId == input.PartyId)
                .ToListAsync();


            // Retrieve the placing customer for the order
            var placingCustomer = await _context.OrderRoles
                .FirstOrDefaultAsync(or => or.OrderId == input.OrderId && or.RoleTypeId == "PLACING_CUSTOMER");

            if (placingCustomer == null)
            {
                _logger.LogError("Placing customer not found for orderId: {0}", input.OrderId);
                return "OrderErrorProcessingOfflinePayments";
            }

            // Process payment methods from input (PaymentMethodId and PaymentMethodTypeId)
            List<OrderPaymentPreference> toBeStored = new List<OrderPaymentPreference>();

            foreach (var paymentMethod in paymentMethods)
            {
                var paymentMethodId = paymentMethod.PaymentMethodId;
                var paymentDetail = input.PaymentDetails
                    .FirstOrDefault(pd => pd.PaymentMethodId == paymentMethodId);

                if (paymentDetail != null && paymentDetail.Amount > 0)
                {
                    try
                    {
                        // Create payment amount string (similar to how Ofbiz checks the parameter value)
                        var paymentAmountStr = paymentDetail.Amount.ToString();
                        if (string.IsNullOrEmpty(paymentAmountStr))
                        {
                            _logger.LogError("Payment amount is empty for PaymentMethodId: {0}", paymentMethodId);
                            return "OrderProblemsPaymentParsingAmount";
                        }

                        // Convert payment amount to a decimal
                        decimal paymentAmount = 0;
                        try
                        {
                            paymentAmount = Convert.ToDecimal(paymentAmountStr);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError("Error parsing payment amount for PaymentMethodId: {0}. Exception: {1}",
                                paymentMethodId, ex.Message);
                            return "OrderProblemsPaymentParsingAmount";
                        }

                        if (paymentAmount > 0)
                        {
                            // Call createPaymentFromOrder service, equivalent to the Ofbiz runSync method
                            var createPaymentFromOrderInput = new CreatePaymentFromOrderInput
                            {
                                OrderId = input.OrderId,
                                PaymentMethodId = paymentMethodId,
                                PaymentRefNum = paymentDetail.Reference,
                                Comments = "Payment received offline and manually entered."
                            };

                            var results = await CreatePaymentFromOrder(createPaymentFromOrderInput);

                            if (!results.IsSuccess)
                            {
                                _logger.LogError("Failed to create payment from order for PaymentMethodId: {0}",
                                    paymentMethodId);
                                return "OrderProblemsPaymentProcessing";
                            }

                            // Approve the order (similar to calling OrderChangeHelper.approveOrder in Ofbiz)
                            await ApproveOrder(input.OrderId, true);

                            // Exit the loop successfully
                            return "success";
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("Failed to process payment for PaymentMethodId: {0}. Exception: {1}",
                            paymentMethodId, ex.Message);
                        return "OrderProblemsPaymentProcessing";
                    }
                }
            }

            foreach (var paymentMethodType in paymentMethodTypes)
            {
                var paymentMethodTypeId = paymentMethodType.PaymentMethodTypeId;
                var paymentDetail =
                    input.PaymentDetails.FirstOrDefault(pd => pd.PaymentMethodTypeId == paymentMethodTypeId);
                var amountStr = paymentDetail?.Amount.ToString();
                var paymentReference = paymentDetail?.Reference;

                if (!string.IsNullOrEmpty(amountStr))
                {
                    decimal paymentTypeAmount = 0;
                    try
                    {
                        paymentTypeAmount = Convert.ToDecimal(amountStr);
                    }
                    catch (Exception)
                    {
                        _logger.LogError("Error parsing payment amount for PaymentMethodTypeId: {0}",
                            paymentMethodTypeId);
                        return "OrderProblemsPaymentParsingAmount";
                    }

                    if (paymentTypeAmount > 0)
                    {
                        var newOrderPaymentPreferenceSequence =
                            await _utilityService.GetNextSequence("OrderPaymentPreference");

                        // Create the OrderPaymentPreference
                        var newPaymentPreference = new OrderPaymentPreference
                        {
                            OrderPaymentPreferenceId = newOrderPaymentPreferenceSequence,
                            OrderId = input.OrderId,
                            PaymentMethodTypeId = paymentMethodTypeId,
                            MaxAmount = paymentTypeAmount,
                            StatusId = "PAYMENT_RECEIVED",
                            CreatedDate = DateTime.UtcNow,
                            CreatedStamp = DateTime.UtcNow,
                            LastUpdatedStamp = DateTime.UtcNow
                        };

                        _context.OrderPaymentPreferences.Add(newPaymentPreference);
                        toBeStored.Add(newPaymentPreference);

                        // Create a payment record
                        var createPaymentFromPreferenceInput = new CreatePaymentFromPreferenceInput
                        {
                            OrderPaymentPreferenceId = newPaymentPreference.OrderPaymentPreferenceId,
                            PaymentRefNum = paymentReference,
                            PaymentFromId = input.PartyId, // Assuming this is the placing customer
                            Comments = "Payment received offline and manually entered.",
                            EventDate = DateTime.UtcNow
                        };

                        var createPaymentResult = await CreatePaymentFromPreference(createPaymentFromPreferenceInput);
                        if (!createPaymentResult.IsSuccess)
                        {
                            _logger.LogError("Failed to create payment from preference for PaymentMethodTypeId: {0}",
                                paymentMethodTypeId);
                            return "OrderProblemsPaymentProcessing";
                        }
                    }
                }
            }


            // Retrieve the current payment preferences
            OrderPaymentPreference offlineValue = null;
            List<OrderPaymentPreference> currentPrefs = null;
            decimal paymentTally = 0;

            try
            {
                currentPrefs = await _context.OrderPaymentPreferences
                    .Where(opp => opp.OrderId == input.OrderId && opp.StatusId != "PAYMENT_CANCELLED")
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError("ERROR: Unable to get existing payment preferences from order: {0}", ex.Message);
            }

            if (currentPrefs != null && currentPrefs.Any())
            {
                foreach (var cp in currentPrefs)
                {
                    string paymentMethodType = cp.PaymentMethodTypeId;
                    if ("EXT_OFFLINE".Equals(paymentMethodType))
                    {
                        offlineValue = cp;
                    }
                    else
                    {
                        if (cp.MaxAmount != 0)
                        {
                            paymentTally += cp.MaxAmount;
                        }
                    }
                }
            }

            // Finalizing the payment process
            bool okayToApprove = false;
            if (paymentTally >= grandTotal)
            {
                // Cancel the offline preference
                okayToApprove = true;
                if (offlineValue != null)
                {
                    offlineValue.StatusId = "PAYMENT_CANCELLED";
                    toBeStored.Add(offlineValue);
                }
            }

            // Store the status changes and the newly created payment preferences
            try
            {
                _context.OrderPaymentPreferences.UpdateRange(toBeStored);
            }
            catch (Exception ex)
            {
                _logger.LogError("Problems storing payment information: {0}", ex.Message);
                return "OrderProblemStoringReceivedPaymentInformation";
            }

            if (okayToApprove)
            {
                // Approve the order and update its status
                await ApproveOrder(input.OrderId, true);
            }

            return "success";
        }
        catch (Exception ex)
        {
            _logger.LogError("Unexpected error in ReceiveOfflinePaymentAsync: {0}", ex.Message);
            return "OrderErrorProcessingOfflinePayments";
        }
    }

    public async Task<CreatePaymentFromOrderResult> CreatePaymentFromOrder(CreatePaymentFromOrderInput input)
    {
        var result = new CreatePaymentFromOrderResult();

        try
        {
            // Step 1: Fetch the order header
            var orderHeader = await _context.OrderHeaders.FirstOrDefaultAsync(o => o.OrderId == input.OrderId);
            if (orderHeader == null)
            {
                _logger.LogError("Order not found for orderId: {0}", input.OrderId);
                result.Message = "OrderProblemsReadingOrderHeaderInformation";
                return result;
            }

            // Step 2: Check if an OrderPaymentPreference already exists using OrderPaymentPreference and Payment tables
            var existingPaymentPrefs = await (
                from opp in _context.OrderPaymentPreferences
                join pmt in _context.Payments on opp.OrderPaymentPreferenceId equals pmt.PaymentPreferenceId into
                    payments
                from pmt in payments
                where opp.OrderId == input.OrderId && opp.StatusId != "PAYMENT_CANCELLED"
                select new
                {
                    opp.OrderPaymentPreferenceId,
                    opp.OrderId,
                    opp.StatusId,
                    PaymentId = pmt != null ? pmt.PaymentId : null,
                    PaymentTypeId = pmt != null ? pmt.PaymentTypeId : null,
                    Amount = pmt != null ? pmt.Amount : (decimal?)null
                }
            ).ToListAsync();

            if (existingPaymentPrefs.Any())
            {
                _logger.LogInformation("Payment not created for order {0}, at least a single payment already exists.",
                    input.OrderId);
                return result;
            }

            // Step 3: Retrieve order roles
            var orderRoleTo = await _context.OrderRoles.FirstOrDefaultAsync(or =>
                or.OrderId == input.OrderId && or.RoleTypeId == "BILL_FROM_VENDOR");
            var orderRoleFrom = await _context.OrderRoles.FirstOrDefaultAsync(or =>
                or.OrderId == input.OrderId && or.RoleTypeId == "BILL_TO_CUSTOMER");

            if (orderRoleTo == null || orderRoleFrom == null)
            {
                _logger.LogError("Order roles not found for orderId: {0}", input.OrderId);
                result.Message = "OrderErrorProcessingOrderRoles";
                return result;
            }

            // Step 4: Handle agreements and determine payment type
            string paymentTypeId;
            string organizationPartyId;
            List<Agreement> agreementList = null;

            if (orderHeader.OrderTypeId == "PURCHASE_ORDER")
            {
                // Query for purchase agreements
                agreementList = await _context.Agreements
                    .Where(a => a.PartyIdFrom == orderRoleFrom.PartyId
                                && a.PartyIdTo == orderRoleTo.PartyId
                                && a.AgreementTypeId == "PURCHASE_AGREEMENT"
                                && a.FromDate <= DateTime.UtcNow
                                && (a.ThruDate == null || a.ThruDate >= DateTime.UtcNow))
                    .ToListAsync();


                paymentTypeId = "VENDOR_PAYMENT";
                organizationPartyId = orderRoleFrom.PartyId;
            }
            else
            {
                // Query for sales agreements
                agreementList = await _context.Agreements
                    .Where(a => a.PartyIdFrom == orderRoleFrom.PartyId
                                && a.PartyIdTo == orderRoleTo.PartyId
                                && a.AgreementTypeId == "SALES_AGREEMENT"
                                && a.FromDate <= DateTime.UtcNow
                                && (a.ThruDate == null || a.ThruDate >= DateTime.UtcNow))
                    .ToListAsync();
                paymentTypeId = "CUSTOMER_PAYMENT";
                organizationPartyId = orderRoleTo.PartyId;
            }

            // Step 5: Handle payment terms if agreements exist
            if (agreementList.Any())
            {
                var orderTerm = await _context.OrderTerms.FirstOrDefaultAsync(ot =>
                    ot.OrderId == input.OrderId && ot.TermTypeId == "FIN_PAYMENT_TERM");
                if (orderTerm?.TermDays != null)
                {
                    input.EffectiveDate = DateTime.UtcNow.AddDays(orderTerm.TermDays.Value);
                }
                else
                {
                    input.EffectiveDate = DateTime.UtcNow;
                }
            }

            // Step 6: Handle currency conversion
            var partyAcctgPreference = await _acctgMiscService.GetPartyAccountingPreferences(organizationPartyId);
            decimal amount = orderHeader.GrandTotal ?? 0;
            string currencyUomId = orderHeader.CurrencyUom;
            decimal actualCurrencyAmount = amount;
            string actualCurrencyUomId = currencyUomId;

            if (partyAcctgPreference?.BaseCurrencyUomId != null &&
                orderHeader.CurrencyUom != partyAcctgPreference.BaseCurrencyUomId)
            {
                // Perform currency conversion
                var invoice = await (from oib in _context.OrderItemBillings
                    join inv in _context.Invoices on oib.InvoiceId equals inv.InvoiceId
                    where oib.OrderId == input.OrderId
                    select inv).FirstOrDefaultAsync();

                DateTime? conversionDate = invoice?.InvoiceDate;

                amount = (decimal)await _commonService.ConvertUom(orderHeader.CurrencyUom,
                    partyAcctgPreference.BaseCurrencyUomId, DateTime.UtcNow, orderHeader.GrandTotal ?? 0, null);

                currencyUomId = partyAcctgPreference.BaseCurrencyUomId;
                actualCurrencyAmount = orderHeader.GrandTotal ?? 0;
                actualCurrencyUomId = orderHeader.CurrencyUom;
            }

            // Step 7: Create payment parameters and process the payment
            var newPaymentSequence = await _utilityService.GetNextSequence("Payment");

            var createPaymentParam = new CreatePaymentParam
            {
                PaymentId = newPaymentSequence,
                PaymentTypeId = paymentTypeId,
                PartyIdFrom = orderRoleFrom.PartyId,
                PartyIdTo = orderRoleTo.PartyId,
                StatusId = "PMNT_NOT_PAID",
                PaymentMethodId = input.PaymentMethodId,
                PaymentMethodTypeId = "COMPANY_ACCOUNT",
                PaymentPreferenceId = null,
                EffectiveDate = input.EffectiveDate,
                Amount = amount
            };
            var payment = await _paymentHelperService.CreatePayment(createPaymentParam);

            // Step 8: Create Order Payment Preference Inline
            var newOrderPaymentPreferenceSequence = await _utilityService.GetNextSequence("OrderPaymentPreference");
            var orderPaymentPreference = new OrderPaymentPreference
            {
                OrderPaymentPreferenceId = newOrderPaymentPreferenceSequence,
                OrderId = input.OrderId,
                PaymentMethodId = input.PaymentMethodId,
                PaymentMethodTypeId = "COMPANY_ACCOUNT",
                MaxAmount = amount,
                StatusId = "PMNT_NOT_PAID",
                CreatedDate = DateTime.UtcNow,
                CreatedStamp = DateTime.UtcNow,
                LastUpdatedStamp = DateTime.UtcNow
            };

            _context.OrderPaymentPreferences.Add(orderPaymentPreference);

            // Step 9: Update the payment with payment preference details
            var updatePaymentParams = new CreatePaymentParam
            {
                PaymentId = payment.PaymentId,
                PaymentMethodTypeId = "COMPANY_ACCOUNT",
                PaymentMethodId = input.PaymentMethodId,
                PaymentPreferenceId = orderPaymentPreference.OrderPaymentPreferenceId,
                StatusId = "PMNT_NOT_PAID",
                EffectiveDate = input.EffectiveDate,
                Amount = amount,
                PartyIdFrom = orderRoleFrom.PartyId,
                PartyIdTo = orderRoleTo.PartyId,
                PaymentTypeId = paymentTypeId
            };

            var updatedPayment = await _paymentHelperService.UpdatePayment(updatePaymentParams);

            // Set results
            result.PaymentId = payment.PaymentId;
            result.IsSuccess = true;
            _logger.LogInformation("Payment {0} with the not-paid status automatically created from order: {1}",
                payment.PaymentId, input.OrderId);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error in CreatePaymentFromOrder: {0}", ex.Message);
            result.Message = "OrderErrorProcessingOrder";
            return result;
        }
    }

    public async Task<CreatePaymentFromPreferenceResult> CreatePaymentFromPreference(
        CreatePaymentFromPreferenceInput input)
    {
        var result = new CreatePaymentFromPreferenceResult();

        try
        {
            // Step 1: Retrieve the order payment preference
            var orderPaymentPreference = await _context.OrderPaymentPreferences
                .FirstOrDefaultAsync(opp => opp.OrderPaymentPreferenceId == input.OrderPaymentPreferenceId);

            if (orderPaymentPreference == null)
            {
                _logger.LogError("Order payment preference not found for ID: {0}", input.OrderPaymentPreferenceId);
                result.Message = "OrderOrderPaymentCannotBeCreated";
                return result;
            }

            // Step 2: Retrieve the order header
            var orderHeader = await _context.OrderHeaders
                .FirstOrDefaultAsync(o => o.OrderId == orderPaymentPreference.OrderId);

            if (orderHeader == null)
            {
                _logger.LogError("Related order header not found for OrderPaymentPreference ID: {0}",
                    input.OrderPaymentPreferenceId);
                result.Message = "OrderOrderPaymentCannotBeCreatedWithRelatedOrderHeader";
                return result;
            }

            // Step 3: Retrieve the product store
            var productStore = await _context.ProductStores
                .FirstOrDefaultAsync(ps => ps.ProductStoreId == orderHeader.ProductStoreId);

            if (productStore == null)
            {
                _logger.LogError("Related product store not found for Order ID: {0}", orderHeader.OrderId);
                result.Message = "OrderOrderPaymentCannotBeCreatedWithRelatedProductStore";
                return result;
            }

            // Step 4: Determine paymentFromId
            string paymentFromId = input.PaymentFromId;
            if (string.IsNullOrEmpty(paymentFromId))
            {
                var billToParty = await _context.OrderRoles
                    .FirstOrDefaultAsync(or =>
                        or.OrderId == orderHeader.OrderId && or.RoleTypeId == "BILL_TO_CUSTOMER");

                paymentFromId = billToParty?.PartyId ?? "_NA_";
            }

            // Step 5: Set payToPartyId
            string payToPartyId = productStore.PayToPartyId;
            if (string.IsNullOrEmpty(payToPartyId))
            {
                _logger.LogError("PayToPartyId not set for ProductStore ID: {0}", productStore.ProductStoreId);
                result.Message = "OrderOrderPaymentCannotBeCreatedPayToPartyIdNotSet";
                return result;
            }

            // Step 6: Create payment parameters
            var paymentParams = new CreatePaymentParam
            {
                PaymentTypeId = "CUSTOMER_PAYMENT",
                PaymentMethodTypeId = orderPaymentPreference.PaymentMethodTypeId,
                PaymentPreferenceId = orderPaymentPreference.OrderPaymentPreferenceId,
                Amount = orderPaymentPreference.MaxAmount,
                StatusId = "PMNT_RECEIVED",
                EffectiveDate = input.EventDate ?? DateTime.UtcNow,
                PartyIdFrom = paymentFromId,
                PartyIdTo = payToPartyId,
                PaymentRefNum = input.PaymentRefNum,
                Comments = input.Comments
            };

            // Step 7: Create the payment
            var paymentResult = await _paymentHelperService.CreatePayment(paymentParams);
            if (paymentResult == null)
            {
                _logger.LogError("Unable to create payment using payment preference for ID: {0}",
                    input.OrderPaymentPreferenceId);
                result.Message = "OrderOrderPaymentCannotBeCreated";
                return result;
            }

            // Set results
            result.PaymentId = paymentResult.PaymentId;
            result.IsSuccess = true;
            _logger.LogInformation("Payment created successfully for OrderPaymentPreference ID: {0}",
                input.OrderPaymentPreferenceId);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error creating payment from preference: {0}", ex.Message);
            result.Message = "OrderOrderPaymentCannotBeCreated";
            return result;
        }
    }

    public async Task<List<BillingAccountModel>> MakePartyBillingAccountList(string partyId,
        string? currencyUomId = null)
    {
        var billingAccountList = new List<BillingAccountModel>();

        currencyUomId ??= "EGP";

        // Corrected GetRelatedParties call with proper parameter ordering and includeFromToSwitched set to true
        var relatedParties = await _partyService.GetRelatedParties(
            partyIdFrom: partyId,
            partyRelationshipTypeId: "AGENT",
            roleTypeIdFrom: "AGENT",
            roleTypeIdTo: "CUSTOMER",
            roleTypeIdFromIncludeAllChildTypes: false,
            includeFromToSwitched: true);

        // Removed the exception throwing; if no related parties, proceed with an empty list
        if (!relatedParties.Any())
        {
            // Return an empty list as the party has no billing accounts
            return billingAccountList;
        }

        // Get billing accounts for related customers where this party is a bill-to customer
        var billingAccountRoles = await _context.BillingAccountRoles
            .Where(bar => relatedParties.Contains(bar.PartyId)
                          && bar.RoleTypeId == "BILL_TO_CUSTOMER"
                          && bar.FromDate <= DateTime.Now && (bar.ThruDate == null || bar.ThruDate >= DateTime.Now))
            .ToListAsync();

        if (billingAccountRoles.Any())
        {
            decimal totalAvailable = 0;

            foreach (var billingAccountRole in billingAccountRoles)
            {
                var billingAccount = await _context.BillingAccounts
                    .FirstOrDefaultAsync(ba => ba.BillingAccountId == billingAccountRole.BillingAccountId);

                // Skip accounts with a thruDate in the past
                if (billingAccount.ThruDate.HasValue && DateTime.Now > billingAccount.ThruDate.Value)
                {
                    continue;
                }

                if (billingAccount.AccountCurrencyUomId == currencyUomId)
                {
                    decimal accountBalance = await GetBillingAccountBalance(billingAccount);
                    decimal accountLimit = GetAccountLimit(billingAccount);

                    var accountAvailable = accountLimit - accountBalance;
                    totalAvailable += accountAvailable;

                    var billingAccountModel = new BillingAccountModel
                    {
                        BillingAccountId = billingAccount.BillingAccountId,
                        AccountCurrencyUomId = billingAccount.AccountCurrencyUomId,
                        AccountBalance = accountBalance,
                        AccountLimit = accountLimit,
                        AccountAvailable = accountAvailable,
                        Description = billingAccount.Description
                    };

                    billingAccountList.Add(billingAccountModel);
                }
            }

            // Sort the billing accounts based on custom logic (BillingAccountComparator equivalent)
            billingAccountList = billingAccountList
                .OrderByDescending(ba => ba.AccountAvailable)
                .ToList();
        }

        return billingAccountList;
    }

    private async Task<CheckItemStatusOutput> CheckItemStatus(string orderId)
    {
        // Initialize the output DTO to return results.
        // Technical Purpose: Holds success or error information for the caller.
        // Business Purpose: Communicates the outcome of the status check to the caller.
        var result = new CheckItemStatusOutput();

        // Begin try-catch block to handle unexpected errors.
        // Technical Purpose: Ensures all exceptions are caught and logged.
        // Business Purpose: Prevents system crashes and provides error feedback.
        try
        {
            // Get the order header using LINQ.
            // Business Purpose: Retrieves the order's metadata to check its status and type.
            // Technical Purpose: Replaces EntityQuery with EF Core LINQ query, using OrderHeader entity.
            OrderHeader orderHeader = await _context.OrderHeaders.FindAsync(orderId);

            // If the order header is null, log an error and return.
            // Business Purpose: Ensures the order exists before processing.
            // Technical Purpose: Handles missing data gracefully, using direct message text.
            if (orderHeader == null)
            {
                // Log the error for debugging.
                // Technical Purpose: Records the issue for system administrators.
                // Business Purpose: Alerts that an invalid order ID was provided.

                // Return an error with direct message text.
                // Business Purpose: Informs the caller that the order was not found.
                // Technical Purpose: Mimics OFBiz’s error handling without localization.
                result.errorMessage = $"OrderCannotUpdateNullOrderHeader: orderId={orderId}";
                return result;
            }

            // Get the order items using LINQ.
            // Business Purpose: Retrieves all items to check their statuses.
            // Technical Purpose: Replaces EntityQuery with EF Core LINQ query, using OrderItem entity.
            List<OrderItem> orderItems = await _utilityService.FindLocalOrDatabaseListAsync<OrderItem>(
                query => query.Where(oi => oi.OrderId == orderId)
            );


            // Get the current order header status and type.
            // Business Purpose: Determines the order’s current state for comparison.
            // Technical Purpose: Extracts fields from the OrderHeader entity.
            string orderHeaderStatusId = orderHeader.StatusId;
            string orderTypeId = orderHeader.OrderTypeId;

            // Initialize flags to track item statuses.
            // Business Purpose: Determines if all items are canceled, completed, or approved.
            // Technical Purpose: Used to decide the new order status.
            bool allCanceled = true;
            bool allComplete = true;
            bool allApproved = true;

            // Check if order items exist.
            // Business Purpose: Ensures there are items to process.
            // Technical Purpose: Avoids null checks in the loop (EF Core returns empty list if none).
            if (orderItems != null)
            {
                // Iterate through each order item.
                // Business Purpose: Evaluates each item’s status to update flags.
                // Technical Purpose: Implements status checking logic using OrderItem entity.
                foreach (OrderItem item in orderItems)
                {
                    // Get the item’s status.
                    // Business Purpose: Drives the logic for order state transitions.
                    // Technical Purpose: Accesses statusId from OrderItem.
                    string statusId = item.StatusId;

                    // Check if the item is not canceled.
                    // Business Purpose: If any item is not canceled, the order cannot be fully canceled.
                    // Technical Purpose: Updates allCanceled flag.
                    if (!"ITEM_CANCELLED".Equals(statusId))
                    {
                        allCanceled = false;

                        // Check if the item is not completed.
                        // Business Purpose: If any item is not completed, the order cannot be fully completed.
                        // Technical Purpose: Updates allComplete flag.
                        if (!"ITEM_COMPLETED".Equals(statusId))
                        {
                            allComplete = false;

                            // Check if the item is not approved.
                            // Business Purpose: If any item is not approved, the order cannot be fully approved.
                            // Technical Purpose: Updates allApproved flag.
                            if (!"ITEM_APPROVED".Equals(statusId))
                            {
                                allApproved = false;

                                // Break the loop as we don’t need to check further.
                                // Technical Purpose: Optimizes by exiting early if allApproved is false.
                                // Business Purpose: Avoids unnecessary processing once approval is ruled out.
                                break;
                            }
                        }
                    }
                }

                // Initialize the new status variable.
                // Business Purpose: Holds the potential new status for the order.
                // Technical Purpose: Prepares for status transition logic.
                string newStatus = null;

                // Check if all items are canceled.
                // Business Purpose: Sets the order to ORDER_CANCELLED if all items are canceled.
                // Technical Purpose: Assigns newStatus based on allCanceled flag.
                if (allCanceled)
                {
                    newStatus = "ORDER_CANCELLED";
                }
                // Check if all items are completed.
                // Business Purpose: Sets the order to ORDER_COMPLETED if all items are completed.
                // Technical Purpose: Assigns newStatus based on allComplete flag.
                else if (allComplete)
                {
                    newStatus = "ORDER_COMPLETED";
                }
                // Check if all items are approved.
                // Business Purpose: Sets the order to ORDER_APPROVED if all items are approved, with additional checks.
                // Technical Purpose: Implements complex approval logic.
                else if (allApproved)
                {
                    // Initialize flag to determine if we should approve the order.
                    // Business Purpose: Controls whether the order status should change to ORDER_APPROVED.
                    // Technical Purpose: Manages approval conditions.
                    bool changeToApprove = true;

                    // Check if the order has a product store ID.
                    // Business Purpose: Ensures store-specific approval rules are applied.
                    // Technical Purpose: Uses UtilValidate to mimic OFBiz’s null/empty check.
                    if (!string.IsNullOrEmpty(orderHeader.ProductStoreId))
                    {
                        // Begin try-catch for product store query.
                        // Technical Purpose: Handles database errors separately.
                        // Business Purpose: Ensures robust handling of store configuration retrieval.
                        try
                        {
                            // Get the product store using LINQ.
                            // Business Purpose: Retrieves store configuration for approval status.
                            // Technical Purpose: Replaces EntityQuery with EF Core LINQ, using ProductStore entity.
                            ProductStore productStore = await _context.ProductStores
                                .Where(ps => ps.ProductStoreId == orderHeader.ProductStoreId)
                                .FirstOrDefaultAsync();

                            // Check if the product store exists.
                            // Business Purpose: Ensures we can apply store-specific rules.
                            // Technical Purpose: Handles null check for productStore.
                            if (productStore != null)
                            {
                                // Get the store’s header approved status.
                                // Business Purpose: Defines the status for approved orders.
                                // Technical Purpose: Accesses headerApprovedStatus from ProductStore.
                                string headerApprovedStatus = productStore.HeaderApprovedStatus;

                                // Check if the store has a defined approval status.
                                // Business Purpose: Applies store-specific approval logic.
                                // Technical Purpose: Validates headerApprovedStatus.
                                if (!string.IsNullOrEmpty(headerApprovedStatus))
                                {
                                    // Check if the current order status matches the store’s approval status.
                                    // Business Purpose: Prevents re-approving an already approved order.
                                    // Technical Purpose: Compares statuses.
                                    if (headerApprovedStatus.Equals(orderHeaderStatusId))
                                    {
                                        // Get the order status history using LINQ.
                                        // Business Purpose: Checks if the approval status is in the history.
                                        // Technical Purpose: Replaces EntityQuery with EF Core LINQ, using OrderStatus entity.
                                        List<OrderStatus> orderStatusList = await _context.OrderStatuses
                                            .Where(os => os.OrderId == orderId
                                                         && os.StatusId == headerApprovedStatus
                                                         && os.OrderItemSeqId == null)
                                            .ToListAsync();

                                        // Check if the status history has 0 or 1 entries.
                                        // Business Purpose: Prevents changing status if already approved once.
                                        // Technical Purpose: Mimics OFBiz’s history check.
                                        if (orderStatusList.Count <= 1)
                                        {
                                            changeToApprove = false;
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            // Log the exception for debugging.
                            // Technical Purpose: Captures database or query errors.
                            // Business Purpose: Alerts administrators to data access issues.
                            string errMsg =
                                $"OrderDatabaseErrorCheckingIfWeShouldChangeOrderHeaderStatusToApproved: {e.ToString()}";

                            // Return an error with direct message text.
                            // Business Purpose: Informs the caller of the database issue.
                            // Technical Purpose: Returns error without localization.
                            result.errorMessage = errMsg;
                            return result;
                        }
                    }

                    // Check if the order is in ORDER_SENT status.
                    // Business Purpose: Prevents approving an order that has been sent.
                    // Technical Purpose: Checks orderHeaderStatusId.
                    if ("ORDER_SENT".Equals(orderHeaderStatusId))
                    {
                        changeToApprove = false;
                    }

                    // Check if the order is completed and is a sales order.
                    // Business Purpose: Prevents approving a completed sales order.
                    // Technical Purpose: Checks orderHeaderStatusId and orderTypeId.
                    if ("ORDER_COMPLETED".Equals(orderHeaderStatusId))
                    {
                        if ("SALES_ORDER".Equals(orderTypeId))
                        {
                            changeToApprove = false;
                        }
                    }

                    // Check if the order is canceled.
                    // Business Purpose: Prevents approving a canceled order.
                    // Technical Purpose: Checks orderHeaderStatusId.
                    if ("ORDER_CANCELLED".Equals(orderHeaderStatusId))
                    {
                        changeToApprove = false;
                    }

                    // Check if the order is on hold.
                    // Business Purpose: Prevents auto-approving an order on hold.
                    // Technical Purpose: Returns success without changing status.
                    if ("ORDER_HOLD".Equals(orderHeaderStatusId))
                    {
                        // Return success directly.
                        // Business Purpose: Indicates no further action is needed for held orders.
                        // Technical Purpose: Mimics OFBiz’s early return.
                        return ServiceUtilReturnSuccess();
                    }

                    // Set the new status to ORDER_APPROVED if allowed.
                    // Business Purpose: Approves the order if all conditions are met.
                    // Technical Purpose: Assigns newStatus based on changeToApprove.
                    if (changeToApprove)
                    {
                        newStatus = "ORDER_APPROVED";
                    }
                }

                // Check if a new status was determined and it differs from the current status.
                // Business Purpose: Updates the order status if a change is needed.
                // Technical Purpose: Prepares to call changeOrderStatus service.
                if (newStatus != null && !newStatus.Equals(orderHeaderStatusId))
                {
                    // Initialize the result for the service call.
                    // Technical Purpose: Holds the result of changeOrderStatus.
                    // Business Purpose: Captures the outcome of the status update.
                    ChangeOrderStatusResponse newSttsResult = null;

                    // Begin try-catch for service call.
                    // Technical Purpose: Handles errors from changeOrderStatus.
                    // Business Purpose: Ensures robust status update.
                    try
                    {
                        // Create the request for changeOrderStatus.
                        // Business Purpose: Prepares parameters to update the order status.
                        // Technical Purpose: Maps CheckItemStatus parameters to ChangeOrderStatusRequest.
                        var request = new ChangeOrderStatusRequest
                        {
                            OrderId = orderId,
                            StatusId = newStatus,
                            SetItemStatus = false, // Default, as not specified in CheckItemStatus
                            ChangeReason = null, // Default, as not specified
                            UserLoginId = null // UserLogin not passed; assume null or inject later
                        };

                        // Call the changeOrderStatus service directly.
                        // Business Purpose: Updates the order’s status to the new value.
                        // Technical Purpose: Replaces dispatcher with direct method call.
                        newSttsResult = await ChangeOrderStatus(request);
                    }
                    catch (Exception e)
                    {
                        // Log the exception for debugging.
                        // Technical Purpose: Captures service invocation errors.
                        // Business Purpose: Alerts administrators to integration issues.

                        // Note: Original code doesn’t return an error here; log and proceed.
                        // Business Purpose: Mimics OFBiz’s behavior of continuing.
                        _logger.LogError(e, "Error in changeOrderStatus for orderId: {OrderId}", orderId);
                    }
                }
            }
            else
            {
                // Log a warning if no order items were found.
                // Business Purpose: Alerts the system that the order has no items.
                // Technical Purpose: Mimics OFBiz’s Debug.logWarning with direct message text.
                _logger.LogWarning("OrderReceivedNullForOrderItemRecordsOrderId: orderId={OrderId}", orderId);
            }

            // Return success if no errors occurred.
            // Business Purpose: Indicates the status check completed successfully.
            // Technical Purpose: Returns a success result.
            return ServiceUtilReturnSuccess();
        }
        catch (Exception e)
        {
            // Log the exception for debugging.
            // Technical Purpose: Captures any unexpected errors in the service.
            // Business Purpose: Alerts administrators to system issues.
            _logger.LogError(e, "Unexpected error in checkItemStatus for orderId: {OrderId}", orderId);

            // Return an error with a generic message.
            // Business Purpose: Informs the caller of an unexpected failure.
            // Technical Purpose: Provides a fallback error response.
            result.errorMessage = "Unexpected error occurred while checking item status";
            return result;
        }
    }

    private CheckItemStatusOutput ServiceUtilReturnSuccess()
    {
        // Create a new output DTO with a success message.
        // Technical Purpose: Populates successMessage and successMessageList.
        return new CheckItemStatusOutput
        {
            successMessage = "Operation completed successfully",
            successMessageList = new List<string> { "Operation completed successfully" }
        };
    }
}