namespace Domain;

public class PartyStatus
{
    public string StatusId { get; set; } = null!;
    public string PartyId { get; set; } = null!;
    public DateTime StatusDate { get; set; }
    public string? ChangeByUserLoginId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public UserLogin? ChangeByUserLogin { get; set; }
    public Party Party { get; set; } = null!;
    public StatusItem Status { get; set; } = null!;
}