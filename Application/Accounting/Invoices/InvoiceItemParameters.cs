namespace Application.Shipments.Invoices;

public class InvoiceItemParameters
{
    public string InvoiceId { get; set; }
    public string InvoiceItemTypeId { get; set; }
    public string? InvoiceItemSeqId { get; set; }
    public string? Description { get; set; }
    public decimal? Amount { get; set; }
    public string? ProductId { get; set; }
    public decimal? Quantity { get; set; }
    public string? TaxAuthPartyId { get; set; }
    public string? TaxAuthGeoId { get; set; }
    public string? TaxAuthorityRateSeqId { get; set; }
    public string? OverrideGlAccountId { get; set; }
}
