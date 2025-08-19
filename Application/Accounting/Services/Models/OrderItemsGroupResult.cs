using Domain;

namespace Application.Accounting.Services.Models;

public class OrderItemsGroupResult
{
    public OrderItemGroup OrderItemGroup { get; set; }
    public List<OrderItem> BillableItems { get; set; } = new List<OrderItem>();
}