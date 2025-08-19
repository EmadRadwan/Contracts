using Domain;
using Microsoft.EntityFrameworkCore;
using Persistence;
using System.Linq.Dynamic.Core;

namespace Application.Order.Orders;

public class OrderReadHelper
{
    // Use two decimal places and rounding mode for decimal math
    public static readonly int scale = 2;
    public static readonly MidpointRounding rounding = MidpointRounding.AwayFromZero;
    public static readonly int taxCalcScale = 2;
    public static readonly int taxFinalScale = 2;
    public static readonly MidpointRounding taxRounding = MidpointRounding.AwayFromZero;
    public static readonly decimal ZERO = decimal.Zero;
    public static readonly decimal percentage = 0.01m;

    protected OrderHeader? orderHeader = null;

    protected List<OrderItemShipGroupAssoc> orderItemAndShipGrp = null;
    protected List<OrderItem> orderItems = null;
    protected List<OrderAdjustment> adjustments = null;
    protected List<OrderPaymentPreference> paymentPrefs = null;
    protected List<OrderStatus> orderStatuses = null;
    protected List<OrderItemPriceInfo> orderItemPriceInfos = null;
    protected List<OrderItemShipGrpInvRes> orderItemShipGrpInvResList = null;
    protected List<ItemIssuance> orderItemIssuances = null;
    protected List<ReturnItem> orderReturnItems = null;
    protected decimal? totalPrice = null;

    // Property for the DbContext, to be injected after instantiation
    public DataContext Context { get; set; }
    private readonly string _orderId;

    // Constructor that accepts the orderId
    public OrderReadHelper(string orderId)
    {
        _orderId = orderId;
    }

    // Parameterless constructor
    public OrderReadHelper()
    {
        _orderId = null;
    }

    // Initialize the order based on the provided orderId and injected Context
    public void InitializeOrder()
    {
        if (string.IsNullOrEmpty(_orderId))
        {
            return; // Skip initialization if no orderId
        }

        if (Context == null)
            throw new InvalidOperationException("DbContext has not been set.");

        orderHeader = Context.OrderHeaders
            .FirstOrDefault(o => o.OrderId == _orderId);

        if (orderHeader == null)
        {
            throw new ArgumentException($"Order not found with orderId [{_orderId}]");
        }
    }

    public async Task<Party> GetBillToParty()
    {
        return await this.GetPartyFromRole("BILL_TO_CUSTOMER");
    }

    public async Task<Party> GetBillFromParty()
    {
        return await this.GetPartyFromRole("BILL_FROM_VENDOR");
    }

    // Async method to get the party based on a specific role
    public async Task<Party> GetPartyFromRole(string roleTypeId)
    {
        if (Context == null)
            throw new InvalidOperationException("DbContext has not been set.");

        // Asynchronously join PartyRoles with OrderRoles to filter by role and order
        var partyRole = await (from pr in Context.OrderRoles
            where pr.RoleTypeId == roleTypeId
                  && pr.OrderId == this.orderHeader.OrderId
            select pr).FirstOrDefaultAsync();

        // Asynchronously return the associated Party entity for the role
        if (partyRole != null)
        {
            return await Context.Parties
                .FirstOrDefaultAsync(p => p.PartyId == partyRole.PartyId);
        }

        return null;
    }

    public async Task<List<OrderItemShipGroupAssoc>> GetOrderItemAndShipGroupAssoc()
    {
        if (orderItemAndShipGrp == null)
        {
            try
            {
                string orderId = orderHeader.OrderId;
                orderItemAndShipGrp = await Context.OrderItemShipGroupAssocs
                    .Where(x => x.OrderId == orderId)
                    .ToListAsync();
            }
            catch (Exception e)
            {
                // Log the warning if needed
                Console.WriteLine($"Warning: {e.Message}");
            }
        }

        return orderItemAndShipGrp;
    }

    public async Task<List<OrderItemShipGroupAssoc>> GetOrderItemAndShipGroupAssoc(string shipGroupSeqId)
    {
        // Reuse the cached list if it exists
        var baseList = await GetOrderItemAndShipGroupAssoc();
        return baseList
            .Where(x => x.ShipGroupSeqId == shipGroupSeqId)
            .ToList();
    }

    public async Task<decimal> GetShippableTotal()
    {
        decimal shippableTotal = ZERO;

        // Get valid order items for the ship group
        List<OrderItem> validItems = await GetValidOrderItems();

        if (validItems != null && validItems.Count > 0)
        {
            foreach (var item in validItems)
            {
                Product product = null;

                try
                {
                    // Get the related Product from OrderItem (assuming a navigation property)
                    product = await Context.Products
                        .FirstOrDefaultAsync(p => p.ProductId == item.ProductId);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Problem getting Product from OrderItem; returning 0: {ex.Message}");
                    return ZERO;
                }

                if (product != null)
                {
                    // Calculate the subtotal and add it to the shippable total
                    shippableTotal += await GetOrderItemSubTotal(item, await GetAdjustments(), false, true);
                }
            }
        }

        return Math.Round(shippableTotal, scale, rounding);
    }

    public async Task<List<OrderItem>> GetValidOrderItems()
    {
        // Fetch the list of order items asynchronously
        var orderItems = await GetOrderItems();

        // Filter out canceled and rejected items using LINQ
        return orderItems
            .Where(item => item.StatusId != "ITEM_CANCELLED" && item.StatusId != "ITEM_REJECTED")
            .ToList();
    }


    public async Task<List<OrderItem>> GetOrderItems()
    {
        // Check if the order items have already been fetched (caching behavior)
        if (orderItems == null)
        {
            try
            {
                // Fetch related OrderItem entities where the OrderId matches the current order header
                orderItems = await Context.OrderItems
                    .Where(item => item.OrderId == orderHeader.OrderId)
                    .OrderBy(item =>
                        item.OrderItemSeqId) // Sort by orderItemSeqId, similar to UtilMisc.toList("orderItemSeqId")
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: {ex.Message}");
            }
        }

        return orderItems;
    }

    public async Task<decimal> GetOrderItemsSubTotal()
    {
        // Fetch valid order items asynchronously
        var validOrderItems = await GetValidOrderItems();

        // Get the adjustments
        var adjustments = await GetAdjustments();

        // Calculate the subtotal for all valid order items
        return await GetOrderItemsSubTotal(validOrderItems, adjustments);
    }

    public async Task<List<OrderAdjustment>> GetAdjustments()
    {
        // Check if adjustments have already been loaded (lazy loading)
        if (adjustments == null)
        {
            try
            {
                // Fetch related OrderAdjustment entities based on the current OrderHeader's OrderId
                adjustments = await Context.OrderAdjustments
                    .Where(adj => adj.OrderId == orderHeader.OrderId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching adjustments: {ex.Message}");
            }

            // If no adjustments were found, initialize an empty list
            if (adjustments == null)
            {
                adjustments = new List<OrderAdjustment>();
            }
        }

        return adjustments;
    }

    public async Task<decimal> GetOrderItemSubTotal(OrderItem orderItem)
    {
        // Call the overloaded method with adjustments
        var adjustments = await GetAdjustments();
        return await GetOrderItemSubTotal(orderItem, adjustments);
    }

    public async Task<decimal> GetOrderItemSubTotal(OrderItem orderItem, List<OrderAdjustment> adjustments)
    {
        // Call the overloaded method with default values for forTax and forShipping
        return await GetOrderItemSubTotal(orderItem, adjustments, false, false);
    }

    public async Task<decimal> GetOrderItemSubTotal(OrderItem orderItem, List<OrderAdjustment> adjustments, bool forTax,
        bool forShipping)
    {
        try
        {
            // Get the unit price and quantity for the order item
            decimal unitPrice = orderItem.UnitPrice ?? 0m;
            decimal quantity = GetOrderItemQuantity(orderItem);
            decimal result = 0m;

            if (unitPrice == 0m || quantity == 0m)
            {
                Console.WriteLine(
                    "[GetOrderItemSubTotal] unitPrice or quantity are null or zero, using 0 for the item base price");
            }
            else
            {
                //Console.WriteLine($"Unit Price: {unitPrice}, Quantity: {quantity}");
                result = unitPrice * quantity;

                // Handle rental order item logic
                if (orderItem.OrderItemTypeId == "RENTAL_ORDER_ITEM")
                {
                    var workOrderItemFulfillments = await Context.WorkOrderItemFulfillments
                        .Where(w => w.OrderId == orderItem.OrderId && w.OrderItemSeqId == orderItem.OrderItemSeqId)
                        .ToListAsync();

                    if (workOrderItemFulfillments != null && workOrderItemFulfillments.Any())
                    {
                        var workEffort = await Context.WorkEfforts
                            .FirstOrDefaultAsync(
                                we => we.WorkEffortId == workOrderItemFulfillments.First().WorkEffortId);

                        if (workEffort != null)
                        {
                            result *= GetWorkEffortRentalQuantity(workEffort);
                        }
                    }
                }
            }

            // Add adjustments to the subtotal
            result += GetOrderItemAdjustmentsTotal(orderItem, adjustments, true, true, false, forTax, forShipping);

            return Math.Round(result, 2, MidpointRounding.AwayFromZero); // Assuming a scale of 2 for rounding
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"[GetOrderItemSubTotal] Error calculating subtotal for OrderItem: {orderItem.OrderItemSeqId}. Exception: {ex.Message}");
            return 0m; // Return 0 as a fallback in case of error
        }
    }


