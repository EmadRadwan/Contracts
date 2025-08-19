namespace Domain;

public class ProductFeatureIactn
{
    public string ProductFeatureId { get; set; } = null!;
    public string ProductFeatureIdTo { get; set; } = null!;
    public string? ProductFeatureIactnTypeId { get; set; }
    public string? ProductId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ProductFeature ProductFeature { get; set; } = null!;
    public ProductFeatureIactnType? ProductFeatureIactnType { get; set; }
    public ProductFeature ProductFeatureIdToNavigation { get; set; } = null!;
}