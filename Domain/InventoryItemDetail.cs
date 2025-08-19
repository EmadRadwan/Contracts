using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;
[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]

public class InventoryItemDetail
{
    public string InventoryItemId { get; set; } = null!;
    public string InventoryItemDetailSeqId { get; set; } = null!;
    public DateTime? EffectiveDate { get; set; }
    public decimal? QuantityOnHandDiff { get; set; }
    public decimal? AvailableToPromiseDiff { get; set; }
    public decimal? AccountingQuantityDiff { get; set; }
    public decimal? UnitCost { get; set; }
    public string? OrderId { get; set; }
    public string? OrderItemSeqId { get; set; }
    public string? ShipGroupSeqId { get; set; }
    public string? ShipmentId { get; set; }
    public string? ShipmentItemSeqId { get; set; }
    public string? ReturnId { get; set; }
    public string? ReturnItemSeqId { get; set; }
    public string? WorkEffortId { get; set; }
    public string? FixedAssetId { get; set; }
    public string? MaintHistSeqId { get; set; }
    public string? ItemIssuanceId { get; set; }
    public string? ReceiptId { get; set; }
    public string? PhysicalInventoryId { get; set; }
    public string? ReasonEnumId { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public FixedAssetMaint? FixedAssetMaint { get; set; }
    public InventoryItem InventoryItem { get; set; } = null!;
    public ItemIssuance? ItemIssuance { get; set; }
    public PhysicalInventory? PhysicalInventory { get; set; }
    public Enumeration? ReasonEnum { get; set; }
    public ShipmentReceipt? Receipt { get; set; }
    public WorkEffort? WorkEffort { get; set; }
}