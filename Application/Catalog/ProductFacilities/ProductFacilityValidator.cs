using Application.ProductFacilities;
using Domain;
using FluentValidation;

namespace Application.Catalog.ProductFacilities;

public class ProductFacilityValidator : AbstractValidator<ProductFacilityDto>
{
    public ProductFacilityValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.FacilityId).NotEmpty();
    }
}