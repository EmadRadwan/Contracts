using FluentValidation;

namespace Application.Facilities.InventoryTransfer;

public class InventoryTransferValidator : AbstractValidator<InventoryTransferDto>
{
    public InventoryTransferValidator()
    {
        RuleFor(x => x.InventoryItemId).NotEmpty();
        RuleFor(x => x.StatusId).NotEmpty();
    }
}