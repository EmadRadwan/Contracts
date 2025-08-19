namespace Domain;

public class EmplPositionFulfillment
{
    public string EmplPositionId { get; set; } = null!;
    public string PartyId { get; set; } = null!;
    public DateTime FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public string? Comments { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public EmplPosition EmplPosition { get; set; } = null!;
    public Party Party { get; set; } = null!;
}