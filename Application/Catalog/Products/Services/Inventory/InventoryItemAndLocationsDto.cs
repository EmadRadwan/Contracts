using Domain;

namespace Application.Catalog.Products.Services.Inventory;

public class InventoryItemAndLocationsDto
{
    public InventoryItem InventoryItem { get; set; }
    public Product Product { get; set; }
    public FacilityLocation FacilityLocation { get; set; }
}