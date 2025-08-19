using Application.Facilities;
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Shipments;

public class PackingSessionLine
{
    // -----------------------
    // Properties (state)
    // -----------------------

    public DataContext Context { get; set; }

    // Create a static logger factory and logger instance.
    private static readonly ILoggerFactory _loggerFactory = LoggerFactory.Create(builder =>
    {
        builder.AddConsole(); // You can add additional providers if needed.
    });

    private static readonly ILogger<PackingSessionLine> _logger = _loggerFactory.CreateLogger<PackingSessionLine>();

    /// <summary>
    /// Issuance service used for issuing items to shipments.
    /// </summary>
    public IIssuanceService IssuanceService { get; set; }

    public IPickListService PickListService { get; set; }

    public string OrderId { get; private set; }
    public string OrderItemSeqId { get; private set; }
    public string ShipGroupSeqId { get; private set; }
    public string ProductId { get; private set; }
    public string InventoryItemId { get; private set; }
    public string ShipmentItemSeqId { get; set; }
    public decimal Quantity { get; set; } = 0m;
    public decimal Weight { get; set; } = 0m;
    public decimal? Height { get; set; } = null;
    public decimal? Width { get; set; } = null;
    public decimal? Length { get; set; } = null;
    public string ShipmentBoxTypeId { get; set; } = null;
    public string WeightPackageSeqId { get; set; } = null;
    public int PackageSeq { get; private set; }

    // -----------------------
    // Constructor
    // -----------------------

    /// <summary>
    /// Creates a new packing session line with the provided values.
    /// </summary>
    public PackingSessionLine(string orderId, string orderItemSeqId,
        string shipGroupSeqId,
        string productId, string inventoryItemId,
        decimal quantity, decimal weight, int packageSeq)
    {
        PickListService = PickListService;

        OrderId = orderId;
        OrderItemSeqId = orderItemSeqId;
        ShipGroupSeqId = shipGroupSeqId;
        ProductId = productId;
        InventoryItemId = inventoryItemId;
        Quantity = quantity;
        Weight = weight;
        // Dimensions and box type remain null by default.
        Height = null;
        Width = null;
        Length = null;
        ShipmentBoxTypeId = null;
        WeightPackageSeqId = null;
        PackageSeq = packageSeq;
    }

    // -----------------------
    // Business Methods
    // -----------------------

    /// <summary>
    /// Checks whether this line represents the same order item as the provided line.
    /// Two lines are considered the same if they have the same orderId, orderItemSeqId,
    /// shipGroupSeqId, and inventoryItemId.
    /// </summary>
    public bool IsSameItem(PackingSessionLine other)
    {
        if (other == null)
            return false;
        return this.OrderId == other.OrderId &&
               this.OrderItemSeqId == other.OrderItemSeqId &&
               this.ShipGroupSeqId == other.ShipGroupSeqId &&
               this.InventoryItemId == other.InventoryItemId;
    }

