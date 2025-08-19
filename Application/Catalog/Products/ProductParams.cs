using Application.Core;

namespace Application.Catalog.Products;

public class ProductParams : PaginationParams
{
    public string? OrderBy { get; set; }
    public string? ProductName { get; set; }
    public string? ProductTypes { get; set; }
    public string? ProductCategory { get; set; }
}