namespace API.Controllers.Accounting;


public class PaymentTotalsDto
{
    public string PaymentId { get; set; }
    public decimal AmountToApply { get; set; }
}