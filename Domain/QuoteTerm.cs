namespace Domain;

public class QuoteTerm
{
    public QuoteTerm()
    {
        QuoteTermAttributes = new HashSet<QuoteTermAttribute>();
    }

    public string TermTypeId { get; set; } = null!;
    public string QuoteId { get; set; } = null!;
    public string QuoteItemSeqId { get; set; } = null!;
    public int? TermValue { get; set; }
    public string? UomId { get; set; }
    public int? TermDays { get; set; }
    public string? TextValue { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Quote Quote { get; set; } = null!;
    public TermType TermType { get; set; } = null!;
    public ICollection<QuoteTermAttribute> QuoteTermAttributes { get; set; }
}