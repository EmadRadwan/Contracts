using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;
[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]

public class ShipmentItem
{
    public ShipmentItem()
    {
        ItemIssuances = new HashSet<ItemIssuance>();
        ReturnItemShipments = new HashSet<ReturnItemShipment>();
        ShipmentItemBillings = new HashSet<ShipmentItemBilling>();
        ShipmentItemFeatures = new HashSet<ShipmentItemFeature>();
        ShipmentPackageContents = new HashSet<ShipmentPackageContent>();
        ShippingDocuments = new HashSet<ShippingDocument>();
    }

    public string ShipmentId { get; set; } = null!;
    public string ShipmentItemSeqId { get; set; } = null!;
    public string? ProductId { get; set; }
    public decimal? Quantity { get; set; }
    public string? ShipmentContentDescription { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Product? Product { get; set; }
    public Shipment Shipment { get; set; } = null!;
    public ICollection<ItemIssuance> ItemIssuances { get; set; }
    public ICollection<ReturnItemShipment> ReturnItemShipments { get; set; }
    public ICollection<ShipmentItemBilling> ShipmentItemBillings { get; set; }
    public ICollection<ShipmentItemFeature> ShipmentItemFeatures { get; set; }
    public ICollection<ShipmentPackageContent> ShipmentPackageContents { get; set; }
    public ICollection<ShippingDocument> ShippingDocuments { get; set; }
}