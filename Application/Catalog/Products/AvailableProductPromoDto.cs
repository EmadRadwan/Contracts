namespace Application.Catalog.Products;

public class AvailableProductPromoDto
{
    public string ProductPromoActionEnumId { get; set; }
    public string ProductPromoId { get; set; }
    public string ProductId { get; set; }
    public string TargetProductId { get; set; }
    public string PromoText { get; set; }
}