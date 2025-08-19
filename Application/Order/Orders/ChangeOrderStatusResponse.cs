namespace Application.Order;

public class ChangeOrderStatusResponse
{
    public string OldStatusId { get; set; }
    public string OrderTypeId { get; set; }
    public string NeedsInventoryIssuance { get; set; }
    public decimal GrandTotal { get; set; }
    public string OrderStatusId { get; set; }
}