namespace Domain;

public class OrderContentType
{
    public OrderContentType()
    {
        InverseParentType = new HashSet<OrderContentType>();
        OrderContents = new HashSet<OrderContent>();
    }

    public string OrderContentTypeId { get; set; } = null!;
    public string? ParentTypeId { get; set; }
    public string? HasTable { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public OrderContentType? ParentType { get; set; }
    public ICollection<OrderContentType> InverseParentType { get; set; }
    public ICollection<OrderContent> OrderContents { get; set; }
}