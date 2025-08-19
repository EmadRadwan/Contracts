namespace Application.ProductFacilities;

public class ProductFacilityDto
{
    public string ProductId { get; set; }
    public string FacilityId { get; set; }
    public string? FacilityName { get; set; }
    public decimal? MinimumStock { get; set; }
    public decimal? ReorderQuantity { get; set; }
    public decimal DaysToShip { get; set; }
    public string? ReplenishMethodEnumId { get; set; }
    public decimal? LastInventoryCount { get; set; }
    public string? RequirementMethodEnumId { get; set; }
}