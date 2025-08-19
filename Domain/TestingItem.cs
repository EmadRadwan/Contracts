namespace Domain;

public class TestingItem
{
    public string TestingId { get; set; } = null!;
    public string TestingSeqId { get; set; } = null!;
    public string? TestingHistory { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Testing Testing { get; set; } = null!;
}