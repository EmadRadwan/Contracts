namespace Domain;

public class TestingSubtype
{
    public string TestingTypeId { get; set; } = null!;
    public string? SubtypeDescription { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }
}