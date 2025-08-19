namespace Application.Manufacturing;

public class AssignPartyToWorkEffortResult
{
    public bool IsError { get; set; }
    public string ErrorMessage { get; set; }
    public DateTime? AssignedFromDate { get; set; }
}