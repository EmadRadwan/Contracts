using Application.Order.Orders;
using FluentValidation;

namespace Application.Facilities;

public class ReceiveInventoryProductValidator : AbstractValidator<OrderItemDto2>
{
    public ReceiveInventoryProductValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
    }
}