using FluentValidation;

namespace Application.Catalog.Products;

public class ProductValidator : AbstractValidator<ProductDto>
{
    public ProductValidator()
    {
        RuleFor(x => x.ProductName).NotEmpty();
        RuleFor(x => x.ProductTypeId).NotEmpty();
    }
}