using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class InvoiceItem
{
    public InvoiceItem()
    {
        InverseParentInvoiceI = new HashSet<InvoiceItem>();
        InvoiceItemAssocInvoiceINavigations = new HashSet<InvoiceItemAssoc>();
        InvoiceItemAssocInvoiceIs = new HashSet<InvoiceItemAssoc>();
        InvoiceItemAttributes = new HashSet<InvoiceItemAttribute>();
        OrderAdjustmentBillings = new HashSet<OrderAdjustmentBilling>();
        OrderItemBillings = new HashSet<OrderItemBilling>();
        ReturnItemBillings = new HashSet<ReturnItemBilling>();
        ShipmentItemBillings = new HashSet<ShipmentItemBilling>();
        TimeEntries = new HashSet<TimeEntry>();
        WorkEffortBillings = new HashSet<WorkEffortBilling>();
    }

    public string InvoiceId { get; set; } = null!;
    public string InvoiceItemSeqId { get; set; } = null!;
    public string? InvoiceItemTypeId { get; set; }
    public string? OverrideGlAccountId { get; set; }
    public string? OverrideOrgPartyId { get; set; }
    public string? InventoryItemId { get; set; }
    public string? ProductId { get; set; }
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
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public InventoryItem? InventoryItem { get; set; }
    public Invoice Invoice { get; set; } = null!;
    public InvoiceItemType? InvoiceItemType { get; set; }
    public GlAccount? OverrideGlAccount { get; set; }
    public Party? OverrideOrgParty { get; set; }
    public InvoiceItem? ParentInvoiceI { get; set; }
    public Product? Product { get; set; }
    public ProductFeature? ProductFeature { get; set; }
    public SalesOpportunity? SalesOpportunity { get; set; }
    public Geo? TaxAuthGeo { get; set; }
    public Party? TaxAuthParty { get; set; }
    public TaxAuthorityRateProduct? TaxAuthorityRateSeq { get; set; }
    public Uom? Uom { get; set; }
    public ICollection<InvoiceItem> InverseParentInvoiceI { get; set; }
    public ICollection<InvoiceItemAssoc> InvoiceItemAssocInvoiceINavigations { get; set; }
    public ICollection<InvoiceItemAssoc> InvoiceItemAssocInvoiceIs { get; set; }
    public ICollection<InvoiceItemAttribute> InvoiceItemAttributes { get; set; }
    public ICollection<OrderAdjustmentBilling> OrderAdjustmentBillings { get; set; }
    public ICollection<OrderItemBilling> OrderItemBillings { get; set; }
    public ICollection<ReturnItemBilling> ReturnItemBillings { get; set; }
    public ICollection<ShipmentItemBilling> ShipmentItemBillings { get; set; }
    public ICollection<TimeEntry> TimeEntries { get; set; }
    public ICollection<WorkEffortBilling> WorkEffortBillings { get; set; }
}