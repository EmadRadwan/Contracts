namespace Domain;

public class ShippingDocument
{
    public string DocumentId { get; set; } = null!;
    public string? ShipmentId { get; set; }
    public string? ShipmentItemSeqId { get; set; }
    public string? ShipmentPackageSeqId { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Document Document { get; set; } = null!;
    public ShipmentPackage? Shipment { get; set; }
    public ShipmentItem? ShipmentI { get; set; }
}