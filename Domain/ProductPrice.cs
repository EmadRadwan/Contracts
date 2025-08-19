using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;
[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]

public class ProductPrice
{
    public string ProductPriceId { get; set; } = null!;
    public string ProductId { get; set; } = null!;
    public string ProductPriceTypeId { get; set; } = null!;
    public string? ProductPricePurposeId { get; set; } = null!;
    public string CurrencyUomId { get; set; } = null!;
    public string? ProductStoreGroupId { get; set; } = null!;
    public DateTime FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public decimal? Price { get; set; }
    public string? TermUomId { get; set; }
    public string? CustomPriceCalcService { get; set; }
    public decimal? PriceWithoutTax { get; set; }
    public decimal? PriceWithTax { get; set; }
    public decimal? TaxAmount { get; set; }
    public decimal? TaxPercentage { get; set; }
    public string? TaxAuthPartyId { get; set; }
    public string? TaxAuthGeoId { get; set; }
    public string? TaxInPrice { get; set; }
    public DateTime? CreatedDate { get; set; }
    public string? CreatedByUserLogin { get; set; }
    public DateTime? LastModifiedDate { get; set; }
    public string? LastModifiedByUserLogin { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }


    public UserLogin? CreatedByUserLoginNavigation { get; set; }
    public Uom? CurrencyUom { get; set; } = null!;
    public CustomMethod? CustomPriceCalcServiceNavigation { get; set; }
    public UserLogin? LastModifiedByUserLoginNavigation { get; set; }
    public Product? Product { get; set; } = null!;
    public ProductPricePurpose? ProductPricePurpose { get; set; } = null!;
    public ProductPriceType? ProductPriceType { get; set; } = null!;
    public ProductStoreGroup? ProductStoreGroup { get; set; } = null!;
    public Geo? TaxAuthGeo { get; set; }
    public Party? TaxAuthParty { get; set; }
    public Uom? TermUom { get; set; }
}