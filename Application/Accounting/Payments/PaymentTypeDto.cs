namespace Application.Accounting.Payments;


public class PaymentTypeDto
{
    public string PaymentTypeId { get; set; } = null!;
    public string? ParentTypeId { get; set; }
    public string? Description { get; set; }
}