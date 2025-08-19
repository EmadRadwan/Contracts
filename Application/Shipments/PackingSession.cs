using Application.Core;
using Application.Facilities;
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Shipments;

public class PackingSession
{
    // --------------------------------------------------
    // 1. Fields/Properties from the Java "PackingSession"
    // --------------------------------------------------


    /// <summary>
    /// Issuance service used for issuing items to shipments.
    /// </summary>
    public IIssuanceService IssuanceService { get; set; }

    public IPickListService PickListService { get; set; }
    public IShipmentHelperService ShipmentHelperService { get; set; }
    public IUtilityService UtilityService { get; set; }

    /// <summary>
    /// Identifier to track the current user (parallel to userLogin in Java).
    /// </summary>
    public UserLogin UserLogin { get; set; }

    /// <summary>
    /// Picker party identifier.
    /// </summary>
    protected string PickerPartyId { get; set; }

    /// <summary>
    /// Primary order ID for this packing session.
    /// </summary>
    protected string PrimaryOrderId { get; set; }

    /// <summary>
    /// Primary Ship Group ID for this packing session.
    /// </summary>
    protected string PrimaryShipGrp { get; set; }

    /// <summary>
    /// The picklist bin ID, if any, used for this packing session.
    /// </summary>
    protected string PicklistBinId { get; set; }

    /// <summary>
    /// The facility from which items are packed.
    /// </summary>
    protected string FacilityId { get; set; }

    /// <summary>
    /// The newly created shipment identifier (once packing is complete).
    /// </summary>
    protected string ShipmentId { get; set; }

    /// <summary>
    /// Any special handling instructions.
    /// </summary>
    protected string Instructions { get; set; }

    /// <summary>
    /// The default weight UOM ID for packages.
    /// </summary>
    protected string WeightUomId { get; set; }

    /// <summary>
    /// A box type ID (used in some shipping contexts).
    /// </summary>
    protected string ShipmentBoxTypeId { get; set; }

    /// <summary>
    /// Additional shipping charge for the session (if any).
    /// </summary>
    protected decimal? AdditionalShippingCharge { get; set; }

    /// <summary>
    /// The current numeric status of the packing session (1 = active, 0 = completed, etc.).
    /// </summary>
    protected int Status { get; set; } = 1;

    /// <summary>
    /// A dictionary that tracks package sequence # -> total weight, used by the session.
    /// </summary>
    protected Dictionary<int, decimal> PackageWeights { get; set; }

    /// <summary>
    /// A list of "pack events" (parallel to packEvents in Java).
    /// </summary>
    //protected List<PackingEvent> PackEvents { get; set; }

    /// <summary>
    /// The main list of lines being packed (parallel to packLines in Java).
    /// </summary>
    protected List<PackingSessionLine> PackLines { get; set; }

    /// <summary>
    /// A list for "item info" objects (parallel to itemInfos in Java).
    /// </summary>
    protected List<ItemDisplay> ItemInfos { get; set; }

    /// <summary>
    /// Tracks the highest package sequence used so far (packageSeq in Java).
    /// </summary>
    protected int PackageSeq { get; set; } = -1;

    /// <summary>
    /// A map for package sequence # -> shipment box type, if each package can differ.
    /// </summary>
    protected Dictionary<int, string> ShipmentBoxTypes { get; set; }


    // --------------------------------------------------
    // 2. The DataContext property, analogous to "Context" in your example
    // --------------------------------------------------

    /// <summary>
    /// EF Core DataContext, set after constructing the object.
    /// This is how we access the database similarly to how Java uses Delegator/dispatcher.
    /// </summary>
    public DataContext Context { get; set; }

    public ILogger<PackingSession> Logger { get; set; }


    // --------------------------------------------------
    // 3. Constructors, parallel to Java
    // --------------------------------------------------

    /// <summary>
    /// Main constructor, similar to 
    ///   PackingSession(LocalDispatcher dispatcher, GenericValue userLogin, 
    ///                  string facilityId, string binId, string orderId, string shipGrp)
    /// but adjusted for C#.
    /// </summary>
    public PackingSession(UserLogin userLogin,
        string facilityId,
        string binId,
        string orderId,
        string shipGrp)
    {
        UserLogin = userLogin;
        FacilityId = facilityId;
        PicklistBinId = binId;
        PrimaryOrderId = orderId;
        PrimaryShipGrp = shipGrp;

        // Initialize collections
        PackLines = new List<PackingSessionLine>();
        //PackEvents = new List<PackingEvent>();
        ItemInfos = new List<ItemDisplay>();
        PackageWeights = new Dictionary<int, decimal>();
        ShipmentBoxTypes = new Dictionary<int, string>();

        // The Java sets packageSeq = 1 initially (or -1?). 
        // We'll mimic the original code's logic:
        PackageSeq = 1;
        Status = 1; // active
    }

