namespace Domain;

public class KeywordThesauru
{
    public string EnteredKeyword { get; set; } = null!;
    public string AlternateKeyword { get; set; } = null!;
    public string? RelationshipEnumId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Enumeration? RelationshipEnum { get; set; }
}