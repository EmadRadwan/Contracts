using FluentValidation;

namespace Application.Order.Quotes;

public class QuoteItemValidator : AbstractValidator<QuoteItemsDto>
{
    public QuoteItemValidator()
    {
        RuleFor(x => x.QuoteId).NotEmpty();
    }
}