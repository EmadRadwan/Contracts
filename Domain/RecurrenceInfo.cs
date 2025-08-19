namespace Domain;

public class RecurrenceInfo
{
    public RecurrenceInfo()
    {
        Invoices = new HashSet<Invoice>();
        JobSandboxes = new HashSet<JobSandbox>();
        ProductAssocs = new HashSet<ProductAssoc>();
        ShoppingLists = new HashSet<ShoppingList>();
        WorkEfforts = new HashSet<WorkEffort>();
    }

    public string RecurrenceInfoId { get; set; } = null!;
    public DateTime? StartDateTime { get; set; }
    public string? ExceptionDateTimes { get; set; }
    public string? RecurrenceDateTimes { get; set; }
    public string? ExceptionRuleId { get; set; }
    public string? RecurrenceRuleId { get; set; }
    public int? RecurrenceCount { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public RecurrenceRule? ExceptionRule { get; set; }
    public RecurrenceRule? RecurrenceRule { get; set; }
    public ICollection<Invoice> Invoices { get; set; }
    public ICollection<JobSandbox> JobSandboxes { get; set; }
    public ICollection<ProductAssoc> ProductAssocs { get; set; }
    public ICollection<ShoppingList> ShoppingLists { get; set; }
    public ICollection<WorkEffort> WorkEfforts { get; set; }
}