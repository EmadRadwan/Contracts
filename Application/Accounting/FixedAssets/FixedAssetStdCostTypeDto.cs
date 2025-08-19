namespace Application.Shipments.FixedAssets;

public class FixedAssetStdCostTypeDto
{
    public string FixedAssetStdCostTypeId { get; set; } = null!;
    public string? ParentTypeId { get; set; }
    public string? HasTable { get; set; }
    public string? Description { get; set; }
}