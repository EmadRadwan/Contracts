namespace Application.Catalog.Products.Services.Inventory;

public class CreatePhysicalInventoryDto
{
    public DateTime? PhysicalInventoryDate { get; set; }
    public string? PartyId { get; set; }
    public string? GeneralComments { get; set; }
}