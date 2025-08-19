using Domain;
using FluentValidation;

namespace Application.Catalog.ProductSuppliers;

public class SupplierProductValidator : AbstractValidator<SupplierProduct>
{
    public SupplierProductValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.PartyId).NotEmpty();
        RuleFor(x => x.CurrencyUomId).NotEmpty();
        RuleFor(x => x.MinimumOrderQuantity).NotEmpty();
        RuleFor(x => x.AvailableFromDate).NotEmpty();
    }
}