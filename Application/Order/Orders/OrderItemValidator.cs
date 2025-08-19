using FluentValidation;

namespace Application.Order.Orders;

public class OrderItemValidator : AbstractValidator<OrderItemsDto>
{
    public OrderItemValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
    }
}