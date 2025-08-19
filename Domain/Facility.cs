using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;
[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]

public class Facility
{
    public Facility()
    {
        AgreementFacilityAppls = new HashSet<AgreementFacilityAppl>();
        Containers = new HashSet<Container>();
        DeliveryDestFacilities = new HashSet<Delivery>();
        DeliveryOriginFacilities = new HashSet<Delivery>();
        FacilityAttributes = new HashSet<FacilityAttribute>();
        FacilityCalendars = new HashSet<FacilityCalendar>();
        FacilityCarrierShipments = new HashSet<FacilityCarrierShipment>();
        FacilityContactMechPurposes = new HashSet<FacilityContactMechPurpose>();
        FacilityContactMeches = new HashSet<FacilityContactMech>();
        FacilityContents = new HashSet<FacilityContent>();
        FacilityGroupMembers = new HashSet<FacilityGroupMember>();
        FacilityLocations = new HashSet<FacilityLocation>();
        FacilityParties = new HashSet<FacilityParty>();
        FixedAssets = new HashSet<FixedAsset>();
        InventoryItems = new HashSet<InventoryItem>();
        InventoryTransferFacilities = new HashSet<InventoryTransfer>();
        InventoryTransferFacilityIdToNavigations = new HashSet<InventoryTransfer>();
        InverseParentFacility = new HashSet<Facility>();
        MrpEvents = new HashSet<MrpEvent>();
        OrderHeaders = new HashSet<OrderHeader>();
        OrderItemShipGroups = new HashSet<OrderItemShipGroup>();
        OrderSummaryEntries = new HashSet<OrderSummaryEntry>();
        Picklists = new HashSet<Picklist>();
        ProdCatalogInvFacilities = new HashSet<ProdCatalogInvFacility>();
        ProductAverageCosts = new HashSet<ProductAverageCost>();
        ProductFacilities = new HashSet<ProductFacility>();
        ProductFacilityAssocFacilities = new HashSet<ProductFacilityAssoc>();
        ProductFacilityAssocFacilityIdToNavigations = new HashSet<ProductFacilityAssoc>();
        ProductStoreFacilities = new HashSet<ProductStoreFacility>();
        ProductStores = new HashSet<ProductStore>();
        Products = new HashSet<Product>();
        ReorderGuidelines = new HashSet<ReorderGuideline>();
        Requirements = new HashSet<Requirement>();
        ReturnHeaders = new HashSet<ReturnHeader>();
        ShipmentDestinationFacilities = new HashSet<Shipment>();
        ShipmentOriginFacilities = new HashSet<Shipment>();
        ShipmentRouteSegmentDestFacilities = new HashSet<ShipmentRouteSegment>();
        ShipmentRouteSegmentOriginFacilities = new HashSet<ShipmentRouteSegment>();
        WorkEffortPartyAssignments = new HashSet<WorkEffortPartyAssignment>();
        WorkEfforts = new HashSet<WorkEffort>();
    }

    public string FacilityId { get; set; } = null!;
    public string? FacilityTypeId { get; set; }
    public string? ParentFacilityId { get; set; }
    public string? OwnerPartyId { get; set; }
    public string? DefaultInventoryItemTypeId { get; set; }
    public string? FacilityName { get; set; }
    public string? FacilityNameArabic { get; set; }
    public string? FacilityNameTurkish { get; set; }
    public string? PrimaryFacilityGroupId { get; set; }
    public int? SquareFootage { get; set; }
    public decimal? FacilitySize { get; set; }
    public string? FacilitySizeUomId { get; set; }
    public string? ProductStoreId { get; set; }
    public int? DefaultDaysToShip { get; set; }
    public DateTime? OpenedDate { get; set; }
    public DateTime? ClosedDate { get; set; }
    public string? Description { get; set; }
    public string? DefaultDimensionUomId { get; set; }
    public string? DefaultWeightUomId { get; set; }
    public string? GeoPointId { get; set; }
    public int? FacilityLevel { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Uom? DefaultDimensionUom { get; set; }
    public InventoryItemType? DefaultInventoryItemType { get; set; }
    public Uom? DefaultWeightUom { get; set; }
    public Uom? FacilitySizeUom { get; set; }
    public FacilityType? FacilityType { get; set; }
    public GeoPoint? GeoPoint { get; set; }
    public Party? OwnerParty { get; set; }
    public Facility? ParentFacility { get; set; }
    public FacilityGroup? PrimaryFacilityGroup { get; set; }
    public ICollection<AgreementFacilityAppl> AgreementFacilityAppls { get; set; }
    public ICollection<Container> Containers { get; set; }
    public ICollection<Delivery> DeliveryDestFacilities { get; set; }
    public ICollection<Delivery> DeliveryOriginFacilities { get; set; }
    public ICollection<FacilityAttribute> FacilityAttributes { get; set; }
    public ICollection<FacilityCalendar> FacilityCalendars { get; set; }
    public ICollection<FacilityCarrierShipment> FacilityCarrierShipments { get; set; }
    public ICollection<FacilityContactMechPurpose> FacilityContactMechPurposes { get; set; }
    public ICollection<FacilityContactMech> FacilityContactMeches { get; set; }
    public ICollection<FacilityContent> FacilityContents { get; set; }
    public ICollection<FacilityGroupMember> FacilityGroupMembers { get; set; }
    public ICollection<FacilityLocation> FacilityLocations { get; set; }
    public ICollection<FacilityParty> FacilityParties { get; set; }
    public ICollection<FixedAsset> FixedAssets { get; set; }
    public ICollection<InventoryItem> InventoryItems { get; set; }
    public ICollection<InventoryTransfer> InventoryTransferFacilities { get; set; }
    public ICollection<InventoryTransfer> InventoryTransferFacilityIdToNavigations { get; set; }
    public ICollection<Facility> InverseParentFacility { get; set; }
    public ICollection<MrpEvent> MrpEvents { get; set; }
    public ICollection<OrderHeader> OrderHeaders { get; set; }
    public ICollection<OrderItemShipGroup> OrderItemShipGroups { get; set; }
    public ICollection<OrderSummaryEntry> OrderSummaryEntries { get; set; }
    public ICollection<Picklist> Picklists { get; set; }
    public ICollection<ProdCatalogInvFacility> ProdCatalogInvFacilities { get; set; }
    public ICollection<ProductAverageCost> ProductAverageCosts { get; set; }
    public ICollection<ProductFacility> ProductFacilities { get; set; }
    public ICollection<ProductFacilityAssoc> ProductFacilityAssocFacilities { get; set; }
    public ICollection<ProductFacilityAssoc> ProductFacilityAssocFacilityIdToNavigations { get; set; }
    public ICollection<ProductStoreFacility> ProductStoreFacilities { get; set; }
    public ICollection<ProductStore> ProductStores { get; set; }
    public ICollection<Product> Products { get; set; }
    public ICollection<ReorderGuideline> ReorderGuidelines { get; set; }
    public ICollection<Requirement> Requirements { get; set; }
    public ICollection<ReturnHeader> ReturnHeaders { get; set; }
    public ICollection<Shipment> ShipmentDestinationFacilities { get; set; }
    public ICollection<Shipment> ShipmentOriginFacilities { get; set; }
    public ICollection<ShipmentRouteSegment> ShipmentRouteSegmentDestFacilities { get; set; }
    public ICollection<ShipmentRouteSegment> ShipmentRouteSegmentOriginFacilities { get; set; }
    public ICollection<WorkEffortPartyAssignment> WorkEffortPartyAssignments { get; set; }
    public ICollection<WorkEffort> WorkEfforts { get; set; }
}