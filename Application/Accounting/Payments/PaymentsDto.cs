namespace Application.Accounting.Payments;


public class PaymentsDto
{
    public string OrderId { get; set; }
    public string? InvoiceId { get; set; }
    public decimal GrandTotal { get; set; }
    public string StatusDescription { get; set; }
    public string ModificationType { get; set; }
    public ICollection<PaymentDto> Payments { get; set; }
}