    /// <summary>
    /// Overload, if you only have userLogin + facilityId from the caller.
    /// The rest is null by default.
    /// </summary>
    public PackingSession(UserLogin userLogin, string facilityId)
        : this(userLogin, facilityId, null, null, null)
    {
    }

    /// <summary>
    /// Overload, if you only have userLogin from the caller.
    /// The rest is null by default.
    /// </summary>
    public PackingSession(UserLogin userLogin)
        : this(userLogin, null, null, null, null)
    {
    }


    // --------------------------------------------------
    // 4. An "InitializeSession" method (if needed),
    //    similar to "InitializeOrder" from your example
    // --------------------------------------------------

    /// <summary>
    /// In case we want to do any further DB lookups after the user sets 
    /// the Context property. For example, load existing lines or statuses 
    /// from the DB. You can adapt it as you like.
    /// </summary>
    public void InitializeSession()
    {
        if (Context == null)
            throw new InvalidOperationException("DataContext has not been set for PackingSession.");

        // e.g. load some related records if needed...
    }


    // --------------------------------------------------
    // 5. Placeholder methods from the Java logic
    // --------------------------------------------------

    /// <summary>
    /// The main method for adding or increasing a line (parallel to
    /// addOrIncreaseLine in Java).
    /// </summary>
    public async Task<List<PackingSessionLine>> AddOrIncreaseLine(
        string orderId,
        string orderItemSeqId,
        string shipGroupSeqId,
        string productId,
        decimal quantity,
        int packageSeqId,
        decimal weight,
        bool update)
    {
        // (1) Check session status.
        if (this.Status == 0)
        {
            throw new Exception(
                "Packing session has been completed; be sure to CLEAR before packing a new order! [000]");
        }

        // (2) Do nothing if not updating and quantity is 0.
        if (!update && quantity == 0)
        {
            return this.PackLines;
        }

        // (3) Resolve the actual product ID.
        productId = Context.Products
            .Where(p => p.ProductId == productId)
            .Select(p => p.ProductId)
            .FirstOrDefault();

        // (4) Use primary values if parameters are null.
        if (orderId == null)
        {
            orderId = this.PrimaryOrderId;
        }

        if (shipGroupSeqId == null)
        {
            shipGroupSeqId = this.PrimaryShipGrp;
        }

        if (orderItemSeqId == null && productId != null)
        {
            orderItemSeqId = await this.FindOrderItemSeqId(productId, orderId, shipGroupSeqId, quantity);
        }

        // (5) Look up reservations. Note: "AddOrIncreaseLine" is used as a lookup alias in OFBiz.
        var invLookup = new Dictionary<string, object>
        {
            { "orderId", orderId },
            { "orderItemSeqId", orderItemSeqId },
            { "shipGroupSeqId", shipGroupSeqId }
        };
        var reservations = Context.OrderItemShipGrpInvRes
            .Where(r => r.OrderId == orderId
                        && r.OrderItemSeqId == orderItemSeqId
                        && r.ShipGroupSeqId == shipGroupSeqId)
            .OrderByDescending(r => r.Quantity) // if you need to sort similar to UtilMisc.toList("quantity DESC")
            .ToList();

        if (reservations == null || reservations.Count == 0)
        {
            throw new Exception("No inventory reservations available; cannot pack this item! [101]");
        }

        // (6) Process reservations.
        if (reservations.Count == 1)
        {
            var res = reservations.First();
            decimal resQty = this.NumAvailableItems(res);
            if (resQty >= quantity)
            {
                int checkCode = this.CheckLineForAdd(res, orderId, orderItemSeqId, shipGroupSeqId, productId,
                    quantity, packageSeqId, update);
                this.CreatePackLineItem(checkCode, res, orderId, orderItemSeqId, shipGroupSeqId, productId,
                    quantity, weight, packageSeqId);
            }
        }
        else
        {
            var toCreateList = new List<(OrderItemShipGrpInvRes res, decimal qty)>();
            decimal qtyRemain = quantity;
            foreach (var res in reservations)
            {
                // Check that the reservation’s inventory item product matches the product to pack.
                string resProductId = res.InventoryItem.ProductId;
                if (!productId.Equals(resProductId))
                    continue;

                decimal resQty = this.NumAvailableItems(res);
                if (resQty > 0)
                {
                    decimal resPackedQty = this.GetPackedQuantity(orderId, orderItemSeqId, shipGroupSeqId,
                        productId, -1);
                    if (resPackedQty >= resQty)
                        continue;
                    else if (!update)
                        resQty -= resPackedQty;

                    decimal thisQty = resQty > qtyRemain ? qtyRemain : resQty;
                    int thisCheck = this.CheckLineForAdd(res, orderId, orderItemSeqId, shipGroupSeqId, productId,
                        thisQty, packageSeqId, update);
                    switch (thisCheck)
                    {
                        case 2:
                            // Log info as needed.
                            toCreateList.Add((res, thisQty: thisQty));
                            qtyRemain -= thisQty;
                            break;
                        case 1:
                            qtyRemain -= thisQty;
                            break;
                        case 0:
                            // Do nothing.
                            break;
                        default:
                            break;
                    }
                }

                if (qtyRemain <= 0)
                    break;
            }

            if (qtyRemain == 0)
            {
                foreach (var tuple in toCreateList)
                {
                    this.CreatePackLineItem(2, tuple.res, orderId, orderItemSeqId, shipGroupSeqId, productId,
                        tuple.qty, weight, packageSeqId);
                }
            }
            else
            {
                throw new Exception("Not enough inventory reservation available; cannot pack the item! [103]");
            }
        }

        // (7) Run add events.
        //this.RunEvents(PackingEvent.EVENT_CODE_ADD);

        return this.PackLines;
    }

