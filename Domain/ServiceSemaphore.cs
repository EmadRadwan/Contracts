namespace Domain;

public class ServiceSemaphore
{
    public string ServiceName { get; set; } = null!;
    public string? LockedByInstanceId { get; set; }
    public string? LockThread { get; set; }
    public DateTime? LockTime { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }
}