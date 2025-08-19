namespace Domain;

public class TenantComponent
{
    public string TenantId { get; set; } = null!;
    public string ComponentName { get; set; } = null!;
    public int? SequenceNum { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Component ComponentNameNavigation { get; set; } = null!;
    public Tenant Tenant { get; set; } = null!;
}