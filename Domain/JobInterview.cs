namespace Domain;

public class JobInterview
{
    public string JobInterviewId { get; set; } = null!;
    public string? JobIntervieweePartyId { get; set; }
    public string? JobRequisitionId { get; set; }
    public string? JobInterviewerPartyId { get; set; }
    public string? JobInterviewTypeId { get; set; }
    public string? GradeSecuredEnumId { get; set; }
    public string? JobInterviewResult { get; set; }
    public DateTime? JobInterviewDate { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Enumeration? GradeSecuredEnum { get; set; }
    public JobInterviewType? JobInterviewType { get; set; }
    public Party? JobIntervieweeParty { get; set; }
    public Party? JobInterviewerParty { get; set; }
    public JobRequisition? JobRequisition { get; set; }
}