    private decimal NumAvailableItems(OrderItemShipGrpInvRes res)
    {
        // Get the reserved quantity from the reservation.
        decimal resQty = (decimal)res.Quantity;
        // Subtract any quantity that is not available; if null, default to 0.
        decimal notAvailable = res.QuantityNotAvailable ?? 0m;
        return resQty - notAvailable;
    }


    public PackingSessionLine FindLine(
        string orderId,
        string orderItemSeqId,
        string shipGroupSeqId,
        string productId,
        string inventoryItemId,
        int packageSeq)
    {
        foreach (var line in this.PackLines)
        {
            if (line.OrderId == orderId &&
                line.OrderItemSeqId == orderItemSeqId &&
                line.ShipGroupSeqId == shipGroupSeqId &&
                line.ProductId == productId &&
                line.InventoryItemId == inventoryItemId &&
                line.PackageSeq == packageSeq)
            {
                return line;
            }
        }

        return null;
    }


    private void CreatePackLineItem(
        int checkCode,
        OrderItemShipGrpInvRes res,
        string orderId,
        string orderItemSeqId,
        string shipGroupSeqId,
        string productId,
        decimal quantity,
        decimal weight,
        int packageSeqId)
    {
        switch (checkCode)
        {
            case 0:
                throw new Exception("Not enough inventory reservation available; cannot pack the item! [201]");
            case 1:
                // Existing line updated; nothing additional to do.
                break;
            case 2:
                // Create a new packing line.
                string invItemId = res.InventoryItemId;
                var newLine = new PackingSessionLine(orderId, orderItemSeqId, shipGroupSeqId, productId, invItemId,
                    quantity, weight, packageSeqId)
                {
                    Context = Context,
                    IssuanceService = IssuanceService,
                    PickListService = PickListService,
                };
                PackLines.Add(newLine);
                break;

            default:
                throw new Exception("Value of checkCode different than expected");
        }

        // Add the line weight to the package weight.
        if (weight > 0)
        {
            AddToPackageWeight(packageSeqId, weight);
        }

        // Update the package sequence if needed.
        if (packageSeqId > PackageSeq)
        {
            PackageSeq = packageSeqId;
        }
    }

    public async Task<string> FindOrderItemSeqId(string productId, string orderId, string shipGroupSeqId,
        decimal quantity)
    {
        // Join OrderItem (oi) and OrderItemShipGroupAssocs (oisga) based on orderId and orderItemSeqId.
        // This query replicates the view "OrderItemAndShipGroupAssoc".
        var orderItems = await (from oi in Context.OrderItems
            join oisga in Context.OrderItemShipGroupAssocs
                on new { oi.OrderId, oi.OrderItemSeqId } equals new { oisga.OrderId, oisga.OrderItemSeqId }
            where oi.OrderId == orderId &&
                  oi.ProductId == productId &&
                  oi.StatusId == "ITEM_APPROVED" &&
                  oisga.ShipGroupSeqId == shipGroupSeqId
            orderby oi.Quantity descending
            select new
            {
                oi.OrderItemSeqId,
                OrderItemQuantity = oi.Quantity
            }).ToListAsync();

        // Loop through each candidate order item.
        foreach (var item in orderItems)
        {
            // For each order item, retrieve its reservations from OrderItemShipGrpInvRes.
            var reservations = await Context.OrderItemShipGrpInvRes
                .Where(r => r.OrderId == orderId &&
                            r.OrderItemSeqId == item.OrderItemSeqId &&
                            r.ShipGroupSeqId == shipGroupSeqId)
                .ToListAsync();

            // If any reservation has enough quantity, return this order item’s sequence id.
            foreach (var res in reservations)
            {
                if (quantity <= res.Quantity)
                {
                    return item.OrderItemSeqId;
                }
            }
        }

        throw new Exception($"No valid order item found for product [{productId}] with quantity: {quantity}");
    }


