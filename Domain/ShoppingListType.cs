namespace Domain;

public class ShoppingListType
{
    public ShoppingListType()
    {
        ShoppingLists = new HashSet<ShoppingList>();
    }

    public string ShoppingListTypeId { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ICollection<ShoppingList> ShoppingLists { get; set; }
}