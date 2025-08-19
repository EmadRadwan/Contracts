namespace Domain;

public class FixedAssetMaint
{
    public FixedAssetMaint()
    {
        FixedAssetMeters = new HashSet<FixedAssetMeter>();
        InventoryItemDetails = new HashSet<InventoryItemDetail>();
        ItemIssuances = new HashSet<ItemIssuance>();
    }

    public string FixedAssetId { get; set; } = null!;
    public string MaintHistSeqId { get; set; } = null!;
    public string? StatusId { get; set; }
    public string? ProductMaintTypeId { get; set; }
    public string? ProductMaintSeqId { get; set; }
    public string? ScheduleWorkEffortId { get; set; }
    public decimal? IntervalQuantity { get; set; }
    public string? IntervalUomId { get; set; }
    public string? IntervalMeterTypeId { get; set; }
    public string? PurchaseOrderId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public FixedAsset FixedAsset { get; set; } = null!;
    public ProductMeterType? IntervalMeterType { get; set; }
    public Uom? IntervalUom { get; set; }
    public ProductMaintType? ProductMaintType { get; set; }
    public OrderHeader? PurchaseOrder { get; set; }
    public WorkEffort? ScheduleWorkEffort { get; set; }
    public StatusItem? Status { get; set; }
    public ICollection<FixedAssetMeter> FixedAssetMeters { get; set; }
    public ICollection<InventoryItemDetail> InventoryItemDetails { get; set; }
    public ICollection<ItemIssuance> ItemIssuances { get; set; }
}