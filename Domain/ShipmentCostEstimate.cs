namespace Domain;

public class ShipmentCostEstimate
{
    public string ShipmentCostEstimateId { get; set; } = null!;
    public string? ShipmentMethodTypeId { get; set; }
    public string? CarrierPartyId { get; set; }
    public string? CarrierRoleTypeId { get; set; }
    public string? ProductStoreShipMethId { get; set; }
    public string? ProductStoreId { get; set; }
    public string? PartyId { get; set; }
    public string? RoleTypeId { get; set; }
    public string? GeoIdTo { get; set; }
    public string? GeoIdFrom { get; set; }
    public string? WeightBreakId { get; set; }
    public string? WeightUomId { get; set; }
    public decimal? WeightUnitPrice { get; set; }
    public string? QuantityBreakId { get; set; }
    public string? QuantityUomId { get; set; }
    public decimal? QuantityUnitPrice { get; set; }
    public string? PriceBreakId { get; set; }
    public string? PriceUomId { get; set; }
    public decimal? PriceUnitPrice { get; set; }
    public decimal? OrderFlatPrice { get; set; }
    public decimal? OrderPricePercent { get; set; }
    public decimal? OrderItemFlatPrice { get; set; }
    public decimal? ShippingPricePercent { get; set; }
    public string? ProductFeatureGroupId { get; set; }
    public decimal? OversizeUnit { get; set; }
    public decimal? OversizePrice { get; set; }
    public decimal? FeaturePercent { get; set; }
    public decimal? FeaturePrice { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public CarrierShipmentMethod? CarrierShipmentMethod { get; set; }
    public Geo? GeoIdFromNavigation { get; set; }
    public Geo? GeoIdToNavigation { get; set; }
    public Party? Party { get; set; }
    public QuantityBreak? PriceBreak { get; set; }
    public Uom? PriceUom { get; set; }
    public ProductStoreShipmentMeth? ProductStoreShipMeth { get; set; }
    public QuantityBreak? QuantityBreak { get; set; }
    public Uom? QuantityUom { get; set; }
    public RoleType? RoleType { get; set; }
    public QuantityBreak? WeightBreak { get; set; }
    public Uom? WeightUom { get; set; }
}