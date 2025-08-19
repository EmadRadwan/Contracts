namespace Domain;

public class ProductKeywordNew
{
    public string ProductId { get; set; } = null!;
    public string Keyword { get; set; } = null!;
    public string KeywordTypeId { get; set; } = null!;
    public int? RelevancyWeight { get; set; }
    public string? StatusId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Enumeration KeywordType { get; set; } = null!;
    public Product Product { get; set; } = null!;
    public StatusItem? Status { get; set; }
}