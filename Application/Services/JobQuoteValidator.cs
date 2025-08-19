using FluentValidation;

namespace Application.Services;

public class JobQuoteValidator : AbstractValidator<JobQuoteDto>
{
    public JobQuoteValidator()
    {
        RuleFor(x => x.QuoteId).NotEmpty();
    }
}