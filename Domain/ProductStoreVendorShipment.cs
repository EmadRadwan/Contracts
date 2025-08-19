namespace Domain;

public class ProductStoreVendorShipment
{
    public string ProductStoreId { get; set; } = null!;
    public string VendorPartyId { get; set; } = null!;
    public string ShipmentMethodTypeId { get; set; } = null!;
    public string CarrierPartyId { get; set; } = null!;
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Party CarrierParty { get; set; } = null!;
    public ProductStore ProductStore { get; set; } = null!;
    public ShipmentMethodType ShipmentMethodType { get; set; } = null!;
    public Party VendorParty { get; set; } = null!;
}