namespace Domain;

public class ShoppingListItem
{
    public ShoppingListItem()
    {
        ShoppingListItemSurveys = new HashSet<ShoppingListItemSurvey>();
    }

    public string ShoppingListId { get; set; } = null!;
    public string ShoppingListItemSeqId { get; set; } = null!;
    public string? ProductId { get; set; }
    public decimal? Quantity { get; set; }
    public decimal? ModifiedPrice { get; set; }
    public DateTime? ReservStart { get; set; }
    public decimal? ReservLength { get; set; }
    public decimal? ReservPersons { get; set; }
    public decimal? QuantityPurchased { get; set; }
    public string? ConfigId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Product? Product { get; set; }
    public ShoppingList ShoppingList { get; set; } = null!;
    public ICollection<ShoppingListItemSurvey> ShoppingListItemSurveys { get; set; }
}