namespace Application.Facilities.FacilityInventories;

public class FacilityInventoryItemDetailDto
{
    public string? ProductId { get; set; }
    public string? ProductName { get; set; }
    public string InventoryItemId { get; set; } = null!;
    public string? FacilityId { get; set; }
    public string? FacilityName { get; set; }
    public decimal? QuantityOnHandTotal { get; set; }
    public decimal? AvailableToPromiseTotal { get; set; }
    public string InventoryItemDetailSeqId { get; set; } = null!;
    public DateTime? EffectiveDate { get; set; }
    public decimal? QuantityOnHandDiff { get; set; }
    public decimal? AvailableToPromiseDiff { get; set; }
    public string? OrderId { get; set; }
    public string? OrderItemSeqId { get; set; }
}