using System.ComponentModel.DataAnnotations;

namespace Application.Facilities.FacilityInventories;

public class FacilityInventoryItemDetailRecord
{
    [Key] public string InventoryItemId { get; set; } = null!;

    [Key] public string InventoryItemDetailSeqId { get; set; } = null!;

    public string? ProductId { get; set; }
    public string? ProductName { get; set; }
    public string? FacilityId { get; set; }
    public string? FacilityName { get; set; }
    public decimal? QuantityOnHandTotal { get; set; }
    public decimal? AvailableToPromiseTotal { get; set; }
    public decimal? AccountingQuantityDiff { get; set; }
    public DateTime? EffectiveDate { get; set; }
    public decimal? QuantityOnHandDiff { get; set; }
    public decimal? AvailableToPromiseDiff { get; set; }
    public string? OrderId { get; set; }
    public string? WorkEffortId { get; set; }
    public string? OrderItemSeqId { get; set; }
}