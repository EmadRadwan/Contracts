namespace Application.Accounting.Services.Models;

public class CreatePaymentGroupParams
{
    public string PaymentGroupTypeId { get; set; }
    public DateTime FromDate { get; set; }
    public string PaymentGroupName { get; set; }
}