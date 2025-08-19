namespace Domain;

public class Lot
{
    public Lot()
    {
        InventoryItems = new HashSet<InventoryItem>();
    }

    public string LotId { get; set; } = null!;
    public DateTime? CreationDate { get; set; }
    public decimal? Quantity { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ICollection<InventoryItem> InventoryItems { get; set; }
}