namespace Application.Catalog.Products.Services.Inventory;

public class ProductInventoryAvailableResponse
{
    public decimal QuantityOnHandTotal { get; set; }
    public decimal AvailableToPromiseTotal { get; set; }
}