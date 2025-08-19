namespace Domain;

public class QuoteItem
{
    public QuoteItem()
    {
        OrderItems = new HashSet<OrderItem>();
    }

    public string QuoteId { get; set; } = null!;
    public string QuoteItemSeqId { get; set; } = null!;
    public string? ProductId { get; set; }
    public string? ProductFeatureId { get; set; }
    public string? DeliverableTypeId { get; set; }
    public string? SkillTypeId { get; set; }
    public string? UomId { get; set; }
    public string? WorkEffortId { get; set; }
    public string? CustRequestId { get; set; }
    public string? CustRequestItemSeqId { get; set; }
    public string? ParentQuoteItemSeqId { get; set; }

    public decimal? Quantity { get; set; }
    public decimal? SelectedAmount { get; set; }
    public decimal? QuoteUnitPrice { get; set; }
    public decimal? QuoteUnitListPrice { get; set; }

    public DateTime? ReservStart { get; set; }
    public decimal? ReservLength { get; set; }
    public decimal? ReservPersons { get; set; }
    public string? ConfigId { get; set; }
    public DateTime? EstimatedDeliveryDate { get; set; }
    public string? Comments { get; set; }
    public string? IsPromo { get; set; }
    public int? LeadTimeDays { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public CustRequest? CustRequest { get; set; }
    public CustRequestItem? CustRequestI { get; set; }
    public DeliverableType? DeliverableType { get; set; }
    public Product? Product { get; set; }
    public ProductFeature? ProductFeature { get; set; }
    public Quote Quote { get; set; } = null!;
    public SkillType? SkillType { get; set; }
    public Uom? Uom { get; set; }
    public WorkEffort? WorkEffort { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; }
}