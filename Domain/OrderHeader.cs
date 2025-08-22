using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;
[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]

public class OrderHeader
{
    public OrderHeader()
    {
        AllocationPlanItems = new HashSet<AllocationPlanItem>();
        CommunicationEventOrders = new HashSet<CommunicationEventOrder>();
        FixedAssetMaintOrders = new HashSet<FixedAssetMaintOrder>();
        FixedAssetMaints = new HashSet<FixedAssetMaint>();
        FixedAssets = new HashSet<FixedAsset>();
        GiftCardFulfillments = new HashSet<GiftCardFulfillment>();
        OrderAdjustments = new HashSet<OrderAdjustment>();
        OrderAttributes = new HashSet<OrderAttribute>();
        OrderContactMeches = new HashSet<OrderContactMech>();
        OrderContents = new HashSet<OrderContent>();
        OrderDeliverySchedules = new HashSet<OrderDeliverySchedule>();
        OrderHeaderNotes = new HashSet<OrderHeaderNote>();
        OrderHeaderWorkEfforts = new HashSet<OrderHeaderWorkEffort>();
        OrderItemAssocOrders = new HashSet<OrderItemAssoc>();
        OrderItemAssocToOrders = new HashSet<OrderItemAssoc>();
        OrderItemBillings = new HashSet<OrderItemBilling>();
        OrderItemGroups = new HashSet<OrderItemGroup>();
        OrderItemRoles = new HashSet<OrderItemRole>();
        OrderItemShipGroupAssocs = new HashSet<OrderItemShipGroupAssoc>();
        OrderItemShipGroups = new HashSet<OrderItemShipGroup>();
        OrderItems = new HashSet<OrderItem>();
        OrderNotifications = new HashSet<OrderNotification>();
        OrderPaymentPreferences = new HashSet<OrderPaymentPreference>();
        OrderProductPromoCodes = new HashSet<OrderProductPromoCode>();
        OrderRequirementCommitments = new HashSet<OrderRequirementCommitment>();
        OrderRoles = new HashSet<OrderRole>();
        OrderShipments = new HashSet<OrderShipment>();
        OrderStatuses = new HashSet<OrderStatus>();
        OrderTerms = new HashSet<OrderTerm>();
        ProductOrderItemEngagements = new HashSet<ProductOrderItem>();
        ProductOrderItemOrders = new HashSet<ProductOrderItem>();
        ProductPromoUses = new HashSet<ProductPromoUse>();
        ReturnItemResponses = new HashSet<ReturnItemResponse>();
        ReturnItems = new HashSet<ReturnItem>();
        Shipments = new HashSet<Shipment>();
        TrackingCodeOrderReturns = new HashSet<TrackingCodeOrderReturn>();
        TrackingCodeOrders = new HashSet<TrackingCodeOrder>();
        WorkOrderItemFulfillments = new HashSet<WorkOrderItemFulfillment>();
    }

    public string OrderId { get; set; } = null!;
    public string? OrderTypeId { get; set; }
    public string? OrderName { get; set; }
    public string? ExternalId { get; set; }
    public string? SalesChannelEnumId { get; set; }
    public DateTime? OrderDate { get; set; }
    public string? Priority { get; set; }
    public DateTime? EntryDate { get; set; }
    public DateTime? PickSheetPrintedDate { get; set; }
    public string? VisitId { get; set; }

    public string? StatusId { get; set; }
    public string? CreatedBy { get; set; }
    public string? FirstAttemptOrderId { get; set; }
    public string? CurrencyUom { get; set; }
    public string? SyncStatusId { get; set; }
    public string? BillingAccountId { get; set; }
    public string? OriginFacilityId { get; set; }
    public string? WebSiteId { get; set; }
    public string? ProductStoreId { get; set; }
    public string? AgreementId { get; set; }
    public string? TerminalId { get; set; }
    public string? TransactionId { get; set; }
    public string? AutoOrderShoppingListId { get; set; }
    public string? NeedsInventoryIssuance { get; set; }
    public string? IsRushOrder { get; set; }
    public string? InternalCode { get; set; }
    public decimal? RemainingSubTotal { get; set; }
    public decimal? GrandTotal { get; set; }
    public int? CurrentMileage { get; set; }

