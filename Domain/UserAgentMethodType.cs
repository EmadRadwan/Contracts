namespace Domain;

public class UserAgentMethodType
{
    public UserAgentMethodType()
    {
        UserAgents = new HashSet<UserAgent>();
    }

    public string UserAgentMethodTypeId { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ICollection<UserAgent> UserAgents { get; set; }
}