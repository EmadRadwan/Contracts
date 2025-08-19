namespace Domain;

public class TarpittedLoginView
{
    public string ViewNameId { get; set; } = null!;
    public string UserLoginId { get; set; } = null!;
    public int? TarpitReleaseDateTime { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }
}