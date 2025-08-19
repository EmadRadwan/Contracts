namespace Domain;

public class PartyClassification
{
    public string PartyId { get; set; } = null!;
    public string PartyClassificationGroupId { get; set; } = null!;
    public DateTime FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Party Party { get; set; } = null!;
    public PartyClassificationGroup PartyClassificationGroup { get; set; } = null!;
}