using Domain;
using FluentValidation;

namespace Application.Catalog.ProductPrices;

public class ProductPriceValidator : AbstractValidator<ProductPrice>
{
    public ProductPriceValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.ProductPriceTypeId).NotEmpty();
        RuleFor(x => x.CurrencyUomId).NotEmpty();
        RuleFor(x => x.Price).NotEmpty();
        RuleFor(x => x.FromDate).NotEmpty();
    }
}