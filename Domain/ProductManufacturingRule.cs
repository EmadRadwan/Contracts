namespace Domain;

public class ProductManufacturingRule
{
    public string RuleId { get; set; } = null!;
    public string? ProductId { get; set; }
    public string? ProductIdFor { get; set; }
    public string? ProductIdIn { get; set; }
    public string? RuleSeqId { get; set; }
    public DateTime? FromDate { get; set; }
    public string? ProductIdInSubst { get; set; }
    public string? ProductFeature { get; set; }
    public string? RuleOperator { get; set; }
    public double? Quantity { get; set; }
    public string? Description { get; set; }
    public DateTime? ThruDate { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Product? Product { get; set; }
    public ProductFeature? ProductFeatureNavigation { get; set; }
    public Product? ProductIdForNavigation { get; set; }
    public Product? ProductIdInNavigation { get; set; }
    public Product? ProductIdInSubstNavigation { get; set; }
}