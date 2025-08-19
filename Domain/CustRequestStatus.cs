namespace Domain;

public class CustRequestStatus
{
    public string CustRequestStatusId { get; set; } = null!;
    public string? StatusId { get; set; }
    public string? CustRequestId { get; set; }
    public string? CustRequestItemSeqId { get; set; }
    public DateTime? StatusDate { get; set; }
    public string? ChangeByUserLoginId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public UserLogin? ChangeByUserLogin { get; set; }
    public CustRequest? CustRequest { get; set; }
    public StatusItem? Status { get; set; }
}