namespace Domain;

public class ProductPriceChange
{
    public string ProductPriceChangeId { get; set; } = null!;
    public string? ProductId { get; set; }
    public string? ProductPriceTypeId { get; set; }
    public string? ProductPricePurposeId { get; set; }
    public string? CurrencyUomId { get; set; }
    public string? ProductStoreGroupId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public decimal? Price { get; set; }
    public decimal? OldPrice { get; set; }
    public DateTime? ChangedDate { get; set; }
    public string? ChangedByUserLogin { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public UserLogin? ChangedByUserLoginNavigation { get; set; }
}