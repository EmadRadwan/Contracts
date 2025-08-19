namespace Domain;

public class PlatformType
{
    public PlatformType()
    {
        UserAgents = new HashSet<UserAgent>();
    }

    public string PlatformTypeId { get; set; } = null!;
    public string? PlatformName { get; set; }
    public string? PlatformVersion { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ICollection<UserAgent> UserAgents { get; set; }
}