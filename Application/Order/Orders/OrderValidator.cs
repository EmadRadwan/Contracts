using FluentValidation;

namespace Application.Order.Orders;

public class OrderValidator : AbstractValidator<OrderDto>
{
    public OrderValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.FromPartyId).NotEmpty();
    }
}