namespace Application.Manufacturing;

public class CostComponentDto
{
    public string? CostComponentId { get; set; } = null!;

    public string? CostComponentTypeId { get; set; }
    public string? CostComponentTypeDescription { get; set; }
    public string? ProductId { get; set; }
    public string? ProductName { get; set; }
    public string? ProductFeatureId { get; set; }
    public string? PartyId { get; set; }
    public string? GeoId { get; set; }
    public string? WorkEffortId { get; set; }
    public string? WorkEffortName { get; set; }
    public string? FixedAssetId { get; set; }
    public string? FixedAssetName { get; set; }
    public string? CostComponentCalcId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public decimal? Cost { get; set; }
    public string? CostUomId { get; set; }
    public string? CurrencyUomId { get; set; }
}