namespace Domain;

public class PerfReviewItem
{
    public string EmployeePartyId { get; set; } = null!;
    public string EmployeeRoleTypeId { get; set; } = null!;
    public string PerfReviewId { get; set; } = null!;
    public string PerfReviewItemSeqId { get; set; } = null!;
    public string? PerfReviewItemTypeId { get; set; }
    public string? PerfRatingTypeId { get; set; }
    public string? Comments { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public PartyRole Employee { get; set; } = null!;
    public Party EmployeeParty { get; set; } = null!;
    public PerfReview PerfReview { get; set; } = null!;
}