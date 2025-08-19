namespace Domain;

public class TenantDataSource
{
    public string TenantId { get; set; } = null!;
    public string EntityGroupName { get; set; } = null!;
    public string? JdbcUri { get; set; }
    public string? JdbcUsername { get; set; }
    public string? JdbcPassword { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Tenant Tenant { get; set; } = null!;
}