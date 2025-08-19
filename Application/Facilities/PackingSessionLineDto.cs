namespace Application.Facilities;

public class PackingSessionLineDto
{
    public string OrderId { get; set; }
    public string OrderItemSeqId { get; set; }
    public string ShipGroupSeqId { get; set; }
    public string ProductId { get; set; }
    public string InventoryItemId { get; set; }
    public string ShipmentItemSeqId { get; set; }
    
    public decimal Quantity { get; set; }
    public decimal Weight { get; set; }
    
    public decimal? Height { get; set; }
    public decimal? Width { get; set; }
    public decimal? Length { get; set; }
    
    public string ShipmentBoxTypeId { get; set; }
    public string WeightPackageSeqId { get; set; }

    public int PackageSeq { get; set; }
}