    private int CheckLineForAdd(
        OrderItemShipGrpInvRes res,
        string orderId,
        string orderItemSeqId,
        string shipGroupSeqId,
        string productId,
        decimal quantity,
        int packageSeqId,
        bool update)
    {
        // Retrieve the inventory item ID and reserved quantity from the reservation record.
        string invItemId = res.InventoryItemId;
        decimal resQty = (decimal)res.Quantity;

        // Find an existing packing line using the instance's PackLines.
        PackingSessionLine line = this.FindLine(orderId, orderItemSeqId, shipGroupSeqId, productId, invItemId,
            packageSeqId);

        // Get the total quantity already packed for this order item (across all packages).
        decimal packedQty = this.GetPackedQuantity(orderId, orderItemSeqId, shipGroupSeqId, productId, null, -1);


        if (line == null)
        {
            // If the reserved quantity is less than the requested quantity, return 0.
            if (resQty < quantity)
            {
                return 0;
            }
            else
            {
                // Otherwise, indicate that a new line should be created.
                return 2;
            }
        }
        else
        {
            // If an existing line is found, calculate the new quantity.
            decimal newQty = update ? quantity : (line.Quantity + quantity);
            if (resQty < newQty)
            {
                // Not enough available quantity.
                return 0;
            }
            else
            {
                // Update the existing line's quantity and return code 1.
                line.Quantity = newQty;
                return 1;
            }
        }
    }

    // And so forth for each of the Java code methods:
    // checkReservations, complete, runEvents, createShipment, issueItemsToShipment, etc.

    public async Task<CompletePackingResult> Complete(bool force)
    {
        try
        {
            // 1) If there are no packing lines, return an "EMPTY" result.
            if (this.PackLines == null || this.PackLines.Count == 0)
            {
                return CompletePackingResult.Empty();
            }

            // 2) Validate that every packing line meets its reservation constraints.
            await CheckReservations(force);

            // 3) Mark the session as complete.
            this.Status = 0;

            // 4) Create the shipment record.
            await CreateShipment();
            if (string.IsNullOrEmpty(this.ShipmentId))
            {
                return CompletePackingResult.Error("Failed to create shipment.");
            }

            // 5) Create packages in the database.
            await CreatePackages();

            // 6) Issue items to the shipment.
            await IssueItemsToShipment();

            // 7) Apply packing lines to packages (e.g. create shipment package contents).
            await ApplyItemsToPackages();

            // 8) Update shipment route segments with total weight and UOM.
            await UpdateShipmentRouteSegments();

            // 9) Mark the shipment as "packed."
            await SetShipmentToPacked();

            // 10) Set the picker on the picklist.
            //    Pass the session's PicklistBinId, PickerPartyId, and the current user login ID.
            await SetPickerOnPicklist(PicklistBinId, PickerPartyId, UserLogin.UserLoginId);

            // 11) Update the picklist status to "PICKLIST_PICKED".
            //    Pass the session's PicklistBinId, PrimaryOrderId, and current user login ID.
            await SetPicklistToPicked(PicklistBinId, PrimaryOrderId, UserLogin.UserLoginId);

            // 12) (Optional) Run any complete events here.

            return CompletePackingResult.Success(this.ShipmentId);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error completing packing process");
            return CompletePackingResult.Error("Exception: " + ex.Message);
        }
    }


    public List<PackingSessionLine> GetLines()
    {
        return this.PackLines;
    }


    public void SetFacilityId(string facilityId)
    {
        this.FacilityId = facilityId;
    }


    // Returns the total packed quantity for the given order, order item, ship group, and product,
    // regardless of package (packageSeq = -1 indicates "all packages").
    private decimal GetPackedQuantity(string orderId, string orderItemSeqId, string shipGroupSeqId, string productId)
    {
        return GetPackedQuantity(orderId, orderItemSeqId, shipGroupSeqId, productId, null, -1);
    }

    // Overload that accepts a package sequence.
    public decimal GetPackedQuantity(string orderId, string orderItemSeqId, string shipGroupSeqId, string productId,
        int packageSeq)
    {
        return GetPackedQuantity(orderId, orderItemSeqId, shipGroupSeqId, productId, null, packageSeq);
    }

