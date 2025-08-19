using Application.Shipments.Accounting;
using Application.Shipments.InvoiceItemTypes;

namespace Application.Accounting.OrganizationGlSettings;

public class InvoiceItemTypeAndGlAccountsDto
{
    public List<InvoiceItemTypeDto> InvoiceItemTypes { get; set; } = new();
    public List<GlAccountOrganizationAndClassDto> GlAccounts { get; set; } = new();
}