namespace Application.Shipments.Taxes;

public class TaxProductCategoryDto
{
    public string TaxAuthGeoId { get; set; }
    public string TaxAuthPartyId { get; set; }
    public string ProductCategoryId { get; set; }
    public string ProductCategoryName { get; set; }  // From ProductCategories
}