    public decimal GetOrderItemQuantity(OrderItem orderItem)
    {
        // Fetch the cancel quantity and order quantity from the order item
        decimal cancelQty = orderItem.CancelQuantity ?? 0m;
        decimal orderQty = orderItem.Quantity ?? 0m;

        // Return the difference between the order quantity and cancel quantity
        return orderQty - cancelQty;
    }

    public decimal GetWorkEffortRentalQuantity(WorkEffort workEffort)
    {
        // Initialize persons as 1 by default
        decimal persons = 1m;
        if (workEffort.ReservPersons.HasValue)
        {
            persons = workEffort.ReservPersons.Value;
        }

        // Initialize secondPersonPerc and nthPersonPerc as 0 by default
        decimal secondPersonPerc = 0m;
        if (workEffort.Reserv2ndPPPerc.HasValue)
        {
            secondPersonPerc = workEffort.Reserv2ndPPPerc.Value;
        }

        decimal nthPersonPerc = 0m;
        if (workEffort.ReservNthPPPerc.HasValue)
        {
            nthPersonPerc = workEffort.ReservNthPPPerc.Value;
        }

        // Calculate the rental length in days
        long length = 1;
        if (workEffort.EstimatedStartDate.HasValue && workEffort.EstimatedCompletionDate.HasValue)
        {
            var duration = workEffort.EstimatedCompletionDate.Value - workEffort.EstimatedStartDate.Value;
            length = (long)duration.TotalDays;
        }

        // Initialize rentalAdjustment as 0 by default
        decimal rentalAdjustment = 0m;

        // Calculate rental adjustment based on the number of persons
        if (persons > 1m)
        {
            if (persons > 2m)
            {
                persons -= 2m;
                if (nthPersonPerc > 0m)
                {
                    rentalAdjustment = persons * nthPersonPerc;
                }
                else
                {
                    rentalAdjustment = persons * secondPersonPerc;
                }

                persons = 2m;
            }

            if (persons == 2m)
            {
                rentalAdjustment += secondPersonPerc;
            }
        }

        // Add 100% for the first person and adjust for length
        rentalAdjustment += 100m;
        rentalAdjustment = Decimal.Round(scale, rounding) * length;

        return rentalAdjustment;
    }

    public decimal GetOrderItemAdjustmentsTotal(OrderItem orderItem, List<OrderAdjustment> adjustments,
        bool includeOther, bool includeTax, bool includeShipping)
    {
        // Call the overloaded method with default values for forTax and forShipping
        return GetOrderItemAdjustmentsTotal(orderItem, adjustments, includeOther, includeTax, includeShipping,
            forTax: false, forShipping: false);
    }

    public decimal GetOrderItemAdjustmentsTotal(OrderItem orderItem, List<OrderAdjustment> adjustments,
        bool includeOther, bool includeTax, bool includeShipping, bool forTax, bool forShipping)
    {
        // Get the order item quantity and unit price
        decimal quantity = GetOrderItemQuantity(orderItem);
        decimal unitPrice = orderItem.UnitPrice ?? 0m;

        // Get the relevant adjustments for this order item
        var itemAdjustments = GetOrderItemAdjustmentList(orderItem, adjustments);

        // Calculate and return the total adjustments for the item
        return CalcItemAdjustments(quantity, unitPrice, itemAdjustments, includeOther, includeTax, includeShipping,
            forTax, forShipping);
    }

    public List<OrderAdjustment> GetOrderItemAdjustmentList(OrderItem orderItem, List<OrderAdjustment> adjustments)
    {
        // Filter the adjustments where the orderItemSeqId matches the orderItem's sequence ID
        return adjustments
            .Where(adj => adj.OrderItemSeqId == orderItem.OrderItemSeqId)
            .ToList();
    }

    public decimal CalcItemAdjustments(decimal quantity, decimal unitPrice, List<OrderAdjustment> adjustments,
        bool includeOther, bool includeTax, bool includeShipping, bool forTax, bool forShipping)
    {
        decimal adjTotal = 0m;

        // Check if there are adjustments to process
        if (adjustments != null && adjustments.Any())
        {
            // Filter the adjustments based on the flags provided
            var filteredAdjs = FilterOrderAdjustments(adjustments, includeOther, includeTax, includeShipping, forTax,
                forShipping);

            // Iterate over the filtered adjustments and calculate the total adjustment
            foreach (var orderAdjustment in filteredAdjs)
            {
                adjTotal += CalcItemAdjustment(orderAdjustment, quantity, unitPrice);
            }
        }

        return adjTotal;
    }

    public List<OrderAdjustment> FilterOrderAdjustments(List<OrderAdjustment> adjustments, bool includeOther,
        bool includeTax, bool includeShipping, bool forTax, bool forShipping)
    {
        List<OrderAdjustment> newOrderAdjustmentsList = new List<OrderAdjustment>();

        if (adjustments != null && adjustments.Any())
        {
            foreach (var orderAdjustment in adjustments)
            {
                bool includeAdjustment = false;

                // Check for tax-related adjustments
                if (orderAdjustment.OrderAdjustmentTypeId == "SALES_TAX" ||
                    orderAdjustment.OrderAdjustmentTypeId == "VAT_TAX" ||
                    orderAdjustment.OrderAdjustmentTypeId == "VAT_PRICE_CORRECT")
                {
                    if (includeTax)
                    {
                        includeAdjustment = true;
                    }
                }
                // Check for shipping-related adjustments
                else if (orderAdjustment.OrderAdjustmentTypeId == "SHIPPING_CHARGES")
                {
                    if (includeShipping)
                    {
                        includeAdjustment = true;
                    }
                }
                // Check for other adjustments
                else
                {
                    if (includeOther)
                    {
                        includeAdjustment = true;
                    }
                }

                // Exclude adjustments for tax if `includeInTax` is set to "N"
                if (forTax && orderAdjustment.IncludeInTax == "N")
                {
                    includeAdjustment = false;
                }

                // Exclude adjustments for shipping if `includeInShipping` is set to "N"
                if (forShipping && orderAdjustment.IncludeInShipping == "N")
                {
                    includeAdjustment = false;
                }

                // Add adjustment to the new list if it should be included
                if (includeAdjustment)
                {
                    newOrderAdjustmentsList.Add(orderAdjustment);
                }
            }
        }

        return newOrderAdjustmentsList;
    }

    public decimal CalcItemAdjustment(OrderAdjustment itemAdjustment, OrderItem item)
    {
        // Call the overloaded method using the quantity and unit price from the order item
        var orderItemQuantity = GetOrderItemQuantity(item);
        return CalcItemAdjustment(itemAdjustment, orderItemQuantity, item.UnitPrice ?? 0m);
    }

    public decimal CalcItemAdjustment(OrderAdjustment itemAdjustment, decimal quantity, decimal unitPrice)
    {
        decimal adjustment = 0m;

        // Check if the adjustment is based on a fixed amount
        if (itemAdjustment.Amount.HasValue)
        {
            // No rounding here, wait until the item total is calculated
            adjustment += itemAdjustment.Amount.Value;
        }
        // If the adjustment is based on a percentage, calculate it using quantity and unit price
        else if (itemAdjustment.SourcePercentage.HasValue)
        {
            // Calculate adjustment = sourcePercentage * quantity * unitPrice * percentage (0.01)
            adjustment += itemAdjustment.SourcePercentage.Value * quantity * unitPrice * 0.01m;
        }

        // Debugging output (optional)
        Console.WriteLine($"CalcItemAdjustment: Adjustment={adjustment}, Quantity={quantity}, UnitPrice={unitPrice}");

        return adjustment;
    }

