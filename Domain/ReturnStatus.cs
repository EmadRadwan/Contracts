namespace Domain;

public class ReturnStatus
{
    public string ReturnStatusId { get; set; } = null!;
    public string? StatusId { get; set; }
    public string? ReturnId { get; set; }
    public string? ReturnItemSeqId { get; set; }
    public string? ChangeByUserLoginId { get; set; }
    public DateTime? StatusDatetime { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public UserLogin? ChangeByUserLogin { get; set; }
    public ReturnHeader? Return { get; set; }
    public StatusItem? Status { get; set; }
}