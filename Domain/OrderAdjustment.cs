using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;
[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]

public class OrderAdjustment
{
    public OrderAdjustment()
    {
        OrderAdjustmentAttributes = new HashSet<OrderAdjustmentAttribute>();
        OrderAdjustmentBillings = new HashSet<OrderAdjustmentBilling>();
        ReturnAdjustments = new HashSet<ReturnAdjustment>();
    }

    public string OrderAdjustmentId { get; set; } = null!;
    public string? OrderAdjustmentTypeId { get; set; }
    public string? OrderId { get; set; }
    public string? OrderItemSeqId { get; set; }
    public string? ShipGroupSeqId { get; set; }
    public string? Comments { get; set; }
    public string? Description { get; set; }
    public decimal? Amount { get; set; }
    public decimal? RecurringAmount { get; set; }
    public decimal? AmountAlreadyIncluded { get; set; }
    public string? ProductPromoId { get; set; }
    public string? ProductPromoRuleId { get; set; }
    public string? ProductPromoActionSeqId { get; set; }
    public string? ProductFeatureId { get; set; }
    public string? CorrespondingProductId { get; set; }
    public string? TaxAuthorityRateSeqId { get; set; }
    public string? SourceReferenceId { get; set; }
    public decimal? SourcePercentage { get; set; }
    public string? CustomerReferenceId { get; set; }
    public string? PrimaryGeoId { get; set; }
    public string? SecondaryGeoId { get; set; }
    public decimal? ExemptAmount { get; set; }
    public string? TaxAuthGeoId { get; set; }
    public string? TaxAuthPartyId { get; set; }
    public string? OverrideGlAccountId { get; set; }
    public string? IncludeInTax { get; set; }
    public string? IncludeInShipping { get; set; }
    public string? IsManual { get; set; }
    public DateTime? CreatedDate { get; set; }
    public string? CreatedByUserLogin { get; set; }
    public DateTime? LastModifiedDate { get; set; }
    public string? LastModifiedByUserLogin { get; set; }
    public string? OriginalAdjustmentId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public UserLogin? CreatedByUserLoginNavigation { get; set; }
    public OrderHeader? Order { get; set; }
    public OrderAdjustmentType? OrderAdjustmentType { get; set; }
    public GlAccount? OverrideGlAccount { get; set; }
    public Geo? PrimaryGeo { get; set; }
    public ProductPromo? ProductPromo { get; set; }
    public Geo? SecondaryGeo { get; set; }
    public TaxAuthority? TaxAuth { get; set; }
    public TaxAuthorityRateProduct? TaxAuthorityRateSeq { get; set; }
    public ICollection<OrderAdjustmentAttribute> OrderAdjustmentAttributes { get; set; }
    public ICollection<OrderAdjustmentBilling> OrderAdjustmentBillings { get; set; }
    public ICollection<ReturnAdjustment> ReturnAdjustments { get; set; }
}