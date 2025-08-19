namespace Application.Manufacturing;

public class TaskCostResult
{
    public decimal TaskCost { get; set; }
    public List<CostByType> CostsByType { get; set; }

    public TaskCostResult()
    {
        CostsByType = new List<CostByType>();
    }
}