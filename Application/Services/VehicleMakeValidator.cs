using FluentValidation;

namespace Application.Services;

public class VehicleMakeValidator : AbstractValidator<VehicleMakeDto>
{
    public VehicleMakeValidator()
    {
        RuleFor(x => x.MakeId).NotEmpty();
        RuleFor(x => x.MakeDescription).NotEmpty();
    }
}