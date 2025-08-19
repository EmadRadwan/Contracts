namespace Domain;

public class ProductFeaturePrice
{
    public string ProductFeatureId { get; set; } = null!;
    public string ProductPriceTypeId { get; set; } = null!;
    public string CurrencyUomId { get; set; } = null!;
    public DateTime FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public decimal? Price { get; set; }
    public DateTime? CreatedDate { get; set; }
    public string? CreatedByUserLogin { get; set; }
    public DateTime? LastModifiedDate { get; set; }
    public string? LastModifiedByUserLogin { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public UserLogin? CreatedByUserLoginNavigation { get; set; }
    public Uom CurrencyUom { get; set; } = null!;
    public UserLogin? LastModifiedByUserLoginNavigation { get; set; }
    public ProductPriceType ProductPriceType { get; set; } = null!;
}