    public async Task<decimal> GetShippableQuantity()
    {
        decimal shippableQuantity = 0m;

        // Get the list of ship groups related to the order item
        var shipGroups = await GetOrderItemShipGroups();
        if (shipGroups != null && shipGroups.Any())
        {
            foreach (var shipGroup in shipGroups)
            {
                shippableQuantity += await GetShippableQuantity(shipGroup.ShipGroupSeqId);
            }
        }

        return Math.Round(shippableQuantity, 2, MidpointRounding.AwayFromZero); // Assuming scale = 2
    }

    public async Task<decimal> GetShippableQuantity(string shipGroupSeqId)
    {
        decimal shippableQuantity = 0m;

        // Get the valid order items for the given ship group sequence ID
        var validItems = await GetValidOrderItems();
        if (validItems != null && validItems.Any())
        {
            foreach (var item in validItems)
            {
                Product? product = null;

                try
                {
                    // Fetch the related Product entity for the order item
                    product = await Context.Products.FindAsync(item.ProductId);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Problem getting Product from OrderItem; returning 0: {ex.Message}");
                    return 0m;
                }

                if (product != null)
                {
                    // Add the quantity of the item to the total shippable quantity
                    shippableQuantity += GetOrderItemQuantity(item);
                }
            }
        }

        return Math.Round(shippableQuantity, 2, MidpointRounding.AwayFromZero); // Assuming scale = 2
    }

    public async Task<List<OrderItemShipGroupAssoc>> GetOrderItemShipGroups()
    {
        try
        {
            // Fetch related OrderItemShipGroupAssoc entities based on the OrderId
            return await Context.OrderItemShipGroupAssocs
                .Where(assoc => assoc.OrderId == orderHeader.OrderId)
                .OrderBy(assoc =>
                    assoc.ShipGroupSeqId) // Ordering by ShipGroupSeqId, similar to UtilMisc.toList("shipGroupSeqId")
                .ToListAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: {ex.Message}");
            return new List<OrderItemShipGroupAssoc>(); // Return an empty list in case of an error
        }
    }

    public async Task<decimal> GetTotalOrderItemsQuantity()
    {
        // Fetch valid order items
        var orderItems = await GetValidOrderItems();
        decimal totalItems = 0m;

        // Iterate through the order items and sum their quantities
        foreach (var orderItem in orderItems)
        {
            totalItems += GetOrderItemQuantity(orderItem);
        }

        // Return the total quantity, rounded to 2 decimal places
        return Math.Round(totalItems, 2, MidpointRounding.AwayFromZero);
    }

    public async Task<double?> GetOrderTermNetDays()
    {
        // Await the asynchronous GetOrderTerms method to get the list of order terms
        var orderTerms = await GetOrderTerms();

        // Filter order terms based on the termTypeId "FIN_PAYMENT_TERM"
        var filteredTerms = orderTerms
            .Where(term => term.TermTypeId == "FIN_PAYMENT_TERM")
            .ToList();

        // If no terms are found, return null
        if (!filteredTerms.Any())
        {
            return null;
        }

        // If more than one term is found, log a warning and use the first one
        if (filteredTerms.Count > 1)
        {
            Console.WriteLine(
                $"Warning: Found {filteredTerms.Count} FIN_PAYMENT_TERM order terms for orderId [{orderHeader.OrderId}], using the first one.");
        }

        // Return the termDays of the first order term
        return filteredTerms[0].TermDays;
    }

    public async Task<List<OrderTerm>> GetOrderTerms()
    {
        try
        {
            // Fetch the related OrderTerm entities for the given OrderHeader
            return await Context.OrderTerms
                .Where(term => term.OrderId == orderHeader.OrderId)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching order terms: {ex.Message}");
            return new List<OrderTerm>(); // Return an empty list in case of an error
        }
    }

    public async Task<decimal> GetOrderItemsSubTotal(List<OrderItem> orderItems, List<OrderAdjustment> adjustments)
    {
        var getOrderItemsSubTotal = await GetOrderItemsSubTotal(orderItems, adjustments, null);
        return getOrderItemsSubTotal;
    }

    public async Task<decimal> GetOrderItemsSubTotal(List<OrderItem> orderItems, List<OrderAdjustment> adjustments,
        List<WorkEffort> workEfforts)
    {
        decimal result = 0m;

        // Iterate over the order items and calculate the subtotal
        foreach (var orderItem in orderItems)
        {
            // Calculate the subtotal for the order item
            decimal itemTotal = await GetOrderItemSubTotal(orderItem, adjustments);

            // Handle rental-specific logic
            if (workEfforts != null && orderItem.OrderItemTypeId == "RENTAL_ORDER_ITEM")
            {
                var workEffort = workEfforts.FirstOrDefault(we => we.WorkEffortId == orderItem.OrderItemSeqId);
                if (workEffort != null)
                {
                    itemTotal *= GetWorkEffortRentalQuantity(workEffort);
                    itemTotal = Math.Round(itemTotal, 2, MidpointRounding.AwayFromZero); // Adjust scale and rounding
                }
            }

            result += Math.Round(itemTotal, 2, MidpointRounding.AwayFromZero);
        }

        return Math.Round(result, 2, MidpointRounding.AwayFromZero); // Final rounding for the result
    }

    public async Task<List<OrderAdjustment>> GetOrderHeaderAdjustments()
    {
        var adjustments = await GetAdjustments();
        var orderHeaderAdjustments = await GetOrderHeaderAdjustments(adjustments, null);
        return orderHeaderAdjustments;
    }

    public async Task<List<OrderAdjustment>> GetOrderHeaderAdjustments(string shipGroupSeqId)
    {
        return await GetOrderHeaderAdjustments(await GetAdjustments(), shipGroupSeqId);
    }

    public async Task<List<OrderAdjustment>> GetOrderHeaderAdjustments(List<OrderAdjustment> adjustments,
        string shipGroupSeqId)
    {
        // Filter by shipGroupSeqId if it's not null
        var toFilter = adjustments;
        if (!string.IsNullOrEmpty(shipGroupSeqId))
        {
            toFilter = toFilter.Where(adj => adj.ShipGroupSeqId == shipGroupSeqId).ToList();
        }

        // Filter adjustments where orderItemSeqId is null, empty, or equals "_NA_"
        var filteredAdjustments = toFilter
            .Where(adj => string.IsNullOrEmpty(adj.OrderItemSeqId) || adj.OrderItemSeqId == "_NA_")
            .ToList();

        return filteredAdjustments;
    }

    public async Task<List<PostalAddress>> GetBillingLocations()
    {
        var billingLocations = new List<PostalAddress>();

        // Retrieve order contact mechanisms with "BILLING_LOCATION" purpose
        var billingCms = await GetOrderContactMechs("BILLING_LOCATION");

        if (billingCms != null && billingCms.Any())
        {
            foreach (var ocm in billingCms)
            {
                if (ocm != null)
                {
                    try
                    {
                        // Find the PostalAddress associated with the contactMechId
                        var postalAddress = await Context.PostalAddresses
                            .FirstOrDefaultAsync(addr => addr.ContactMechId == ocm.ContactMechId);

                        if (postalAddress != null)
                        {
                            billingLocations.Add(postalAddress);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(
                            $"Warning: Problem retrieving PostalAddress for contactMechId {ocm.ContactMechId}: {ex.Message}");
                    }
                }
            }
        }

        return billingLocations;
    }

    public async Task<List<OrderContactMech>> GetOrderContactMechs(string purposeTypeId)
    {
        try
        {
            // Fetch order contact mechanisms that match the given purposeTypeId
            return await Context.OrderContactMeches
                .Where(ocm => ocm.OrderId == orderHeader.OrderId && ocm.ContactMechPurposeTypeId == purposeTypeId)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"Warning: Problem retrieving OrderContactMechs for purposeTypeId {purposeTypeId}: {ex.Message}");
            return null;
        }
    }

    public async Task<List<OrderAdjustment>> GetAvailableOrderHeaderAdjustments()
    {
        if (Context == null)
            throw new InvalidOperationException("DbContext has not been set.");

        try
        {
            // Fetch order header adjustments
            var orderHeaderAdjustments = await GetOrderHeaderAdjustments();

            var filteredAdjustments = new List<OrderAdjustment>();

            foreach (var orderAdjustment in orderHeaderAdjustments)
            {
                // Count matching return adjustments based on the current order adjustment
                var count = await Context.ReturnAdjustments
                    .CountAsync(ra => ra.OrderAdjustmentId == orderAdjustment.OrderAdjustmentId);

                // Include the adjustment if no related return adjustments are found
                if (count == 0)
                {
                    filteredAdjustments.Add(orderAdjustment);
                }
            }

            return filteredAdjustments;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching available order header adjustments: {ex.Message}");
            return new List<OrderAdjustment>();
        }
    }

