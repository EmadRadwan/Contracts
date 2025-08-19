namespace Application.Accounting.OrganizationGlSettings;

public class InventoryValuationContext
{
    /// <summary>
    /// A collection of inventory valuation line items, each representing aggregated data 
    /// (product, cost, quantities) derived from InventoryItemDetailForSum.
    /// </summary>
    public List<InventoryValuationItem> Items { get; set; } = new List<InventoryValuationItem>();

    /// <summary>
    /// The total computed value of all items in the valuation.
    /// </summary>
    public decimal TotalValue { get; set; }
}