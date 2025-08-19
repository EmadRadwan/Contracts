namespace Domain;

public class SalesForecastDetail
{
    public string SalesForecastId { get; set; } = null!;
    public string SalesForecastDetailId { get; set; } = null!;
    public decimal? Amount { get; set; }
    public string? QuantityUomId { get; set; }
    public decimal? Quantity { get; set; }
    public string? ProductId { get; set; }
    public string? ProductCategoryId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Product? Product { get; set; }
    public ProductCategory? ProductCategory { get; set; }
    public Uom? QuantityUom { get; set; }
    public SalesForecast SalesForecast { get; set; } = null!;
}