    public decimal GetAllOrderItemsAdjustmentsTotal(
        List<OrderItem> orderItems,
        List<OrderAdjustment> adjustments,
        bool includeOther,
        bool includeTax,
        bool includeShipping)
    {
        if (orderItems == null || !orderItems.Any())
            return ZERO;

        decimal result = ZERO;

        // Iterate through the order items
        foreach (var orderItem in orderItems)
        {
            // Add the total adjustments for the current order item
            result += GetOrderItemAdjustmentsTotal(orderItem, adjustments, includeOther, includeTax, includeShipping);
        }

        // Return the result rounded to the specified scale and rounding mode
        return Math.Round(result, scale, rounding);
    }

    public decimal CalcOrderAdjustments(
        List<OrderAdjustment> orderHeaderAdjustments,
        decimal subTotal,
        bool includeOther,
        bool includeTax,
        bool includeShipping)
    {
        decimal adjTotal = ZERO;

        if (orderHeaderAdjustments != null && orderHeaderAdjustments.Any())
        {
            // Filter the adjustments based on inclusion flags
            var filteredAdjs = FilterOrderAdjustments(
                orderHeaderAdjustments,
                includeOther,
                includeTax,
                includeShipping,
                forTax: false,
                forShipping: false);

            foreach (var orderAdjustment in filteredAdjs)
            {
                // Calculate the adjustment for each filtered adjustment and add to the total
                adjTotal += CalcOrderAdjustment(orderAdjustment, subTotal);
                adjTotal = Math.Round(adjTotal, scale, rounding); // Maintain the required scale and rounding
            }
        }

        return Math.Round(adjTotal, scale, rounding); // Return the final total rounded
    }

    public decimal CalcOrderAdjustment(OrderAdjustment orderAdjustment, decimal orderSubTotal)
    {
        decimal adjustment = ZERO;

        // If the adjustment has a fixed amount
        if (orderAdjustment.Amount.HasValue)
        {
            adjustment += orderAdjustment.Amount.Value;
        }
        // If the adjustment is based on a percentage
        else if (orderAdjustment.SourcePercentage.HasValue)
        {
            var percent = orderAdjustment.SourcePercentage.Value;
            var amount = orderSubTotal * percent * percentage;
            adjustment += amount;
        }

        // If the adjustment type is "SALES_TAX", apply tax-specific rounding and scale
        if (orderAdjustment.OrderAdjustmentTypeId == "SALES_TAX")
        {
            return Math.Round(adjustment, taxCalcScale, taxRounding);
        }

        // Apply default rounding and scale
        return Math.Round(adjustment, scale, rounding);
    }

    public async Task<decimal> GetOrderGrandTotal()
    {
        if (totalPrice == null)
        {
            var orderGrandTotal = await GetValidOrderItems();
            var adjustments = await GetAdjustments();
            totalPrice = await GetOrderGrandTotal(orderGrandTotal, adjustments);
        }

        return totalPrice.Value;
    }

    public async Task<decimal> GetOrderGrandTotal(List<OrderItem> orderItems, List<OrderAdjustment> adjustments)
    {
        // Calculate the total tax amount from the adjustments
        decimal taxGrandTotal = GetTaxGrandTotal(adjustments);

        // Exclude tax adjustments from the adjustments list
        var nonTaxAdjustments = adjustments
            .Where(adj => adj.OrderAdjustmentTypeId != "SALES_TAX")
            .ToList();

        // Calculate the subtotal of order items, including adjustments (excluding tax adjustments)
        decimal total = await GetOrderItemsSubTotal(orderItems, nonTaxAdjustments);

        // Get order header adjustments excluding tax adjustments
        var orderHeaderAdjustments = await GetOrderHeaderAdjustments();
        orderHeaderAdjustments = orderHeaderAdjustments
            .Where(adj => adj.OrderAdjustmentTypeId != "SALES_TAX")
            .ToList();

        // Calculate the total of order header adjustments (excluding tax adjustments)
        decimal adj = CalcOrderAdjustments(
            orderHeaderAdjustments,
            total,
            includeOther: true,
            includeTax: false,
            includeShipping: true);

        // Sum up total + adj + taxGrandTotal
        total = total + adj + taxGrandTotal;

        // Apply rounding and scaling
        total = Math.Round(total, scale, rounding);

        return total;
    }

    // Helper method to calculate the total tax from adjustments
    public decimal GetTaxGrandTotal(List<OrderAdjustment> adjustments)
    {
        decimal taxGrandTotal = ZERO;

        // Filter adjustments where OrderAdjustmentTypeId == "SALES_TAX"
        var taxAdjustments = adjustments
            .Where(adj => adj.OrderAdjustmentTypeId == "SALES_TAX");

        foreach (var adj in taxAdjustments)
        {
            if (adj.Amount.HasValue)
            {
                taxGrandTotal += adj.Amount.Value;
            }
        }

        // Apply tax-specific rounding and scaling
        return Math.Round(taxGrandTotal, taxFinalScale, taxRounding);
    }

    /** Gets the total return credited amount with refunds and credits to the billing account figured in */
    public async Task<decimal> GetReturnedCreditTotalWithBillingAccountBd()
    {
        // Calculate the total returned credit for the order
        var orderReturnedCreditTotal = GetOrderReturnedTotalByTypeBd("RTN_CREDIT", false);

        // Calculate the total returned refund for the billing account
        var billingAccountReturnedRefundTotal = GetBillingAccountReturnedTotalByTypeBd("RTN_REFUND");

        // Calculate the total returned credit for the billing account
        var billingAccountReturnedCreditTotal = GetBillingAccountReturnedTotalByTypeBd("RTN_CREDIT");

        // Return the sum of order returned credit and billing account returned refund,
        // subtracting the billing account returned credit total
        return Math.Round(
            orderReturnedCreditTotal + billingAccountReturnedRefundTotal - billingAccountReturnedCreditTotal,
            scale,
            rounding);
    }

    /** Gets the total return credit for COMPLETED and RECEIVED returns. */
    public decimal GetOrderReturnedCreditTotalBd()
    {
        var orderReturnedTotalByTypeBd = GetOrderReturnedTotalByTypeBd("RTN_CREDIT", false);
        return orderReturnedTotalByTypeBd;
    }

    public async Task<decimal> GetOrderReturnedTotal(bool includeAll)
    {
        return GetOrderReturnedTotalByTypeBd(null, includeAll);
    }

    public decimal GetOrderReturnedTotalByTypeBd(string returnTypeId, bool includeAll)
    {
        decimal returnedAmount = ZERO;

        // Get the base list of return items
        List<ReturnItem> returnedItemsBase = GetOrderReturnItems();

        // Filter by returnTypeId if specified
        if (!string.IsNullOrEmpty(returnTypeId))
        {
            returnedItemsBase = returnedItemsBase
                .Where(ri => ri.ReturnTypeId == returnTypeId)
                .ToList();
        }

        List<ReturnItem> returnedItems = new List<ReturnItem>();

        if (!includeAll)
        {
            // Get items with status RETURN_ACCEPTED, RETURN_RECEIVED, or RETURN_COMPLETED
            var acceptedStatuses = new[] { "RETURN_ACCEPTED", "RETURN_RECEIVED", "RETURN_COMPLETED" };
            returnedItems.AddRange(returnedItemsBase
                .Where(ri => acceptedStatuses.Contains(ri.StatusId)));
        }
        else
        {
            // Get all items except those with status RETURN_CANCELLED
            returnedItems.AddRange(returnedItemsBase
                .Where(ri => ri.StatusId != "RETURN_CANCELLED"));
        }

        string orderId = orderHeader.OrderId;
        List<string> returnHeaderList = new List<string>();

        foreach (var returnedItem in returnedItems)
        {
            if (returnedItem.ReturnPrice.HasValue && returnedItem.ReturnQuantity.HasValue)
            {
                decimal amount = returnedItem.ReturnPrice.Value * returnedItem.ReturnQuantity.Value;
                amount = Math.Round(amount, scale, rounding);
                returnedAmount += amount;
            }

            var itemAdjustmentCondition = new Dictionary<string, object>
            {
                { "ReturnId", returnedItem.ReturnId },
                { "ReturnItemSeqId", returnedItem.ReturnItemSeqId }
            };

            if (!string.IsNullOrEmpty(returnTypeId))
            {
                itemAdjustmentCondition.Add("ReturnTypeId", returnTypeId);
            }

            decimal itemAdjustmentTotal = GetReturnAdjustmentTotal(itemAdjustmentCondition);
            returnedAmount += itemAdjustmentTotal;

            if (orderId == returnedItem.OrderId && !returnHeaderList.Contains(returnedItem.ReturnId))
            {
                returnHeaderList.Add(returnedItem.ReturnId);
            }
        }

        // Get returned amount from return header adjustments where orderId matches current orderHeader.OrderId
        foreach (var returnId in returnHeaderList)
        {
            var returnHeaderAdjFilter = new Dictionary<string, object>
            {
                { "ReturnId", returnId },
                { "ReturnItemSeqId", "_NA_" }
            };

            if (!string.IsNullOrEmpty(returnTypeId))
            {
                returnHeaderAdjFilter.Add("ReturnTypeId", returnTypeId);
            }

            decimal headerAdjustmentTotal = GetReturnAdjustmentTotal(returnHeaderAdjFilter);
            headerAdjustmentTotal = Math.Round(headerAdjustmentTotal, scale, rounding);
            returnedAmount += headerAdjustmentTotal;
        }

        returnedAmount = Math.Round(returnedAmount, scale, rounding);
        return returnedAmount;
    }

