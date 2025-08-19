namespace Application.Shipments.InvoiceTypes;

public class InvoiceTypeDto
{
    public string InvoiceTypeId { get; set; } = null!;
    public string? ParentTypeId { get; set; }
    public string? HasTable { get; set; }
    public string? Description { get; set; }
}