namespace Application.Order.Orders;

public class SetItemStatusRequest
{
    public string OrderId { get; set; }
    public string OrderItemSeqId { get; set; }
    public string FromStatusId { get; set; }
    public string StatusId { get; set; }
    public DateTime? StatusDateTime { get; set; }
    public string ChangeReason { get; set; }
    public string UserLoginId { get; set; }
}