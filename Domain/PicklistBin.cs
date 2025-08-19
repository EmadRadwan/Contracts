namespace Domain;

public class PicklistBin
{
    public PicklistBin()
    {
        PicklistItems = new HashSet<PicklistItem>();
        Shipments = new HashSet<Shipment>();
    }

    public string PicklistBinId { get; set; } = null!;
    public string? PicklistId { get; set; }
    public int? BinLocationNumber { get; set; }
    public string? PrimaryOrderId { get; set; }
    public string? PrimaryShipGroupSeqId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Picklist? Picklist { get; set; }
    public OrderItemShipGroup? Primary { get; set; }
    public ICollection<PicklistItem> PicklistItems { get; set; }
    public ICollection<Shipment> Shipments { get; set; }
}