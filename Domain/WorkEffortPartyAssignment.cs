namespace Domain;

public class WorkEffortPartyAssignment
{
    public WorkEffortPartyAssignment()
    {
        ApplicationSandboxes = new HashSet<ApplicationSandbox>();
    }

    public string WorkEffortId { get; set; } = null!;
    public string PartyId { get; set; } = null!;
    public string RoleTypeId { get; set; } = null!;
    public DateTime FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public string? AssignedByUserLoginId { get; set; }
    public string? StatusId { get; set; }
    public DateTime? StatusDateTime { get; set; }
    public string? ExpectationEnumId { get; set; }
    public string? DelegateReasonEnumId { get; set; }
    public string? FacilityId { get; set; }
    public string? Comments { get; set; }
    public string? MustRsvp { get; set; }
    public string? AvailabilityStatusId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public UserLogin? AssignedByUserLogin { get; set; }
    public StatusItem? AvailabilityStatus { get; set; }
    public Enumeration? DelegateReasonEnum { get; set; }
    public Enumeration? ExpectationEnum { get; set; }
    public Facility? Facility { get; set; }
    public PartyRole PartyRole { get; set; } = null!;
    public StatusItem? Status { get; set; }
    public WorkEffort WorkEffort { get; set; } = null!;
    public ICollection<ApplicationSandbox> ApplicationSandboxes { get; set; }
}