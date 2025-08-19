namespace Domain;

public class FixedAssetIdent
{
    public string FixedAssetId { get; set; } = null!;
    public string FixedAssetIdentTypeId { get; set; } = null!;
    public string? IdValue { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public FixedAsset FixedAsset { get; set; } = null!;
    public FixedAssetIdentType FixedAssetIdentType { get; set; } = null!;
}