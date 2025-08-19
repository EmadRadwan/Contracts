namespace Domain;

public class Addendum
{
    public string AddendumId { get; set; } = null!;
    public string? AgreementId { get; set; }
    public string? AgreementItemSeqId { get; set; }
    public DateTime? AddendumCreationDate { get; set; }
    public DateTime? AddendumEffectiveDate { get; set; }
    public string? AddendumText { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Agreement? Agreement { get; set; }
    public AgreementItem? AgreementI { get; set; }
}