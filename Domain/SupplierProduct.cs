using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;
[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]

public class SupplierProduct
{
    public string ProductId { get; set; } = null!;
    public string PartyId { get; set; } = null!;
    public DateTime AvailableFromDate { get; set; }
    public DateTime? AvailableThruDate { get; set; }
    public string? SupplierPrefOrderId { get; set; }
    public string? SupplierRatingTypeId { get; set; }
    public decimal? StandardLeadTimeDays { get; set; }
    public decimal MinimumOrderQuantity { get; set; }
    public decimal? OrderQtyIncrements { get; set; }
    public decimal? UnitsIncluded { get; set; }
    public string? QuantityUomId { get; set; }
    public string? AgreementId { get; set; }
    public string? AgreementItemSeqId { get; set; }
    public decimal? LastPrice { get; set; }
    public decimal? ShippingPrice { get; set; }
    public string CurrencyUomId { get; set; } = null!;
    public string? SupplierProductName { get; set; }
    public string? SupplierProductId { get; set; }
    public string? CanDropShip { get; set; }
    public string? Comments { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public AgreementItem? AgreementI { get; set; }
    public Uom? CurrencyUom { get; set; }
    public Party? Party { get; set; }
    public Product? Product { get; set; }
    public Uom? QuantityUom { get; set; }
    public SupplierPrefOrder? SupplierPrefOrder { get; set; }
    public SupplierRatingType? SupplierRatingType { get; set; }
}