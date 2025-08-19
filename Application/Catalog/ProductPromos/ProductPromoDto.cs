namespace Application.Catalog.ProductPromos;

public class ProductPromoDto
{
    public string ProductPromoId { get; set; } = null!;
    public string? PromoName { get; set; }
    public string? PromoText { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public string ProductPromoRuleId { get; set; } = null!;
    public string ProductPromoCondSeqId { get; set; } = null!;
    public string? InputParamEnumId { get; set; }
    public string? InputParamEnumDescription { get; set; }
    public string? OperatorEnumId { get; set; }
    public string? OperatorEnumDescription { get; set; }
    public string? CondValue { get; set; }
    public string ProductPromoActionSeqId { get; set; } = null!;
    public string? ProductPromoActionEnumId { get; set; }
    public string? ProductPromoActionEnumDescription { get; set; }
    public decimal? Quantity { get; set; }
    public decimal? Amount { get; set; }
    public string ProductId { get; set; } = null!;
    public string ProductCategoryId { get; set; } = null!;
    public string? IncludeSubCategories { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public bool? IsUsed { get; set; }
}