namespace Domain;

public class OrderTermAttribute
{
    public string TermTypeId { get; set; } = null!;
    public string OrderId { get; set; } = null!;
    public string OrderItemSeqId { get; set; } = null!;
    public string AttrName { get; set; } = null!;
    public string? AttrValue { get; set; }
    public string? AttrDescription { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public OrderTerm OrderTerm { get; set; } = null!;
}