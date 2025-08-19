namespace Domain;

public class ProductCategoryRollup
{
    public string ProductCategoryId { get; set; } = null!;
    public string ParentProductCategoryId { get; set; } = null!;
    public DateTime FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public int? SequenceNum { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ProductCategory ParentProductCategory { get; set; } = null!;
    public ProductCategory ProductCategory { get; set; } = null!;
}