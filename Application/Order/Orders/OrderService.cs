using API.Middleware;
using Application._Base;



using Application.Catalog.Products;
using Application.Catalog.Products.Services.Inventory;
using Application.Catalog.ProductStores;
using Application.Core;
using Application.Facilities;
using Application.Order;
using Application.Order.Orders;
using Application.order.Quotes;
using Application.Order.Quotes;
using Application.Shipments;
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;
using Serilog;

namespace Application.order.Orders;

public interface IOrderService
{
    Task<OrderHeader> CreateJobOrderFromQuote(QuoteDto quoteDto);
    Task<OrderHeader> CreateSalesOrder(OrderDto orderDto);
    Task<OrderHeader> CreatePurchaseOrder(OrderDto orderDto);
    Task<OrderHeader> UpdateOrApproveSalesOrder(OrderDto orderDto, string modificationType);
    Task<OrderHeader> UpdateOrApprovePurchaseOrder(OrderDto orderDto, string modificationType);
}

public class OrderService : BaseService, IOrderService
{
    private readonly IFacilityService _facilityService;
    private readonly IProductService _productService;

    private readonly IProductStoreService _productStoreService;
    private readonly IQuoteService _quoteService;
    private readonly IShipmentService _shipmentService;
    private readonly IInventoryService _inventoryService;
    private readonly IOrderHelperService _orderHelperService;


    public OrderService(DataContext context, IUtilityService utilityService,
        IProductStoreService productStoreService,
        ILogger<OrderService> logger, IFacilityService facilityService, IQuoteService quoteService,
        IShipmentService shipmentService, IProductService productService,
        IInventoryService inventoryService, IOrderHelperService orderHelperService) : base(
        context, utilityService, logger)
    {
        _productStoreService = productStoreService;
        _productService = productService;
        _shipmentService = shipmentService;
        _facilityService = facilityService;
        _quoteService = quoteService;
        _inventoryService = inventoryService;
        _orderHelperService = orderHelperService;
    }

    public async Task<OrderHeader> GetOrderById(string orderId)
    {
        var order = await _context.OrderHeaders.SingleOrDefaultAsync(x => x.OrderId == orderId);

        if (order == null) throw new ArgumentException("Order not found", nameof(orderId));

        return order;
    }

    public async Task<List<OrderItem>> GetOrderItemsById(string orderId)
    {
        var orderItems = await _context.OrderItems.Where(x => x.OrderId == orderId).ToListAsync();

        if (orderItems == null) throw new ArgumentException("Order items not found", nameof(orderId));

        return orderItems;
    }

    public async Task MarkOrderItemsAsCompleted(List<OrderItemDto2> orderItems)
    {
        foreach (var orderItem in orderItems) await MarkOrderItemAsCompleted(orderItem);
    }

    public async Task<OrderHeader> CreatePurchaseOrder(OrderDto orderDto)
    {
        var stamp = DateTime.UtcNow;

        // get purchase order new serial
        var newOrderSerial = await _utilityService.GetNextOrderSequence("PURCHASE_ORDER");
        // get default product store currency
        var productStoreDefaultCurrencyId = await _productStoreService.GetProductStoreDefaultCurrencyId();
        // get product store
        var productStore = await _productStoreService.GetProductStoreForLoggedInUser();

        var newOrder = new OrderHeader
        {
            OrderId = newOrderSerial,
            OrderTypeId = "PURCHASE_ORDER",
            StatusId = "ORDER_CREATED",
            InternalRemarks = orderDto.InternalRemarks,
            CurrencyUom = orderDto.CurrencyUomId,
            AgreementId = orderDto.AgreementId,
            GrandTotal = orderDto.GrandTotal,
            InvoicePerShipment = "Y",
            LastUpdatedStamp = stamp,
            OrderDate = stamp,
            EntryDate = stamp,
            CreatedStamp = stamp
        };

        //TODO: consider using entity OrderHeaderNote for both internal and customer remarks

        _context.OrderHeaders.Add(newOrder);

        // create order status
        _utilityService.CreateOrderStatus(newOrderSerial, "ORDER_CREATED");

        // create "SHIP_FROM_VENDOR" order role
        _utilityService.CreateOrderRole(newOrderSerial, "SHIP_FROM_VENDOR", orderDto.FromPartyId);

        // create "BILL_FROM_VENDOR" order role
        _utilityService.CreateOrderRole(newOrderSerial, "BILL_FROM_VENDOR", orderDto.FromPartyId);

        // create "SUPPLIER_AGENT" order role
        _utilityService.CreateOrderRole(newOrderSerial, "SUPPLIER_AGENT", orderDto.FromPartyId);

        // get product store pay to party id
        var payToPartyId = await _productStoreService.GetProductStorePayToPartId();

        // create "BILL_TO_CUSTOMER" order role
        _utilityService.CreateOrderRole(newOrderSerial, "BILL_TO_CUSTOMER", payToPartyId);

        // create OrderItemShipGroup
        var orderItemShipGroup = _shipmentService.CreateOrderItemShipGroup(newOrderSerial);

        // create order items
        await CreatePurchaseOrderItems(orderDto.OrderItems, newOrderSerial);

        var createdOrderItems = await _utilityService.FindLocalOrDatabaseListAsync<OrderItem>(
            query => query.Where(ii => ii.OrderId == newOrderSerial)
        );

        foreach (var orderItem in createdOrderItems)
        {
            await _shipmentService.AddOrderItemShipGroupAssoc(orderItem.OrderId, orderItem.OrderItemSeqId,
                orderItemShipGroup.ShipGroupSeqId, (decimal)orderItem.Quantity);
        }


        // create order adjustments
        await CreateOrderAdjustments(orderDto.OrderAdjustments, newOrderSerial);


        return await Task.FromResult(newOrder);
    }

