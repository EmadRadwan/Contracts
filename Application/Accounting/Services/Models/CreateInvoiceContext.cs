using Domain;

namespace Application.Accounting.Services.Models;

public class CreateInvoiceContext
{
    public string OrderId { get; set; }
    public List<OrderItem> BillItems { get; set; }
}