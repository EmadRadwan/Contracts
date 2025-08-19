namespace Application.Manufacturing;

public class ChangeProductionRunStatusResult
{
    public bool Success { get; set; } = true;
    public string Message { get; set; }
    public string ErrorMessage { get; set; }
    public string CurrentStatusId { get; set; }
    public string CurrentStatusDescription { get; set; }
    public string FacilityName { get; set; }
    public DateTime EstimatedCompletionDate { get; set; }
    public DateTime EstimatedStartDate { get; set; }
    public Dictionary<string, object> Data { get; set; } = new Dictionary<string, object>();
}