    public async Task<OrderHeader> CreateJobOrderFromQuote(QuoteDto quoteDto)
    {
        var stamp = DateTime.UtcNow;

        // get purchase order new serial
        var newOrderSerial = await _utilityService.GetNextOrderSequence("SALES_ORDER");
        // get default product store currency
        var productStoreDefaultCurrencyId = await _productStoreService.GetProductStoreDefaultCurrencyId();
        // get product store
        var productStore = await _productStoreService.GetProductStoreForLoggedInUser();

        var newOrder = new OrderHeader
        {
            OrderId = newOrderSerial,
            OrderTypeId = "SALES_ORDER",
            CustomerRemarks = quoteDto.CustomerRemarks,
            InternalRemarks = quoteDto.InternalRemarks,
            CurrentMileage = quoteDto.CurrentMileage,
            StatusId = "ORDER_CREATED",
            CurrencyUom = productStoreDefaultCurrencyId,
            ProductStoreId = productStore.ProductStoreId,
            InvoicePerShipment = "Y",
            GrandTotal = quoteDto.GrandTotal,
            LastUpdatedStamp = stamp,
            OrderDate = stamp,
            EntryDate = stamp,
            CreatedStamp = stamp
        };

        _context.OrderHeaders.Add(newOrder);

        // create order status
        _utilityService.CreateOrderStatus(newOrderSerial, "ORDER_CREATED");


        // create "BILL_TO_CUSTOMER" order role
        _utilityService.CreateOrderRole(newOrderSerial, "PLACING_CUSTOMER", quoteDto.FromPartyId);

        _utilityService.CreateOrderRole(newOrderSerial, "BILL_TO_CUSTOMER", quoteDto.FromPartyId);

        // get product store pay to party id
        var payToPartyId = await _productStoreService.GetProductStorePayToPartId();

        // create "BILL_FROM_VENDOR" order role
        _utilityService.CreateOrderRole(newOrderSerial, "BILL_FROM_VENDOR", payToPartyId);

        // create OrderItemShipGroup
        _shipmentService.CreateOrderItemShipGroup(newOrderSerial);

        // create order items
        await CreateJobOrderItemsFromQuoteItems(quoteDto.QuoteItems, newOrderSerial);

        // create order adjustments
        await CreateJobOrderAdjustments(quoteDto.QuoteAdjustments, newOrderSerial);


        await _quoteService.ChangeQuoteStatusAsOrdered(quoteDto);

        return await Task.FromResult(newOrder);
    }

