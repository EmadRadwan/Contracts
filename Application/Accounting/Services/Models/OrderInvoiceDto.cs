using Domain;

namespace Application.Accounting.Services.Models;

public class OrderInvoiceDto
{
    // The order identifier.
    // Technical: This property holds the unique orderId from the ItemIssuance record.
    public string OrderId { get; set; }
    
    // The related Invoice entity.
    // Technical: This property holds the Invoice associated with the order, allowing access
    // to invoice details such as the current status.
    public Invoice Invoice { get; set; }
}