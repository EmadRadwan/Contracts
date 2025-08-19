namespace Domain;

public class OrderTerm
{
    public OrderTerm()
    {
        OrderTermAttributes = new HashSet<OrderTermAttribute>();
    }

    public string TermTypeId { get; set; } = null!;
    public string OrderId { get; set; } = null!;
    public string OrderItemSeqId { get; set; } = null!;
    public decimal? TermValue { get; set; }
    public int? TermDays { get; set; }
    public string? TextValue { get; set; }
    public string? Description { get; set; }
    public string? DescriptionArabic { get; set; }
    public string? DescriptionTurkish { get; set; }
    public string? UomId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public OrderHeader Order { get; set; } = null!;
    public TermType TermType { get; set; } = null!;
    public Uom? Uom { get; set; }
    public ICollection<OrderTermAttribute> OrderTermAttributes { get; set; }
}