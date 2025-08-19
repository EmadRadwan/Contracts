namespace Domain;

public class TestingNode
{
    public TestingNode()
    {
        InversePrimaryParentNode = new HashSet<TestingNode>();
        TestingNodeMembers = new HashSet<TestingNodeMember>();
    }

    public string TestingNodeId { get; set; } = null!;
    public string? PrimaryParentNodeId { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public TestingNode? PrimaryParentNode { get; set; }
    public ICollection<TestingNode> InversePrimaryParentNode { get; set; }
    public ICollection<TestingNodeMember> TestingNodeMembers { get; set; }
}