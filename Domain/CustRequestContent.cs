namespace Domain;

public class CustRequestContent
{
    public string CustRequestId { get; set; } = null!;
    public string ContentId { get; set; } = null!;
    public DateTime FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Content Content { get; set; } = null!;
    public CustRequest CustRequest { get; set; } = null!;
}