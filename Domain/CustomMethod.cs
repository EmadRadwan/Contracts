using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class CustomMethod
{
    public CustomMethod()
    {
        Contents = new HashSet<Content>();
        CostComponentCalcs = new HashSet<CostComponentCalc>();
        FixedAssetDepMethods = new HashSet<FixedAssetDepMethod>();
        PartyAcctgPreferenceInvoiceSeqCustMeths = new HashSet<PartyAcctgPreference>();
        PartyAcctgPreferenceOrderSeqCustMeths = new HashSet<PartyAcctgPreference>();
        PartyAcctgPreferenceQuoteSeqCustMeths = new HashSet<PartyAcctgPreference>();
        ProductAssocs = new HashSet<ProductAssoc>();
        ProductPrices = new HashSet<ProductPrice>();
        ProductPromoActions = new HashSet<ProductPromoAction>();
        ProductPromoConds = new HashSet<ProductPromoCond>();
        ProductStorePaymentSettings = new HashSet<ProductStorePaymentSetting>();
        ProductStoreShipmentMeths = new HashSet<ProductStoreShipmentMeth>();
        ProductStoreTelecomSettings = new HashSet<ProductStoreTelecomSetting>();
        UomConversionDateds = new HashSet<UomConversionDated>();
        UomConversions = new HashSet<UomConversion>();
        WorkEfforts = new HashSet<WorkEffort>();
    }

    public string CustomMethodId { get; set; } = null!;
    public string? CustomMethodTypeId { get; set; }
    public string? CustomMethodName { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public CustomMethodType? CustomMethodType { get; set; }
    public ICollection<Content> Contents { get; set; }
    public ICollection<CostComponentCalc> CostComponentCalcs { get; set; }
    public ICollection<FixedAssetDepMethod> FixedAssetDepMethods { get; set; }
    public ICollection<PartyAcctgPreference> PartyAcctgPreferenceInvoiceSeqCustMeths { get; set; }
    public ICollection<PartyAcctgPreference> PartyAcctgPreferenceOrderSeqCustMeths { get; set; }
    public ICollection<PartyAcctgPreference> PartyAcctgPreferenceQuoteSeqCustMeths { get; set; }
    public ICollection<ProductAssoc> ProductAssocs { get; set; }
    public ICollection<ProductPrice> ProductPrices { get; set; }
    public ICollection<ProductPromoAction> ProductPromoActions { get; set; }
    public ICollection<ProductPromoCond> ProductPromoConds { get; set; }
    public ICollection<ProductStorePaymentSetting> ProductStorePaymentSettings { get; set; }
    public ICollection<ProductStoreShipmentMeth> ProductStoreShipmentMeths { get; set; }
    public ICollection<ProductStoreTelecomSetting> ProductStoreTelecomSettings { get; set; }
    public ICollection<UomConversionDated> UomConversionDateds { get; set; }
    public ICollection<UomConversion> UomConversions { get; set; }
    public ICollection<WorkEffort> WorkEfforts { get; set; }
}