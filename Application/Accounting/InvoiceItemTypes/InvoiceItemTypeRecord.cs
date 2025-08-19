using System.ComponentModel.DataAnnotations;

namespace Application.Shipments.InvoiceItemTypes;

public class InvoiceItemTypeRecord
{
    [Key] public string InvoiceItemTypeId { get; set; } = null!;

    public string? ParentTypeId { get; set; }
    public string? HasTable { get; set; }
    public string? Description { get; set; }
    public string? DefaultGlAccountId { get; set; }
}