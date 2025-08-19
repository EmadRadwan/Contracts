namespace Domain;

public class FixedAssetMaintOrder
{
    public string FixedAssetId { get; set; } = null!;
    public string MaintHistSeqId { get; set; } = null!;
    public string OrderId { get; set; } = null!;
    public string OrderItemSeqId { get; set; } = null!;
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public FixedAsset FixedAsset { get; set; } = null!;
    public OrderHeader Order { get; set; } = null!;
}