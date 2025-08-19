namespace Domain;

public class OrderItemContactMech
{
    public string OrderId { get; set; } = null!;
    public string OrderItemSeqId { get; set; } = null!;
    public string ContactMechPurposeTypeId { get; set; } = null!;
    public string? ContactMechId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ContactMech? ContactMech { get; set; }
    public ContactMechPurposeType ContactMechPurposeType { get; set; } = null!;
    public OrderItem OrderI { get; set; } = null!;
}