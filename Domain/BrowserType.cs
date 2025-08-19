namespace Domain;

public class BrowserType
{
    public BrowserType()
    {
        UserAgents = new HashSet<UserAgent>();
    }

    public string BrowserTypeId { get; set; } = null!;
    public string? BrowserName { get; set; }
    public string? BrowserVersion { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ICollection<UserAgent> UserAgents { get; set; }
}