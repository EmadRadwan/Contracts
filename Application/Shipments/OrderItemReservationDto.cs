namespace Application.Shipments;

public class OrderItemReservationDto
{
    // From OrderItem (alias "OI")
    public string OrderId { get; set; }
    public string OrderItemSeqId { get; set; }
    public string ProductId { get; set; }
    
    // From InventoryItem (alias "II") via the join on OrderItemShipGrpInvRes (alias "OISGIR")
    // We use this field to indicate the actual product id from inventory.
    public string InventoryProductId { get; set; }
    
    // Aggregated values from OrderItemShipGrpInvRes ("totQuantityReserved" and "totQuantityNotAvailable")
    public decimal TotQuantityReserved { get; set; }
    public decimal TotQuantityNotAvailable { get; set; }
    
    // Calculated value: reserved quantity available.
    public decimal TotQuantityAvailable => TotQuantityReserved - TotQuantityNotAvailable;
}
