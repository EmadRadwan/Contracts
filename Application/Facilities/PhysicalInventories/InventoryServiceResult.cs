namespace Application.Facilities.PhysicalInventories;

public class InventoryServiceResult
{
    public bool Success { get; set; }
    public decimal? AvailableToPromiseTotal { get; set; }
    public decimal? QuantityOnHandTotal { get; set; }
}