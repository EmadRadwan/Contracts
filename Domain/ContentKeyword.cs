namespace Domain;

public class ContentKeyword
{
    public string ContentId { get; set; } = null!;
    public string Keyword { get; set; } = null!;
    public int? RelevancyWeight { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Content Content { get; set; } = null!;
}