namespace Domain;

public class Subscription
{
    public Subscription()
    {
        SubscriptionAttributes = new HashSet<SubscriptionAttribute>();
        SubscriptionCommEvents = new HashSet<SubscriptionCommEvent>();
        SubscriptionFulfillmentPieces = new HashSet<SubscriptionFulfillmentPiece>();
    }

    public string SubscriptionId { get; set; } = null!;
    public string? Description { get; set; }
    public string? SubscriptionResourceId { get; set; }
    public string? CommunicationEventId { get; set; }
    public string? ContactMechId { get; set; }
    public string? OriginatedFromPartyId { get; set; }
    public string? OriginatedFromRoleTypeId { get; set; }
    public string? PartyId { get; set; }
    public string? RoleTypeId { get; set; }
    public string? PartyNeedId { get; set; }
    public string? NeedTypeId { get; set; }
    public string? OrderId { get; set; }
    public string? OrderItemSeqId { get; set; }
    public string? ProductId { get; set; }
    public string? ProductCategoryId { get; set; }
    public string? InventoryItemId { get; set; }
    public string? SubscriptionTypeId { get; set; }
    public string? ExternalSubscriptionId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public DateTime? PurchaseFromDate { get; set; }
    public DateTime? PurchaseThruDate { get; set; }
    public int? MaxLifeTime { get; set; }
    public string? MaxLifeTimeUomId { get; set; }
    public int? AvailableTime { get; set; }
    public string? AvailableTimeUomId { get; set; }
    public int? UseCountLimit { get; set; }
    public int? UseTime { get; set; }
    public string? UseTimeUomId { get; set; }
    public string? AutomaticExtend { get; set; }
    public int? CanclAutmExtTime { get; set; }
    public string? CanclAutmExtTimeUomId { get; set; }
    public int? GracePeriodOnExpiry { get; set; }
    public string? GracePeriodOnExpiryUomId { get; set; }
    public DateTime? ExpirationCompletedDate { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Uom? AvailableTimeUom { get; set; }
    public Uom? CanclAutmExtTimeUom { get; set; }
    public ContactMech? ContactMech { get; set; }
    public Uom? GracePeriodOnExpiryUom { get; set; }
    public InventoryItem? InventoryItem { get; set; }
    public Uom? MaxLifeTimeUom { get; set; }
    public NeedType? NeedType { get; set; }
    public OrderItem? OrderI { get; set; }
    public Party? OriginatedFromParty { get; set; }
    public RoleType? OriginatedFromRoleType { get; set; }
    public Party? Party { get; set; }
    public Product? Product { get; set; }
    public ProductCategory? ProductCategory { get; set; }
    public RoleType? RoleType { get; set; }
    public SubscriptionResource? SubscriptionResource { get; set; }
    public SubscriptionType? SubscriptionType { get; set; }
    public Uom? UseTimeUom { get; set; }
    public ICollection<SubscriptionAttribute> SubscriptionAttributes { get; set; }
    public ICollection<SubscriptionCommEvent> SubscriptionCommEvents { get; set; }
    public ICollection<SubscriptionFulfillmentPiece> SubscriptionFulfillmentPieces { get; set; }
}