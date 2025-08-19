namespace Domain;

public class SurveyMultiResp
{
    public SurveyMultiResp()
    {
        SurveyMultiRespColumns = new HashSet<SurveyMultiRespColumn>();
    }

    public string SurveyId { get; set; } = null!;
    public string SurveyMultiRespId { get; set; } = null!;
    public string? MultiRespTitle { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Survey Survey { get; set; } = null!;
    public ICollection<SurveyMultiRespColumn> SurveyMultiRespColumns { get; set; }
}