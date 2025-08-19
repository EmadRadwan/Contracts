namespace Domain;

public class TestingStatus
{
    public string TestingStatusId { get; set; } = null!;
    public string? TestingId { get; set; }
    public string? StatusId { get; set; }
    public DateTime? StatusDate { get; set; }
    public string? ChangeByUserLoginId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public UserLogin? ChangeByUserLogin { get; set; }
    public StatusItem? Status { get; set; }
}