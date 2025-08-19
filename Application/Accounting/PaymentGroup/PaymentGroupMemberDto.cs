namespace Application.Accounting.PaymentGroup;

public class PaymentGroupMemberDto
{
    public string PaymentGroupId { get; set; }
    public string PaymentId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
}