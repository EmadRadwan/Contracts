namespace Domain;

public class GlAccountHistory
{
    public string GlAccountId { get; set; } = null!;
    public string OrganizationPartyId { get; set; } = null!;
    public string CustomTimePeriodId { get; set; } = null!;
    public decimal? OpeningBalance { get; set; }
    public decimal? PostedDebits { get; set; }
    public decimal? PostedCredits { get; set; }
    public decimal? EndingBalance { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public CustomTimePeriod CustomTimePeriod { get; set; } = null!;
    public GlAccount GlAccount { get; set; } = null!;
    public Party OrganizationParty { get; set; } = null!;
}