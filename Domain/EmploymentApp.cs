namespace Domain;

public class EmploymentApp
{
    public string ApplicationId { get; set; } = null!;
    public string? EmplPositionId { get; set; }
    public string? StatusId { get; set; }
    public string? EmploymentAppSourceTypeId { get; set; }
    public string? ApplyingPartyId { get; set; }
    public string? ReferredByPartyId { get; set; }
    public DateTime? ApplicationDate { get; set; }
    public string? ApproverPartyId { get; set; }
    public string? JobRequisitionId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Party? ApproverParty { get; set; }
    public JobRequisition? JobRequisition { get; set; }
}