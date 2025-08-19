namespace Domain;

public class ShipmentRouteSegment
{
    public ShipmentRouteSegment()
    {
        ShipmentPackageRouteSegs = new HashSet<ShipmentPackageRouteSeg>();
    }

    public string ShipmentId { get; set; } = null!;
    public string ShipmentRouteSegmentId { get; set; } = null!;
    public string? DeliveryId { get; set; }
    public string? OriginFacilityId { get; set; }
    public string? DestFacilityId { get; set; }
    public string? OriginContactMechId { get; set; }
    public string? OriginTelecomNumberId { get; set; }
    public string? DestContactMechId { get; set; }
    public string? DestTelecomNumberId { get; set; }
    public string? CarrierPartyId { get; set; }
    public string? ShipmentMethodTypeId { get; set; }
    public string? CarrierServiceStatusId { get; set; }
    public string? CarrierDeliveryZone { get; set; }
    public string? CarrierRestrictionCodes { get; set; }
    public string? CarrierRestrictionDesc { get; set; }
    public decimal? BillingWeight { get; set; }
    public string? BillingWeightUomId { get; set; }
    public decimal? ActualTransportCost { get; set; }
    public decimal? ActualServiceCost { get; set; }
    public decimal? ActualOtherCost { get; set; }
    public decimal? ActualCost { get; set; }
    public string? CurrencyUomId { get; set; }
    public DateTime? ActualStartDate { get; set; }
    public DateTime? ActualArrivalDate { get; set; }
    public DateTime? EstimatedStartDate { get; set; }
    public DateTime? EstimatedArrivalDate { get; set; }
    public string? TrackingIdNumber { get; set; }
    public string? TrackingDigest { get; set; }
    public string? UpdatedByUserLoginId { get; set; }
    public DateTime? LastUpdatedDate { get; set; }
    public string? HomeDeliveryType { get; set; }
    public DateTime? HomeDeliveryDate { get; set; }
    public string? ThirdPartyAccountNumber { get; set; }
    public string? ThirdPartyPostalCode { get; set; }
    public string? ThirdPartyCountryGeoCode { get; set; }
    public byte[]? UpsHighValueReport { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Uom? BillingWeightUom { get; set; }
    public Party? CarrierParty { get; set; }
    public StatusItem? CarrierServiceStatus { get; set; }
    public Uom? CurrencyUom { get; set; }
    public Delivery? Delivery { get; set; }
    public PostalAddress? DestContactMech { get; set; }
    public Facility? DestFacility { get; set; }
    public TelecomNumber? DestTelecomNumber { get; set; }
    public PostalAddress? OriginContactMech { get; set; }
    public Facility? OriginFacility { get; set; }
    public TelecomNumber? OriginTelecomNumber { get; set; }
    public Shipment Shipment { get; set; } = null!;
    public ShipmentMethodType? ShipmentMethodType { get; set; }
    public ICollection<ShipmentPackageRouteSeg> ShipmentPackageRouteSegs { get; set; }
}