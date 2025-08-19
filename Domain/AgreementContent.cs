namespace Domain;

public class AgreementContent
{
    public string AgreementId { get; set; } = null!;
    public string AgreementItemSeqId { get; set; } = null!;
    public string AgreementContentTypeId { get; set; } = null!;
    public string ContentId { get; set; } = null!;
    public DateTime FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Agreement Agreement { get; set; } = null!;
    public AgreementContentType AgreementContentType { get; set; } = null!;
    public Content Content { get; set; } = null!;
}