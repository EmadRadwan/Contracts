namespace Domain;

public class EmplPositionType
{
    public EmplPositionType()
    {
        EmplPositionTypeClasses = new HashSet<EmplPositionTypeClass>();
        EmplPositionTypeRateNews = new HashSet<EmplPositionTypeRateNew>();
        InverseParentType = new HashSet<EmplPositionType>();
        RateAmounts = new HashSet<RateAmount>();
        ValidResponsibilities = new HashSet<ValidResponsibility>();
    }

    public string EmplPositionTypeId { get; set; } = null!;
    public string? ParentTypeId { get; set; }
    public string? HasTable { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public EmplPositionType? ParentType { get; set; }
    public ICollection<EmplPositionTypeClass> EmplPositionTypeClasses { get; set; }
    public ICollection<EmplPositionTypeRateNew> EmplPositionTypeRateNews { get; set; }
    public ICollection<EmplPositionType> InverseParentType { get; set; }
    public ICollection<RateAmount> RateAmounts { get; set; }
    public ICollection<ValidResponsibility> ValidResponsibilities { get; set; }
}