    public async Task<OrderHeader> CreateSalesOrder(OrderDto orderDto)
    {
        var stamp = DateTime.UtcNow;

        // get purchase order new serial
        var newOrderSerial = await _utilityService.GetNextOrderSequence("SALES_ORDER");
        // get default product store currency
        //var productStoreDefaultCurrencyId = await _productStoreService.GetProductStoreDefaultCurrencyId();
        // get product store
        var productStore = await _productStoreService.GetProductStoreForLoggedInUser();
        //TODO: add Currency from frontend, also add AgreementId
        var newOrder = new OrderHeader
        {
            OrderId = newOrderSerial,
            OrderTypeId = "SALES_ORDER",
            CustomerRemarks = orderDto.CustomerRemarks,
            InternalRemarks = orderDto.InternalRemarks,
            BillingAccountId = orderDto.BillingAccountId,
            StatusId = "ORDER_CREATED",
            CurrencyUom = orderDto.CurrencyUomId,
            AgreementId = orderDto.AgreementId,
            ProductStoreId = productStore.ProductStoreId,
            InvoicePerShipment = "Y",
            GrandTotal = orderDto.GrandTotal,
            LastUpdatedStamp = stamp,
            OrderDate = stamp,
            EntryDate = stamp,
            CreatedStamp = stamp
        };

        _context.OrderHeaders.Add(newOrder);

        // Use ForContext to add the "Transaction" property for filtering
        var loggerForTransaction = Log.ForContext("Transaction", "create sales order");
        loggerForTransaction.Information("Creating sales order with ID");


        //_logger.LogInformation("Create Sales order header. {Transaction} for {@OrderHeader}","create sales order", newOrder);


        // create order status
        _utilityService.CreateOrderStatus(newOrderSerial, "ORDER_CREATED");

        //_logger.LogInformation("Create Sales order status. {Transaction} for {OrderId}","create sales order", newOrder.OrderId);


        // create "BILL_TO_CUSTOMER" order role
        _utilityService.CreateOrderRole(newOrderSerial, "PLACING_CUSTOMER", orderDto.FromPartyId);

        _utilityService.CreateOrderRole(newOrderSerial, "BILL_TO_CUSTOMER", orderDto.FromPartyId);

        // get product store pay to party id
        var payToPartyId = await _productStoreService.GetProductStorePayToPartId();

        // create "BILL_FROM_VENDOR" order role
        _utilityService.CreateOrderRole(newOrderSerial, "BILL_FROM_VENDOR", payToPartyId);

        // create OrderItemShipGroup
        var orderItemShipGroup = _shipmentService.CreateOrderItemShipGroup(newOrderSerial);

        // loop through order items and add to ShipGroupSeqId and ReserveOrderEnumId
        foreach (var orderItem in orderDto.OrderItems)
        {
            orderItem.ShipGroupSeqId = orderItemShipGroup.ShipGroupSeqId;
            orderItem.ReserveOrderEnumId = productStore.ReserveOrderEnumId;
        }

        // create order items
        await CreateSalesOrderItems(orderDto.OrderItems, newOrderSerial);

        var createdOrderItems = await _utilityService.FindLocalOrDatabaseListAsync<OrderItem>(
            query => query.Where(ii => ii.OrderId == newOrderSerial)
        );

        foreach (var orderItem in createdOrderItems)
        {
            await _shipmentService.AddOrderItemShipGroupAssoc(orderItem.OrderId, orderItem.OrderItemSeqId,
                orderItemShipGroup.ShipGroupSeqId, (decimal)orderItem.Quantity);
        }

        // create order adjustments
        await CreateOrderAdjustments(orderDto.OrderAdjustments, newOrderSerial);


        await CreateOrderTerms(orderDto.OrderTerms, newOrderSerial);

        var orderPaymentInfoInput = new OrderPaymentInfoInput
        {
            OrderId = newOrderSerial,
            GrandTotal = orderDto.GrandTotal ?? 0, // Default to 0 if GrandTotal is null
            PaymentTotal = 0,
            BillingAccountId = orderDto?.BillingAccountId,
            BillingAccountAmt =
                orderDto?.UseUpToFromBillingAccount ?? 0, // Default to 0 if UseUpToFromBillingAccount is null
            PaymentInfo = new List<CartPaymentInfo>
            {
                new CartPaymentInfo
                {
                    PaymentMethodTypeId = orderDto?.BillingAccountId != null
                        ? "EXT_BILLACT"
                        : orderDto?.PaymentMethodTypeId,
                    PaymentMethodId = orderDto?.PaymentMethodId,
                    Amount = orderDto.GrandTotal ?? 0, // Default to 0 if GrandTotal is null
                }
            }
        };


        var allOrderPaymentPreferences = await _orderHelperService.MakeAllOrderPaymentInfos(orderPaymentInfoInput);

        var orderApproved = await _orderHelperService.ApproveOrder(newOrderSerial, false);
        if (orderApproved)
        {
            newOrder.StatusId = "ORDER_APPROVED";
        }

        return await Task.FromResult(newOrder);
    }

    public async Task<OrderHeader> UpdateOrApproveSalesOrder(OrderDto orderDto, string modificationType)
    {
        var stamp = DateTime.UtcNow;

        // Create a detached entity with the primary key
        var orderToUpdate = new OrderHeader { OrderId = orderDto.OrderId };

        // Attach the entity to the context and mark it as modified
        _context.Attach(orderToUpdate);

        // Update the specific properties
        orderToUpdate.GrandTotal = orderDto.GrandTotal;
        orderToUpdate.LastUpdatedStamp = stamp;

        if (modificationType == "APPROVE")
        {
            orderToUpdate.StatusId = "ORDER_APPROVED";

            // create order status
            _utilityService.CreateOrderStatus(orderDto.OrderId, "ORDER_APPROVED");
        }


        // update order items
        await UpdateOrApproveSalesOrderItems(orderDto.OrderItems, modificationType, orderDto.OrderId);

        if (orderDto.OrderAdjustments is { Count: > 0 })
            await UpdateJobOrderAdjustments(orderDto.OrderAdjustments, orderDto.OrderId);

        // update order role BILL_FROM_VENDOR
        _utilityService.UpdateOrderRole(orderDto.OrderId, "PLACING_CUSTOMER", orderDto.FromPartyId);
        // update order role SHIP_FROM_VENDOR
        _utilityService.UpdateOrderRole(orderDto.OrderId, "BILL_TO_CUSTOMER", orderDto.FromPartyId);

        return orderToUpdate;
    }

