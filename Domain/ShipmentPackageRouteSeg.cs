namespace Domain;

public class ShipmentPackageRouteSeg
{
    public string ShipmentId { get; set; } = null!;
    public string ShipmentPackageSeqId { get; set; } = null!;
    public string ShipmentRouteSegmentId { get; set; } = null!;
    public string? TrackingCode { get; set; }
    public string? BoxNumber { get; set; }
    public byte[]? LabelImage { get; set; }
    public byte[]? LabelIntlSignImage { get; set; }
    public string? LabelHtml { get; set; }
    public string? LabelPrinted { get; set; }
    public byte[]? InternationalInvoice { get; set; }
    public decimal? PackageTransportCost { get; set; }
    public decimal? PackageServiceCost { get; set; }
    public decimal? PackageOtherCost { get; set; }
    public decimal? CodAmount { get; set; }
    public decimal? InsuredAmount { get; set; }
    public string? CurrencyUomId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Uom? CurrencyUom { get; set; }
    public ShipmentPackage Shipment { get; set; } = null!;
    public ShipmentRouteSegment ShipmentNavigation { get; set; } = null!;
}