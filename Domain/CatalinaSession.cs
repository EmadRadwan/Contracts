namespace Domain;

public class CatalinaSession
{
    public string SessionId { get; set; } = null!;
    public int? SessionSize { get; set; }
    public byte[]? SessionInfo { get; set; }
    public string? IsValid { get; set; }
    public int? MaxIdle { get; set; }
    public int? LastAccessed { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }
}