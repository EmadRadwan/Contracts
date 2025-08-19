namespace Domain;

public class PerfReview
{
    public PerfReview()
    {
        PerfReviewItems = new HashSet<PerfReviewItem>();
    }

    public string EmployeePartyId { get; set; } = null!;
    public string EmployeeRoleTypeId { get; set; } = null!;
    public string PerfReviewId { get; set; } = null!;
    public string? ManagerPartyId { get; set; }
    public string? ManagerRoleTypeId { get; set; }
    public string? PaymentId { get; set; }
    public string? EmplPositionId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public string? Comments { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public PartyRole Employee { get; set; } = null!;
    public Party EmployeeParty { get; set; } = null!;
    public Party? ManagerParty { get; set; }
    public Payment? Payment { get; set; }
    public ICollection<PerfReviewItem> PerfReviewItems { get; set; }
}