    /** Gets the total refunded to the order billing account by type.  Specify null to get total over all types. */
    public decimal GetBillingAccountReturnedTotalByTypeBd(string returnTypeId)
    {
        decimal returnedAmount = 0m;

        try
        {
            // Retrieve all order return items
            var returnedItemsBase = GetOrderReturnItems();

            // Filter by returnTypeId if provided
            if (!string.IsNullOrEmpty(returnTypeId))
            {
                returnedItemsBase = returnedItemsBase
                    .Where(item => item.ReturnTypeId == returnTypeId)
                    .ToList();
            }

            // Filter items with status "RETURN_RECEIVED" or "RETURN_COMPLETED"
            var returnedItems = returnedItemsBase
                .Where(item => item.StatusId == "RETURN_RECEIVED" || item.StatusId == "RETURN_COMPLETED")
                .ToList();

            // Sum up the response amounts for items with a billing account
            foreach (var returnItem in returnedItems)
            {
                // Retrieve the associated ReturnItemResponse
                var returnItemResponse = returnItem.ReturnItemResponse;

                if (returnItemResponse == null)
                    continue;

                if (string.IsNullOrEmpty(returnItemResponse.BillingAccountId))
                    continue;

                // Add the response amount, applying scale and rounding
                decimal responseAmount = returnItemResponse.ResponseAmount ?? 0m;
                responseAmount = Math.Round(responseAmount, scale, MidpointRounding.AwayFromZero);

                returnedAmount += responseAmount;
            }
        }
        catch (Exception ex)
        {
            throw;
        }

        return returnedAmount;
    }

    public decimal GetReturnAdjustmentTotal(IDictionary<string, object> condition)
    {
        decimal total = 0m;

        try
        {
            // Start with all ReturnAdjustments
            var query = Context.ReturnAdjustments.AsQueryable();

            // Apply conditions dynamically using Dynamic LINQ
            foreach (var kvp in condition)
            {
                string propertyName = kvp.Key;
                object value = kvp.Value;

                // Use Dynamic LINQ to build the query
                query = query.Where($"{propertyName} == @0", value);
            }

            // Retrieve the filtered adjustments
            var adjustments = query.ToList();

            // Calculate the total
            foreach (var returnAdjustment in adjustments)
            {
                bool isSalesTaxAdj = returnAdjustment.ReturnAdjustmentTypeId == "RET_SALES_TAX_ADJ";
                decimal amount = returnAdjustment.Amount ?? 0m;
                decimal adjustedAmount = SetScaleByType(isSalesTaxAdj, amount);

                total += adjustedAmount;
            }
        }
        catch (Exception ex)
        {
            throw;
        }

        return total;
    }

    private decimal SetScaleByType(bool isSalesTaxAdj, decimal amount)
    {
        int scale = isSalesTaxAdj ? 4 : 2; // Use 4 decimal places for sales tax adjustments
        return Math.Round(amount, scale, MidpointRounding.AwayFromZero);
    }

