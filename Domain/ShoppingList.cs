namespace Domain;

public class ShoppingList
{
    public ShoppingList()
    {
        InverseParentShoppingList = new HashSet<ShoppingList>();
        OrderHeaders = new HashSet<OrderHeader>();
        ShoppingListItemSurveys = new HashSet<ShoppingListItemSurvey>();
        ShoppingListItems = new HashSet<ShoppingListItem>();
        ShoppingListWorkEfforts = new HashSet<ShoppingListWorkEffort>();
    }

    public string ShoppingListId { get; set; } = null!;
    public string? ShoppingListTypeId { get; set; }
    public string? ParentShoppingListId { get; set; }
    public string? ProductStoreId { get; set; }
    public string? VisitorId { get; set; }
    public string? PartyId { get; set; }
    public string? ListName { get; set; }
    public string? Description { get; set; }
    public string? IsPublic { get; set; }
    public string? IsActive { get; set; }
    public string? CurrencyUom { get; set; }
    public string? ShipmentMethodTypeId { get; set; }
    public string? CarrierPartyId { get; set; }
    public string? CarrierRoleTypeId { get; set; }
    public string? ContactMechId { get; set; }
    public string? PaymentMethodId { get; set; }
    public string? RecurrenceInfoId { get; set; }
    public DateTime? LastOrderedDate { get; set; }
    public DateTime? LastAdminModified { get; set; }
    public string? ProductPromoCodeId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public CarrierShipmentMethod? CarrierShipmentMethod { get; set; }
    public ContactMech? ContactMech { get; set; }
    public ShoppingList? ParentShoppingList { get; set; }
    public Party? Party { get; set; }
    public PaymentMethod? PaymentMethod { get; set; }
    public ProductPromoCode? ProductPromoCode { get; set; }
    public ProductStore? ProductStore { get; set; }
    public RecurrenceInfo? RecurrenceInfo { get; set; }
    public ShoppingListType? ShoppingListType { get; set; }
    public ICollection<ShoppingList> InverseParentShoppingList { get; set; }
    public ICollection<OrderHeader> OrderHeaders { get; set; }
    public ICollection<ShoppingListItemSurvey> ShoppingListItemSurveys { get; set; }
    public ICollection<ShoppingListItem> ShoppingListItems { get; set; }
    public ICollection<ShoppingListWorkEffort> ShoppingListWorkEfforts { get; set; }
}