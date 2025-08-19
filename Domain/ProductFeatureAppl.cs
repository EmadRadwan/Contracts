namespace Domain;

public class ProductFeatureAppl
{
    public ProductFeatureAppl()
    {
        ProductFeatureApplAttrs = new HashSet<ProductFeatureApplAttr>();
    }

    public string ProductId { get; set; } = null!;
    public string ProductFeatureId { get; set; } = null!;
    public string? ProductFeatureApplTypeId { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public int? SequenceNum { get; set; }
    public decimal? Amount { get; set; }
    public decimal? RecurringAmount { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Product Product { get; set; } = null!;
    public ProductFeature ProductFeature { get; set; } = null!;
    public ProductFeatureApplType? ProductFeatureApplType { get; set; }
    public ICollection<ProductFeatureApplAttr> ProductFeatureApplAttrs { get; set; }
}