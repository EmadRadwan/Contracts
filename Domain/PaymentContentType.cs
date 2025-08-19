namespace Domain;

public class PaymentContentType
{
    public PaymentContentType()
    {
        InverseParentType = new HashSet<PaymentContentType>();
        PaymentContents = new HashSet<PaymentContent>();
    }

    public string PaymentContentTypeId { get; set; } = null!;
    public string? ParentTypeId { get; set; }
    public string? HasTable { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public PaymentContentType? ParentType { get; set; }
    public ICollection<PaymentContentType> InverseParentType { get; set; }
    public ICollection<PaymentContent> PaymentContents { get; set; }
}