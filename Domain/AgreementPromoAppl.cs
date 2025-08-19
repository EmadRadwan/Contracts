namespace Domain;

public class AgreementPromoAppl
{
    public string AgreementId { get; set; } = null!;
    public string AgreementItemSeqId { get; set; } = null!;
    public string ProductPromoId { get; set; } = null!;
    public DateTime FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public int? SequenceNum { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public AgreementItem AgreementI { get; set; } = null!;
    public ProductPromo ProductPromo { get; set; } = null!;
}