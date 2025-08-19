namespace Application.Catalog.Products.Services.Inventory;

public class CreateInventoryItemDetailParam
{
    public string? InventoryItemId { get; set; }
    public string? OrderId { get; set; }
    public string? OrderItemSeqId { get; set; }
    public string? ShipGroupSeqId { get; set; }
    public string? PhysicalInventoryId { get; set; }
    public string? ItemIssuanceId { get; set; }
    public string? WorkEffortId { get; set; }
    public string? ReasonEnumId { get; set; }
    public string? Description { get; set; }
    public decimal? AvailableToPromiseDiff { get; set; }
    public decimal? QuantityOnHandDiff { get; set; }
    public decimal? AccountingQuantityDiff { get; set; }
}