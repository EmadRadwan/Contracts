namespace Domain;

public class PartyBenefit
{
    public string RoleTypeIdFrom { get; set; } = null!;
    public string RoleTypeIdTo { get; set; } = null!;
    public string PartyIdFrom { get; set; } = null!;
    public string PartyIdTo { get; set; } = null!;
    public string BenefitTypeId { get; set; } = null!;
    public DateTime FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public string? PeriodTypeId { get; set; }
    public decimal? Cost { get; set; }
    public double? ActualEmployerPaidPercent { get; set; }
    public int? AvailableTime { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public BenefitType BenefitType { get; set; } = null!;
    public Party PartyIdFromNavigation { get; set; } = null!;
    public Party PartyIdToNavigation { get; set; } = null!;
    public PartyRole PartyRole { get; set; } = null!;
    public PartyRole PartyRoleNavigation { get; set; } = null!;
}