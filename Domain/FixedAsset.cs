using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class FixedAsset
{
    public FixedAsset()
    {
        AccommodationMaps = new HashSet<AccommodationMap>();
        AccommodationSpots = new HashSet<AccommodationSpot>();
        AcctgTrans = new HashSet<AcctgTran>();
        CostComponents = new HashSet<CostComponent>();
        Deliveries = new HashSet<Delivery>();
        FixedAssetAttributes = new HashSet<FixedAssetAttribute>();
        FixedAssetDepMethods = new HashSet<FixedAssetDepMethod>();
        FixedAssetGeoPoints = new HashSet<FixedAssetGeoPoint>();
        FixedAssetIdents = new HashSet<FixedAssetIdent>();
        FixedAssetMaintOrders = new HashSet<FixedAssetMaintOrder>();
        FixedAssetMaints = new HashSet<FixedAssetMaint>();
        FixedAssetProducts = new HashSet<FixedAssetProduct>();
        FixedAssetRegistrations = new HashSet<FixedAssetRegistration>();
        FixedAssetStdCosts = new HashSet<FixedAssetStdCost>();
        InventoryItems = new HashSet<InventoryItem>();
        InverseParentFixedAsset = new HashSet<FixedAsset>();
        PartyFixedAssetAssignments = new HashSet<PartyFixedAssetAssignment>();
        Requirements = new HashSet<Requirement>();
        WorkEffortFixedAssetAssigns = new HashSet<WorkEffortFixedAssetAssign>();
        WorkEfforts = new HashSet<WorkEffort>();
    }

    public string FixedAssetId { get; set; } = null!;
    public string? FixedAssetTypeId { get; set; }
    public string? ParentFixedAssetId { get; set; }
    public string? InstanceOfProductId { get; set; }
    public string? ClassEnumId { get; set; }
    public string? PartyId { get; set; }
    public string? RoleTypeId { get; set; }
    public string? FixedAssetName { get; set; }
    public string? AcquireOrderId { get; set; }
    public string? AcquireOrderItemSeqId { get; set; }
    public DateTime? DateAcquired { get; set; }
    public DateTime? DateLastServiced { get; set; }
    public DateTime? DateNextService { get; set; }
    public DateTime? ExpectedEndOfLife { get; set; }
    public DateTime? ActualEndOfLife { get; set; }
    public decimal? ProductionCapacity { get; set; }
    public string? UomId { get; set; }
    public string? CalendarId { get; set; }
    public string? SerialNumber { get; set; }
    public string? LocatedAtFacilityId { get; set; }
    public string? LocatedAtLocationSeqId { get; set; }
    public decimal? SalvageValue { get; set; }
    public decimal? Depreciation { get; set; }
    public decimal? PurchaseCost { get; set; }
    public string? PurchaseCostUomId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public OrderHeader? AcquireOrder { get; set; }
    public OrderItem? AcquireOrderI { get; set; }
    public TechDataCalendar? Calendar { get; set; }
    public Enumeration? ClassEnum { get; set; }
    public FixedAssetType? FixedAssetType { get; set; }
    public Product? InstanceOfProduct { get; set; }
    public Facility? LocatedAtFacility { get; set; }
    public FixedAsset? ParentFixedAsset { get; set; }
    public Party? Party { get; set; }
    public RoleType? RoleType { get; set; }
    public Uom? Uom { get; set; }
    public ICollection<AccommodationMap> AccommodationMaps { get; set; }
    public ICollection<AccommodationSpot> AccommodationSpots { get; set; }
    public ICollection<AcctgTran> AcctgTrans { get; set; }
    public ICollection<CostComponent> CostComponents { get; set; }
    public ICollection<Delivery> Deliveries { get; set; }
    public ICollection<FixedAssetAttribute> FixedAssetAttributes { get; set; }
    public ICollection<FixedAssetDepMethod> FixedAssetDepMethods { get; set; }
    public ICollection<FixedAssetGeoPoint> FixedAssetGeoPoints { get; set; }
    public ICollection<FixedAssetIdent> FixedAssetIdents { get; set; }
    public ICollection<FixedAssetMaintOrder> FixedAssetMaintOrders { get; set; }
    public ICollection<FixedAssetMaint> FixedAssetMaints { get; set; }
    public ICollection<FixedAssetProduct> FixedAssetProducts { get; set; }
    public ICollection<FixedAssetRegistration> FixedAssetRegistrations { get; set; }
    public ICollection<FixedAssetStdCost> FixedAssetStdCosts { get; set; }
    public ICollection<InventoryItem> InventoryItems { get; set; }
    public ICollection<FixedAsset> InverseParentFixedAsset { get; set; }
    public ICollection<PartyFixedAssetAssignment> PartyFixedAssetAssignments { get; set; }
    public ICollection<Requirement> Requirements { get; set; }
    public ICollection<WorkEffortFixedAssetAssign> WorkEffortFixedAssetAssigns { get; set; }
    public ICollection<WorkEffort> WorkEfforts { get; set; }
}