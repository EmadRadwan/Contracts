using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class ProductFeature
{
    public ProductFeature()
    {
        CostComponents = new HashSet<CostComponent>();
        DesiredFeatures = new HashSet<DesiredFeature>();
        InvoiceItems = new HashSet<InvoiceItem>();
        ProductFeatureApplAttrs = new HashSet<ProductFeatureApplAttr>();
        ProductFeatureAppls = new HashSet<ProductFeatureAppl>();
        ProductFeatureDataResources = new HashSet<ProductFeatureDataResource>();
        ProductFeatureGroupAppls = new HashSet<ProductFeatureGroupAppl>();
        ProductFeatureIactnProductFeatureIdToNavigations = new HashSet<ProductFeatureIactn>();
        ProductFeatureIactnProductFeatures = new HashSet<ProductFeatureIactn>();
        ProductManufacturingRules = new HashSet<ProductManufacturingRule>();
        QuoteItems = new HashSet<QuoteItem>();
        ShipmentItemFeatures = new HashSet<ShipmentItemFeature>();
        SupplierProductFeatures = new HashSet<SupplierProductFeature>();
    }

    public string ProductFeatureId { get; set; } = null!;
    public string? ProductFeatureTypeId { get; set; }
    public string? ProductFeatureCategoryId { get; set; }
    public string? Description { get; set; }
    public string? DescriptionArabic { get; set; }
    public string? UomId { get; set; }
    public decimal? NumberSpecified { get; set; }
    public decimal? DefaultAmount { get; set; }
    public int? DefaultSequenceNum { get; set; }
    public string? Abbrev { get; set; }
    public string? IdCode { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ProductFeatureCategory? ProductFeatureCategory { get; set; }
    public ProductFeatureType? ProductFeatureType { get; set; }
    public Uom? Uom { get; set; }
    public ICollection<CostComponent> CostComponents { get; set; }
    public ICollection<DesiredFeature> DesiredFeatures { get; set; }
    public ICollection<InvoiceItem> InvoiceItems { get; set; }
    public ICollection<ProductFeatureApplAttr> ProductFeatureApplAttrs { get; set; }
    public ICollection<ProductFeatureAppl> ProductFeatureAppls { get; set; }
    public ICollection<ProductFeatureDataResource> ProductFeatureDataResources { get; set; }
    public ICollection<ProductFeatureGroupAppl> ProductFeatureGroupAppls { get; set; }
    public ICollection<ProductFeatureIactn> ProductFeatureIactnProductFeatureIdToNavigations { get; set; }
    public ICollection<ProductFeatureIactn> ProductFeatureIactnProductFeatures { get; set; }
    public ICollection<ProductManufacturingRule> ProductManufacturingRules { get; set; }
    public ICollection<QuoteItem> QuoteItems { get; set; }
    public ICollection<ShipmentItemFeature> ShipmentItemFeatures { get; set; }
    public ICollection<SupplierProductFeature> SupplierProductFeatures { get; set; }
}