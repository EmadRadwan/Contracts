namespace Application.Order.Orders;

public class SalesOrderItemAssocDto
{
    /// <summary>
    /// Gets or sets the Order ID of the associated Sales Order.
    /// </summary>
    public string OrderId { get; set; }

    /// <summary>
    /// Gets or sets the Order Item Association Type ID.
    /// </summary>
    public string OrderItemAssocTypeId { get; set; }

    // Add other relevant properties as needed
}