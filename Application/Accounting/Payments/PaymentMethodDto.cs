namespace Application.Accounting.Payments;


public class PaymentMethodDto
{
    public string PaymentMethodId { get; set; } = null!;
    public string? PaymentMethodTypeId { get; set; }
    public string? PartyId { get; set; }
    public string? GlAccountId { get; set; }
    public string? FinAccountId { get; set; }
    public string? Description { get; set; }
}