namespace Domain;

public class Component
{
    public Component()
    {
        TenantComponents = new HashSet<TenantComponent>();
    }

    public string ComponentName { get; set; } = null!;
    public string? RootLocation { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ICollection<TenantComponent> TenantComponents { get; set; }
}