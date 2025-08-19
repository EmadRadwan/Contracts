using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;
[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]

public class InventoryItem
{
    public InventoryItem()
    {
        AcctgTrans = new HashSet<AcctgTran>();
        AcctgTransEntries = new HashSet<AcctgTransEntry>();
        InventoryItemAttributes = new HashSet<InventoryItemAttribute>();
        InventoryItemDetails = new HashSet<InventoryItemDetail>();
        InventoryItemLabelAppls = new HashSet<InventoryItemLabelAppl>();
        InventoryItemStatuses = new HashSet<InventoryItemStatus>();
        InventoryItemVariances = new HashSet<InventoryItemVariance>();
        InventoryTransfers = new HashSet<InventoryTransfer>();
        InvoiceItems = new HashSet<InvoiceItem>();
        ItemIssuances = new HashSet<ItemIssuance>();
        OrderItemShipGrpInvRes = new HashSet<OrderItemShipGrpInvRes>();
        OrderItems = new HashSet<OrderItem>();
        PicklistItems = new HashSet<PicklistItem>();
        ShipmentReceipts = new HashSet<ShipmentReceipt>();
        Subscriptions = new HashSet<Subscription>();
        WorkEffortInventoryAssigns = new HashSet<WorkEffortInventoryAssign>();
        WorkEffortInventoryRes = new HashSet<WorkEffortInventoryRes>();
        WorkEffortInventoryProduceds = new HashSet<WorkEffortInventoryProduced>();
    }

    public string InventoryItemId { get; set; } = null!;
    public string? InventoryItemTypeId { get; set; }
    public string? ProductId { get; set; }
    public string? PartyId { get; set; }
    public string? OwnerPartyId { get; set; }
    public string? StatusId { get; set; }
    public DateTime? DatetimeReceived { get; set; }
    public DateTime? DatetimeManufactured { get; set; }
    public DateTime? ExpireDate { get; set; }
    public string? FacilityId { get; set; }
    public string? ContainerId { get; set; }
    public string? LotId { get; set; }
    public string? UomId { get; set; }
    public string? BinNumber { get; set; }
    public string? LocationSeqId { get; set; }
    public string? Comments { get; set; }
    public decimal? QuantityOnHandTotal { get; set; }
    public decimal? AvailableToPromiseTotal { get; set; }
    public decimal? AccountingQuantityTotal { get; set; }
    public decimal? QuantityOnHand { get; set; }
    public decimal? AvailableToPromise { get; set; }
    public string? SerialNumber { get; set; }
    public string? SoftIdentifier { get; set; }
    public string? ActivationNumber { get; set; }
    public DateTime? ActivationValidThru { get; set; }
    public decimal? UnitCost { get; set; }
    public string? CurrencyUomId { get; set; }
    public string? FixedAssetId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Container? Container { get; set; }
    public Uom? CurrencyUom { get; set; }
    public Facility? Facility { get; set; }
    public FixedAsset? FixedAsset { get; set; }
    public InventoryItemType? InventoryItemType { get; set; }
    public Lot? Lot { get; set; }
    public Party? OwnerParty { get; set; }
    public Party? Party { get; set; }
    public Product? Product { get; set; }
    public StatusItem? Status { get; set; }
    public Uom? Uom { get; set; }
    public ICollection<AcctgTran> AcctgTrans { get; set; }
    public ICollection<AcctgTransEntry> AcctgTransEntries { get; set; }
    public ICollection<InventoryItemAttribute> InventoryItemAttributes { get; set; }
    public ICollection<InventoryItemDetail> InventoryItemDetails { get; set; }
    public ICollection<InventoryItemLabelAppl> InventoryItemLabelAppls { get; set; }
    public ICollection<InventoryItemStatus> InventoryItemStatuses { get; set; }
    public ICollection<InventoryItemVariance> InventoryItemVariances { get; set; }
    public ICollection<InventoryTransfer> InventoryTransfers { get; set; }
    public ICollection<InvoiceItem> InvoiceItems { get; set; }
    public ICollection<ItemIssuance> ItemIssuances { get; set; }
    public ICollection<OrderItemShipGrpInvRes> OrderItemShipGrpInvRes { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; }
    public ICollection<PicklistItem> PicklistItems { get; set; }
    public ICollection<ShipmentReceipt> ShipmentReceipts { get; set; }
    public ICollection<Subscription> Subscriptions { get; set; }
    public ICollection<WorkEffortInventoryAssign> WorkEffortInventoryAssigns { get; set; }
    public ICollection<WorkEffortInventoryProduced> WorkEffortInventoryProduceds { get; set; }
    public ICollection<WorkEffortInventoryRes> WorkEffortInventoryRes { get; set; }
    public ICollection<InventoryItemFeature> Features { get; set; }

}