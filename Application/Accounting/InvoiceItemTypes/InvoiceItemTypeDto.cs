namespace Application.Shipments.InvoiceItemTypes;

public class InvoiceItemTypeDto
{
    public string InvoiceItemTypeId { get; set; } = null!;
    public string? ParentTypeId { get; set; }
    public string? HasTable { get; set; }
    public string? Description { get; set; }
}