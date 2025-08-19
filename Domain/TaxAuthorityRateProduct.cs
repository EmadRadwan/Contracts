using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class TaxAuthorityRateProduct
{
    public TaxAuthorityRateProduct()
    {
        InvoiceItems = new HashSet<InvoiceItem>();
        OrderAdjustments = new HashSet<OrderAdjustment>();
        ReturnAdjustments = new HashSet<ReturnAdjustment>();
    }

    public string TaxAuthorityRateSeqId { get; set; } = null!;
    public string? TaxAuthGeoId { get; set; }
    public string? TaxAuthPartyId { get; set; }
    public string? TaxAuthorityRateTypeId { get; set; }
    public string? ProductStoreId { get; set; }
    public string? ProductCategoryId { get; set; }
    public string? TitleTransferEnumId { get; set; }
    public decimal? MinItemPrice { get; set; }
    public decimal? MinPurchase { get; set; }
    public string? TaxShipping { get; set; }
    public decimal? TaxPercentage { get; set; }
    public string? TaxPromotions { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public string? Description { get; set; }
    public string? IsTaxInShippingPrice { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ProductCategory? ProductCategory { get; set; }
    public ProductStore? ProductStore { get; set; }
    public TaxAuthority? TaxAuth { get; set; }
    public TaxAuthorityRateType? TaxAuthorityRateType { get; set; }
    public ICollection<InvoiceItem> InvoiceItems { get; set; }
    public ICollection<OrderAdjustment> OrderAdjustments { get; set; }
    public ICollection<ReturnAdjustment> ReturnAdjustments { get; set; }
}