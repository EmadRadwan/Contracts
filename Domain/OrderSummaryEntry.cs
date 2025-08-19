namespace Domain;

public class OrderSummaryEntry
{
    public DateTime EntryDate { get; set; }
    public string ProductId { get; set; } = null!;
    public string FacilityId { get; set; } = null!;
    public decimal? TotalQuantity { get; set; }
    public decimal? GrossSales { get; set; }
    public decimal? ProductCost { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Facility Facility { get; set; } = null!;
    public Product Product { get; set; } = null!;
}