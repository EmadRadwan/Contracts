namespace Application.Order.Orders.Returns;

public class OrderHeaderItemAndRolesDto
{
    public string OrderId { get; set; }
    public DateTime OrderDate { get; set; }
    public string PartyId { get; set; }
    public string RoleTypeId { get; set; }
    public string OrderTypeId { get; set; }
    public string StatusId { get; set; }
    public string ProductId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string ItemDescription { get; set; }
    public string OrderItemSeqId { get; set; }
}
