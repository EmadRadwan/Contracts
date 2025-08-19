namespace Domain;

public class ProductStoreShipmentMeth
{
    public ProductStoreShipmentMeth()
    {
        ShipmentCostEstimates = new HashSet<ShipmentCostEstimate>();
    }

    public string ProductStoreShipMethId { get; set; } = null!;
    public string? ProductStoreId { get; set; }
    public string? ShipmentMethodTypeId { get; set; }
    public string? PartyId { get; set; }
    public string? RoleTypeId { get; set; }
    public string? CompanyPartyId { get; set; }
    public decimal? MinWeight { get; set; }
    public decimal? MaxWeight { get; set; }
    public decimal? MinSize { get; set; }
    public decimal? MaxSize { get; set; }
    public decimal? MinTotal { get; set; }
    public decimal? MaxTotal { get; set; }
    public string? AllowUspsAddr { get; set; }
    public string? RequireUspsAddr { get; set; }
    public string? AllowCompanyAddr { get; set; }
    public string? RequireCompanyAddr { get; set; }
    public string? IncludeNoChargeItems { get; set; }
    public string? IncludeFeatureGroup { get; set; }
    public string? ExcludeFeatureGroup { get; set; }
    public string? IncludeGeoId { get; set; }
    public string? ExcludeGeoId { get; set; }
    public string? ServiceName { get; set; }
    public string? ConfigProps { get; set; }
    public string? ShipmentCustomMethodId { get; set; }
    public string? ShipmentGatewayConfigId { get; set; }
    public int? SequenceNumber { get; set; }
    public decimal? AllowancePercent { get; set; }
    public decimal? MinimumPrice { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public CustomMethod? ShipmentCustomMethod { get; set; }
    public ShipmentGatewayConfig? ShipmentGatewayConfig { get; set; }
    public ShipmentMethodType? ShipmentMethodType { get; set; }
    public ICollection<ShipmentCostEstimate> ShipmentCostEstimates { get; set; }
}