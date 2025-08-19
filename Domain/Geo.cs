using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class Geo
{
    public Geo()
    {
        AgreementGeographicalApplics = new HashSet<AgreementGeographicalApplic>();
        CostComponents = new HashSet<CostComponent>();
        GeoAssocGeoIdToNavigations = new HashSet<GeoAssoc>();
        GeoAssocGeos = new HashSet<GeoAssoc>();
        InvoiceItems = new HashSet<InvoiceItem>();
        OrderAdjustmentPrimaryGeos = new HashSet<OrderAdjustment>();
        OrderAdjustmentSecondaryGeos = new HashSet<OrderAdjustment>();
        PaymentApplications = new HashSet<PaymentApplication>();
        PostalAddressBoundaries = new HashSet<PostalAddressBoundary>();
        PostalAddressCityGeos = new HashSet<PostalAddress>();
        PostalAddressCountryGeos = new HashSet<PostalAddress>();
        PostalAddressCountyGeos = new HashSet<PostalAddress>();
        PostalAddressMunicipalityGeos = new HashSet<PostalAddress>();
        PostalAddressPostalCodeGeos = new HashSet<PostalAddress>();
        PostalAddressStateProvinceGeos = new HashSet<PostalAddress>();
        ProductGeos = new HashSet<ProductGeo>();
        ProductPrices = new HashSet<ProductPrice>();
        Products = new HashSet<Product>();
        QuoteAdjustmentPrimaryGeos = new HashSet<QuoteAdjustment>();
        QuoteAdjustmentSecondaryGeos = new HashSet<QuoteAdjustment>();
        ReorderGuidelines = new HashSet<ReorderGuideline>();
        ReturnAdjustmentPrimaryGeos = new HashSet<ReturnAdjustment>();
        ReturnAdjustmentSecondaryGeos = new HashSet<ReturnAdjustment>();
        SegmentGroupGeos = new HashSet<SegmentGroupGeo>();
        ShipmentCostEstimateGeoIdFromNavigations = new HashSet<ShipmentCostEstimate>();
        ShipmentCostEstimateGeoIdToNavigations = new HashSet<ShipmentCostEstimate>();
        ShipmentTimeEstimateGeoIdFromNavigations = new HashSet<ShipmentTimeEstimate>();
        ShipmentTimeEstimateGeoIdToNavigations = new HashSet<ShipmentTimeEstimate>();
        SurveyQuestions = new HashSet<SurveyQuestion>();
        TaxAuthorities = new HashSet<TaxAuthority>();
    }

    public string GeoId { get; set; } = null!;
    public string? GeoTypeId { get; set; }
    public string? GeoName { get; set; }
    public string? GeoCode { get; set; }
    public string? GeoSecCode { get; set; }
    public string? Abbreviation { get; set; }
    public string? WellKnownText { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public GeoType? GeoType { get; set; }
    public CountryAddressFormat CountryAddressFormat { get; set; } = null!;
    public ICollection<AgreementGeographicalApplic> AgreementGeographicalApplics { get; set; }
    public ICollection<CostComponent> CostComponents { get; set; }
    public ICollection<GeoAssoc> GeoAssocGeoIdToNavigations { get; set; }
    public ICollection<GeoAssoc> GeoAssocGeos { get; set; }
    public ICollection<InvoiceItem> InvoiceItems { get; set; }
    public ICollection<OrderAdjustment> OrderAdjustmentPrimaryGeos { get; set; }
    public ICollection<OrderAdjustment> OrderAdjustmentSecondaryGeos { get; set; }
    public ICollection<PaymentApplication> PaymentApplications { get; set; }
    public ICollection<PostalAddressBoundary> PostalAddressBoundaries { get; set; }
    public ICollection<PostalAddress> PostalAddressCityGeos { get; set; }
    public ICollection<PostalAddress> PostalAddressCountryGeos { get; set; }
    public ICollection<PostalAddress> PostalAddressCountyGeos { get; set; }
    public ICollection<PostalAddress> PostalAddressMunicipalityGeos { get; set; }
    public ICollection<PostalAddress> PostalAddressPostalCodeGeos { get; set; }
    public ICollection<PostalAddress> PostalAddressStateProvinceGeos { get; set; }
    public ICollection<ProductGeo> ProductGeos { get; set; }
    public ICollection<ProductPrice> ProductPrices { get; set; }
    public ICollection<Product> Products { get; set; }
    public ICollection<QuoteAdjustment> QuoteAdjustmentPrimaryGeos { get; set; }
    public ICollection<QuoteAdjustment> QuoteAdjustmentSecondaryGeos { get; set; }
    public ICollection<ReorderGuideline> ReorderGuidelines { get; set; }
    public ICollection<ReturnAdjustment> ReturnAdjustmentPrimaryGeos { get; set; }
    public ICollection<ReturnAdjustment> ReturnAdjustmentSecondaryGeos { get; set; }
    public ICollection<SegmentGroupGeo> SegmentGroupGeos { get; set; }
    public ICollection<ShipmentCostEstimate> ShipmentCostEstimateGeoIdFromNavigations { get; set; }
    public ICollection<ShipmentCostEstimate> ShipmentCostEstimateGeoIdToNavigations { get; set; }
    public ICollection<ShipmentTimeEstimate> ShipmentTimeEstimateGeoIdFromNavigations { get; set; }
    public ICollection<ShipmentTimeEstimate> ShipmentTimeEstimateGeoIdToNavigations { get; set; }
    public ICollection<SurveyQuestion> SurveyQuestions { get; set; }
    public ICollection<TaxAuthority> TaxAuthorities { get; set; }
}