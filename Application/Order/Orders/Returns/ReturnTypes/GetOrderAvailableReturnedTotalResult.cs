namespace Application.Order.Orders.Returns.ReturnTypes;

public class GetOrderAvailableReturnedTotalResult
{
    public decimal AvailableReturnTotal { get; set; }
    public decimal OrderTotal { get; set; }
    public decimal ReturnTotal { get; set; }
}