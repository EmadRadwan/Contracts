#nullable enable

using System.ComponentModel.DataAnnotations;

namespace Application.Facilities.FacilityInventories;

public class FacilityInventoryRecord
{
    [Key] public string? ProductId { get; set; }

    [Key] public string? FacilityId { get; set; }

    public string? ProductName { get; set; }
    public string? FacilityName { get; set; }
    public decimal? QuantityOnHandTotal { get; set; }
    public decimal? AvailableToPromiseTotal { get; set; }
    public decimal? OrderedQuantity { get; set; }
    public decimal? UsageQuantity { get; set; }
    public decimal? QuantityOnOrder { get; set; }
    public decimal? OffsetQOHQtyAvailable { get; set; }
    public decimal? OffsetATPQtyAvailable { get; set; }
    public decimal? AvailableToPromiseMinusMinimumStock { get; set; }
    public decimal? QuantityOnHandMinusMinimumStock { get; set; }
    public decimal? ListPrice { get; set; }
    public decimal? WholeSalePrice { get; set; }
    public decimal? DefaultPrice { get; set; }
    public string? QuantityUomId { get; set; }
    public decimal? MinimumStock { get; set; }
    public decimal? ReorderQuantity { get; set; }
}