namespace Application.Manufacturing;

public class IssueProductionRunTaskResult
{
    public List<InsufficientItem> InsufficientItems { get; set; } = new List<InsufficientItem>();
}

public class InsufficientItem
{
    public string ProductName { get; set; }
    public decimal QuantityMissing { get; set; }
}