namespace Domain;

public class Tenant
{
    public Tenant()
    {
        TenantComponents = new HashSet<TenantComponent>();
        TenantDataSources = new HashSet<TenantDataSource>();
        TenantDomainNames = new HashSet<TenantDomainName>();
    }

    public string TenantId { get; set; } = null!;
    public string? TenantName { get; set; }
    public string? InitialPath { get; set; }
    public string? Disabled { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public TenantKeyEncryptingKey TenantKeyEncryptingKey { get; set; } = null!;
    public ICollection<TenantComponent> TenantComponents { get; set; }
    public ICollection<TenantDataSource> TenantDataSources { get; set; }
    public ICollection<TenantDomainName> TenantDomainNames { get; set; }
}