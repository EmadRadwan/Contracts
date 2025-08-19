namespace Domain;

public class PartyResume
{
    public string ResumeId { get; set; } = null!;
    public string? PartyId { get; set; }
    public string? ContentId { get; set; }
    public DateTime? ResumeDate { get; set; }
    public string? ResumeText { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Party? Party { get; set; }
}