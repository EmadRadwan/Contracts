namespace Application.Order.Orders.Returns;

public class CreateReturnItemContext
{
    public string ReturnId { get; set; }
    public string ReturnItemTypeId { get; set; }
    public string ReturnTypeId { get; set; }
    public string OrderId { get; set; }
    public string OrderItemSeqId { get; set; }
    public decimal ReturnQuantity { get; set; }
    public decimal ReturnPrice { get; set; }
    public string IncludeAdjustments { get; set; } // Default to "Y" if not specified
    public string UserLoginPartyId { get; set; }

    // Optional fields for logging or debugging
    public string StatusId { get; set; } // e.g., "RETURN_REQUESTED"
}
