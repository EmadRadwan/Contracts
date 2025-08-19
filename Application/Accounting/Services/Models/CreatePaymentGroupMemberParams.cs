namespace Application.Accounting.Services.Models;

public class CreatePaymentGroupMemberParams
{
    public string PaymentGroupId { get; set; }
    public string PaymentId { get; set; }
    public DateTime FromDate { get; set; }
}