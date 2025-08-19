using System.ComponentModel.DataAnnotations;

namespace Application.Manufacturing;

public class BillOfMaterialRecord
{
    [Key] public string? ProductId { get; set; }

    public string? ProductName { get; set; }
    public string? ProductDescription { get; set; }
    public string? ProductAssocTypeDescription { get; set; }

    public string? UomDescription { get; set; }
}