    public async Task<OrderHeader> UpdateOrApprovePurchaseOrder(OrderDto orderDto, string modificationType)
    {
        var stamp = DateTime.UtcNow;

        // Create a detached entity with the primary key
        var orderToUpdate = await _context.OrderHeaders.FirstOrDefaultAsync(x => x.OrderId == orderDto.OrderId);

        // Update the specific properties
        orderToUpdate.GrandTotal = orderDto.GrandTotal;
        orderToUpdate.LastUpdatedStamp = stamp;


        // update order items
        await UpdateOrApprovePurchaseOrderItems(orderDto.OrderItems, modificationType, orderDto.OrderId);

        if (orderDto.OrderAdjustments is { Count: > 0 })
            await UpdateJobOrderAdjustments(orderDto.OrderAdjustments, orderDto.OrderId);

        // update order role BILL_FROM_VENDOR
        _utilityService.UpdateOrderRole(orderDto.OrderId, "BILL_FROM_VENDOR", orderDto.FromPartyId);
        // update order role SHIP_FROM_VENDOR
        _utilityService.UpdateOrderRole(orderDto.OrderId, "SHIP_FROM_VENDOR", orderDto.FromPartyId);
        // update "SUPPLIER_AGENT" order role
        _utilityService.UpdateOrderRole(orderDto.OrderId, "SUPPLIER_AGENT", orderDto.FromPartyId);

        if (modificationType == "APPROVE")
        {
            // Set the status on the order header using ChangeOrderStatus
            var orderStatusRequest = new ChangeOrderStatusRequest
            {
                OrderId = orderDto.OrderId,
                StatusId = "ORDER_APPROVED",
                SetItemStatus = true
            };

            var statusResult = await _orderHelperService.ChangeOrderStatus(orderStatusRequest);
            if (statusResult == null)
            {
                Console.WriteLine($"Error changing order status: Status result is null");
                throw new Exception("Order status change failed.");
            }

            orderToUpdate.StatusId = "ORDER_APPROVED";

            // create shipment
            //await _shipmentService.CreatePurchaseShipment(orderDto.OrderId, orderDto.OrderItems, orderDto.FromPartyId);

            /*// create order status
            _utilityService.CreateOrderStatus(orderDto.OrderId, "ORDER_APPROVED");
            // create OrderPaymentPreference
            var orderPaymentPreference = await _paymentService.CreatePurchaseOrderPaymentPreference(orderDto);

            // create order status PMNT_NOT_PAID
            var orderStatus = _utilityService.CreateOrderStatus(orderDto.OrderId, "PMNT_NOT_PAID");
            // create payment
            var payment = await _paymentService.CreatePurchaseOrderPayment(orderDto, orderPaymentPreference);*/
        }

        return orderToUpdate;
    }

    
    public async Task<OrderAdjustment> GetOrderAdjustmentById(string orderAdjustmentId)
    {
        var orderAdjustment = await _context.OrderAdjustments.Where(x => x.OrderAdjustmentId == orderAdjustmentId)
            .FirstOrDefaultAsync();


        return orderAdjustment;
    }

    public async Task<OrderItem> GetOrderItemById(string orderId, string orderItemSeqId)
    {
        var orderItem =
            await _context.OrderItems.SingleOrDefaultAsync(x =>
                x.OrderId == orderId && x.OrderItemSeqId == orderItemSeqId);

        //if (orderItem == null) throw new ArgumentException("Order item not found", nameof(orderId));

        return orderItem;
    }

    private Task MarkOrderItemAsCompleted(OrderItemDto2 orderItem)
    {
        orderItem.StatusId = "ITEM_COMPLETED";
        orderItem.LastUpdatedStamp = DateTime.UtcNow;

        return Task.CompletedTask;
    }

    private async Task CreatePurchaseOrderItems(List<OrderItemDto2> orderItems, string orderId)
    {
        foreach (var orderItem in orderItems)
        {
            if (orderItem.IsProductDeleted) continue;
            CreatePurchaseOrderItem(orderItem, orderId);
        }

        await Task.CompletedTask;
    }

    public async Task CreateJobOrderItemsFromQuoteItems(List<QuoteItemDto2> quoteItems, string orderId)
    {
        foreach (var quoteItem in quoteItems)
        {
            if (quoteItem.IsProductDeleted) continue;
            await CreateJobOrderItem(quoteItem, orderId);
        }

        await Task.CompletedTask;
    }

    private async Task CreateSalesOrderItems(List<OrderItemDto2> orderItems, string orderId)
    {
        foreach (var orderItem in orderItems)
        {
            if (orderItem.IsProductDeleted) continue;
            await CreateSalesOrderItem(orderItem, orderId);
        }

        await Task.CompletedTask;
    }

    private async Task CreateJobOrderAdjustments(List<QuoteAdjustmentDto2> quoteAdjustments, string orderId)
    {
        foreach (var quoteAdjustment in quoteAdjustments)
        {
            if (quoteAdjustment.IsAdjustmentDeleted) continue;
            //_logger.LogDebug("Starting CreateJobOrderAdjustments");

            var createdAdjustment = await CreateJobOrderAdjustment(quoteAdjustment, orderId);
            //_logger.LogDebug("Finished CreateJobOrderAdjustments {createdAdjustment}", createdAdjustment);
        }

        await Task.CompletedTask;
    }

    private async Task CreateOrderAdjustments(List<OrderAdjustmentDto2> orderAdjustments, string orderId)
    {
        foreach (var orderAdjustment in orderAdjustments)
        {
            if (orderAdjustment.IsAdjustmentDeleted) continue;
            // //_logger.LogDebug("Starting CreateJobOrderAdjustments");

            CreateOrderAdjustment(orderAdjustment, orderId);

            // //_logger.LogDebug("Finished CreateJobOrderAdjustments {createdAdjustment}", createdAdjustment);
        }

        await Task.CompletedTask;
    }

    private async Task CreateOrderTerms(List<OrderTermDto> orderTerms, string orderId)
    {
        foreach (var orderTerm in orderTerms)
        {
            if (orderTerm.IsTermDeleted) continue;

            CreateOrderTerm(orderTerm.TermTypeId, orderTerm.TermValue ?? 0m, orderTerm.TermDays ?? 0, orderId);
        }

        await Task.CompletedTask;
    }

