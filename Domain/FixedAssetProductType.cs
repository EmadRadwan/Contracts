namespace Domain;

public class FixedAssetProductType
{
    public FixedAssetProductType()
    {
        FixedAssetProducts = new HashSet<FixedAssetProduct>();
    }

    public string FixedAssetProductTypeId { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ICollection<FixedAssetProduct> FixedAssetProducts { get; set; }
}