    // Core method: If inventoryItemId is null, it sums across all inventory items.
    public decimal GetPackedQuantity(string orderId, string orderItemSeqId, string shipGroupSeqId, string productId,
        string inventoryItemId, int packageSeq)
    {
        decimal total = 0m;
        foreach (var line in this.PackLines)
        {
            if (line.OrderId == orderId &&
                line.OrderItemSeqId == orderItemSeqId &&
                line.ShipGroupSeqId == shipGroupSeqId &&
                line.ProductId == productId)
            {
                if (inventoryItemId == null || inventoryItemId == line.InventoryItemId)
                {
                    if (packageSeq == -1 || packageSeq == line.PackageSeq)
                    {
                        total += line.Quantity;
                    }
                }
            }
        }

        return total;
    }

    // Overload: Return total packed quantity for a given product (normalize productId first).
    private decimal GetPackedQuantity(string productId, int packageSeq)
    {
        if (productId != null)
        {
            try
            {
                productId = Context.Products
                    .Where(p => p.ProductId == productId)
                    .Select(p => p.ProductId)
                    .FirstOrDefault();
            }
            catch (Exception ex)
            {
                Logger.LogDebug(ex, "Error normalizing productId in GetPackedQuantity");
            }
        }

        decimal total = 0m;
        if (productId != null)
        {
            foreach (var line in this.PackLines)
            {
                if (line.ProductId == productId)
                {
                    if (packageSeq == -1 || packageSeq == line.PackageSeq)
                    {
                        total += line.Quantity;
                    }
                }
            }
        }

        return total;
    }

// Overload: Return total packed quantity for a given package across all items.
    public decimal GetPackedQuantity(int packageSeq)
    {
        decimal total = 0m;
        foreach (var line in this.PackLines)
        {
            if (packageSeq == -1 || packageSeq == line.PackageSeq)
            {
                total += line.Quantity;
            }
        }

        return total;
    }

    // Overload: Return total packed quantity for a product across all packages.
    public decimal GetPackedQuantity(string productId)
    {
        return GetPackedQuantity(productId, -1);
    }

    private async Task CheckReservations(bool ignore)
    {
        var errors = new List<string>();

        // Loop through each packing line stored in the instance.
        foreach (var line in this.PackLines)
        {
            // Get the reserved and packed quantities for the line.
            decimal reservedQty =
                await GetCurrentReservedQuantity(line.OrderId, line.OrderItemSeqId, line.ShipGroupSeqId,
                    line.ProductId);
            decimal packedQty =
                GetPackedQuantity(line.OrderId, line.OrderItemSeqId, line.ShipGroupSeqId, line.ProductId);

            // If the packed quantity does not match the reserved quantity, record an error.
            if (packedQty != reservedQty)
            {
                /*errors.Add(
                    $"Packed amount does not match reserved amount for item ({line.ProductId}) [{packedQty} / {reservedQty}]");*/
            }
        }

        // If errors exist, either throw an exception or log warnings based on the ignore flag.
        if (errors.Count > 0)
        {
            if (!ignore)
            {
                throw new Exception("Attempt to pack order failed: " + string.Join("; ", errors));
            }
            else
            {
                Logger.LogWarning("Packing warnings: " + string.Join("; ", errors));
            }
        }
    }

