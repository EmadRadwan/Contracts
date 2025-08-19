namespace Domain;

public class RuntimeDatum
{
    public RuntimeDatum()
    {
        ApplicationSandboxes = new HashSet<ApplicationSandbox>();
        JobSandboxes = new HashSet<JobSandbox>();
        WorkEfforts = new HashSet<WorkEffort>();
    }

    public string RuntimeDataId { get; set; } = null!;
    public string? RuntimeInfo { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ICollection<ApplicationSandbox> ApplicationSandboxes { get; set; }
    public ICollection<JobSandbox> JobSandboxes { get; set; }
    public ICollection<WorkEffort> WorkEfforts { get; set; }
}