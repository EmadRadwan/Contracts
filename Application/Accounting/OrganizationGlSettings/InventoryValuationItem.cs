namespace Application.Accounting.OrganizationGlSettings;

public class InventoryValuationItem
{
    /// <summary>
    /// The ID of the product being valued.
    /// </summary>
    public string? ProductId { get; set; }
    public string? ProductName { get; set; }
    public string? QuantityUomDescription { get; set; }

    /// <summary>
    /// The unit cost of the product.
    /// </summary>
    public decimal? UnitCost { get; set; }

    /// <summary>
    /// The currency UOM used for valuation.
    /// </summary>
    public string? CurrencyUomId { get; set; }

    /// <summary>
    /// The aggregated accounting quantity sum for the product.
    /// </summary>
    public decimal? AccountingQuantitySum { get; set; }

    /// <summary>
    /// The aggregated quantity on hand sum for the product.
    /// </summary>
    public decimal? QuantityOnHandSum { get; set; }

    /// <summary>
    /// The computed value of the product (e.g., AccountingQuantitySum * UnitCost).
    /// </summary>
    public decimal? Value { get; set; }
}