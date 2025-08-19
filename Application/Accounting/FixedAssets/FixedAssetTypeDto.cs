namespace Application.Shipments.FixedAssets;

public class FixedAssetTypeDto
{
    public string FixedAssetTypeId { get; set; } = null!;
    public string? ParentTypeId { get; set; }
    public string? HasTable { get; set; }
    public string? Description { get; set; }
}