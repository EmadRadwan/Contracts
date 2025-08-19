namespace Application.Facilities.PhysicalInventories;

public class ProductInventoryItem
{
    public string ProductId { get; set; }
    public string ProductName { get; set; }
    public string InternalName { get; set; }
    public string FacilityId { get; set; }
    public string StatusId { get; set; }
    public string InventoryItemTypeId { get; set; }
    public string ProductFacilityId { get; set; }
    public string InventoryComments { get; set; }
    public decimal? QuantityOnHandTotal { get; set; }
    public decimal? AvailableToPromiseTotal { get; set; }    
    public decimal? AccountingQuantityTotal { get; set; }    
}