namespace Domain;

public class PartyQualType
{
    public PartyQualType()
    {
        InverseParentType = new HashSet<PartyQualType>();
        PartyQuals = new HashSet<PartyQual>();
    }

    public string PartyQualTypeId { get; set; } = null!;
    public string? ParentTypeId { get; set; }
    public string? HasTable { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public PartyQualType? ParentType { get; set; }
    public ICollection<PartyQualType> InverseParentType { get; set; }
    public ICollection<PartyQual> PartyQuals { get; set; }
}