using FluentValidation;

namespace Application.Accounting.Payments;


public class PaymentValidator : AbstractValidator<PaymentsDto>
{
    public PaymentValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
    }
}