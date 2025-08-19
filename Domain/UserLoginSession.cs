namespace Domain;

public class UserLoginSession
{
    public string UserLoginId { get; set; } = null!;
    public DateTime? SavedDate { get; set; }
    public string? SessionData { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public UserLogin UserLogin { get; set; } = null!;
}