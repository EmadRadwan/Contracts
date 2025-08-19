using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;
[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]

public class OrderItemShipGroup
{
    public OrderItemShipGroup()
    {
        OrderItemShipGroupAssocs = new HashSet<OrderItemShipGroupAssoc>();
        PicklistBins = new HashSet<PicklistBin>();
        PicklistItems = new HashSet<PicklistItem>();
    }

    public string OrderId { get; set; } = null!;
    public string ShipGroupSeqId { get; set; } = null!;
    public string? ShipmentMethodTypeId { get; set; }
    public string? SupplierPartyId { get; set; }
    public string? SupplierAgreementId { get; set; }
    public string? VendorPartyId { get; set; }
    public string? CarrierPartyId { get; set; }
    public string? CarrierRoleTypeId { get; set; }
    public string? FacilityId { get; set; }
    public string? ContactMechId { get; set; }
    public string? TelecomContactMechId { get; set; }
    public string? TrackingNumber { get; set; }
    public string? ShippingInstructions { get; set; }
    public string? MaySplit { get; set; }
    public string? GiftMessage { get; set; }
    public string? IsGift { get; set; }
    public DateTime? ShipAfterDate { get; set; }
    public DateTime? ShipByDate { get; set; }
    public DateTime? EstimatedShipDate { get; set; }
    public DateTime? EstimatedDeliveryDate { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public PartyRole? Carrier { get; set; }
    public Party? CarrierParty { get; set; }
    public CarrierShipmentMethod? CarrierShipmentMethod { get; set; }
    public ContactMech? ContactMech { get; set; }
    public PostalAddress? ContactMechNavigation { get; set; }
    public Facility? Facility { get; set; }
    public OrderHeader Order { get; set; } = null!;
    public ShipmentMethodType? ShipmentMethodType { get; set; }
    public Agreement? SupplierAgreement { get; set; }
    public Party? SupplierParty { get; set; }
    public ContactMech? TelecomContactMech { get; set; }
    public TelecomNumber? TelecomContactMechNavigation { get; set; }
    public Party? VendorParty { get; set; }
    public ICollection<OrderItemShipGroupAssoc> OrderItemShipGroupAssocs { get; set; }
    public ICollection<PicklistBin> PicklistBins { get; set; }
    public ICollection<PicklistItem> PicklistItems { get; set; }
}