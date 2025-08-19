namespace Domain;

public class WebUserPreference
{
    public string UserLoginId { get; set; } = null!;
    public string PartyId { get; set; } = null!;
    public string VisitId { get; set; } = null!;
    public string WebPreferenceTypeId { get; set; } = null!;
    public string? WebPreferenceValue { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Party Party { get; set; } = null!;
    public UserLogin UserLogin { get; set; } = null!;
    public WebPreferenceType WebPreferenceType { get; set; } = null!;
}