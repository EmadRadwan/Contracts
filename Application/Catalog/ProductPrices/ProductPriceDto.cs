namespace Application.Catalog.ProductPrices;

public class ProductPriceDto
{
    public string ProductId { get; set; }
    public string ProductPriceTypeId { get; set; }
    public string CurrencyUomId { get; set; }
    public string ProductPriceTypeDescription { get; set; }
    public string CurrencyUomDescription { get; set; }

    public DateTime FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public decimal? Price { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public byte[]? RowVersion { get; set; }
}