namespace Domain;

public class DeductionType
{
    public DeductionType()
    {
        Deductions = new HashSet<Deduction>();
        InverseParentType = new HashSet<DeductionType>();
    }

    public string DeductionTypeId { get; set; } = null!;
    public string? ParentTypeId { get; set; }
    public string? HasTable { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public DeductionType? ParentType { get; set; }
    public ICollection<Deduction> Deductions { get; set; }
    public ICollection<DeductionType> InverseParentType { get; set; }
}