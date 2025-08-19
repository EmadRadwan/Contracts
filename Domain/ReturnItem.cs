namespace Domain;

public class ReturnItem
{
    public ReturnItem()
    {
        ReturnItemBillings = new HashSet<ReturnItemBilling>();
        ReturnItemShipments = new HashSet<ReturnItemShipment>();
        ShipmentReceipts = new HashSet<ShipmentReceipt>();
    }

    public string ReturnId { get; set; } = null!;
    public string ReturnItemSeqId { get; set; } = null!;
    public string? ReturnReasonId { get; set; }
    public string? ReturnTypeId { get; set; }
    public string? ReturnItemTypeId { get; set; }
    public string? ProductId { get; set; }
    public string? Description { get; set; }
    public string? OrderId { get; set; }
    public string? OrderItemSeqId { get; set; }
    public string? StatusId { get; set; }
    public string? ExpectedItemStatus { get; set; }
    public decimal? ReturnQuantity { get; set; }
    public decimal? ReceivedQuantity { get; set; }
    public decimal? ReturnPrice { get; set; }
    public string? ReturnItemResponseId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public StatusItem? ExpectedItemStatusNavigation { get; set; }
    public OrderHeader? Order { get; set; }
    public OrderItem? OrderI { get; set; }
    public Product? Product { get; set; }
    public ReturnHeader Return { get; set; } = null!;
    public ReturnItemResponse? ReturnItemResponse { get; set; }
    public ReturnItemType? ReturnItemType { get; set; }
    public ReturnReason? ReturnReason { get; set; }
    public ReturnType? ReturnType { get; set; }
    public StatusItem? Status { get; set; }
    public ICollection<ReturnItemBilling> ReturnItemBillings { get; set; }
    public ICollection<ReturnItemShipment> ReturnItemShipments { get; set; }
    public ICollection<ShipmentReceipt> ShipmentReceipts { get; set; }
}