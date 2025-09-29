namespace Application.Projects;

public class IssueMaterialsForCertificateResult
{
    public List<InsufficientItem> InsufficientItems { get; set; } = new List<InsufficientItem>();
}

public class InsufficientItem
{
    public string ProductName { get; set; }
    public decimal QuantityMissing { get; set; }
}