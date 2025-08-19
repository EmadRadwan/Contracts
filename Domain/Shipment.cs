using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;
[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]

public class Shipment
{
    public Shipment()
    {
        AcctgTrans = new HashSet<AcctgTran>();
        OrderShipments = new HashSet<OrderShipment>();
        ReturnItemShipments = new HashSet<ReturnItemShipment>();
        ShipmentAttributes = new HashSet<ShipmentAttribute>();
        ShipmentContactMeches = new HashSet<ShipmentContactMech>();
        ShipmentItems = new HashSet<ShipmentItem>();
        ShipmentPackages = new HashSet<ShipmentPackage>();
        ShipmentRouteSegments = new HashSet<ShipmentRouteSegment>();
        ShipmentStatuses = new HashSet<ShipmentStatus>();
    }

    public string ShipmentId { get; set; } = null!;
    public string? ShipmentTypeId { get; set; }
    public string? StatusId { get; set; }
    public string? PrimaryOrderId { get; set; }
    public string? PrimaryReturnId { get; set; }
    public string? PrimaryShipGroupSeqId { get; set; }
    public string? PicklistBinId { get; set; }
    public DateTime? EstimatedReadyDate { get; set; }
    public DateTime? EstimatedShipDate { get; set; }
    public string? EstimatedShipWorkEffId { get; set; }
    public DateTime? EstimatedArrivalDate { get; set; }
    public string? EstimatedArrivalWorkEffId { get; set; }
    public DateTime? LatestCancelDate { get; set; }
    public decimal? EstimatedShipCost { get; set; }
    public string? CurrencyUomId { get; set; }
    public string? HandlingInstructions { get; set; }
    public string? OriginFacilityId { get; set; }
    public string? DestinationFacilityId { get; set; }
    public string? OriginContactMechId { get; set; }
    public string? OriginTelecomNumberId { get; set; }
    public string? DestinationContactMechId { get; set; }
    public string? DestinationTelecomNumberId { get; set; }
    public string? PartyIdTo { get; set; }
    public string? PartyIdFrom { get; set; }
    public decimal? AdditionalShippingCharge { get; set; }
    public string? AddtlShippingChargeDesc { get; set; }
    public DateTime? CreatedDate { get; set; }
    public string? CreatedByUserLogin { get; set; }
    public DateTime? LastModifiedDate { get; set; }
    public string? LastModifiedByUserLogin { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Uom? CurrencyUom { get; set; }
    public PostalAddress? DestinationContactMech { get; set; }
    public Facility? DestinationFacility { get; set; }
    public TelecomNumber? DestinationTelecomNumber { get; set; }
    public WorkEffort? EstimatedArrivalWorkEff { get; set; }
    public WorkEffort? EstimatedShipWorkEff { get; set; }
    public PostalAddress? OriginContactMech { get; set; }
    public Facility? OriginFacility { get; set; }
    public TelecomNumber? OriginTelecomNumber { get; set; }
    public Party? PartyIdFromNavigation { get; set; }
    public Party? PartyIdToNavigation { get; set; }
    public PicklistBin? PicklistBin { get; set; }
    public OrderHeader? PrimaryOrder { get; set; }
    public ReturnHeader? PrimaryReturn { get; set; }
    public ShipmentType? ShipmentType { get; set; }
    public StatusItem? Status { get; set; }
    public ICollection<AcctgTran> AcctgTrans { get; set; }
    public ICollection<OrderShipment> OrderShipments { get; set; }
    public ICollection<ReturnItemShipment> ReturnItemShipments { get; set; }
    public ICollection<ShipmentAttribute> ShipmentAttributes { get; set; }
    public ICollection<ShipmentContactMech> ShipmentContactMeches { get; set; }
    public ICollection<ShipmentItem> ShipmentItems { get; set; }
    public ICollection<ShipmentPackage> ShipmentPackages { get; set; }
    public ICollection<ShipmentRouteSegment> ShipmentRouteSegments { get; set; }
    public ICollection<ShipmentStatus> ShipmentStatuses { get; set; }
}