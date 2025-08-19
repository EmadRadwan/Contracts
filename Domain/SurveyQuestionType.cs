namespace Domain;

public class SurveyQuestionType
{
    public SurveyQuestionType()
    {
        SurveyQuestions = new HashSet<SurveyQuestion>();
    }

    public string SurveyQuestionTypeId { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ICollection<SurveyQuestion> SurveyQuestions { get; set; }
}