namespace Domain;

public class TestingNodeMember
{
    public string TestingNodeId { get; set; } = null!;
    public string TestingId { get; set; } = null!;
    public DateTime FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public DateTime? ExtendFromDate { get; set; }
    public DateTime? ExtendThruDate { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Testing Testing { get; set; } = null!;
    public TestingNode TestingNode { get; set; } = null!;
}