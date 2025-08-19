namespace Domain;

public class QuoteTermAttribute
{
    public string TermTypeId { get; set; } = null!;
    public string QuoteId { get; set; } = null!;
    public string QuoteItemSeqId { get; set; } = null!;
    public string AttrName { get; set; } = null!;
    public string? AttrValue { get; set; }
    public string? AttrDescription { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public QuoteTerm QuoteTerm { get; set; } = null!;
}