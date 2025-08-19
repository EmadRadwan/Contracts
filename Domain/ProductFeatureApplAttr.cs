namespace Domain;

public class ProductFeatureApplAttr
{
    public string ProductId { get; set; } = null!;
    public string ProductFeatureId { get; set; } = null!;
    public DateTime FromDate { get; set; }
    public string AttrName { get; set; } = null!;
    public string? AttrValue { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Product Product { get; set; } = null!;
    public ProductFeature ProductFeature { get; set; } = null!;
    public ProductFeatureAppl ProductFeatureAppl { get; set; } = null!;
}