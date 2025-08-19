using Domain;

namespace Application.Catalog.Products.Services.Inventory;

public class InventoryItemAndLocationResult
{
    public InventoryItem InventoryItem { get; set; }
    public FacilityLocation FacilityLocation { get; set; }
}