    private OrderItem CreatePurchaseOrderItem(OrderItemDto2 orderItem, string orderId)
    {
        var stamp = DateTime.UtcNow;

        var newItem = new OrderItem
        {
            OrderId = orderId,
            OrderItemSeqId = orderItem.OrderItemSeqId,
            ProductId = orderItem.ProductId,
            ProductFeatureId = orderItem.ProductFeatureId,
            ItemDescription = orderItem.ProductName,
            Quantity = orderItem.Quantity,
            UnitPrice = orderItem.UnitPrice,
            OrderItemTypeId = "PRODUCT_ORDER_ITEM",
            StatusId = "ITEM_CREATED",
            CreatedStamp = stamp,
            LastUpdatedStamp = stamp
        };
        _context.OrderItems.Add(newItem);

        // create order_status ITEM_CREATED
        _utilityService.CreateOrderItemStatus(newItem, "ITEM_CREATED");

        SetUnitPriceAsLastPrice(orderItem);

        return newItem;
    }

    private async Task<OrderItem> CreateSalesOrderItem(OrderItemDto2 orderItem, string orderId)
    {
        var stamp = DateTime.UtcNow;


        var newItem = new OrderItem
        {
            OrderId = orderId,
            OrderItemSeqId = orderItem.OrderItemSeqId,
            ParentOrderItemSeqId = orderItem.ParentOrderItemSeqId,
            ProductId = orderItem.ProductId,
            ItemDescription = orderItem.ProductName,
            IsPromo = orderItem.IsPromo,
            Quantity = orderItem.Quantity,
            UnitPrice = orderItem.UnitPrice,
            UnitListPrice = orderItem.UnitListPrice,
            OrderItemTypeId = "PRODUCT_ORDER_ITEM",
            StatusId = "ITEM_CREATED",
            CreatedStamp = stamp,
            LastUpdatedStamp = stamp
        };
        _context.OrderItems.Add(newItem);
        

        // create order_status ITEM_CREATED
        _utilityService.CreateOrderItemStatus(newItem, "ITEM_CREATED");
        orderItem.OrderId = orderId;

        var isPhysicalProduct = await _productService.CheckIsPhysicalProduct(orderItem);
        
        if (isPhysicalProduct)
        {
            await _inventoryService.ReserveProductInventory(
                quantity: (decimal)orderItem.Quantity,
                productId: orderItem.ProductId,
                orderId: orderItem.OrderId,
                orderItemSeqId: orderItem.OrderItemSeqId,
                shipGroupSeqId: orderItem.ShipGroupSeqId,
                reserveOrderEnumId: orderItem.ReserveOrderEnumId,
                productFeatureId: orderItem.ProductFeatureId
            );
        }

        return newItem;
    }

    private async Task<OrderItem> CreateJobOrderItem(QuoteItemDto2 quoteItem, string orderId)
    {
        var stamp = DateTime.UtcNow;

        //_logger.LogDebug("Starting CreateQuoteItem");

        var newItem = new OrderItem
        {
            OrderId = orderId,
            OrderItemSeqId = quoteItem.QuoteItemSeqId,
            ProductId = quoteItem.ProductId,
            ItemDescription = quoteItem.ProductName,
            Quantity = quoteItem.Quantity,
            UnitListPrice = quoteItem.UnitListPrice,
            UnitPrice = quoteItem
                .UnitListPrice, // updated to use UnitListPrice instead of UnitPrice as the latter may include adjustments
            OrderItemTypeId = "PRODUCT_ORDER_ITEM",
            StatusId = "ITEM_CREATED",
            CreatedStamp = stamp,
            LastUpdatedStamp = stamp
        };
        _context.OrderItems.Add(newItem);

        // create new OrderItemDto2 and populate it with OrderItem data
        var newOrderItemDto = new OrderItemDto2
        {
            OrderId = orderId,
            OrderItemSeqId = quoteItem.QuoteItemSeqId,
            ProductId = quoteItem.ProductId,
            ProductName = quoteItem.ProductName,
            Quantity = quoteItem.Quantity,
            UnitListPrice = quoteItem.UnitListPrice,
            UnitPrice = quoteItem
                .UnitListPrice, // updated to use UnitListPrice instead of UnitPrice as the latter may include adjustments
            OrderItemTypeId = "PRODUCT_ORDER_ITEM",
            StatusId = "ITEM_CREATED",
            CreatedStamp = stamp,
            LastUpdatedStamp = stamp
        };

        // create order_status ITEM_CREATED
        var orderStatus = _utilityService.CreateOrderItemStatus(newItem, "ITEM_CREATED");
        var isPhysicalProduct = await _productService.CheckIsPhysicalProduct(newOrderItemDto);

        if (isPhysicalProduct)
            await _facilityService.ReserveInventoryForSalesOrderItem(newOrderItemDto);

        return newItem;
    }

    private OrderAdjustment CreateOrderAdjustment(OrderAdjustmentDto2 orderAdjustment, string orderId)
    {
        var stamp = DateTime.UtcNow;

        //_logger.LogDebug("Starting Creating OrderAdjustment", orderAdjustment);

        var newAdjustment = new OrderAdjustment
        {
            OrderAdjustmentId = orderAdjustment.OrderAdjustmentId,
            OrderAdjustmentTypeId = orderAdjustment.OrderAdjustmentTypeId,
            OrderId = orderId,
            OrderItemSeqId = orderAdjustment.OrderItemSeqId,
            CorrespondingProductId = orderAdjustment.CorrespondingProductId,
            Description = orderAdjustment.Description,
            IsManual = orderAdjustment.IsManual,
            SourcePercentage = orderAdjustment.SourcePercentage,
            ProductPromoId = orderAdjustment.ProductPromoId,
            Amount = orderAdjustment.Amount,
            OverrideGlAccountId = orderAdjustment.OverrideGlAccountId,
            TaxAuthGeoId = orderAdjustment.TaxAuthGeoId,
            TaxAuthPartyId = orderAdjustment.TaxAuthPartyId,
            LastUpdatedStamp = stamp,
            CreatedStamp = stamp
        };
        _context.OrderAdjustments.Add(newAdjustment);

        return newAdjustment;
    }

