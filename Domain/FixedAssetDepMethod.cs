namespace Domain;

public class FixedAssetDepMethod
{
    public string DepreciationCustomMethodId { get; set; } = null!;
    public string FixedAssetId { get; set; } = null!;
    public DateTime? FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public CustomMethod DepreciationCustomMethod { get; set; } = null!;
    public FixedAsset FixedAsset { get; set; } = null!;
}