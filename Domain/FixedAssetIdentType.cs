namespace Domain;

public class FixedAssetIdentType
{
    public FixedAssetIdentType()
    {
        FixedAssetIdents = new HashSet<FixedAssetIdent>();
    }

    public string FixedAssetIdentTypeId { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ICollection<FixedAssetIdent> FixedAssetIdents { get; set; }
}