    private async Task<OrderAdjustment> CreateJobOrderAdjustment(QuoteAdjustmentDto2 quoteAdjustment, string orderId)
    {
        var stamp = DateTime.UtcNow;

        //_logger.LogDebug("Starting _utilityService.GetNextSequence OrderAdjustment");
        var newQuoteAdjustmentSerial = await _utilityService.GetNextSequence("QuoteAdjustment");
        //_logger.LogDebug("Finished _utilityService.GetNextSequence OrderAdjustment {newQuoteAdjustmentSerial}",newQuoteAdjustmentSerial);

        //_logger.LogDebug("Starting CreateOrderAdjustment");

        var newAdjustment = new OrderAdjustment
        {
            OrderAdjustmentId = newQuoteAdjustmentSerial,
            OrderAdjustmentTypeId = quoteAdjustment.QuoteAdjustmentTypeId,
            OrderId = orderId,
            OrderItemSeqId = quoteAdjustment.QuoteItemSeqId,
            CorrespondingProductId = quoteAdjustment.CorrespondingProductId,
            Description = quoteAdjustment.Description,
            IsManual = quoteAdjustment.IsManual,
            SourcePercentage = quoteAdjustment.SourcePercentage,
            ProductPromoId = quoteAdjustment.ProductPromoId,
            Amount = quoteAdjustment.Amount,
            LastUpdatedStamp = stamp,
            CreatedStamp = stamp
        };
        _context.OrderAdjustments.Add(newAdjustment);

        return newAdjustment;
    }

    private async Task UpdateJobOrderAdjustments(List<OrderAdjustmentDto2> updatedOrderAdjustments,
        string originalOrderId)
    {
        //_logger.LogDebug("Starting UpdateOrderAdjustments");

        foreach (var orderAdjustment in updatedOrderAdjustments)
            await UpdateJobOrderAdjustment(orderAdjustment, originalOrderId);
        //_logger.LogDebug("Finished UpdateOrderAdjustments");

        await Task.CompletedTask;
    }

    private async Task UpdateJobOrderAdjustment(OrderAdjustmentDto2 updatedOrderAdjustment, string originalOrderId)
    {
        // get order adjustment
        var savedOrderAdjustment = await GetOrderAdjustmentById(updatedOrderAdjustment.OrderAdjustmentId);
        if (savedOrderAdjustment == null)
        {
            CreateOrderAdjustment(updatedOrderAdjustment, originalOrderId);
            return;
        }

        if (updatedOrderAdjustment.IsAdjustmentDeleted)
            DeleteOrderAdjustment(savedOrderAdjustment);

        savedOrderAdjustment.Amount = updatedOrderAdjustment.Amount;

        await Task.CompletedTask;
    }

    private async Task UpdateOrApprovePurchaseOrderItems(List<OrderItemDto2> updatedOrderItems, string modificationType,
        string originalOrderId)
    {
        foreach (var orderItem in updatedOrderItems)
            await UpdateOrApprovePurchaseOrderItem(orderItem, modificationType, originalOrderId);

        await Task.CompletedTask;
    }

    private async Task UpdateOrApproveSalesOrderItems(List<OrderItemDto2> updatedOrderItems, string modificationType,
        string originalOrderId)
    {
        foreach (var orderItem in updatedOrderItems)
            await UpdateOrApproveSalesOrderItem(orderItem, modificationType, originalOrderId);

        await Task.CompletedTask;
    }

    private async Task UpdateOrApprovePurchaseOrderItem(OrderItemDto2 updatedOrderItem, string modificationType,
        string originalOrderId)
    {
        // get order items
        var savedOrderItem = await GetOrderItemById(updatedOrderItem.OrderId, updatedOrderItem.OrderItemSeqId);
        if (savedOrderItem == null)
        {
            CreatePurchaseOrderItem(updatedOrderItem, originalOrderId);
            return;
        }

        if (updatedOrderItem.IsProductDeleted)
        {
            await _utilityService.DeleteAllOrderItemStatusAsync(updatedOrderItem);
            await DeletePurchaseOrderItem(savedOrderItem);
            return;
        }


        savedOrderItem.Quantity = updatedOrderItem.Quantity;
        savedOrderItem.UnitPrice = updatedOrderItem.UnitPrice;
        savedOrderItem.LastUpdatedStamp = DateTime.UtcNow;

        SetUnitPriceAsLastPrice(updatedOrderItem);


        await _utilityService.UpdateAllOrderItemStatusAsync(updatedOrderItem);


        if (modificationType == "APPROVE")
        {
            await CreateOrderItemStatus(updatedOrderItem, "ITEM_APPROVED");
            savedOrderItem.StatusId = "ITEM_APPROVED";
        }


        await Task.CompletedTask;
    }


