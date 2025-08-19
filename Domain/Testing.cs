namespace Domain;

public class Testing
{
    public Testing()
    {
        TestingItems = new HashSet<TestingItem>();
        TestingNodeMembers = new HashSet<TestingNodeMember>();
    }

    public string TestingId { get; set; } = null!;
    public string? TestingTypeId { get; set; }
    public string? TestingName { get; set; }
    public string? Description { get; set; }
    public string? Comments { get; set; }
    public int? TestingSize { get; set; }
    public DateTime? TestingDate { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public TestingType? TestingType { get; set; }
    public ICollection<TestingItem> TestingItems { get; set; }
    public ICollection<TestingNodeMember> TestingNodeMembers { get; set; }
}