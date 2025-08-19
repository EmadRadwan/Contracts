namespace Domain;

public class Employment
{
    public Employment()
    {
        AgreementEmploymentAppls = new HashSet<AgreementEmploymentAppl>();
        PayHistories = new HashSet<PayHistory>();
    }

    public string RoleTypeIdFrom { get; set; } = null!;
    public string RoleTypeIdTo { get; set; } = null!;
    public string PartyIdFrom { get; set; } = null!;
    public string PartyIdTo { get; set; } = null!;
    public DateTime FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public string? TerminationReasonId { get; set; }
    public string? TerminationTypeId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Party PartyIdFromNavigation { get; set; } = null!;
    public Party PartyIdToNavigation { get; set; } = null!;
    public PartyRole PartyRole { get; set; } = null!;
    public PartyRole PartyRoleNavigation { get; set; } = null!;
    public ICollection<AgreementEmploymentAppl> AgreementEmploymentAppls { get; set; }
    public ICollection<PayHistory> PayHistories { get; set; }
}