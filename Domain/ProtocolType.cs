namespace Domain;

public class ProtocolType
{
    public ProtocolType()
    {
        UserAgents = new HashSet<UserAgent>();
    }

    public string ProtocolTypeId { get; set; } = null!;
    public string? ProtocolName { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ICollection<UserAgent> UserAgents { get; set; }
}