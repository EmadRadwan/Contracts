namespace Application.Order.Orders;

public class ItemReturnedQuantity
{
    // Represents the order item sequence identifier
    public string orderItemSeqId { get; set; }
    // Represents the accumulated returned quantity for that order item
    public decimal returnedQuantity { get; set; }
}