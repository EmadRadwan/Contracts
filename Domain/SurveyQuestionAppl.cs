namespace Domain;

public class SurveyQuestionAppl
{
    public string SurveyId { get; set; } = null!;
    public string SurveyQuestionId { get; set; } = null!;
    public DateTime FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public string? SurveyPageSeqId { get; set; }
    public string? SurveyMultiRespId { get; set; }
    public string? SurveyMultiRespColId { get; set; }
    public string? RequiredField { get; set; }
    public int? SequenceNum { get; set; }
    public string? ExternalFieldRef { get; set; }
    public string? WithSurveyQuestionId { get; set; }
    public string? WithSurveyOptionSeqId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Survey Survey { get; set; } = null!;
    public SurveyQuestion SurveyQuestion { get; set; } = null!;
    public SurveyQuestionOption? WithSurvey { get; set; }
}