using Domain;

namespace Application.Catalog.Products;

public class ProductVariantTreeResponse
{
    public string ResponseMessage { get; set; }
    public string ErrorMessage { get; set; }
    public Dictionary<string, List<string>> VariantTree { get; set; }
    public List<string> VirtualVariant { get; set; }
    public List<Product> UnavailableVariants { get; set; }
    public Dictionary<string, Product> VariantSample { get; set; }
}