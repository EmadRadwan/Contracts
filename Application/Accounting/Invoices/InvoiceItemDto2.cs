using Application.Catalog.Products;
namespace Application.Shipments.Invoices;

public class InvoiceItemDto2
{
    public string InvoiceId { get; set; } = null!;
    public ProductLovDto? InvoiceItemProduct { get; set; }
    public string InvoiceItemSeqId { get; set; } = null!;
    public string? InvoiceItemTypeId { get; set; }
    public string? InvoiceItemTypeDescription { get; set; }
    public string? OverrideGlAccountId { get; set; }
    public string? OverrideOrgPartyId { get; set; }
    public string? InventoryItemId { get; set; }
    public string? ProductId { get; set; }
    public string? ProductName { get; set; }
    public string? ProductFeatureId { get; set; }
    public string? ParentInvoiceId { get; set; }
    public string? ParentInvoiceItemSeqId { get; set; }
    public string? UomId { get; set; }
    public string? TaxableFlag { get; set; }
    public decimal? Quantity { get; set; }
    public decimal? Amount { get; set; }
    public string? Description { get; set; }
    public string? TaxAuthPartyId { get; set; }
    public string? TaxAuthGeoId { get; set; }
    public string? TaxAuthorityRateSeqId { get; set; }
    public string? SalesOpportunityId { get; set; }
}