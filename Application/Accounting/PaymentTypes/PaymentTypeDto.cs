namespace Application.Shipments.PaymentTypes;

public class PaymentTypeDto
{
    public string PaymentTypeId { get; set; } = null!;
    public string? ParentTypeId { get; set; }
    public string? HasTable { get; set; }
    public string? Description { get; set; }
}