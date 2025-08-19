namespace Domain;

public class PostalAddress
{
    public PostalAddress()
    {
        BillingAccounts = new HashSet<BillingAccount>();
        CheckAccounts = new HashSet<CheckAccount>();
        CreditCards = new HashSet<CreditCard>();
        EftAccounts = new HashSet<EftAccount>();
        GiftCards = new HashSet<GiftCard>();
        OrderItemShipGroups = new HashSet<OrderItemShipGroup>();
        PayPalPaymentMethods = new HashSet<PayPalPaymentMethod>();
        PostalAddressBoundaries = new HashSet<PostalAddressBoundary>();
        ShipmentDestinationContactMeches = new HashSet<Shipment>();
        ShipmentOriginContactMeches = new HashSet<Shipment>();
        ShipmentRouteSegmentDestContactMeches = new HashSet<ShipmentRouteSegment>();
        ShipmentRouteSegmentOriginContactMeches = new HashSet<ShipmentRouteSegment>();
    }

    public string ContactMechId { get; set; } = null!;
    public string? ToName { get; set; }
    public string? AttnName { get; set; }
    public string? Address1 { get; set; }
    public string? Address2 { get; set; }
    public int? HouseNumber { get; set; }
    public string? HouseNumberExt { get; set; }
    public string? Directions { get; set; }
    public string? City { get; set; }
    public string? CityGeoId { get; set; }
    public string? PostalCode { get; set; }
    public string? PostalCodeExt { get; set; }
    public string? CountryGeoId { get; set; }
    public string? StateProvinceGeoId { get; set; }
    public string? CountyGeoId { get; set; }
    public string? MunicipalityGeoId { get; set; }
    public string? PostalCodeGeoId { get; set; }
    public string? GeoPointId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Geo? CityGeo { get; set; }
    public ContactMech ContactMech { get; set; } = null!;
    public Geo? CountryGeo { get; set; }
    public Geo? CountyGeo { get; set; }
    public GeoPoint? GeoPoint { get; set; }
    public Geo? MunicipalityGeo { get; set; }
    public Geo? PostalCodeGeo { get; set; }
    public Geo? StateProvinceGeo { get; set; }
    public ICollection<BillingAccount> BillingAccounts { get; set; }
    public ICollection<CheckAccount> CheckAccounts { get; set; }
    public ICollection<CreditCard> CreditCards { get; set; }
    public ICollection<EftAccount> EftAccounts { get; set; }
    public ICollection<GiftCard> GiftCards { get; set; }
    public ICollection<OrderItemShipGroup> OrderItemShipGroups { get; set; }
    public ICollection<PayPalPaymentMethod> PayPalPaymentMethods { get; set; }
    public ICollection<PostalAddressBoundary> PostalAddressBoundaries { get; set; }
    public ICollection<Shipment> ShipmentDestinationContactMeches { get; set; }
    public ICollection<Shipment> ShipmentOriginContactMeches { get; set; }
    public ICollection<ShipmentRouteSegment> ShipmentRouteSegmentDestContactMeches { get; set; }
    public ICollection<ShipmentRouteSegment> ShipmentRouteSegmentOriginContactMeches { get; set; }
}