namespace Application.Shipments.FixedAssets;

public class FixedAssetStdCostDto
{
    public string FixedAssetId { get; set; } = null!;
    public string FixedAssetStdCostTypeId { get; set; } = null!;
    public DateTime FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public string? AmountUomId { get; set; }
    public decimal? Amount { get; set; }
}