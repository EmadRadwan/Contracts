namespace Domain;

public class SurveyApplType
{
    public SurveyApplType()
    {
        ProductStoreSurveyAppls = new HashSet<ProductStoreSurveyAppl>();
        SurveyTriggers = new HashSet<SurveyTrigger>();
    }

    public string SurveyApplTypeId { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ICollection<ProductStoreSurveyAppl> ProductStoreSurveyAppls { get; set; }
    public ICollection<SurveyTrigger> SurveyTriggers { get; set; }
}