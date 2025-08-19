namespace Domain;

public class EntityKeyStore
{
    public string KeyName { get; set; } = null!;
    public string? KeyText { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }
}