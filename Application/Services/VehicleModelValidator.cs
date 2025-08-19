using FluentValidation;

namespace Application.Services;

public class VehicleModelValidator : AbstractValidator<VehicleModelDto>
{
    public VehicleModelValidator()
    {
        RuleFor(x => x.ModelId).NotEmpty();
        RuleFor(x => x.ModelDescription).NotEmpty();
    }
}