using Application.Parties.Parties;
using FluentValidation;

namespace Application.Parties;

public class PartyValidator : AbstractValidator<PartyDto>
{
    public PartyValidator()
    {
        RuleFor(x => x.MobileContactNumber).NotEmpty();
        RuleFor(x => x.MobileContactNumber).NotEmpty();
    }
}