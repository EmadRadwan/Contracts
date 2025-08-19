namespace Domain;

public class WorkEffortSurveyAppl
{
    public string WorkEffortId { get; set; } = null!;
    public string SurveyId { get; set; } = null!;
    public DateTime FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ProductStoreSurveyAppl Survey { get; set; } = null!;
    public Survey SurveyNavigation { get; set; } = null!;
    public WorkEffort WorkEffort { get; set; } = null!;
}