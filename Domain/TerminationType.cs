namespace Domain;

public class TerminationType
{
    public TerminationType()
    {
        InverseParentType = new HashSet<TerminationType>();
    }

    public string TerminationTypeId { get; set; } = null!;
    public string? ParentTypeId { get; set; }
    public string? HasTable { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public TerminationType? ParentType { get; set; }
    public ICollection<TerminationType> InverseParentType { get; set; }
}