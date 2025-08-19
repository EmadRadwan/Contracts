namespace Application.Catalog.Products;

public class FinishedProductLovDto
{
    public string ProductId { get; set; }
    public string ProductName { get; set; }
    public decimal Quantity { get; set; } // From ProductAssoc
}