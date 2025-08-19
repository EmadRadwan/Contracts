namespace Domain;

public class WebPreferenceType
{
    public WebPreferenceType()
    {
        WebUserPreferences = new HashSet<WebUserPreference>();
    }

    public string WebPreferenceTypeId { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ICollection<WebUserPreference> WebUserPreferences { get; set; }
}