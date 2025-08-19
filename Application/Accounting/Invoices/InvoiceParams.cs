using Application.Core;

namespace Application.Shipments.Invoices;

public class InvoiceParams : PaginationParams
{
    public string? OrderBy { get; set; }
    public string? SearchTerm { get; set; }
    public string? InvoiceTypeId { get; set; }
    public string? Categories { get; set; }
}