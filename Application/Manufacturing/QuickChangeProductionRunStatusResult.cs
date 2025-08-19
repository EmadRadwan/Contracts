namespace Application.Manufacturing;

public class QuickChangeProductionRunStatusResult
{
    public bool Success { get; set; } = true;
    public string ErrorMessage { get; set; }
    public string CurrentStatusId { get; set; }
    public string CurrentStatusDescription { get; set; }
    public string FacilityName { get; set; }
    public DateTime EstimatedCompletionDate { get; set; }
    public DateTime ActualCompletionDate { get; set; }
    public DateTime EstimatedStartDate { get; set; }
    public DateTime ActualStartDate { get; set; }

}