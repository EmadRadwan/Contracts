using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class PeriodType
{
    public PeriodType()
    {
        CustomTimePeriods = new HashSet<CustomTimePeriod>();
        PayHistories = new HashSet<PayHistory>();
        RateAmounts = new HashSet<RateAmount>();
    }

    public string PeriodTypeId { get; set; } = null!;
    public string? Description { get; set; }
    public int? PeriodLength { get; set; }
    public string? UomId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Uom? Uom { get; set; }
    public ICollection<CustomTimePeriod> CustomTimePeriods { get; set; }
    public ICollection<PayHistory> PayHistories { get; set; }
    public ICollection<RateAmount> RateAmounts { get; set; }
}