namespace Application.Facilities;

public class BulkPackItemDto
{
    public string OrderId { get; set; }
    public string OrderItemSeqId { get; set; }
    public string ProductId { get; set; }

    public string InternalName { get; set; } // from Product.internalName
    public decimal OrderedQuantity { get; set; } 
    public decimal ShippedQuantity { get; set; } 
    public decimal PackedQuantity { get; set; } // from packSession lines

    // If you want "qty to pack" as a separate field:
    public decimal QtyToPack => OrderedQuantity - (ShippedQuantity + PackedQuantity);

    // If needed:
    public int PackageSeq { get; set; }
    public decimal Weight { get; set; }
    // etc. for “numPackages” or “BoxType,” if your form uses them
}
