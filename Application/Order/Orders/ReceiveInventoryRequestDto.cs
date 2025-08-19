namespace Application.order.Orders;

public class ReceiveInventoryRequestDto
{
    /// <summary>
    /// Gets or sets the Facility ID.
    /// </summary>
    public string? FacilityId { get; set; }

    /// <summary>
    /// Gets or sets the Purchase Order ID.
    /// </summary>
    public string PurchaseOrderId { get; set; }
    
}