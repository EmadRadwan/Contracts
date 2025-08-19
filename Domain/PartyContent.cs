namespace Domain;

public class PartyContent
{
    public string PartyId { get; set; } = null!;
    public string ContentId { get; set; } = null!;
    public string PartyContentTypeId { get; set; } = null!;
    public DateTime FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Content Content { get; set; } = null!;
    public Party Party { get; set; } = null!;
    public PartyContentType PartyContentType { get; set; } = null!;
}