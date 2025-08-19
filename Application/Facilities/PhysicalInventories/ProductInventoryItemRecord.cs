using System.ComponentModel.DataAnnotations;
namespace Application.Facilities.PhysicalInventories;

public class ProductInventoryItemRecord
{
    public string ProductId { get; set; }
    public string FacilityId { get; set; }
    public string ProductName { get; set; }
    public string InternalName { get; set; }
    public string InventoryItemTypeId { get; set; }
    public string ProductFacilityId { get; set; }
    public string InventoryComments { get; set; }
    public decimal? ProductATP { get; set; }
    public decimal? ProductQOH { get; set; }    
}