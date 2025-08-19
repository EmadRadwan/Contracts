namespace Domain;

public class ProductPromoProduct
{
    public string ProductPromoId { get; set; } = null!;
    public string ProductPromoRuleId { get; set; } = null!;
    public string ProductPromoActionSeqId { get; set; } = null!;
    public string ProductPromoCondSeqId { get; set; } = null!;
    public string ProductId { get; set; } = null!;
    public string? ProductPromoApplEnumId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Product Product { get; set; } = null!;
    public ProductPromo ProductPromo { get; set; } = null!;
    public Enumeration? ProductPromoApplEnum { get; set; }
}