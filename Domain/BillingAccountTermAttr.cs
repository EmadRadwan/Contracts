namespace Domain;

public class BillingAccountTermAttr
{
    public string BillingAccountTermId { get; set; } = null!;
    public string AttrName { get; set; } = null!;
    public string? AttrValue { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public BillingAccountTerm BillingAccountTerm { get; set; } = null!;
}