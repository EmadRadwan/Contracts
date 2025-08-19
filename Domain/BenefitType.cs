namespace Domain;

public class BenefitType
{
    public BenefitType()
    {
        InverseParentType = new HashSet<BenefitType>();
        PartyBenefits = new HashSet<PartyBenefit>();
    }

    public string BenefitTypeId { get; set; } = null!;
    public string? BenefitName { get; set; }
    public string? ParentTypeId { get; set; }
    public string? HasTable { get; set; }
    public string? Description { get; set; }
    public double? EmployerPaidPercentage { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public BenefitType? ParentType { get; set; }
    public ICollection<BenefitType> InverseParentType { get; set; }
    public ICollection<PartyBenefit> PartyBenefits { get; set; }
}