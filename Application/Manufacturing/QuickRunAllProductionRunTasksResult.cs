namespace Application.Manufacturing;

public class QuickRunAllProductionRunTasksResult
{
    public bool Success { get; set; }
    public bool Error => !Success;
    public string ErrorMessage { get; set; } 
}