using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;
[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]

public class ItemIssuance
{
    public ItemIssuance()
    {
        InventoryItemDetails = new HashSet<InventoryItemDetail>();
        InventoryTransfers = new HashSet<InventoryTransfer>();
        ItemIssuanceRoles = new HashSet<ItemIssuanceRole>();
        OrderItemBillings = new HashSet<OrderItemBilling>();
    }

    public string ItemIssuanceId { get; set; } = null!;
    public string? OrderId { get; set; }
    public string? OrderItemSeqId { get; set; }
    public string? ShipGroupSeqId { get; set; }
    public string? InventoryItemId { get; set; }
    public string? ShipmentId { get; set; }
    public string? ShipmentItemSeqId { get; set; }
    public string? FixedAssetId { get; set; }
    public string? MaintHistSeqId { get; set; }
    public DateTime? IssuedDateTime { get; set; }
    public string? IssuedByUserLoginId { get; set; }
    public decimal? Quantity { get; set; }
    public decimal? CancelQuantity { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public FixedAssetMaint? FixedAssetMaint { get; set; }
    public InventoryItem? InventoryItem { get; set; }
    public UserLogin? IssuedByUserLogin { get; set; }
    public OrderItem? OrderI { get; set; }
    public ShipmentItem? ShipmentI { get; set; }
    public ICollection<InventoryItemDetail> InventoryItemDetails { get; set; }
    public ICollection<InventoryTransfer> InventoryTransfers { get; set; }
    public ICollection<ItemIssuanceRole> ItemIssuanceRoles { get; set; }
    public ICollection<OrderItemBilling> OrderItemBillings { get; set; }
}