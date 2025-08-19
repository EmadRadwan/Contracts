namespace Application.Manufacturing;

public class ChangeProductionRunTaskStatusResult
{
    public string OldStatusId { get; set; }
    public string NewStatusId { get; set; }
    public string SuccessMessage { get; set; }
    public string ErrorMessage { get; set; }
    public string CurrentStatusDescription { get; set; }
    public DateTime? MainProductionRunStartDate { get; set; }
    public string MainProductionRunStatus { get; set; }
}
