using FluentValidation;

namespace Application.Services;

public class VehicleValidator : AbstractValidator<VehicleDto>
{
    public VehicleValidator()
    {
        RuleFor(x => x.ChassisNumber).NotEmpty();
        RuleFor(x => x.PlateNumber).NotEmpty();
    }
}