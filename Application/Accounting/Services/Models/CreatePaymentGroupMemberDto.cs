namespace Application.Accounting.Services.Models;

public class CreatePaymentGroupMemberDto
{
    public string PaymentGroupId { get; set; }
    public string PaymentId { get; set; }
    public DateTime? FromDate { get; set; }
}
