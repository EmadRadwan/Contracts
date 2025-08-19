namespace Application.Catalog.Products.Services.Inventory;

public class CreateInventoryItemVarianceDto
{
    public string InventoryItemId { get; set; } = default!;
    public string PhysicalInventoryId { get; set; } = default!;
    public decimal? AvailableToPromiseVar { get; set; }
    public decimal? QuantityOnHandVar { get; set; }
    public string? VarianceReasonId { get; set; }
    public string? Comments { get; set; }
}