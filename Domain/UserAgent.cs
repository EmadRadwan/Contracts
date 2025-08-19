namespace Domain;

public class UserAgent
{
    public UserAgent()
    {
        Visits = new HashSet<Visit>();
    }

    public string UserAgentId { get; set; } = null!;
    public string? BrowserTypeId { get; set; }
    public string? PlatformTypeId { get; set; }
    public string? ProtocolTypeId { get; set; }
    public string? UserAgentTypeId { get; set; }
    public string? UserAgentMethodTypeId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public BrowserType? BrowserType { get; set; }
    public PlatformType? PlatformType { get; set; }
    public ProtocolType? ProtocolType { get; set; }
    public UserAgentMethodType? UserAgentMethodType { get; set; }
    public UserAgentType? UserAgentType { get; set; }
    public ICollection<Visit> Visits { get; set; }
}