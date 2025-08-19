namespace Domain;

public class PriorityType
{
    public PriorityType()
    {
        PartyRelationships = new HashSet<PartyRelationship>();
    }

    public string PriorityTypeId { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ICollection<PartyRelationship> PartyRelationships { get; set; }
}