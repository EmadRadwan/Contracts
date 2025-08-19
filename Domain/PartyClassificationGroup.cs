using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;
[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]

public class PartyClassificationGroup
{
    public PartyClassificationGroup()
    {
        InverseParentGroup = new HashSet<PartyClassificationGroup>();
        MarketInterests = new HashSet<MarketInterest>();
        PartyClassifications = new HashSet<PartyClassification>();
        SegmentGroupClassifications = new HashSet<SegmentGroupClassification>();
    }

    public string PartyClassificationGroupId { get; set; } = null!;
    public string? PartyClassificationTypeId { get; set; }
    public string? ParentGroupId { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public PartyClassificationGroup? ParentGroup { get; set; }
    public PartyClassificationType? PartyClassificationType { get; set; }
    public ICollection<PartyClassificationGroup> InverseParentGroup { get; set; }
    public ICollection<MarketInterest> MarketInterests { get; set; }
    public ICollection<PartyClassification> PartyClassifications { get; set; }
    public ICollection<SegmentGroupClassification> SegmentGroupClassifications { get; set; }
}