    private async Task UpdateOrApproveSalesOrderItem(OrderItemDto2 updatedOrderItem, string modificationType,
        string originalOrderId)
    {
        var isPhysicalProduct = false;
        // get order items
        var savedOrderItem = await GetOrderItemById(updatedOrderItem.OrderId, updatedOrderItem.OrderItemSeqId);

        if (savedOrderItem == null)
        {
            await CreateSalesOrderItem(updatedOrderItem, originalOrderId);
            return;
        }

        if (updatedOrderItem.IsProductDeleted)
        {
            isPhysicalProduct = await _productService.CheckIsPhysicalProduct(updatedOrderItem);
            if (!isPhysicalProduct)
            {
                await _facilityService.RemoveSalesOrderItemInventoryReservation(updatedOrderItem);
                await _facilityService.AdjustInventoryItemForDeletedOrUpdatedOrderItem(updatedOrderItem);
            }

            await _utilityService.DeleteAllOrderItemStatusAsync(updatedOrderItem);
            await DeleteJobOrderItem(savedOrderItem);
            return;
        }

        // for updated order item, remove inventory reservation as new set will be created

        isPhysicalProduct = await _productService.CheckIsPhysicalProduct(updatedOrderItem);
        if (!isPhysicalProduct)
        {
            await _facilityService.RemoveSalesOrderItemInventoryReservation(updatedOrderItem);
            await _facilityService.AdjustInventoryItemForDeletedOrUpdatedOrderItem(updatedOrderItem);
        }

        await _utilityService.UpdateAllOrderItemStatusAsync(updatedOrderItem);

        if (modificationType == "APPROVE") await CreateOrderItemStatus(updatedOrderItem, "ITEM_APPROVED");

        if (!isPhysicalProduct) await _facilityService.ReserveInventoryForSalesOrderItem(updatedOrderItem);

        savedOrderItem.Quantity = updatedOrderItem.Quantity;
        savedOrderItem.UnitListPrice = updatedOrderItem.UnitListPrice;
        savedOrderItem.UnitPrice = updatedOrderItem.UnitListPrice;
        savedOrderItem.LastUpdatedStamp = DateTime.UtcNow;

        await Task.CompletedTask;
    }


    public async Task<OrderHeader> ApproveJobOrder(OrderDto orderDto)
    {
        var stamp = DateTime.UtcNow;

        var order = await GetOrderById(orderDto.OrderId);

        order.GrandTotal = orderDto.GrandTotal;
        order.LastUpdatedStamp = stamp;
        order.StatusId = "ORDER_APPROVED";


        // approve order items
        await ApproveJobOrderItems(orderDto.OrderItems);


        // create order status PMNT_NOT_PAID
        var orderStatus = _utilityService.CreateOrderStatus(orderDto.OrderId, "PMNT_NOT_PAID");

        return order;
    }

    private async Task ApprovePurchaseOrderItems(List<OrderItemDto2> updatedOrderItems)
    {
        foreach (var orderItem in updatedOrderItems) await ApprovePurchaseOrderItem(orderItem);

        await Task.CompletedTask;
    }

    private async Task ApproveJobOrderItems(List<OrderItemDto2> updatedOrderItems)
    {
        foreach (var orderItem in updatedOrderItems) await ApproveJobOrderItem(orderItem);

        await Task.CompletedTask;
    }

    private async Task ApprovePurchaseOrderItem(OrderItemDto2 updatedOrderItem)
    {
        // get order items
        var savedOrderItem = await GetOrderItemById(updatedOrderItem.OrderId, updatedOrderItem.OrderItemSeqId);
        if (savedOrderItem == null)
        {
            CreatePurchaseOrderItem(updatedOrderItem, updatedOrderItem.OrderId);
            return;
        }

        if (updatedOrderItem.IsProductDeleted) await DeletePurchaseOrderItem(savedOrderItem);

        savedOrderItem.Quantity = updatedOrderItem.Quantity;
        //todo: consider using UnitListPrice instead of UnitPrice as the latter may include adjustments
        savedOrderItem.UnitPrice = updatedOrderItem.UnitPrice;
        savedOrderItem.LastUpdatedStamp = DateTime.UtcNow;
        savedOrderItem.StatusId = "ITEM_APPROVED";

        await Task.CompletedTask;
    }

    private async Task ApproveJobOrderItem(OrderItemDto2 updatedOrderItem)
    {
        // get order items
        var savedOrderItem = await GetOrderItemById(updatedOrderItem.OrderId, updatedOrderItem.OrderItemSeqId);
        if (savedOrderItem == null)
        {
            await CreateSalesOrderItem(updatedOrderItem, updatedOrderItem.OrderId);
            return;
        }

        if (updatedOrderItem.IsProductDeleted) await DeleteJobOrderItem(savedOrderItem);

        savedOrderItem.Quantity = updatedOrderItem.Quantity;
        savedOrderItem.UnitPrice =
            updatedOrderItem
                .UnitListPrice; // updated to use UnitListPrice instead of UnitPrice as the latter may include adjustments
        savedOrderItem.LastUpdatedStamp = DateTime.UtcNow;
        savedOrderItem.StatusId = "ITEM_APPROVED";

        await Task.CompletedTask;
    }

