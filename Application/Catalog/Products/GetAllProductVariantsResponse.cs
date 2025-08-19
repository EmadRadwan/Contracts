using Domain;

namespace Application.Catalog.Products;

public class GetAllProductVariantsResponse
{
    public List<ProductAssoc> AssocProducts { get; set; } = new List<ProductAssoc>();
    public string ResponseMessage { get; set; }
}
