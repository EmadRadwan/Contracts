namespace Application.Order.Orders.Returns;

public class UpdateReturnHeaderDto
{
    public string ReturnId { get; set; }
    public string? StatusId { get; set; }
    public string? PaymentMethodId { get; set; }
    public string? OldStatusId { get; set; }
}