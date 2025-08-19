namespace Domain;

public class ShipmentPackage
{
    public ShipmentPackage()
    {
        ShipmentPackageContents = new HashSet<ShipmentPackageContent>();
        ShipmentPackageRouteSegs = new HashSet<ShipmentPackageRouteSeg>();
        ShipmentReceipts = new HashSet<ShipmentReceipt>();
        ShippingDocuments = new HashSet<ShippingDocument>();
    }

    public string ShipmentId { get; set; } = null!;
    public string ShipmentPackageSeqId { get; set; } = null!;
    public string? ShipmentBoxTypeId { get; set; }
    public DateTime? DateCreated { get; set; }
    public decimal? BoxLength { get; set; }
    public decimal? BoxHeight { get; set; }
    public decimal? BoxWidth { get; set; }
    public string? DimensionUomId { get; set; }
    public decimal? Weight { get; set; }
    public string? WeightUomId { get; set; }
    public decimal? InsuredValue { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Uom? DimensionUom { get; set; }
    public Shipment Shipment { get; set; } = null!;
    public ShipmentBoxType? ShipmentBoxType { get; set; }
    public Uom? WeightUom { get; set; }
    public ICollection<ShipmentPackageContent> ShipmentPackageContents { get; set; }
    public ICollection<ShipmentPackageRouteSeg> ShipmentPackageRouteSegs { get; set; }
    public ICollection<ShipmentReceipt> ShipmentReceipts { get; set; }
    public ICollection<ShippingDocument> ShippingDocuments { get; set; }
}