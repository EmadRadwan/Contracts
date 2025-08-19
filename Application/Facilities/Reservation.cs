using Domain;

namespace Application.Facilities;

public class Reservation
{
    public string InventoryItemId { get; set; }
    public decimal Quantity { get; set; }
    public decimal? QuantityNotAvailable { get; set; }
    public InventoryItem InventoryItem { get; set; }
}