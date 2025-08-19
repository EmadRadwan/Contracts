namespace Domain;

public class RateType
{
    public RateType()
    {
        PartyRateNews = new HashSet<PartyRateNew>();
        RateAmounts = new HashSet<RateAmount>();
        TimeEntries = new HashSet<TimeEntry>();
    }

    public string RateTypeId { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ICollection<PartyRateNew> PartyRateNews { get; set; }
    public ICollection<RateAmount> RateAmounts { get; set; }
    public ICollection<TimeEntry> TimeEntries { get; set; }
}