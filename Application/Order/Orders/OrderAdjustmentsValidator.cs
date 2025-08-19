using FluentValidation;

namespace Application.Order.Orders;

public class OrderAdjustmentsValidator : AbstractValidator<OrderAdjustmentsDto>
{
    public OrderAdjustmentsValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
    }
}