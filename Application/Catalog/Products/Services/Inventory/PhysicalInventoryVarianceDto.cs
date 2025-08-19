namespace Application.Catalog.Products.Services.Inventory;

public class PhysicalInventoryVarianceDto
{
    // For PhysicalInventory
    public string InventoryItemId { get; set; }
    public string? PhysicalInventoryId { get; set; }
    public DateTime? PhysicalInventoryDate { get; set; }
    public string? PartyId { get; set; }
    public string? GeneralComments { get; set; }

    // For InventoryItemVariance
    public decimal? QuantityOnHandVar { get; set; }       // Quantity On Hand Variation
    public decimal? AvailableToPromiseVar { get; set; }   // Available To Promise Variation
    public decimal? UnitCost { get; set; }
    public string? VarianceReasonId { get; set; }         // Matches VarianceReasonId field in Variance
    public string? Comments { get; set; }                 // Comments for the Variance
}
