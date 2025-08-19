namespace Domain;

public class RateAmount
{
    public string RateTypeId { get; set; } = null!;
    public string RateCurrencyUomId { get; set; } = null!;
    public string PeriodTypeId { get; set; } = null!;
    public string WorkEffortId { get; set; } = null!;
    public string PartyId { get; set; } = null!;
    public string EmplPositionTypeId { get; set; } = null!;
    public DateTime FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public decimal? RateAmount1 { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public EmplPositionType EmplPositionType { get; set; } = null!;
    public Party Party { get; set; } = null!;
    public PeriodType PeriodType { get; set; } = null!;
    public Uom RateCurrencyUom { get; set; } = null!;
    public RateType RateType { get; set; } = null!;
    public WorkEffort WorkEffort { get; set; } = null!;
}