using FluentValidation;

namespace Application.Services;

public class ServiceRateValidator : AbstractValidator<ServiceRateDto>
{
    public ServiceRateValidator()
    {
        RuleFor(x => x.MakeId).NotEmpty();
        RuleFor(x => x.FromDate).NotEmpty();
    }
}