namespace Application.WorkEfforts;

public class WorkEffortPartyAssignmentDto
{
    public string WorkEffortId { get; set; }
    public string PartyId { get; set; }
    public string PartyName { get; set; }
    public string RoleTypeId { get; set; }
    public string RoleTypeDescription { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public string? StatusId { get; set; }
    public string? StatusDescription { get; set; }
    public DateTime? StatusDateTime { get; set; }
    public string? AvailabilityStatusId { get; set; }
    public string? AvailabilityStatusDescription { get; set; }
}