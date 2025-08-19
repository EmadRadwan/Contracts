using FluentValidation;

namespace Application.Accounting.Payments;


public class UpdatePaymentStatusValidator : AbstractValidator<PaymentChangeStatusDto>
{
    public UpdatePaymentStatusValidator()
    {
        RuleFor(x => x.PaymentId).NotEmpty();
        RuleFor(x => x.StatusId).NotEmpty();
    }
}