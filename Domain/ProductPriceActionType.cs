namespace Domain;

public class ProductPriceActionType
{
    public ProductPriceActionType()
    {
        ProductPriceActions = new HashSet<ProductPriceAction>();
    }

    public string ProductPriceActionTypeId { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ICollection<ProductPriceAction> ProductPriceActions { get; set; }
}