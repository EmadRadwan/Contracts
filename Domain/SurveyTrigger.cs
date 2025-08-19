namespace Domain;

public class SurveyTrigger
{
    public string SurveyId { get; set; } = null!;
    public string SurveyApplTypeId { get; set; } = null!;
    public DateTime FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Survey Survey { get; set; } = null!;
    public SurveyApplType SurveyApplType { get; set; } = null!;
}