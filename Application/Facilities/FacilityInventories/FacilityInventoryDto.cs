#nullable enable

namespace Application.Facilities.FacilityInventories;

public class FacilityInventoryDto
{
    public string? ProductId { get; set; }
    public string? FacilityId { get; set; }
    public string? ProductName { get; set; }
    public decimal? QuantityOnHandTotal { get; set; }
    public decimal? AvailableToPromiseTotal { get; set; }
    public decimal? OrderedQuantity { get; set; }
    public decimal? AvailableToPromiseMinusMinimumStock { get; set; }
    public decimal? QuantityOnHandMinusMinimumStock { get; set; }
    public decimal? ListPrice { get; set; }
    public decimal? DefaultPrice { get; set; }
    public string? QuantityUomId { get; set; }
    public decimal? MinimumStock { get; set; }
    public decimal? ReorderQuantity { get; set; }
}