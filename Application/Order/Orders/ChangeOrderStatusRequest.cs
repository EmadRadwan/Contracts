namespace Application.Order.Orders;

public class ChangeOrderStatusRequest
{
    public string OrderId { get; set; }
    public string StatusId { get; set; }
    public bool SetItemStatus { get; set; }
    public string ChangeReason { get; set; }
    public string UserLoginId { get; set; }
}