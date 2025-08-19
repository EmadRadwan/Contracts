using FluentValidation;

namespace Application.Order.CustomerRequests;

public class CustRequestValidator : AbstractValidator<CustRequestDto>
{
    public CustRequestValidator()
    {
        RuleFor(x => x.CustRequestId).NotEmpty();
        RuleFor(x => x.FromPartyId).NotEmpty();
    }
}