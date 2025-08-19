namespace Domain;

public class ProductAttribute
{
    public string ProductId { get; set; } = null!;
    public string AttrName { get; set; } = null!;
    public string? AttrValue { get; set; }
    public string? AttrType { get; set; }
    public string? AttrDescription { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Product Product { get; set; } = null!;
}