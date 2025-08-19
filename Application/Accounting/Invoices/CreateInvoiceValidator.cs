using FluentValidation;

namespace Application.Shipments.Invoices;

public class CreateInvoiceValidator : AbstractValidator<InvoiceDto3>
{
    public CreateInvoiceValidator()
    {
        RuleFor(x => x.InvoiceTypeId).NotEmpty();
    }
}