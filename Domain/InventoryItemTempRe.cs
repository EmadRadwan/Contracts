namespace Domain;

public class InventoryItemTempRe
{
    public string VisitId { get; set; } = null!;
    public string ProductId { get; set; } = null!;
    public string ProductStoreId { get; set; } = null!;
    public decimal? Quantity { get; set; }
    public DateTime? ReservedDate { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Product Product { get; set; } = null!;
    public ProductStore ProductStore { get; set; } = null!;
}