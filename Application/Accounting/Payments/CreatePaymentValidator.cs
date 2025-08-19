using FluentValidation;

namespace Application.Accounting.Payments;


public class CreatePaymentValidator : AbstractValidator<PaymentDto>
{
    public CreatePaymentValidator()
    {
        RuleFor(x => x.PaymentTypeId).NotEmpty();
        RuleFor(x => x.PaymentMethodId).NotEmpty();
    }
}