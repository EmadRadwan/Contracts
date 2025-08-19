using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;
[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]

public class OrderItem
{
    public OrderItem()
    {
        AllocationPlanItems = new HashSet<AllocationPlanItem>();
        FinAccountTrans = new HashSet<FinAccountTran>();
        FixedAssets = new HashSet<FixedAsset>();
        GiftCardFulfillments = new HashSet<GiftCardFulfillment>();
        ItemIssuances = new HashSet<ItemIssuance>();
        OrderItemAttributes = new HashSet<OrderItemAttribute>();
        OrderItemBillings = new HashSet<OrderItemBilling>();
        OrderItemChanges = new HashSet<OrderItemChange>();
        OrderItemContactMeches = new HashSet<OrderItemContactMech>();
        OrderItemGroupOrders = new HashSet<OrderItemGroupOrder>();
        OrderItemPriceInfos = new HashSet<OrderItemPriceInfo>();
        OrderItemRoles = new HashSet<OrderItemRole>();
        OrderItemShipGroupAssocs = new HashSet<OrderItemShipGroupAssoc>();
        OrderItemShipGrpInvRes = new HashSet<OrderItemShipGrpInvRes>();
        OrderRequirementCommitments = new HashSet<OrderRequirementCommitment>();
        PicklistItems = new HashSet<PicklistItem>();
        ProductOrderItemEngagementIs = new HashSet<ProductOrderItem>();
        ProductOrderItemOrderIs = new HashSet<ProductOrderItem>();
        ReturnItems = new HashSet<ReturnItem>();
        ShipmentReceipts = new HashSet<ShipmentReceipt>();
        Subscriptions = new HashSet<Subscription>();
        WorkOrderItemFulfillments = new HashSet<WorkOrderItemFulfillment>();
    }

    public string OrderId { get; set; } = null!;
    public string OrderItemSeqId { get; set; } = null!;
    public string? ParentOrderItemSeqId { get; set; }
    public string? ExternalId { get; set; }
    public string? OrderItemTypeId { get; set; }
    public string? OrderItemGroupSeqId { get; set; }
    public string? IsItemGroupPrimary { get; set; }
    public string? FromInventoryItemId { get; set; }
    public string? BudgetId { get; set; }
    public string? BudgetItemSeqId { get; set; }
    public string? ProductId { get; set; }
    public string? SupplierProductId { get; set; }
    public string? ProductFeatureId { get; set; }
    public string? ProdCatalogId { get; set; }
    public string? ProductCategoryId { get; set; }
    public string? IsPromo { get; set; }
    public string? QuoteId { get; set; }
    public string? QuoteItemSeqId { get; set; }
    public string? ShoppingListId { get; set; }
    public string? ShoppingListItemSeqId { get; set; }
    public string? SubscriptionId { get; set; }
    public string? DeploymentId { get; set; }
    public decimal? Quantity { get; set; }
    public decimal? CancelQuantity { get; set; }
    public decimal? SelectedAmount { get; set; }
    public decimal? UnitPrice { get; set; }
    public decimal? UnitListPrice { get; set; }
    public decimal? UnitAverageCost { get; set; }
    public decimal? UnitRecurringPrice { get; set; }
    public string? IsModifiedPrice { get; set; }
    public string? RecurringFreqUomId { get; set; }
    public string? ItemDescription { get; set; }
    public string? Comments { get; set; }
    public string? CorrespondingPoId { get; set; }
    public string? StatusId { get; set; }
    public string? SyncStatusId { get; set; }
    public DateTime? EstimatedShipDate { get; set; }
    public DateTime? EstimatedDeliveryDate { get; set; }
    public DateTime? AutoCancelDate { get; set; }
    public DateTime? DontCancelSetDate { get; set; }
    public string? DontCancelSetUserLogin { get; set; }
    public DateTime? ShipBeforeDate { get; set; }
    public DateTime? ShipAfterDate { get; set; }
    public DateTime? ReserveAfterDate { get; set; }
    public DateTime? CancelBackOrderDate { get; set; }
    public string? OverrideGlAccountId { get; set; }
    public string? SalesOpportunityId { get; set; }
    public string? ChangeByUserLoginId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public UserLogin? ChangeByUserLogin { get; set; }
    public UserLogin? DontCancelSetUserLoginNavigation { get; set; }
    public InventoryItem? FromInventoryItem { get; set; }
    public OrderHeader Order { get; set; } = null!;
    public OrderItemGroup? OrderI { get; set; }
    public OrderItemType? OrderItemType { get; set; }
    public GlAccount? OverrideGlAccount { get; set; }
    public Product? Product { get; set; }
    public QuoteItem? QuoteI { get; set; }
    public Uom? RecurringFreqUom { get; set; }
    public SalesOpportunity? SalesOpportunity { get; set; }
    public StatusItem? Status { get; set; }
    public StatusItem? SyncStatus { get; set; }
    public ICollection<AllocationPlanItem> AllocationPlanItems { get; set; }
    public ICollection<FinAccountTran> FinAccountTrans { get; set; }
    public ICollection<FixedAsset> FixedAssets { get; set; }
    public ICollection<GiftCardFulfillment> GiftCardFulfillments { get; set; }
    public ICollection<ItemIssuance> ItemIssuances { get; set; }
    public ICollection<OrderItemAttribute> OrderItemAttributes { get; set; }
    public ICollection<OrderItemBilling> OrderItemBillings { get; set; }
    public ICollection<OrderItemChange> OrderItemChanges { get; set; }
    public ICollection<OrderItemContactMech> OrderItemContactMeches { get; set; }
    public ICollection<OrderItemGroupOrder> OrderItemGroupOrders { get; set; }
    public ICollection<OrderItemPriceInfo> OrderItemPriceInfos { get; set; }
    public ICollection<OrderItemRole> OrderItemRoles { get; set; }
    public ICollection<OrderItemShipGroupAssoc> OrderItemShipGroupAssocs { get; set; }
    public ICollection<OrderItemShipGrpInvRes> OrderItemShipGrpInvRes { get; set; }
    public ICollection<OrderRequirementCommitment> OrderRequirementCommitments { get; set; }
    public ICollection<PicklistItem> PicklistItems { get; set; }
    public ICollection<ProductOrderItem> ProductOrderItemEngagementIs { get; set; }
    public ICollection<ProductOrderItem> ProductOrderItemOrderIs { get; set; }
    public ICollection<ReturnItem> ReturnItems { get; set; }
    public ICollection<ShipmentReceipt> ShipmentReceipts { get; set; }
    public ICollection<Subscription> Subscriptions { get; set; }
    public ICollection<WorkOrderItemFulfillment> WorkOrderItemFulfillments { get; set; }
}