namespace Domain;

public class TestingType
{
    public TestingType()
    {
        Testings = new HashSet<Testing>();
    }

    public string TestingTypeId { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ICollection<Testing> Testings { get; set; }
}