namespace Application.Catalog.Products;

public class ProductLovParams
{
    public string? SearchTerm { get; set; }
    public string? SupplierId { get; set; }
    public string? VehicleId { get; set; }
    public string? FacilityId { get; set; }
    public string? ProductId { get; set; }
    public int Skip { get; set; }
    public int PageSize { get; set; }
}