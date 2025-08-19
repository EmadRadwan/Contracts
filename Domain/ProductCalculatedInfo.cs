namespace Domain;

public class ProductCalculatedInfo
{
    public string ProductId { get; set; } = null!;
    public decimal? TotalQuantityOrdered { get; set; }
    public int? TotalTimesViewed { get; set; }
    public decimal? AverageCustomerRating { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Product Product { get; set; } = null!;
}