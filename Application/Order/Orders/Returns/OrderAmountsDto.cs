namespace Application.Order.Orders.Returns;

public class OrderAmountsDto
{
    public string OrderId { get; set; }
    public decimal OrderTotal { get; set; }
    public decimal AmountAlreadyCredited { get; set; }
    public decimal AmountAlreadyRefunded { get; set; }
}
