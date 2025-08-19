using Domain;
using FluentValidation;

namespace Application.Catalog.ProductCategories;

public class ProductCategoryValidator : AbstractValidator<ProductCategoryMember>
{
    public ProductCategoryValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.ProductCategoryId).NotEmpty();
        RuleFor(x => x.FromDate).NotEmpty();
    }
}