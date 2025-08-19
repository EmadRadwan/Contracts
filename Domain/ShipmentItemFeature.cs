namespace Domain;

public class ShipmentItemFeature
{
    public string ShipmentId { get; set; } = null!;
    public string ShipmentItemSeqId { get; set; } = null!;
    public string ProductFeatureId { get; set; } = null!;
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ProductFeature ProductFeature { get; set; } = null!;
    public ShipmentItem ShipmentI { get; set; } = null!;
}