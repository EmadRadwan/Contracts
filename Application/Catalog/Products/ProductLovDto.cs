namespace Application.Catalog.Products;

public class ProductLovDto
{
    public string ProductId { get; set; }
    public string ProductName { get; set; }
    public string? ProductTypeId { get; set; }
    public string? FacilityId { get; set; }
    public string? FacilityName { get; set; }
    public string? InventoryItem { get; set; }
    public decimal? QuantityOnHandTotal { get; set; }
    public decimal? AvailableToPromiseTotal { get; set; }
    public List<ProductLovDto>? RelatedRecords { get; set; }
    public decimal? Price { get; set; }
    public decimal? ListPrice { get; set; }
    public decimal? LastPrice { get; set; }
    public string? ProductFeatureId { get; set; } // Supports color variant ID
    public string? ColorDescription { get; set; } // Supports color name for display
    public DateTime? AvailableFromDate { get; set; }
    public DateTime? AvailableThruDate { get; set; }
    public string? QuantityUom { get; set; }
    public string? QuantityIncluded { get; set; }
    public string? UomDescription { get; set; }
}