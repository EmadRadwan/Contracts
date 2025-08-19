namespace Domain;

public class ShipmentReceipt
{
    public ShipmentReceipt()
    {
        AcctgTrans = new HashSet<AcctgTran>();
        InventoryItemDetails = new HashSet<InventoryItemDetail>();
        OrderItemBillings = new HashSet<OrderItemBilling>();
        ReturnItemBillings = new HashSet<ReturnItemBilling>();
        ShipmentReceiptRoles = new HashSet<ShipmentReceiptRole>();
    }

    public string ReceiptId { get; set; } = null!;
    public string? InventoryItemId { get; set; }
    public string? ProductId { get; set; }
    public string? ShipmentId { get; set; }
    public string? ShipmentItemSeqId { get; set; }
    public string? ShipmentPackageSeqId { get; set; }
    public string? OrderId { get; set; }
    public string? OrderItemSeqId { get; set; }
    public string? ReturnId { get; set; }
    public string? ReturnItemSeqId { get; set; }
    public string? RejectionId { get; set; }
    public string? ReceivedByUserLoginId { get; set; }
    public DateTime? DatetimeReceived { get; set; }
    public string? ItemDescription { get; set; }
    public decimal? QuantityAccepted { get; set; }
    public decimal? QuantityRejected { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public InventoryItem? InventoryItem { get; set; }
    public OrderItem? OrderI { get; set; }
    public Product? Product { get; set; }
    public UserLogin? ReceivedByUserLogin { get; set; }
    public RejectionReason? Rejection { get; set; }
    public ReturnItem? ReturnI { get; set; }
    public ShipmentPackage? Shipment { get; set; }
    public ICollection<AcctgTran> AcctgTrans { get; set; }
    public ICollection<InventoryItemDetail> InventoryItemDetails { get; set; }
    public ICollection<OrderItemBilling> OrderItemBillings { get; set; }
    public ICollection<ReturnItemBilling> ReturnItemBillings { get; set; }
    public ICollection<ShipmentReceiptRole> ShipmentReceiptRoles { get; set; }
}