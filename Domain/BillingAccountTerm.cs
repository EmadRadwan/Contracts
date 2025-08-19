using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;
[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]

public class BillingAccountTerm
{
    public BillingAccountTerm()
    {
        BillingAccountTermAttrs = new HashSet<BillingAccountTermAttr>();
    }

    public string BillingAccountTermId { get; set; } = null!;
    public string? BillingAccountId { get; set; }
    public string? TermTypeId { get; set; }
    public decimal? TermValue { get; set; }
    public int? TermDays { get; set; }
    public string? UomId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public BillingAccount? BillingAccount { get; set; }
    public TermType? TermType { get; set; }
    public Uom? Uom { get; set; }
    public ICollection<BillingAccountTermAttr> BillingAccountTermAttrs { get; set; }
}