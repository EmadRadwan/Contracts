namespace Application.Shipments;

public class ShipmentItemCreateParameters
{
    public string ShipmentId { get; set; } // Required (PK field)
    public string ShipmentItemSeqId { get; set; } // Optional (INOUT parameter)
    public string ProductId { get; set; } // Optional (Non-PK field)

    public decimal Quantity { get; set; } // Optional (Non-PK field)
    // Include other non-PK fields as needed
}