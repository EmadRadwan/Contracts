namespace Domain;

public class DesiredFeature
{
    public string DesiredFeatureId { get; set; } = null!;
    public string RequirementId { get; set; } = null!;
    public string? ProductFeatureId { get; set; }
    public string? OptionalInd { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ProductFeature? ProductFeature { get; set; }
    public Requirement Requirement { get; set; } = null!;
}