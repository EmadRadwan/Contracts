namespace Domain;

public class ProductPromoContent
{
    public string ProductPromoId { get; set; } = null!;
    public string ContentId { get; set; } = null!;
    public string ProductPromoContentTypeId { get; set; } = null!;
    public DateTime FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Content Content { get; set; } = null!;
    public ProductPromo ProductPromo { get; set; } = null!;
    public ProductContentType ProductPromoContentType { get; set; } = null!;
}