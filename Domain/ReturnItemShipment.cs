namespace Domain;

public class ReturnItemShipment
{
    public string ReturnId { get; set; } = null!;
    public string ReturnItemSeqId { get; set; } = null!;
    public string ShipmentId { get; set; } = null!;
    public string ShipmentItemSeqId { get; set; } = null!;
    public decimal? Quantity { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ReturnHeader Return { get; set; } = null!;
    public ReturnItem ReturnI { get; set; } = null!;
    public Shipment Shipment { get; set; } = null!;
    public ShipmentItem ShipmentI { get; set; } = null!;
}