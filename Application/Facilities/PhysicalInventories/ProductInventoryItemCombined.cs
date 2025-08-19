namespace Application.Facilities.PhysicalInventories;

public class ProductInventoryItemCombined
{
    public string InventoryItemId { get; set; }
    public string ProductId { get; set; }
    public string InternalName { get; set; }
    public decimal? ItemATP { get; set; } // Item-level Available to Promise
    public decimal? ItemQOH { get; set; } // Item-level Quantity on Hand
    public decimal? ProductATP { get; set; } // Product-level Available to Promise
    public decimal? ProductQOH { get; set; } // Product-level Quantity on Hand
}
