using FluentValidation;

namespace Application.Order.Quotes;

public class QuoteValidator : AbstractValidator<QuoteDto>
{
    public QuoteValidator()
    {
        RuleFor(x => x.QuoteId).NotEmpty();
        RuleFor(x => x.FromPartyId).NotEmpty();
    }
}