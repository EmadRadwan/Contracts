namespace Application.Catalog.Products.Services.Inventory;

public class ReservationData
{
    public string OrderId { get; set; }
    public string ShipGroupSeqId { get; set; }
    public string OrderItemSeqId { get; set; }
    public string InventoryItemId { get; set; }
    public DateTime CurrentPromisedDate { get; set; }
    public decimal? QuantityNotAvailable { get; set; }
    public decimal? AvailableToPromiseTotal { get; set; }
    public int Priority { get; set; }
    public DateTime ReservedDatetime { get; set; }
    public string SequenceId { get; set; }
}