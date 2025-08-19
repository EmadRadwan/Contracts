namespace Domain;

public class AgreementEmploymentAppl
{
    public string AgreementId { get; set; } = null!;
    public string AgreementItemSeqId { get; set; } = null!;
    public string PartyIdFrom { get; set; } = null!;
    public string PartyIdTo { get; set; } = null!;
    public string RoleTypeIdFrom { get; set; } = null!;
    public string RoleTypeIdTo { get; set; } = null!;
    public DateTime FromDate { get; set; }
    public DateTime? AgreementDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public AgreementItem AgreementI { get; set; } = null!;
    public Employment Employment { get; set; } = null!;
}