    public async Task IssueItemToShipment(string shipmentId, string picklistBinId, string userLoginId,
        decimal? issueQuantity)
    {
        try
        {
            // Use the line's quantity if no specific quantity is provided.
            decimal qty = issueQuantity ?? this.Quantity;

            // Build the issue data object.
            var issueData = new IssueOrderItemShipGrpInvResParameters
            {
                ShipmentId = shipmentId,
                OrderId = this.OrderId,
                OrderItemSeqId = this.OrderItemSeqId,
                ShipGroupSeqId = this.ShipGroupSeqId,
                InventoryItemId = this.InventoryItemId,
                Quantity = qty,
                EventDate = DateTime.UtcNow // Assuming EventDate is the current timestamp
            };

            // Call the issuance service asynchronously.
            var issueResponse = await IssuanceService.IssueOrderItemShipGrpInvResToShipment(issueData);

            // Retrieve the shipment item sequence ID from the response.
            string shipmentItemSeqId = issueResponse.ShipmentItemSeqId;
            if (string.IsNullOrEmpty(shipmentItemSeqId))
            {
                throw new Exception("Issue item did not return a valid shipment item sequence ID!");
            }
            else
            {
                this.SetShipmentItemSeqId(shipmentItemSeqId);
            }

            // If a picklist bin ID is provided, update the picklist item.
            if (!string.IsNullOrEmpty(picklistBinId))
            {
                _logger.LogInformation("Looking up picklist item for bin ID #{PicklistBinId}", picklistBinId);

                // Query the database for the picklist item using LINQ
                var plItem = await Context.PicklistItems
                    .Where(pi => pi.PicklistBinId == picklistBinId &&
                                 pi.OrderId == this.OrderId &&
                                 pi.OrderItemSeqId == this.OrderItemSeqId &&
                                 pi.ShipGroupSeqId == this.ShipGroupSeqId &&
                                 pi.InventoryItemId == this.InventoryItemId)
                    .FirstOrDefaultAsync();

                if (plItem != null)
                {
                    _logger.LogInformation("Found picklist bin: {@PicklistItem}", plItem);
                    // Compare the picklist item quantity with the issued quantity.
                    string itemStatusId = plItem.Quantity == qty ? "PICKITEM_COMPLETED" : "PICKITEM_CANCELLED";

                    // Call the update picklist item service with the proper parameters.
                    await PickListService.UpdatePicklistItem(
                        picklistBinId, this.OrderId, this.OrderItemSeqId, this.ShipGroupSeqId, this.InventoryItemId,
                        itemStatusId, qty);
                }
                else
                {
                    _logger.LogInformation(
                        "No picklist item was found for lookup: PicklistBinId={PicklistBinId}, OrderId={OrderId}, OrderItemSeqId={OrderItemSeqId}, ShipGroupSeqId={ShipGroupSeqId}, InventoryItemId={InventoryItemId}",
                        picklistBinId, this.OrderId, this.OrderItemSeqId, this.ShipGroupSeqId, this.InventoryItemId);
                }
            }
            else
            {
                _logger.LogWarning("*** NO Picklist Bin ID set; cannot update picklist status!");
            }
        }
        catch (Exception ex)
        {
            // Log the exception and rethrow it for higher-level handling.
            _logger.LogError(ex, "Error occurred while issuing item to shipment.");
            throw;
        }
    }

    /// <summary>
    /// Sets the ShipmentItemSeqId property.
    /// </summary>
    public void SetShipmentItemSeqId(string shipmentItemSeqId)
    {
        this.ShipmentItemSeqId = shipmentItemSeqId;
    }

    /// <summary>
    /// Applies this packing line to a shipment package.
    /// This creates a record linking the shipment item with a package.
    /// </summary>
    /// <param name="shipmentId">The shipment identifier.</param>
    /// <param name="userLoginId">The user login id.</param>
    /// <returns>An asynchronous task.</returns>
    public async Task ApplyLineToPackage(string shipmentId, string userLoginId)
    {
        // Format the package sequence as a 5-digit string.
        string shipmentPackageSeqId = PackageSeq.ToString("D5");

        var packageData = new Dictionary<string, object>
        {
            { "shipmentId", shipmentId },
            { "shipmentItemSeqId", this.ShipmentItemSeqId },
            { "quantity", this.Quantity },
            { "shipmentPackageSeqId", shipmentPackageSeqId },
            { "userLogin", userLoginId }
        };

        var packageResponse = await AddShipmentContentToPackage(packageData);
        if (packageResponse.IsError)
        {
            throw new Exception(packageResponse.ErrorMessage);
        }
    }


    private async Task<ServiceResponse> AddShipmentContentToPackage(Dictionary<string, object> packageData)
    {
        // Simulate an async call.
        await Task.Delay(100);
        return new ServiceResponse { IsError = false };
    }

    // -----------------------
    // Logging helpers (replace with your logging framework)
    // -----------------------
    private void DebugLogInfo(string message)
    {
        Console.WriteLine("[INFO] " + message);
    }

    private void DebugLogWarning(string message)
    {
        Console.WriteLine("[WARN] " + message);
    }
}

public class ServiceResponse
{
    public bool IsError { get; set; }
    public string ErrorMessage { get; set; }
    public Dictionary<string, object> Data { get; set; } = new Dictionary<string, object>();

    public T GetValue<T>(string key)
    {
        if (Data != null && Data.TryGetValue(key, out object value) && value is T)
        {
            return (T)value;
        }

        return default;
    }
}