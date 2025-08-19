using Application.Core;

namespace Application.Facilities.FacilityInventories;

public class FacilityInventoryParams : PaginationParams
{
    public string? FacilityId { get; set; }
    public string? ProductId { get; set; }
    public string? InventoryItemId { get; set; }
    public string? OrderBy { get; set; }
    public string? SearchTerm { get; set; }
    public string? Types { get; set; }
    public string? Categories { get; set; }
}