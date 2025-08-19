namespace Domain;

public class UserLoginHistory
{
    public string UserLoginId { get; set; } = null!;
    public string? VisitId { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public string? PasswordUsed { get; set; }
    public string? SuccessfulLogin { get; set; }
    public string? OriginUserLoginId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }
    public string? PartyId { get; set; }

    public Party? Party { get; set; }
    public UserLogin UserLogin { get; set; } = null!;
}