    public List<ReturnItem> GetOrderReturnItems()
    {
        if (this.orderReturnItems == null)
        {
            try
            {
                // Retrieve the orderId from the orderHeader
                string orderId = orderHeader.OrderId;

                // Query the database using Entity Framework to get ReturnItems with the specified orderId
                this.orderReturnItems = Context.ReturnItems
                    .Where(ri => ri.OrderId == orderId)
                    .ToList();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        return this.orderReturnItems;
    }

    /** Gets the total return refund amount with refunds and credits to the billing account figured in */
    public decimal GetReturnedRefundTotalWithBillingAccountBd()
    {
        var oetOrderReturnedRefundTotalBd = GetOrderReturnedRefundTotalBd();
        var billingAccountReturnedCreditTotalBd = GetBillingAccountReturnedCreditTotalBd();
        var billingAccountReturnedRefundTotalBd = GetBillingAccountReturnedRefundTotalBd();

        var returnedRefundTotalWithBillingAccountBd =
            oetOrderReturnedRefundTotalBd + billingAccountReturnedCreditTotalBd
            - billingAccountReturnedRefundTotalBd;

        return returnedRefundTotalWithBillingAccountBd;
    }

    /** Gets the total return refunded for COMPLETED and RECEIVED returns. */
    public decimal GetOrderReturnedRefundTotalBd()
    {
        var orderReturnedTotalByTypeBd = GetOrderReturnedTotalByTypeBd("RTN_REFUND", false);
        return orderReturnedTotalByTypeBd;
    }

    /// <summary>
    /// Calculates the total billing account returned credit amount for return type "RTN_CREDIT".
    /// </summary>
    /// <returns>The total returned credit amount as a decimal.</returns>
    public decimal GetBillingAccountReturnedCreditTotalBd()
    {
        var billingAccountReturnedTotalByTypeBd = GetBillingAccountReturnedTotalByTypeBd("RTN_CREDIT");
        return billingAccountReturnedTotalByTypeBd;
    }

    /// <summary>
    /// Calculates the total returned refund amount for return type "RTN_REFUND" applied to the billing account.
    /// </summary>
    /// <returns>The total returned refund amount as a decimal.</returns>
    public decimal GetBillingAccountReturnedRefundTotalBd()
    {
        var billingAccountReturnedTotalByTypeBd = GetBillingAccountReturnedTotalByTypeBd("RTN_REFUND");
        return billingAccountReturnedTotalByTypeBd;
    }

    public async Task<List<ItemIssuance>> GetOrderItemIssuances(OrderItem orderItem)
    {
        return await GetOrderItemIssuances(orderItem, null);
    }

    /// <summary>
    /// Fetches all ItemIssuances for the given order item, optionally filtering by shipmentId.
    /// </summary>
    /// <param name="orderItem">The OrderItem to look up issuances for.</param>
    /// <param name="shipmentId">Optional shipment identifier. If not null, filter results to that shipment.</param>
    /// <returns>A list of matching ItemIssuance records.</returns>
    public async Task<List<ItemIssuance>> GetOrderItemIssuances(OrderItem orderItem, string shipmentId)
    {
        if (orderItem == null)
        {
            return null;
        }

        // If we haven't yet cached the issuances for this order, load them
        if (orderItemIssuances == null)
        {
            // Query the ItemIssuances by orderId
            // (in Java: from(\"ItemIssuance\").where(\"orderId\", orderId).queryList())
            orderItemIssuances = await Context.ItemIssuances
                .Where(ii => ii.OrderId == orderItem.OrderId)
                .ToListAsync();
        }

        // Now filter the cached list by orderItemSeqId, and (optionally) shipmentId
        var results = orderItemIssuances
            .Where(ii => ii.OrderItemSeqId == orderItem.OrderItemSeqId);

        if (!string.IsNullOrEmpty(shipmentId))
        {
            results = results.Where(ii => ii.ShipmentId == shipmentId);
        }

        return results.ToList();
    }

    public async Task<decimal> GetItemShippedQuantity(OrderItem orderItem)
    {
        // We'll assume you have a method: GetOrderItemIssuances(orderItem)
        // that returns a list of 'ItemIssuance' or some EF entity with { quantity, cancelQuantity }
        var issuanceList = await GetOrderItemIssuances(orderItem);

        // quantityShipped is a decimal with default 0
        decimal quantityShipped = 0m;

        if (issuanceList != null)
        {
            foreach (var issue in issuanceList)
            {
                // Convert to decimal safely
                decimal issueQty = issue.Quantity ?? 0m;
                decimal cancelQty = issue.CancelQuantity ?? 0m;

                // Add (issueQty - cancelQty) to the total
                quantityShipped += (issueQty - cancelQty);
            }
        }

        // If you want a specific scale and rounding, for example scale=2 (cents) 
        // and rounding=MidpointRounding.AwayFromZero, do something like:
        quantityShipped = decimal.Round(quantityShipped, 2, MidpointRounding.AwayFromZero);

        return quantityShipped;
    }

    public async Task<decimal> GetShippingTotal()
    {
        var orderHeaderAdjustments = await GetOrderHeaderAdjustments();
        var orderItemsSubTotal = await GetOrderItemsSubTotal();
        return CalcOrderAdjustments(orderHeaderAdjustments, orderItemsSubTotal, false, false, true);
    }

    public async Task<decimal> GetOrderNonReturnedTaxAndShipping()
    {
        try
        {
            // Define constants for rounding and scaling similar to Java BigDecimal's setScale
            int scale = 2; // Number of decimal places to round to (example value)
            // Use MidpointRounding.ToEven to mimic Java's rounding mode (can be adjusted if needed)
            MidpointRounding rounding = MidpointRounding.ToEven;
            // ZERO constant represented as 0m (decimal)
            decimal ZERO = 0m;

            // ============================================================================
            // Step 1: Retrieve all returned items from the order.
            // Business Purpose: To get all return records so that we can calculate how much of each order item was returned.
            List<ReturnItem> returnedItemsBase = null;
            try
            {
                // Call the function to retrieve returned items (assumed to be implemented elsewhere)
                returnedItemsBase = GetOrderReturnItems();
            }
            catch (Exception ex)
            {
                // Log and rethrow any exception that occurs during retrieval of returned items.
                Console.WriteLine("Error in getOrderReturnItems: " + ex.Message);
                throw;
            }

            // Create a new list for filtered returned items, initializing the capacity based on the base list count.
            List<ReturnItem> returnedItems = new List<ReturnItem>(returnedItemsBase.Count);

            // ============================================================================
            // Step 2: Filter the returned items to include only those with status "RETURN_RECEIVED" or "RETURN_COMPLETED".
            // Business Purpose: Only certain return statuses are relevant for calculating non-returned adjustments.
            try
            {
                // Use LINQ to filter items with status "RETURN_RECEIVED"
                var receivedItems = returnedItemsBase.Where(item => item.StatusId == "RETURN_RECEIVED").ToList();
                // Use LINQ to filter items with status "RETURN_COMPLETED"
                var completedItems = returnedItemsBase.Where(item => item.StatusId == "RETURN_COMPLETED").ToList();
                // Combine both filtered lists into the returnedItems list.
                returnedItems.AddRange(receivedItems);
                returnedItems.AddRange(completedItems);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error filtering returnedItems: " + ex.Message);
                throw;
            }

            // ============================================================================
            // Step 3: Build a list of returned quantities per order item using a DTO.
            // Business Purpose: To accumulate the returned quantity per order item (keyed by orderItemSeqId)
            List<ItemReturnedQuantity> itemReturnedQuantities = new List<ItemReturnedQuantity>();
            try
            {
                // Loop through each returned item from the filtered list.
                foreach (ReturnItem returnedItem in returnedItems)
                {
                    // Retrieve the orderItemSeqId from the returned item.
                    string orderItemSeqId = returnedItem.OrderItemSeqId;
                    // Retrieve the returnQuantity from the returned item.
                    decimal? returnedQuantity = returnedItem.ReturnQuantity;

                    // Only process if both orderItemSeqId and returnedQuantity are valid.
                    if (orderItemSeqId != null && returnedQuantity.HasValue)
                    {
                        // Attempt to find an existing DTO in the list for the current orderItemSeqId.
                        ItemReturnedQuantity existing =
                            itemReturnedQuantities.FirstOrDefault(x => x.orderItemSeqId == orderItemSeqId);
                        if (existing == null)
                        {
                            // If not found, create a new DTO and add it to the list.
                            itemReturnedQuantities.Add(new ItemReturnedQuantity
                            {
                                orderItemSeqId = orderItemSeqId,
                                returnedQuantity = returnedQuantity.Value
                            });
                        }
                        else
                        {
                            // If found, add the current returnedQuantity to the existing quantity.
                            existing.returnedQuantity = existing.returnedQuantity + returnedQuantity.Value;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error building itemReturnedQuantities: " + ex.Message);
                throw;
            }

            // ============================================================================
            // Step 4: Initialize accumulators for the not-returned totals.
            // Business Purpose: To prepare variables that will hold the sum of subtotals, tax, and shipping for items not returned.
            decimal totalSubTotalNotReturned = ZERO; // For item subtotals not returned.
            decimal totalTaxNotReturned = ZERO; // For item taxes not returned.
            decimal totalShippingNotReturned = ZERO; // For item shipping amounts not returned.

            // ============================================================================
            // Step 5: Process each valid order item to calculate the pro-rated amounts for not returned items.
            // Business Purpose: For each order item, determine the fraction that was not returned and apply that factor to the subtotal, tax, and shipping.
            try
            {
                // Retrieve valid order items using the provided helper.
                List<OrderItem> validOrderItems = await GetValidOrderItems();
                foreach (OrderItem orderItem in validOrderItems)
                {
                    // Retrieve the quantity ordered for the current order item.
                    decimal? itemQuantityDbl = orderItem.Quantity;
                    // Skip items with null or zero quantity.
                    if (!itemQuantityDbl.HasValue || itemQuantityDbl.Value == ZERO)
                    {
                        continue;
                    }

                    // Assign the order item quantity.
                    decimal itemQuantity = itemQuantityDbl.Value;

                    // Retrieve the item subtotal, tax, and shipping values using respective helper functions.
                    decimal itemSubTotal = await GetOrderItemSubTotal(orderItem);
                    decimal itemTaxes = await GetOrderItemTax(orderItem);
                    decimal itemShipping = GetOrderItemShipping(orderItem);

                    // Retrieve the returned quantity for the current order item.
                    // The orderItemSeqId is assumed to be stored in the orderItem with key "orderItemSeqId".
                    string orderItemSeqId = orderItem.OrderItemSeqId;
                    // Initialize quantityReturned to ZERO.
                    decimal quantityReturned = ZERO;
                    // Search for the corresponding returned quantity DTO in the list.
                    ItemReturnedQuantity itemQty =
                        itemReturnedQuantities.FirstOrDefault(x => x.orderItemSeqId == orderItemSeqId);
                    if (itemQty != null)
                    {
                        quantityReturned = itemQty.returnedQuantity;
                    }

                    // Calculate the quantity that has not been returned.
                    decimal quantityNotReturned = itemQuantity - quantityReturned;

                    // Compute the pro-rated factor: the ratio of the quantity not returned to the total ordered quantity.
                    // This factor will be used to proportionally reduce the subtotal, tax, and shipping.
                    decimal factorNotReturned = 0m;
                    try
                    {
                        factorNotReturned =
                            quantityNotReturned / itemQuantity; // Division in C# gives high precision by default.
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error calculating factorNotReturned: " + ex.Message);
                        throw;
                    }

                    // Calculate the not-returned portion of the subtotal and round it to the defined scale.
                    decimal subTotalNotReturned = Math.Round(itemSubTotal * factorNotReturned, scale, rounding);
                    // Similarly, calculate the not-returned portion of tax.
                    decimal itemTaxNotReturned = Math.Round(itemTaxes * factorNotReturned, scale, rounding);
                    // And the not-returned portion of shipping.
                    decimal itemShippingNotReturned = Math.Round(itemShipping * factorNotReturned, scale, rounding);

                    // Accumulate the calculated values into the total variables.
                    totalSubTotalNotReturned += subTotalNotReturned;
                    totalTaxNotReturned += itemTaxNotReturned;
                    totalShippingNotReturned += itemShippingNotReturned;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error processing valid order items: " + ex.Message);
                throw;
            }

            // ============================================================================
            // Step 6: Calculate the overall order-level factor for not returned amounts.
            // Business Purpose: To determine how much of the entire order's subtotal remains after returns.
            decimal orderItemsSubTotal = ZERO;
            try
            {
                // Retrieve the subtotal for all order items.
                orderItemsSubTotal = await GetOrderItemsSubTotal();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error retrieving order items subtotal: " + ex.Message);
                throw;
            }

            // Initialize the factor for not returned amounts as ZERO.
            decimal orderFactorNotReturned = ZERO;
            if (orderItemsSubTotal != ZERO)
            {
                try
                {
                    // Compute the factor as the ratio of the subtotal not returned to the overall order items subtotal.
                    orderFactorNotReturned = totalSubTotalNotReturned / orderItemsSubTotal;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error calculating orderFactorNotReturned: " + ex.Message);
                    throw;
                }
            }

            // ============================================================================
            // Step 7: Calculate the tax and shipping adjustments for the entire order.
            // Business Purpose: To apply the overall not-returned factor to header-level tax and shipping values.
            decimal orderTaxNotReturned = ZERO;
            decimal orderShippingNotReturned = ZERO;
            try
            {
                // Multiply the order header's tax total by the not-returned factor and round the result.
                orderTaxNotReturned = Math.Round(await GetHeaderTaxTotal() * orderFactorNotReturned, scale, rounding);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error calculating orderTaxNotReturned: " + ex.Message);
                throw;
            }

            try
            {
                // Multiply the shipping total by the not-returned factor and round the result.
                orderShippingNotReturned =
                    Math.Round(await GetShippingTotal() * orderFactorNotReturned, scale, rounding);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error calculating orderShippingNotReturned: " + ex.Message);
                throw;
            }

            // ============================================================================
            // Step 8: Sum up all tax and shipping adjustments from individual items and the overall order.
            // Business Purpose: To obtain the final value representing total tax and shipping adjustments for non-returned items.
            decimal result = 0m;
            try
            {
                result = totalTaxNotReturned + totalShippingNotReturned + orderTaxNotReturned +
                         orderShippingNotReturned;
                // Finally, round the result to the defined scale.
                result = Math.Round(result, scale, rounding);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error calculating final result: " + ex.Message);
                throw;
            }

            // Return the computed total tax and shipping adjustments for items not returned.
            return result;
        }
        catch (Exception ex)
        {
            // Log any exception that occurred during the execution of the entire method.
            Console.WriteLine("Exception in getOrderNonReturnedTaxAndShipping: " + ex.Message);
            throw;
        }
    }

    // This method calculates the header tax total for an order by applying order header adjustments 
// to the order items subtotal using specific adjustment flags.
// Business Context: This calculation determines the amount of tax adjustments that need to be applied 
// based on adjustments made at the header level (such as discounts, fees, etc.) and the base price of the order items.
    public async Task<decimal> GetHeaderTaxTotal()
    {
        try
        {
            // ---------------------------------------------------------------------
            // Step 1: Retrieve the order header adjustments.
            // Business Perspective:
            // The order header adjustments represent modifications (e.g., discounts, fees) that apply to the entire order.
            // These adjustments are required to compute the tax amount accurately.
            // Technical Perspective:
            // Call the helper function GetOrderHeaderAdjustments() to obtain these adjustments.
            List<OrderAdjustment> orderHeaderAdjustments = new List<OrderAdjustment>();
            try
            {
                // Retrieve order header adjustments using the helper function.
                orderHeaderAdjustments = await GetOrderHeaderAdjustments();
            }
            catch (Exception ex)
            {
                // Log the exception if retrieving adjustments fails.
                Console.WriteLine("Error in GetOrderHeaderAdjustments: " + ex.Message);
                throw;
            }

            // ---------------------------------------------------------------------
            // Step 2: Retrieve the order items subtotal.
            // Business Perspective:
            // The order items subtotal represents the total price of all items in the order before adjustments.
            // This value is used as the base for applying header-level adjustments.
            // Technical Perspective:
            // Call the helper function GetOrderItemsSubTotal() to obtain the subtotal.
            decimal orderItemsSubTotal = 0m;
            try
            {
                // Retrieve the subtotal for all order items.
                orderItemsSubTotal = await GetOrderItemsSubTotal();
            }
            catch (Exception ex)
            {
                // Log the exception if retrieving the subtotal fails.
                Console.WriteLine("Error in GetOrderItemsSubTotal: " + ex.Message);
                throw;
            }

            // ---------------------------------------------------------------------
            // Step 3: Calculate the header tax total using the adjustments and the subtotal.
            // Business Perspective:
            // The header tax total is calculated by applying the retrieved order header adjustments 
            // to the order items subtotal. The boolean flags determine which adjustments are applied.
            // Technical Perspective:
            // Call the static helper method calcOrderAdjustments from OrderReadHelper with the following parameters:
            // - orderHeaderAdjustments: the list of header adjustments,
            // - orderItemsSubTotal: the base total of order items,
            // - false, true, false: flags indicating which types of adjustments to consider (tax flag is true).
            decimal result = 0m;
            try
            {
                result = CalcOrderAdjustments(orderHeaderAdjustments, orderItemsSubTotal, false, true,
                    false);
            }
            catch (Exception ex)
            {
                // Log any exception that occurs during the adjustment calculation.
                Console.WriteLine("Error in calcOrderAdjustments: " + ex.Message);
                throw;
            }

            // Return the computed header tax total.
            return result;
        }
        catch (Exception ex)
        {
            // Log any exception that occurs in the entire GetHeaderTaxTotal() method.
            Console.WriteLine("Exception in GetHeaderTaxTotal: " + ex.Message);
            throw;
        }
    }

    // This method calculates the tax adjustment total for a given order item.
// Business Perspective: It determines the tax-related adjustments that apply to the item,
// ensuring accurate tax calculation on a per-item basis.
// Technical Perspective: The calculation is performed by calling GetOrderItemAdjustmentsTotal with
// the includeOther flag set to false, includeTax flag set to true, and includeShipping flag set to false.
    public async Task<decimal> GetOrderItemTax(OrderItem orderItem)
    {
        try
        {
            // ---------------------------------------------------------------------
            // Retrieve the adjustments for this order item.
            // Assumption: The OrderItem object has a property OrderAdjustments that provides
            // a List<OrderAdjustment> associated with this order item.
            List<OrderAdjustment> adjustments = await Context.OrderAdjustments
                .Where(adj => adj.OrderItemSeqId == orderItem.OrderItemSeqId && adj.OrderId == orderItem.OrderId)
                .ToListAsync();

            // ---------------------------------------------------------------------
            // Call the overloaded GetOrderItemAdjustmentsTotal method with the required parameters.
            // Parameters:
            // - orderItem: the order item for which the adjustments are being calculated.
            // - adjustments: the list of OrderAdjustment objects associated with the order item.
            // - false: indicates that adjustments categorized as "other" should not be included.
            // - true: indicates that tax adjustments should be included.
            // - false: indicates that shipping adjustments should not be included.
            // The call returns the total tax adjustment amount for the given order item.
            return GetOrderItemAdjustmentsTotal(orderItem, adjustments, false, true, false);
        }
        catch (Exception ex)
        {
            // Log the exception with a message and rethrow it to be handled further up the call chain.
            Console.WriteLine("Exception in GetOrderItemTax: " + ex.Message);
            throw;
        }
    }

    // This method calculates the shipping adjustments for a given order item.
// Business Perspective:
//   - The purpose of this method is to determine the total shipping-related adjustments
//     (e.g., shipping fees or discounts) applicable to the order item.
//   - This value is essential for accurately computing the overall shipping cost for the order.
// Technical Perspective:
//   - The method delegates the calculation to GetOrderItemAdjustmentsTotal, passing specific flags:
//     includeOther = false, includeTax = false, and includeShipping = true, ensuring that only
//     shipping adjustments are included.
//   - Any exceptions are caught, logged, and then rethrown for higher-level handling.
    public decimal GetOrderItemShipping(OrderItem orderItem)
    {
        try
        {
            // Retrieve the list of adjustments associated with the order item.
            // Assumption: The OrderItem object has a property "OrderAdjustments" that provides
            // a List<OrderAdjustment> which is used for adjustment calculations.
            List<OrderAdjustment> adjustments = Context.OrderAdjustments
                .Where(adj => adj.OrderItemSeqId == orderItem.OrderItemSeqId && adj.OrderId == orderItem.OrderId)
                .ToList();

            // Call the helper method GetOrderItemAdjustmentsTotal with the following parameters:
            // - orderItem: The current order item.
            // - adjustments: The list of adjustments relevant to the order item.
            // - false: Do not include "other" types of adjustments.
            // - false: Do not include tax adjustments.
            // - true: Include shipping adjustments.
            // The return value represents the total shipping adjustment for the order item.
            return GetOrderItemAdjustmentsTotal(orderItem, adjustments, false, false, true);
        }
        catch (Exception ex)
        {
            // Log the exception with a detailed message for troubleshooting.
            Console.WriteLine("Exception in GetOrderItemShipping: " + ex.Message);
            // Rethrow the exception to ensure that calling methods can handle it appropriately.
            throw;
        }
    }

    public async Task<List<Payment>> GetOrderPayments()
    {
        return await GetOrderPayments(null);
    }

    /**
 * Gets order payments associated with the order.
 * @param orderPaymentPreference The specific order payment preference to filter by, or null for all preferences.
 * @return A list of Payment entities, or an empty list if an error occurs.
 */
    public async Task<List<Payment>> GetOrderPayments(OrderPaymentPreference orderPaymentPreference)
    {
        var orderPayments = new List<Payment>();
        List<OrderPaymentPreference> prefs = null;

        try
        {
            if (orderPaymentPreference == null)
            {
                // Fetch all payment preferences for the order
                prefs = await GetPaymentPreferences();
            }
            else
            {
                // Use the provided payment preference as a single-item list
                prefs = new List<OrderPaymentPreference> { orderPaymentPreference };
            }

            if (prefs != null)
            {
                foreach (var payPref in prefs)
                {
                    // Fetch related Payment entities for the payment preference
                    var payments = await Context.Payments
                        .Where(p => p.PaymentPreferenceId == payPref.OrderPaymentPreferenceId)
                        .ToListAsync();

                    orderPayments.AddRange(payments);
                }
            }

            return orderPayments;
        }
        catch (Exception ex)
        {
            // Log error and return an empty list to mimic Java's null return on error
            Console.WriteLine($"Error fetching order payments: {ex.Message}");
            return new List<Payment>();
        }
    }

    public async Task<List<OrderPaymentPreference>> GetPaymentPreferences()
    {
        if (paymentPrefs == null)
        {
            try
            {
                paymentPrefs = await Context.OrderPaymentPreferences
                    .Where(p => p.OrderId == orderHeader.OrderId)
                    .OrderBy(p => p.OrderPaymentPreferenceId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching payment preferences: {ex.Message}");
                paymentPrefs = new List<OrderPaymentPreference>();
            }
        }

        return paymentPrefs;
    }

    /// <summary>
    /// Gets the product store ID associated with the order.
    /// </summary>
    /// <returns>The product store ID as a string.</returns>
    public string GetProductStoreId()
    {
        return orderHeader.ProductStoreId;
    }

    /// <summary>
    /// Retrieves the ProductStore entity associated with the order, or null if an error occurs.
    /// </summary>
    /// <returns>The ProductStore entity, or null if the store cannot be retrieved.</returns>
    public async Task<ProductStore> GetProductStore()
    {
        string productStoreId = orderHeader.ProductStoreId;
        try
        {
            var productStore = await Context.ProductStores
                .Where(ps => ps.ProductStoreId == productStoreId)
                .FirstOrDefaultAsync();

            return productStore;
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"Failed to get product store for order header [OrderId: {orderHeader.OrderId}] due to exception: {ex.Message}");
            return null;
        }
    }

    public async Task<decimal> GetBillingAccountBalance(BillingAccount billingAccount)
    {
        if (billingAccount == null || string.IsNullOrEmpty(billingAccount.BillingAccountId))
        {
            Console.WriteLine("Billing account is null or has no ID; returning zero.");
            return ZERO;
        }

        try
        {
            decimal balance = ZERO;
            decimal accountLimit = GetAccountLimit(billingAccount);
            balance += accountLimit;

            // Query replicating OrderPurchasePaymentSummary view
            var orderPaymentSums = await (from oh in Context.OrderHeaders
                    join opp in Context.OrderPaymentPreferences on oh.OrderId equals opp.OrderId
                    where oh.BillingAccountId == billingAccount.BillingAccountId
                          && opp.PaymentMethodTypeId == "EXT_BILLACT"
                          && !new[] { "ORDER_CANCELLED", "ORDER_REJECTED" }.Contains(oh.StatusId)
                          && !new[] { "PAYMENT_SETTLED", "PAYMENT_RECEIVED", "PAYMENT_DECLINED", "PAYMENT_CANCELLED" }
                              .Contains(opp.StatusId)
                    group opp by new
                    {
                        oh.OrderId,
                        oh.StatusId,
                        oh.BillingAccountId,
                        opp.PaymentMethodTypeId,
                        PreferenceStatusId = opp.StatusId
                    }
                    into g
                    select g.Sum(opp => opp.MaxAmount))
                .ToListAsync();

            foreach (var maxAmount in orderPaymentSums)
            {
                balance -= maxAmount;
            }

            // Query payment applications
            var paymentAppls = await Context.PaymentApplications
                .Where(pa => pa.BillingAccountId == billingAccount.BillingAccountId)
                .ToListAsync();

            // TODO: Handle cancelled payments?
            foreach (var paymentAppl in paymentAppls)
            {
                if (string.IsNullOrEmpty(paymentAppl.InvoiceId))
                {
                    balance += paymentAppl.AmountApplied ?? 0m;
                }
            }

            return Math.Round(balance, scale, rounding);
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"Error calculating billing account balance for ID {billingAccount.BillingAccountId}: {ex.Message}");
            throw;
        }
    }

    public decimal GetAccountLimit(BillingAccount billingAccount)
    {
        if (billingAccount == null)
        {
            Console.WriteLine("Billing account is null; assuming zero limit.");
            return ZERO;
        }

        if (billingAccount.AccountLimit.HasValue)
        {
            return Math.Round(billingAccount.AccountLimit.Value, scale, rounding);
        }

        Console.WriteLine(
            $"Billing Account [{billingAccount.BillingAccountId}] does not have an account limit defined, assuming zero.");
        return ZERO;
    }

    public async Task<decimal> GetItemBackOrderedQuantity(OrderItem orderItem)
    {
        decimal backOrdered = ZERO;
        // Retrieve estimated ship date and auto-cancel date from the order item
        DateTime? shipDate = orderItem.EstimatedShipDate;
        DateTime? autoCancel = orderItem.AutoCancelDate;

        // Fetch the list of OrderItemShipGrpInvRes for the given order item
        var reses = await GetOrderItemShipGrpInvResList(orderItem);

        if (reses != null && reses.Any())
        {
            foreach (var res in reses)
            {
                // Get the promised date, preferring currentPromisedDate, falling back to promisedDatetime
                DateTime? promised = res.CurrentPromisedDate ?? res.PromisedDatetime;

                // REFACTOR: Use QUANTITY_NOT_AVAILABLE to directly capture back-ordered quantity,
                // ensuring only unavailable quantities contribute to backOrdered.
                // Retained original condition as a fallback to maintain existing business logic.
                if (res.QuantityNotAvailable > 0m || autoCancel != null || (shipDate != null && promised != null && shipDate > promised))
                {
                    // REFACTOR: Use QUANTITY_NOT_AVAILABLE instead of QUANTITY,
                    // as it explicitly represents the back-ordered amount per reservation.
                    decimal resQty = res.QuantityNotAvailable ?? 0m;
                    if (resQty != 0m)
                    {
                        backOrdered += resQty;
                        backOrdered = Math.Round(backOrdered, scale, rounding);
                    }
                }
            }
        }

        return backOrdered;
    }
    public async Task<List<OrderItemShipGrpInvRes>> GetOrderItemShipGrpInvResList(OrderItem orderItem)
    {
        // REFACTOR: Added null check for orderItem.OrderId and orderItem.OrderItemSeqId to prevent null reference exceptions
        // and ensure valid input before querying the database.
        if (orderItem == null || string.IsNullOrEmpty(orderItem.OrderId) ||
            string.IsNullOrEmpty(orderItem.OrderItemSeqId))
        {
            // REFACTOR: Log warning and return empty list instead of null to maintain consistent return type and avoid null checks by callers.
            Console.WriteLine("Invalid input: orderItem or its OrderId/OrderItemSeqId is null or empty.");
            return new List<OrderItemShipGrpInvRes>();
        }

        // REFACTOR: Initialize orderItemShipGrpInvResList as a local variable to avoid state persistence issues and ensure thread safety.
        // Previously, the field could retain stale data across calls, leading to inconsistent results.
        var orderItemShipGrpInvResList = new List<OrderItemShipGrpInvRes>();

        try
        {
            // REFACTOR: Added null check in the Where clause to handle NULL values in OrderId column,
            // preventing InvalidCastException when DB returns NULL.
            orderItemShipGrpInvResList = await Context.OrderItemShipGrpInvRes
                .Where(res => res.OrderId != null && res.OrderId == orderItem.OrderId)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            // REFACTOR: Enhanced logging to include stack trace for better debugging.
            // Returns empty list to maintain consistent return type and mimic OFBiz behavior.
            Console.WriteLine(
                $"Error fetching OrderItemShipGrpInvRes List for OrderId {orderItem.OrderId}: {ex.Message}\n{ex.StackTrace}");
            return new List<OrderItemShipGrpInvRes>();
        }

        // REFACTOR: Added null check for OrderItemSeqId to handle potential NULL values in the cached list,
        // ensuring robust filtering and preventing runtime errors.
        return orderItemShipGrpInvResList
            .Where(res => res.OrderItemSeqId != null && res.OrderItemSeqId == orderItem.OrderItemSeqId)
            .ToList();
    }
}