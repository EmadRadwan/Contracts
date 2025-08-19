namespace Domain;

public class TenantKeyEncryptingKey
{
    public string TenantId { get; set; } = null!;
    public string? KekText { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Tenant Tenant { get; set; } = null!;
}