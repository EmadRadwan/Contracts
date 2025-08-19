namespace Domain;

public class EntityAuditLog
{
    public string AuditHistorySeqId { get; set; } = null!;
    public string? ChangedEntityName { get; set; }
    public string? ChangedFieldName { get; set; }
    public string? PkCombinedValueText { get; set; }
    public string? OldValueText { get; set; }
    public string? NewValueText { get; set; }
    public DateTime? ChangedDate { get; set; }
    public string? ChangedByInfo { get; set; }
    public string? ChangedSessionInfo { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }
}