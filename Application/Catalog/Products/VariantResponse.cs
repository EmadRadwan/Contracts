namespace Application.Catalog.Products;

public class VariantResponse
{
    public List<VariantProduct> Variants { get; set; } = new List<VariantProduct>(); // List of variants
}