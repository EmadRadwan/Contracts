namespace Domain;

public class PaymentContent
{
    public string PaymentId { get; set; } = null!;
    public string PaymentContentTypeId { get; set; } = null!;
    public string ContentId { get; set; } = null!;
    public DateTime FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Content Content { get; set; } = null!;
    public Payment Payment { get; set; } = null!;
    public PaymentContentType PaymentContentType { get; set; } = null!;
}