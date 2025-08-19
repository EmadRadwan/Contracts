namespace Domain;

public class ShipmentPackageContent
{
    public string ShipmentId { get; set; } = null!;
    public string ShipmentPackageSeqId { get; set; } = null!;
    public string ShipmentItemSeqId { get; set; } = null!;
    public decimal? Quantity { get; set; }
    public string? SubProductId { get; set; }
    public decimal? SubProductQuantity { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ShipmentPackage Shipment { get; set; } = null!;
    public ShipmentItem ShipmentI { get; set; } = null!;
    public Product? SubProduct { get; set; }
}