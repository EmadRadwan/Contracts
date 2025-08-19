using FluentValidation;

namespace Application.Services;

public class ServiceSpecificationValidator : AbstractValidator<ServiceSpecificationDto>
{
    public ServiceSpecificationValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.FromDate).NotEmpty();
    }
}