    private async Task<decimal> GetCurrentReservedQuantity(string orderId, string orderItemSeqId, string shipGroupSeqId,
        string productId)
    {
        try
        {
            // Query using joins over the underlying tables.
            // This mimics the view "OrderItemAndShipGrpInvResAndItemSum" by joining OrderItems, OrderItemShipGrpInvRes, and InventoryItems.
            var amounts = await (
                from oi in this.Context.OrderItems
                join oisgir in this.Context.OrderItemShipGrpInvRes
                    on new { oi.OrderId, oi.OrderItemSeqId } equals new { oisgir.OrderId, oisgir.OrderItemSeqId }
                join ii in this.Context.InventoryItems
                    on oisgir.InventoryItemId equals ii.InventoryItemId
                where oi.OrderId == orderId
                      && oi.OrderItemSeqId == orderItemSeqId
                      && oisgir.ShipGroupSeqId == shipGroupSeqId
                      && ii.ProductId == productId
                select (oisgir.Quantity - (oisgir.QuantityNotAvailable ?? 0m))
            ).ToListAsync();

            if (amounts == null || amounts.Count == 0)
            {
                return -1m;
            }

            return (decimal)amounts.Sum();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex,
                "Error retrieving reserved quantity for orderId {OrderId}, orderItemSeqId {OrderItemSeqId}, shipGroupSeqId {ShipGroupSeqId}, productId {ProductId}",
                orderId, orderItemSeqId, shipGroupSeqId, productId);
            return -1m;
        }
    }

    private async Task CreateShipment()
    {
        var shipmentContext = new ShipmentContext
        {
            OriginFacilityId = this.FacilityId,
            PrimaryShipGroupSeqId = this.PrimaryShipGrp,
            PrimaryOrderId = this.PrimaryOrderId,
            ShipmentTypeId = "OUTGOING_SHIPMENT",
            StatusId = "SHIPMENT_INPUT",
        };

        // Determine partyIdTo from OrderRole for role "SHIP_TO_CUSTOMER"
        if (!string.IsNullOrEmpty(this.PrimaryOrderId))
        {
            var orderRoleShipTo = await this.Context.OrderRoles
                .Where(o => o.OrderId == this.PrimaryOrderId && o.RoleTypeId == "SHIP_TO_CUSTOMER")
                .FirstOrDefaultAsync();

            if (orderRoleShipTo != null && !string.IsNullOrEmpty(orderRoleShipTo.PartyId))
            {
                shipmentContext.PartyIdTo = orderRoleShipTo.PartyId;
            }
        }

        // Determine partyIdFrom
        string partyIdFrom = null;
        if (!string.IsNullOrEmpty(this.PrimaryOrderId))
        {
            var orderItemShipGroup = await this.Context.OrderItemShipGroups
                .Where(o => o.OrderId == this.PrimaryOrderId && o.ShipGroupSeqId == this.PrimaryShipGrp)
                .FirstOrDefaultAsync();

            if (orderItemShipGroup != null)
            {
                if (!string.IsNullOrEmpty(orderItemShipGroup.VendorPartyId))
                {
                    partyIdFrom = orderItemShipGroup.VendorPartyId;
                }
                else if (!string.IsNullOrEmpty(orderItemShipGroup.FacilityId))
                {
                    var facility = await this.Context.Facilities
                        .Where(f => f.FacilityId == orderItemShipGroup.FacilityId)
                        .FirstOrDefaultAsync();

                    if (facility != null && !string.IsNullOrEmpty(facility.OwnerPartyId))
                    {
                        partyIdFrom = facility.OwnerPartyId;
                    }
                }

                if (string.IsNullOrEmpty(partyIdFrom))
                {
                    // Try role "SHIP_FROM_VENDOR"
                    var orderRoleShipFrom = await this.Context.OrderRoles
                        .Where(o => o.OrderId == this.PrimaryOrderId && o.RoleTypeId == "SHIP_FROM_VENDOR")
                        .FirstOrDefaultAsync();

                    if (orderRoleShipFrom != null && !string.IsNullOrEmpty(orderRoleShipFrom.PartyId))
                    {
                        partyIdFrom = orderRoleShipFrom.PartyId;
                    }
                    else
                    {
                        // Fallback to "BILL_FROM_VENDOR"
                        orderRoleShipFrom = await this.Context.OrderRoles
                            .Where(o => o.OrderId == this.PrimaryOrderId && o.RoleTypeId == "BILL_FROM_VENDOR")
                            .FirstOrDefaultAsync();
                        partyIdFrom = orderRoleShipFrom?.PartyId;
                    }
                }
            }
        }
        else if (!string.IsNullOrEmpty(this.FacilityId))
        {
            var facility = await this.Context.Facilities
                .Where(f => f.FacilityId == this.FacilityId)
                .FirstOrDefaultAsync();

            if (facility != null && !string.IsNullOrEmpty(facility.OwnerPartyId))
            {
                partyIdFrom = facility.OwnerPartyId;
            }
        }

        shipmentContext.PartyIdFrom = partyIdFrom;

        Logger.LogInformation("Creating new shipment with context: {@ShipmentContext}", shipmentContext);

        try
        {
            this.ShipmentId = await ShipmentHelperService.CreateShipment(shipmentContext);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error creating shipment");
            throw new Exception("Error creating shipment: " + ex.Message);
        }
    }

    private async Task CreatePackages()
    {
        // Loop through each package index from 0 to PackageSeq - 1.
        for (int i = 0; i < this.PackageSeq; i++)
        {
            // Format the package sequence as a 5-digit string (e.g., "00001").
            string shipmentPackageSeqId = (i + 1).ToString("D5");

            // Retrieve additional data for the package.
            // Here, GetShipmentBoxType and GetPackageWeight are instance methods that use the state of this session.
            string shipmentBoxTypeId = GetShipmentBoxType(i + 1);
            decimal? packageWeight = GetPackageWeight(i + 1);

            // Create a new ShipmentPackage entity using instance state.
            var shipmentPackage = new ShipmentPackage
            {
                ShipmentId = this.ShipmentId,
                ShipmentPackageSeqId = shipmentPackageSeqId,
                ShipmentBoxTypeId = shipmentBoxTypeId,
                Weight = packageWeight,
                WeightUomId = this.WeightUomId,
                //UserLogin = _userAccessor.GetUserLoginId() // Retrieve the current user login.
            };

            Context.ShipmentPackages.Add(shipmentPackage);
        }
    }

    public void SetShipmentBoxType(int packageSeqId, string shipmentBoxType)
    {
        if (string.IsNullOrEmpty(shipmentBoxType))
        {
            ShipmentBoxTypes.Remove(packageSeqId);
        }
        else
        {
            ShipmentBoxTypes[packageSeqId] = shipmentBoxType;
        }
    }

    public string GetShipmentBoxType(int packageSeqId)
    {
        if (ShipmentBoxTypes == null)
            return null;

        return ShipmentBoxTypes.TryGetValue(packageSeqId, out var boxType) ? boxType : null;
    }


    public decimal? GetPackageWeight(int packageSeqId)
    {
        if (this.PackageWeights == null)
            return null;

        this.PackageWeights.TryGetValue(packageSeqId, out decimal packageWeight);
        return packageWeight;
    }

    public async Task IssueItemsToShipment()
    {
        // Create a list to keep track of processed lines.
        var processedLines = new List<PackingSessionLine>();

        // Iterate over each packing line stored in the instance.
        foreach (var line in this.PackLines)
        {
            if (CheckLine(processedLines, line))
            {
                // Sum the total packed quantity for this line across all matching lines.
                decimal totalPacked = GetPackedQuantity(line.OrderId, line.OrderItemSeqId, line.ShipGroupSeqId,
                    line.ProductId, line.InventoryItemId, -1);
                // Issue the item to the shipment.
                // The line itself handles the issuance given the shipment data.
                await line.IssueItemToShipment(this.ShipmentId, this.PicklistBinId, this.UserLogin.UserLoginId,
                    totalPacked);

                // Add this line to the processed list to avoid duplications.
                processedLines.Add(line);
            }
        }
    }

    protected bool CheckLine(List<PackingSessionLine> processedLines, PackingSessionLine line)
    {
        foreach (var l in processedLines)
        {
            if (line.IsSameItem(l))
            {
                line.ShipmentItemSeqId = l.ShipmentItemSeqId;
                return false;
            }
        }

        return true;
    }

    public async Task ApplyItemsToPackages()
    {
        foreach (var line in this.PackLines)
        {
            // Apply each line to a package using the instance ShipmentId and UserLogin.
            await line.ApplyLineToPackage(this.ShipmentId, this.UserLogin?.UserLoginId);
        }
    }

    public async Task UpdateShipmentRouteSegments()
    {
        // Retrieve the total weight from the current session.
        decimal shipmentWeight = GetTotalWeight();
        if (shipmentWeight <= 0)
            return;

        // Query all shipment route segments associated with the current shipment.
        var segments = await Context.ShipmentRouteSegments
            .Where(s => s.ShipmentId == this.ShipmentId)
            .ToListAsync();

        if (segments == null || segments.Count == 0)
            return;

        // Update each segment with the billing weight and the weight UOM.
        foreach (var segment in segments)
        {
            segment.BillingWeight = shipmentWeight;
            segment.BillingWeightUomId = this.WeightUomId;
        }

        // Save the updates.
        // await Context.SaveChangesAsync();
    }

    public decimal GetTotalWeight()
    {
        decimal total = 0m;
        for (int i = 0; i < this.PackageSeq; i++)
        {
            decimal? packageWeight = GetPackageWeight(i);
            if (packageWeight.HasValue)
            {
                total += packageWeight.Value;
            }
        }

        return total;
    }

    private async Task SetShipmentToPacked()
    {
        // Create the parameters object with values from your context.
        var updateParams = new ShipmentUpdateParameters
        {
            ShipmentId = this.ShipmentId, // assuming shipmentId is a class-level variable
            StatusId = "SHIPMENT_PACKED",
            // Other fields (like EstimatedShipDate, OriginFacilityId, etc.) can be left null
            // if you only want to update the status.
        };

        // Call the service to update the shipment.
        var updateResult = await ShipmentHelperService.UpdateShipment(updateParams);
    }


    private async Task SetPickerOnPicklist(string picklistBinId, string pickerPartyId, string userLoginId)
    {
        if (!string.IsNullOrEmpty(picklistBinId))
        {
            // First, find the PicklistBin record by its ID.
            var bin = await Context.PicklistBins
                .FirstOrDefaultAsync(b => b.PicklistBinId == picklistBinId);
            if (bin != null)
            {
                string picklistId = bin.PicklistId;
                // Check if a PicklistRole already exists for the given picklistId, pickerPartyId and role "PICKER".
                var existingRole = await Context.PicklistRoles
                    .FirstOrDefaultAsync(r => r.PicklistId == picklistId &&
                                              r.PartyId == pickerPartyId &&
                                              r.RoleTypeId == "PICKER");
                // If no existing role is found, create one.
                if (existingRole == null)
                {
                    var newRole = new PicklistRole
                    {
                        PicklistId = picklistId,
                        PartyId = pickerPartyId,
                        RoleTypeId = "PICKER",
                        //UserLogin = userLoginId
                    };
                    Context.PicklistRoles.Add(newRole);
                }
            }
            else
            {
                Logger.LogInformation("No PicklistBin record found for picklistBinId {PicklistBinId}", picklistBinId);
            }
        }
    }

    private async Task SetPicklistToPicked(string picklistBinId, string primaryOrderId, string userLoginId)
    {
        // Terminal statuses that should not be updated.
        string[] terminalStatuses = { "PICKLIST_PICKED", "PICKLIST_COMPLETED", "PICKLIST_CANCELLED" };

        if (!string.IsNullOrEmpty(picklistBinId))
        {
            // Use a join between Picklists and PicklistBins on picklistId
            var record = await (from p in Context.Picklists
                    join pb in Context.PicklistBins on p.PicklistId equals pb.PicklistId
                    where pb.PicklistBinId == picklistBinId
                    select new { Picklist = p, PicklistBin = pb })
                .FirstOrDefaultAsync();

            if (record != null)
            {
                // Check the status on the Picklist record (not PicklistBin)
                if (!terminalStatuses.Contains(record.Picklist.StatusId))
                {
                    record.Picklist.StatusId = "PICKLIST_PICKED";
                    //record.Picklist.UserLogin = userLoginId;
                    Context.Picklists.Update(record.Picklist);
                    //await Context.SaveChangesAsync();
                }
            }
            else
            {
                Logger.LogInformation("No picklist record found for picklistBinId {PicklistBinId}", picklistBinId);
            }
        }
        else
        {
            // When no picklistBinId is provided, update all picklists for the given primaryOrderId.
            var records = await (from p in Context.Picklists
                    join pb in Context.PicklistBins on p.PicklistId equals pb.PicklistId
                    where pb.PrimaryOrderId == primaryOrderId
                    select new { Picklist = p, PicklistBin = pb })
                .ToListAsync();

            if (records != null && records.Any())
            {
                foreach (var rec in records)
                {
                    // Again, check the status on the Picklist record.
                    if (!terminalStatuses.Contains(rec.Picklist.StatusId))
                    {
                        rec.Picklist.StatusId = "PICKLIST_PICKED";
                        //rec.Picklist.UserLogin = userLoginId;
                        Context.Picklists.Update(rec.Picklist);
                    }
                }
            }
        }
    }

    private void AddToPackageWeight(int packageSeqId, decimal weight)
    {
        // If weight is 0, there's nothing to add.
        if (weight == 0)
            return;

        // Retrieve the current package weight.
        decimal? packageWeight = GetPackageWeight(packageSeqId);

        // If there's no current weight, new weight is the given weight; otherwise, sum them.
        decimal newPackageWeight = packageWeight.HasValue ? packageWeight.Value + weight : weight;

        // Update the package weight.
        SetPackageWeight(packageSeqId, newPackageWeight);
    }

    public void SetPackageWeight(int packageSeqId, decimal? packageWeight)
    {
        if (!packageWeight.HasValue)
        {
            PackageWeights.Remove(packageSeqId);
        }
        else
        {
            PackageWeights[packageSeqId] = packageWeight.Value;
        }
    }

    public void SetHandlingInstructions(string instructions)
    {
        Instructions = instructions;
    }

    public void SetPickerPartyId(string partyId)
    {
        PickerPartyId = partyId;
    }

    public void SetAdditionalShippingCharge(decimal? additionalShippingCharge)
    {
        AdditionalShippingCharge = additionalShippingCharge;
    }

    public void SetWeightUomId(string weightUomId)
    {
        WeightUomId = weightUomId;
    }

    public void SetPrimaryOrderId(string orderId)
    {
        this.PrimaryOrderId = orderId;
    }

    public void SetPrimaryShipGroupSeqId(string shipGroupSeqId)
    {
        this.PrimaryShipGrp = shipGroupSeqId;
    }

    public void SetPicklistBinId(string binId)
    {
        this.PicklistBinId = binId;
    }

    public void AddItemInfo(List<ShippableItemDto> infos)
    {
        foreach (var dto in infos)
        {
            var newItem = new ItemDisplay(dto);
            // Try to find an existing item that is equal to newItem.
            var existingItem = ItemInfos.FirstOrDefault(item => item.Equals(newItem));
            if (existingItem != null)
            {
                existingItem.Quantity += newItem.Quantity;
            }
            else
            {
                ItemInfos.Add(newItem);
            }
        }
    }

    public List<ItemDisplay> GetItemInfos()
    {
        return ItemInfos;
    }
}