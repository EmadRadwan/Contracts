namespace Application.Catalog.Products.Services.Inventory;

public class OrderItemShipGrpInvResDto
{
    public string OrderId { get; set; } = null!;
    public string ShipGroupSeqId { get; set; } = null!;
    public string OrderItemSeqId { get; set; } = null!;
    public string InventoryItemId { get; set; } = null!;
    public string? ReserveOrderEnumId { get; set; }
    public decimal? Quantity { get; set; }
    public decimal? QuantityNotAvailable { get; set; }
    public DateTime? ReservedDatetime { get; set; }
    public DateTime? CreatedDatetime { get; set; }
    public DateTime? PromisedDatetime { get; set; }
    public DateTime? CurrentPromisedDate { get; set; }
    public int? Priority { get; set; }
    public string SequenceId { get; set; }
}