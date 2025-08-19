namespace Domain;

public class ProductStoreGroupRollup
{
    public string ProductStoreGroupId { get; set; } = null!;
    public string ParentGroupId { get; set; } = null!;
    public DateTime FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public int? SequenceNum { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ProductStoreGroup ParentGroup { get; set; } = null!;
    public ProductStoreGroup ProductStoreGroup { get; set; } = null!;
}