    private async Task DeletePurchaseOrderItem(OrderItem orderItem)
    {
        // delete order item status
        var orderItemStatuses = await _context.OrderStatuses.Where(x =>
            x.OrderId == orderItem.OrderId && x.OrderItemSeqId == orderItem.OrderItemSeqId).ToListAsync();
        _context.OrderStatuses.RemoveRange(orderItemStatuses);


        // delete order item
        _context.OrderItems.Remove(orderItem);
    }

    private async Task DeleteJobOrderItem(OrderItem orderItem)
    {
        // delete order item status
        var orderItemStatuses = await _context.OrderStatuses.Where(x =>
            x.OrderId == orderItem.OrderId && x.OrderItemSeqId == orderItem.OrderItemSeqId).ToListAsync();
        _context.OrderStatuses.RemoveRange(orderItemStatuses);


        // delete order item
        _context.OrderItems.Remove(orderItem);
    }

    private async Task CreateOrderItemStatus(OrderItemDto2 orderItem, string statusTypeId)
    {
        var stamp = DateTime.UtcNow;
        var orderStatus = new OrderStatus
        {
            OrderStatusId = Guid.NewGuid().ToString(),
            StatusId = statusTypeId,
            OrderId = orderItem.OrderId,
            OrderItemSeqId = orderItem.OrderItemSeqId,
            StatusDatetime = stamp,
            LastUpdatedStamp = stamp,
            CreatedStamp = stamp
        };
        _context.OrderStatuses.Add(orderStatus);
        await Task.CompletedTask;
    }

    private void DeleteOrderAdjustment(OrderAdjustment orderAdjustment)
    {
        // delete quote item
        _context.OrderAdjustments.Remove(orderAdjustment);
    }

    private void SetUnitPriceAsLastPrice(OrderItemDto2 orderItem)
    {
        // get product supplier from order roles
        var productSupplierId = _context.OrderRoles
            .Where(x => x.OrderId == orderItem.OrderId && x.RoleTypeId == "BILL_FROM_VENDOR")
            .Select(x => x.PartyId)
            .FirstOrDefault();

        // get order currency
        var orderCurrency = _context.OrderHeaders
            .Where(x => x.OrderId == orderItem.OrderId)
            .Select(x => x.CurrencyUom)
            .FirstOrDefault();

        // get product supplier
        var selectedProductSuppliers = _context.SupplierProducts
            .Where(ps => ps.ProductId == orderItem.ProductId
                         && ps.PartyId == productSupplierId && ps.AvailableThruDate == null &&
                         ps.CurrencyUomId == orderCurrency)
            .ToList();


        foreach (var supplierProduct in selectedProductSuppliers)
        {
            var nowTimestamp = DateTime.Now;

            if (orderItem.UnitPrice != supplierProduct.LastPrice)
            {
                var newSupplierProduct = CloneSupplierProduct(supplierProduct);
                newSupplierProduct.AvailableFromDate = nowTimestamp;
                newSupplierProduct.LastPrice = orderItem.UnitPrice;
                _context.Add(newSupplierProduct);

                supplierProduct.AvailableThruDate = nowTimestamp;
            }
        }
    }

    private SupplierProduct CloneSupplierProduct(SupplierProduct source)
    {
        var stamp = DateTime.UtcNow;
        return new SupplierProduct
        {
            ProductId = source.ProductId,
            PartyId = source.PartyId,
            SupplierPrefOrderId = source.SupplierPrefOrderId,
            MinimumOrderQuantity = source.MinimumOrderQuantity,
            CurrencyUomId = source.CurrencyUomId,
            LastPrice = source.LastPrice,
            CreatedStamp = stamp,
            LastUpdatedStamp = stamp
        };
    }

    private OrderTerm CreateOrderTerm(string termTypeId, decimal termValue, int termDays, string orderId,
        string orderItemSeqId = "_NA_")
    {
        try
        {
            // Logging can be added here if necessary

            // Creating a new instance of OrderTerm entity to be inserted into the database
            var orderTerm = new OrderTerm
            {
                TermTypeId = termTypeId, // Type of term being created, required input
                TermValue = termValue, // Value of the term (could be monetary, percentage, or textual), optional input
                TermDays = termDays, // Number of days applicable for the term, optional input
                OrderId = orderId, // The order this term is associated with, required input
                OrderItemSeqId = orderItemSeqId // Sequence ID for an order item, optional and defaults to "_NA_"
            };

            // Adding the new OrderTerm entity to the database context
            _context.OrderTerms.Add(orderTerm);

            // Returning the created entity back to the caller
            return orderTerm;
        }
        catch (Exception ex)
        {
            // Logging the exception for debugging and issue resolution
            _logger.LogError(ex, "Error occurred while creating an OrderTerm.");
            throw;
        }
    }
}


// notes for sales order refactoring
// 1. BillingAccountId  needs to be added to OrderHeader; check if purchase order also needs this
// 3. AgreementId &&  Terms needs to be added to OrderHeader -> next release
// 4. CurrencyUom needs to be added to OrderHeader -> next release to allow user to select currency
// 5. CreateOrderNotes
// 6. WorkEffort records is getting passed to CreateOrder 
// 7. orderPaymentInfo is getting passed to CreateOrder -> check its details from the logs 
// 8. check how 'ProductFacility' is handled in the current code
// 10. orderProductPromoCodes & orderItemProductPromoUse --> Next release