    public string? IsViewed { get; set; }
    public string? InvoicePerShipment { get; set; }
    public string? CustomerRemarks { get; set; }
    public string? InternalRemarks { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ShoppingList? AutoOrderShoppingList { get; set; }
    public BillingAccount? BillingAccount { get; set; }
    public UserLogin? CreatedByNavigation { get; set; }
    public Uom? CurrencyUomNavigation { get; set; }
    public OrderType? OrderType { get; set; }
    public Facility? OriginFacility { get; set; }
    public ProductStore? ProductStore { get; set; }
    public Enumeration? SalesChannelEnum { get; set; }
    public StatusItem? Status { get; set; }
    public StatusItem? SyncStatus { get; set; }
    public WebSite? WebSite { get; set; }

    public ICollection<AllocationPlanItem> AllocationPlanItems { get; set; }
    public ICollection<CommunicationEventOrder> CommunicationEventOrders { get; set; }
    public ICollection<FixedAssetMaintOrder> FixedAssetMaintOrders { get; set; }
    public ICollection<FixedAssetMaint> FixedAssetMaints { get; set; }
    public ICollection<FixedAsset> FixedAssets { get; set; }
    public ICollection<GiftCardFulfillment> GiftCardFulfillments { get; set; }
    public ICollection<OrderAdjustment> OrderAdjustments { get; set; }
    public ICollection<OrderAttribute> OrderAttributes { get; set; }
    public ICollection<OrderContactMech> OrderContactMeches { get; set; }
    public ICollection<OrderContent> OrderContents { get; set; }
    public ICollection<OrderDeliverySchedule> OrderDeliverySchedules { get; set; }
    public ICollection<OrderHeaderNote> OrderHeaderNotes { get; set; }
    public ICollection<OrderHeaderWorkEffort> OrderHeaderWorkEfforts { get; set; }
    public ICollection<OrderItemAssoc> OrderItemAssocOrders { get; set; }
    public ICollection<OrderItemAssoc> OrderItemAssocToOrders { get; set; }
    public ICollection<OrderItemBilling> OrderItemBillings { get; set; }
    public ICollection<OrderItemGroup> OrderItemGroups { get; set; }
    public ICollection<OrderItemRole> OrderItemRoles { get; set; }
    public ICollection<OrderItemShipGroupAssoc> OrderItemShipGroupAssocs { get; set; }
    public ICollection<OrderItemShipGroup> OrderItemShipGroups { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; }
    public ICollection<OrderNotification> OrderNotifications { get; set; }
    public ICollection<OrderPaymentPreference> OrderPaymentPreferences { get; set; }
    public ICollection<OrderProductPromoCode> OrderProductPromoCodes { get; set; }
    public ICollection<OrderRequirementCommitment> OrderRequirementCommitments { get; set; }
    public ICollection<OrderRole> OrderRoles { get; set; }
    public ICollection<OrderShipment> OrderShipments { get; set; }
    public ICollection<OrderStatus> OrderStatuses { get; set; }
    public ICollection<OrderTerm> OrderTerms { get; set; }
    public ICollection<ProductOrderItem> ProductOrderItemEngagements { get; set; }
    public ICollection<ProductOrderItem> ProductOrderItemOrders { get; set; }
    public ICollection<ProductPromoUse> ProductPromoUses { get; set; }
    public ICollection<ReturnItemResponse> ReturnItemResponses { get; set; }
    public ICollection<ReturnItem> ReturnItems { get; set; }
    public ICollection<Shipment> Shipments { get; set; }
    public ICollection<TrackingCodeOrderReturn> TrackingCodeOrderReturns { get; set; }
    public ICollection<TrackingCodeOrder> TrackingCodeOrders { get; set; }
    public ICollection<WorkOrderItemFulfillment> WorkOrderItemFulfillments { get; set; }
    public ICollection<WorkEffort> WorkEfforts { get; set; }
}