using Domain;
using FluentValidation;

namespace Application.Facilities.Facilities;

public class FacilityValidator : AbstractValidator<Facility>
{
    public FacilityValidator()
    {
        RuleFor(x => x.FacilityName).NotEmpty();
        RuleFor(x => x.FacilityTypeId).NotEmpty();
    }
}