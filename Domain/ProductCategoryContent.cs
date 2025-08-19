namespace Domain;

public class ProductCategoryContent
{
    public string ProductCategoryId { get; set; } = null!;
    public string ContentId { get; set; } = null!;
    public string ProdCatContentTypeId { get; set; } = null!;
    public DateTime FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public DateTime? PurchaseFromDate { get; set; }
    public DateTime? PurchaseThruDate { get; set; }
    public int? UseCountLimit { get; set; }
    public decimal? UseDaysLimit { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Content Content { get; set; } = null!;
    public ProductCategoryContentType ProdCatContentType { get; set; } = null!;
    public ProductCategory ProductCategory { get; set; } = null!;
}