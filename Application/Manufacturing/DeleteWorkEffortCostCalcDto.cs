namespace Application.Manufacturing;

public class DeleteWorkEffortCostCalcDto
{
    public string WorkEffortId { get; set; }
    public string CostComponentCalcId { get; set; }
    public string CostComponentTypeId { get; set; }
    public string FromDate { get; set; }
}