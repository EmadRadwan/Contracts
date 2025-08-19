namespace Application.Accounting.BillingAccounts;

public class BillingAccountInvoiceDto
{
    public string BillingAccountId { get; set; }
    public string InvoiceId { get; set; }
    public string InvoiceTypeId { get; set; }
    public string InvoiceTypeDescription { get; set; }
    public DateTime InvoiceDate { get; set; }
    public string StatusId { get; set; }
    public string StatusDescription { get; set; }
    public string Description { get; set; }

    // Party Descriptions
    public string DescriptionFrom { get; set; }
    public string DescriptionTo { get; set; }

    // Computed Fields
    public bool PaidInvoice { get; set; }
    public decimal AmountToApply { get; set; }
    public decimal Total { get; set; }
}
