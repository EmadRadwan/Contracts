namespace Domain;

public class ProductFeatureGroupAppl
{
    public string ProductFeatureGroupId { get; set; } = null!;
    public string ProductFeatureId { get; set; } = null!;
    public DateTime FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public int? SequenceNum { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ProductFeature ProductFeature { get; set; } = null!;
    public ProductFeatureGroup ProductFeatureGroup { get; set; } = null!;
}