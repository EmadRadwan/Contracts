namespace Domain;

public class ProductFeatureDataResource
{
    public string DataResourceId { get; set; } = null!;
    public string ProductFeatureId { get; set; } = null!;
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public DataResource DataResource { get; set; } = null!;
    public ProductFeature ProductFeature { get; set; } = null!;
}