namespace Application.Order.Orders.Returns;

public class ReturnableItemsResult
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public List<ReturnableItemInfo> ReturnableItems { get; set; }
}