using Domain;

namespace Application.Accounting.Services.Models;

public class BillableItem
{
    public OrderItem OrderItem { get; set; }
    public decimal AvailableQuantity { get; set; }

    public BillableItem(OrderItem orderItem, decimal availableQuantity)
    {
        OrderItem = orderItem;
        AvailableQuantity = availableQuantity;
    }
}
