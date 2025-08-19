using Application.Order.Orders;

namespace Application.Accounting.Taxes;

public class CalculateTaxRequestDto
{
    public List<OrderItemDto2> OrderItems { get; set; }
    public List<OrderAdjustmentDto2> OrderAdjustments { get; set; }
}