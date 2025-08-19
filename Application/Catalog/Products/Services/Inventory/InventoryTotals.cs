namespace Application.Catalog.Products.Services.Inventory;

public class InventoryTotals
{
    public decimal? AvailableToPromiseTotal { get; set; }
    public decimal? QuantityOnHandTotal { get; set; }
    public decimal? AccountingQuantityTotal { get; set; }
}