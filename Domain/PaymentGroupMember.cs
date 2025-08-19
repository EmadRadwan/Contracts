namespace Domain;

public class PaymentGroupMember
{
    public string PaymentGroupId { get; set; } = null!;
    public string PaymentId { get; set; } = null!;
    public DateTime FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public int? SequenceNum { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Payment Payment { get; set; } = null!;
    public PaymentGroup PaymentGroup { get; set; } = null!;
}